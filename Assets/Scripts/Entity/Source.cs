using System.Collections.Generic;
using SpiceSharp.Components;
using UnityEngine;

/// <summary>
/// 单个独立电源，准确使用时可用于标准电源等场景，派生出标称类，用于待测电源和干电池
/// </summary>
public class Source : EntityBase, ISource
{
	[ReadOnly] public double E = 1.5f;
	[ReadOnly] public double R = 100;
	private int G, V;

	public override void EntityAwake() { }

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

	public override EntityData Save()
	{
		return new SourceStandData(E, transform.position, transform.rotation, ChildPortID);
	}
}

[System.Serializable]
public class SourceStandData : EntityData
{
	private readonly double value;
	public SourceStandData(double value, Vector3 pos, Quaternion angle, List<int> id) : base(pos, angle, id)
	{
		this.value = value;
	}

	public override void Load()
	{
		Source source = EntityCreator.CreateEntity<Source>(posfloat, anglefloat, IDList);
		source.E = value;
	}
}