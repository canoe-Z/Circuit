using SpiceSharp.Components;

/// <summary>
/// 电流计
/// </summary>
public class Gmeter : EntityBase, ICalculatorUpdate
{
	private double MaxI;
	private double R = 10;
	private MyKnob myKnob;
	private MyPin myPin;
	private int PortID_Left, PortID_Right;

	public override void EntityAwake()
	{
		myPin = GetComponentInChildren<MyPin>();

		// 和元件自身属性相关的初始化要放在Awake()中，实例化后可能改变
		// 必须手动初始化Pin来保证Pin的初始化顺序
		myPin.PinAwake();
		myPin.CloseText();
		myPin.SetString("G", 150);

		myKnob = GetComponentInChildren<MyKnob>();
		myKnob.Devide = 5;

		// 第一次执行初始化，此后受事件控制
		myKnob.KnobEvent += UpdateKnob;
		UpdateKnob();
	}

	void Start()
	{
		// CalculatorUpdate()统一在Start()中执行，保证在实例化并写入元件自身属性完毕后执行
		CircuitCalculator.CalculateEvent += CalculatorUpdate;
		CalculatorUpdate();

		PortID_Left = ChildPorts[0].ID;
		PortID_Right = ChildPorts[1].ID;
	}

	public void CalculatorUpdate()
	{
		// 计算示数
		ChildPorts[0].I = (ChildPorts[1].U - ChildPorts[0].U) / R;
		double doublePin = ChildPorts[0].I / MaxI * 2;
		myPin.SetPos(doublePin + 0.5f);
	}

	private void UpdateKnob()
	{
		// 更新参数
		MaxI = 0.1;
		R = 10;
		for (int i = 0; i < myKnob.KnobPos_int; i++)
		{
			MaxI *= 0.01;
			R *= 2;
		}
	}

	public override void LoadElement() => CircuitCalculator.UF.Union(PortID_Left, PortID_Right);

	public override void SetElement(int entityID)
	{
		CircuitCalculator.SpiceEntities.Add(new Resistor(entityID.ToString(), PortID_Left.ToString(), PortID_Right.ToString(), R));
		CircuitCalculator.SpicePorts.AddRange(ChildPorts);
	}

	public override EntityData Save() => new SimpleEntityData<Gmeter>(this);
}
