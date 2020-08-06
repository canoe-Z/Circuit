using System.Collections.Generic;
using SpiceSharp.Components;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 单个独立电源，准确使用时可用于标准电源等场景，派生出标称类，用于待测电源和干电池
/// </summary>
public class Source : EntityBase, ISource
{
	protected double E, R;
	protected int PortID_G, PortID_V;

	protected Text sourceText;
	public override void EntityAwake()
	{
		sourceText = GetComponentInChildren<Text>();
	}

	void Start()
	{
		sourceText.text = E.ToString() + "V";

		PortID_G = ChildPorts[0].ID;
		PortID_V = ChildPorts[1].ID;
	}

	public override void LoadElement() => CircuitCalculator.UF.Union(PortID_G, PortID_V);

	public override void SetElement()
	{
		int EntityID = CircuitCalculator.EntityNum;

		CircuitCalculator.SpiceEntities.Add(new VoltageSource(EntityID.ToString(), PortID_V.ToString(), string.Concat(EntityID.ToString(), "_rPort"), E));
		CircuitCalculator.SpiceEntities.Add(new Resistor(string.Concat(EntityID.ToString(), "_r"), string.Concat(EntityID.ToString(), "_rPort"), PortID_G.ToString(), R));
		//默认不接地，连接到电路中使用，如果电路中没有形成对0的通路，将其接地
	}

	public void GroundCheck()
	{
		if (IsConnected())
		{
			if (!CircuitCalculator.UF.Connected(PortID_G, 0))
			{
				CircuitCalculator.UF.Union(PortID_G, 0);
				CircuitCalculator.GNDLines.Add(new GNDLine(PortID_G));
			}
		}
	}

	public static GameObject Create(double E, double R, Float3 pos = null, Float4 angle = null, List<int> IDList = null)
	{
		Source source = BaseCreate<Source>(pos, angle, IDList);
		source.E = E;
		source.R = R;
		return source.gameObject;
	}

	public override EntityData Save() => new SourceStandData(E, R, transform.position, transform.rotation, ChildPortID);
}

[System.Serializable]
public class SourceStandData : EntityData
{
	private readonly double E, R;
	public SourceStandData(double E, double R, Vector3 pos, Quaternion angle, List<int> IDList) : base(pos, angle, IDList)
	{
		this.E = E;
		this.R = R;
	}

	public override void Load() => Source.Create(E, R, pos, angle, IDList);
}