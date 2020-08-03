using SpiceSharp.Components;
using UnityEngine;

public class Voltmeter : EntityBase, ICalculatorUpdate
{
	private const double MaxU0 = 1.5;
	private const double MaxU1 = 5;
	private const double MaxU2 = 15;

	private const double R0 = 1500;
	private const double R1 = 5000;
	private const double R2 = 15000;

	MyPin myPin;
	private int GND, V0, V1, V2;

	public override void EntityAwake()
	{
		myPin = GetComponentInChildren<MyPin>();
		myPin.PinAwake();
		myPin.SetString("V", 150);
	}

	void Start()
	{
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