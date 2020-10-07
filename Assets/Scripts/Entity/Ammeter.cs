using SpiceSharp.Components;

/// <summary>
/// 三量程电流表
/// </summary>
public class Ammeter : EntityBase, ICalculatorUpdate
{
	private readonly double MaxI0 = 0.015;
	private readonly double MaxI1 = 0.15;
	private readonly double MaxI2 = 1.5;

	private readonly double R0 = 2;
	private readonly double R1 = 1;
	private readonly double R2 = 0.2;

	private int PortID_GND, PortID_V0, PortID_V1, PortID_V2;
	private MyPin myPin;

	public override void EntityAwake()
	{
		myPin = GetComponentInChildren<MyPin>();

		// 和元件自身属性相关的初始化要放在Awake()中，实例化后可能改变
		// 必须手动初始化Pin来保证Pin的初始化顺序
		myPin.PinAwake();
		myPin.CloseText();
		myPin.SetString("A", 150);
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
		// 计算自身电流
		ChildPorts[1].I = (ChildPorts[1].U - ChildPorts[0].U) / R0;
		ChildPorts[2].I = (ChildPorts[2].U - ChildPorts[0].U) / R1;
		ChildPorts[3].I = (ChildPorts[3].U - ChildPorts[0].U) / R2;

		double doublePin = 0;
		doublePin += (ChildPorts[1].I) / MaxI0;
		doublePin += (ChildPorts[2].I) / MaxI1;
		doublePin += (ChildPorts[3].I) / MaxI2;

		myPin.SetPos(doublePin);
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

	public override EntityData Save() => new AmmeterData(this);

	[System.Serializable]
	public class AmmeterData : EntityData
	{
		private readonly float randomFloat;
		public AmmeterData(Ammeter ammeter)
		{
			baseData = new EntityBaseData(ammeter);
			randomFloat = ammeter.myPin.knobZero.KnobPos;
		}

		public override void Load()
		{
			Ammeter ammeter = BaseCreate<Ammeter>(baseData);
			ammeter.myPin.knobZero.SetKnobRot(randomFloat);
		}
	}
}
