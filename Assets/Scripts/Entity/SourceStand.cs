using System.Collections.Generic;
using SpiceSharp.Components;
using UnityEngine;

public class SourceStand : EntityBase, ISource
{
	public double E = 1.5f;
	public double R = 100;
	private int G, V;

	public override void EntityAwake() { }

	override public bool IsConnected()
	{
		if (ChildPorts[0].Connected == 1 || ChildPorts[1].Connected == 1)
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	override public void LoadElement()
	{
		G = ChildPorts[0].ID;
		V = ChildPorts[1].ID;
		CircuitCalculator.UF.Union(G, V);
	}

	override public void SetElement()
	{
		int EntityID = CircuitCalculator.EntityNum;
		G = ChildPorts[0].ID;
		V = ChildPorts[1].ID;

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
		return new SourceStandData(transform.position, transform.rotation, ChildPortID);
	}
}

[System.Serializable]
public class SourceStandData : EntityData
{
	public SourceStandData(Vector3 pos, Quaternion angle, List<int> id) : base(pos, angle, id) { }

	override public void Load()
	{
		EntityCreator.CreateEntity<SourceStand>(posfloat, anglefloat, IDList);
	}
}