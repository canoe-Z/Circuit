using SpiceSharp.Components;

public class Ammeter : EntityBase, ICalculatorUpdate
{
	private const double MaxI0 = 0.05;
	private const double MaxI1 = 0.1;
	private const double MaxI2 = 0.5;

	private const double R0 = 2;
	private const double R1 = 1;
	private const double R2 = 0.2;

	private int GND, V0, V1, V2;
	private readonly string[] ResistorID = new string[3];

	private MyPin myPin;

	public override void EntityAwake()
	{
		myPin = GetComponentInChildren<MyPin>();
		myPin.PinAwake();
		myPin.SetString("A", 150);
	}

	void Start()
	{
		// CalculatorUpdate()统一在Start()中执行，以防内阻量程改变
		CircuitCalculator.CalculateEvent += CalculatorUpdate;
		CalculatorUpdate();

		GND = ChildPorts[0].ID;
		V0 = ChildPorts[1].ID;
		V1 = ChildPorts[2].ID;
		V2 = ChildPorts[3].ID;
	}

	public void CalculatorUpdate()
	{
		// 计算自身电流
		ChildPorts[1].I = (ChildPorts[1].U - ChildPorts[0].U) / R0;
		ChildPorts[2].I = (ChildPorts[2].U - ChildPorts[0].U) / R1;
		ChildPorts[3].I = (ChildPorts[3].U - ChildPorts[0].U) / R2;

		double doublePin = 0;
		doublePin += (ChildPorts[1].I) / MaxI0;
		doublePin += (ChildPorts[2].I) / MaxI1;
		doublePin += (ChildPorts[3].I) / MaxI2;

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
		// 电流表属于简单元件
		return new SimpleEntityData<Ammeter>(transform.position, transform.rotation, ChildPortID);
	}
}
