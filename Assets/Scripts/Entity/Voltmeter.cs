﻿using SpiceSharp.Components;

public class Voltmeter : EntityBase, ICalculatorUpdate
{
	private readonly double MaxU0 = 1.5;
	private readonly double MaxU1 = 5;
	private readonly double MaxU2 = 15;

	private readonly double R0 = 1500;
	private readonly double R1 = 5000;
	private readonly double R2 = 15000;

	MyPin myPin;
	private int GND, V0, V1, V2;

	public override void EntityAwake()
	{
		myPin = GetComponentInChildren<MyPin>();

		// 和元件自身属性相关的初始化要放在Awake()中，实例化后可能改变
		myPin.PinAwake();
		myPin.SetString("V", 150);
	}

	void Start()
	{
		// CalculatorUpdate()统一在Start()中执行，保证在实例化并写入元件自身属性完毕后执行
		CircuitCalculator.CalculateEvent += CalculatorUpdate;
		CalculatorUpdate();

		GND = ChildPorts[0].ID;
		V0 = ChildPorts[1].ID;
		V1 = ChildPorts[2].ID;
		V2 = ChildPorts[3].ID;
	}
	public void CalculatorUpdate()
	{
		//计算指针偏移量
		double GNDu = ChildPorts[0].U;
		double doublePin = 0;
		doublePin += (ChildPorts[1].U - GNDu) / MaxU0;
		doublePin += (ChildPorts[2].U - GNDu) / MaxU1;
		doublePin += (ChildPorts[3].U - GNDu) / MaxU2;

		myPin.SetPos((float)doublePin);
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

		CircuitCalculator.SpicePorts.AddRange(ChildPorts);
	}

	public override EntityData Save() => new SimpleEntityData<Voltmeter>(transform.position, transform.rotation, ChildPortID);
}