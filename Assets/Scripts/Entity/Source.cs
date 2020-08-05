using System.Collections.Generic;
using SpiceSharp.Components;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 单个独立电源，准确使用时可用于标准电源等场景，派生出标称类，用于待测电源和干电池
/// </summary>
public class Source : EntityBase, ISource
{
	private double E = 1.5f;
	private double R = 100;
	public string strToShow = "忘记设置了";

	private int G, V;

	Text text;
	public override void EntityAwake()
	{
		text = GetComponentInChildren<Text>();
	}
	void Update()
	{
		text.text = strToShow;
	}

	void Start()
	{
		G = ChildPorts[0].ID;
		V = ChildPorts[1].ID;
	}

	public override void LoadElement()
	{
		CircuitCalculator.UF.Union(G, V);
	}

	public override void SetElement()
	{
		int EntityID = CircuitCalculator.EntityNum;

		CircuitCalculator.SpiceEntities.Add(new VoltageSource(EntityID.ToString(), V.ToString(), string.Concat(EntityID.ToString(), "_rPort"), E));
		CircuitCalculator.SpiceEntities.Add(new Resistor(string.Concat(EntityID.ToString(), "_r"), string.Concat(EntityID.ToString(), "_rPort"), G.ToString(), R));
		//默认不接地，连接到电路中使用，如果电路中没有形成对0的通路，将其接地
	}

	public void GroundCheck()
	{
		if (IsConnected())
		{
			if (!CircuitCalculator.UF.Connected(G, 0))
			{
				CircuitCalculator.UF.Union(G, 0);
				CircuitCalculator.GNDLines.Add(new GNDLine(G));
			}
		}
	}

	public static Source Create(double E, Float3 pos = null, Float4 angle = null, List<int> IDList = null)
	{
		Source source = BaseCreate<Source>(pos, angle, IDList);
		source.E = E;
		return source;
	}

	public override EntityData Save()
	{
		return new SourceStandData(E, transform.position, transform.rotation, ChildPortID);
	}
}

[System.Serializable]
public class SourceStandData : EntityData
{
	private readonly double value;
	public SourceStandData(double value, Vector3 pos, Quaternion angle, List<int> IDList) : base(pos, angle, IDList)
	{
		this.value = value;
	}

	public override void Load() => Source.Create(value, pos, angle, IDList);
}