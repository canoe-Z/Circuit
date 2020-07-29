using SpiceSharp.Components;
using UnityEngine;

public class Ammeter : EntityBase, IAmmeter
{
	public double MaxI0 = 0.05;
	public double MaxI1 = 0.1;
	public double MaxI2 = 0.5;

	public double R0 = 2;
	public double R1 = 1;
	public double R2 = 0.2;

	public int GND, V0, V1, V2;

	private GameObject pin = null;
	// 1单位1分米1600像素，750像素=0.46875，1500像素=0.9375，800爆表0.5
	private float pinPos = 0;

	public override void EntityAwake()
	{
		pin = transform.GetChildByName("Pin").gameObject;
	}

	void Update()
	{
		double doublePin = 0;
		doublePin += (ChildPorts[1].I) / MaxI0;
		doublePin += (ChildPorts[2].I) / MaxI1;
		doublePin += (ChildPorts[3].I) / MaxI2;
		doublePin -= 0.5;
		pinPos = (float)(doublePin * 0.9375);
		if (pinPos > 0.5) pinPos = 0.5f;
		else if (pinPos < -0.5) pinPos = -0.5f;

		Vector3 pos = pin.transform.localPosition;
		pos.z = pinPos;
		pin.transform.localPosition = pos;
	}

	void Start()
    {
		GND = ChildPorts[0].ID;
		V0 = ChildPorts[1].ID;
		V1 = ChildPorts[2].ID;
		V2 = ChildPorts[3].ID;
	}

	public override void LoadElement()
	{
		CircuitCalculator.UF.Union(GND, V0);
		CircuitCalculator.UF.Union(GND, V1);
		CircuitCalculator.UF.Union(GND, V2);
	}
	public override void SetElement()
	{
		int EntityID = CircuitCalculator.EntityNum;

		string[] ResistorID = new string[3];
		for (int i = 0; i < 3; i++)
		{
			ResistorID[i] = string.Concat(EntityID, "_", i);
		}

		CircuitCalculator.SpiceEntities.Add(new Resistor(ResistorID[0], GND.ToString(), V0.ToString(), R0));
		CircuitCalculator.SpiceEntities.Add(new Resistor(ResistorID[1], GND.ToString(), V1.ToString(), R1));
		CircuitCalculator.SpiceEntities.Add(new Resistor(ResistorID[2], GND.ToString(), V2.ToString(), R2));

		CircuitCalculator.SpicePorts.Add(ChildPorts[0]);
		CircuitCalculator.SpicePorts.Add(ChildPorts[1]);
		CircuitCalculator.SpicePorts.Add(ChildPorts[2]);
		CircuitCalculator.SpicePorts.Add(ChildPorts[3]);
	}

	// 计算自身电流
	public void CalculateCurrent()
	{
		ChildPorts[1].I = (ChildPorts[1].U - ChildPorts[0].U) / R0;
		ChildPorts[2].I = (ChildPorts[2].U - ChildPorts[0].U) / R1;
		ChildPorts[3].I = (ChildPorts[3].U - ChildPorts[0].U) / R2;
	}

	public override EntityData Save()
	{
		// 电流表属于简单元件
		return new SimpleEntityData<Ammeter>(transform.position, transform.rotation, ChildPortID);
	}
}
