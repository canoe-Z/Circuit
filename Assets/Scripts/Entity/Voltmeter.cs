using SpiceSharp.Components;
using UnityEngine;

public class Voltmeter : EntityBase
{
	public double MaxU0 = 1.5;
	public double MaxU1 = 5;
	public double MaxU2 = 15;

	public double R0 = 1500;
	public double R1 = 5000;
	public double R2 = 15000;

	MyPin myPin;

	public override void EntityAwake()
	{
		myPin = GetComponentInChildren<MyPin>();
		myPin.MySetString("V", 150);
	}

	void Update()
	{
		//计算指针偏移量
		double GNDu = ChildPorts[0].U;
		double doublePin = 0;
		doublePin += (ChildPorts[1].U - GNDu) / MaxU0;
		doublePin += (ChildPorts[2].U - GNDu) / MaxU1;
		doublePin += (ChildPorts[3].U - GNDu) / MaxU2;

		myPin.MyChangePos((float)doublePin);
	}


	public override void LoadElement()//添加元件
	{
		int GND = ChildPorts[0].ID;
		int V0 = ChildPorts[1].ID;
		int V1 = ChildPorts[2].ID;
		int V2 = ChildPorts[3].ID;
		CircuitCalculator.UF.Union(GND, V0);
		CircuitCalculator.UF.Union(GND, V1);
		CircuitCalculator.UF.Union(GND, V2);
	}

	public override void SetElement()//添加元件
	{
		int EntityID = CircuitCalculator.EntityNum;
		int GND = ChildPorts[0].ID;
		int V0 = ChildPorts[1].ID;
		int V1 = ChildPorts[2].ID;
		int V2 = ChildPorts[3].ID;
		//指定三个电阻的ID
		string[] ResistorID = new string[3];
		for (int i = 0; i < 3; i++)
		{
			ResistorID[i] = string.Concat(EntityID, "_", i);
		}
		//获取端口ID并完成内部连接
		CircuitCalculator.SpiceEntities.Add(new Resistor(ResistorID[0], GND.ToString(), V0.ToString(), R0));
		CircuitCalculator.SpiceEntities.Add(new Resistor(ResistorID[1], GND.ToString(), V1.ToString(), R1));
		CircuitCalculator.SpiceEntities.Add(new Resistor(ResistorID[2], GND.ToString(), V2.ToString(), R2));
		CircuitCalculator.SpicePorts.Add(ChildPorts[0]);
		CircuitCalculator.SpicePorts.Add(ChildPorts[1]);
		CircuitCalculator.SpicePorts.Add(ChildPorts[2]);
		CircuitCalculator.SpicePorts.Add(ChildPorts[3]);
	}

	public override EntityData Save()
	{
		return new SimpleEntityData<Voltmeter>(transform.position, transform.rotation, ChildPortID);
	}
}