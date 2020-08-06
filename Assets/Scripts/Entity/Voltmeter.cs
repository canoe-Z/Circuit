using SpiceSharp.Components;

/// <summary>
/// 三量程电压表
/// </summary>
public class Voltmeter : EntityBase, ICalculatorUpdate
{
	private readonly double MaxU0 = 1.5;
	private readonly double MaxU1 = 5;
	private readonly double MaxU2 = 15;

	private readonly double R0 = 1500;
	private readonly double R1 = 5000;
	private readonly double R2 = 15000;

	private MyPin myPin;
	private int PortID_GND, PortID_V0, PortID_V1, PortID_V2;

	public override void EntityAwake()
	{
		myPin = GetComponentInChildren<MyPin>();

		// 和元件自身属性相关的初始化要放在Awake()中，实例化后可能改变
		myPin.PinAwake();
		myPin.CloseText();
		myPin.SetString("V", 150);
	}

	void Start()
	{
		// CalculatorUpdate()统一在Start()中执行，保证在实例化并写入元件自身属性完毕后执行
		CircuitCalculator.CalculateEvent += CalculatorUpdate;
		CalculatorUpdate();

		PortID_GND = ChildPorts[0].ID;
		PortID_V0 = ChildPorts[1].ID;
		PortID_V1 = ChildPorts[2].ID;
		PortID_V2 = ChildPorts[3].ID;
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
		CircuitCalculator.UF.Union(PortID_GND, PortID_V0);
		CircuitCalculator.UF.Union(PortID_GND, PortID_V1);
		CircuitCalculator.UF.Union(PortID_GND, PortID_V2);
	}

	public override void SetElement(int entityID)
	{
		CircuitCalculator.SpiceEntities.Add(new Resistor(string.Concat(entityID, "_", 0), PortID_GND.ToString(), PortID_V0.ToString(), R0));
		CircuitCalculator.SpiceEntities.Add(new Resistor(string.Concat(entityID, "_", 1), PortID_GND.ToString(), PortID_V1.ToString(), R1));
		CircuitCalculator.SpiceEntities.Add(new Resistor(string.Concat(entityID, "_", 2), PortID_GND.ToString(), PortID_V2.ToString(), R2));

		CircuitCalculator.SpicePorts.AddRange(ChildPorts);
	}

	public override EntityData Save() => new SimpleEntityData<Voltmeter>(transform.position, transform.rotation, ChildPortID);
}