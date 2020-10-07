using SpiceSharp.Components;
using UnityEngine.UI;

/// <summary>
/// 热敏电阻
/// </summary>
public class Thermistor : EntityBase, ICalculatorUpdate
{
	private double RValue;
	private double TWill = 90;
	private double TNow = 30;
	private MyKnob knob;
	private Text TWillText;
	private Text TNowText;
	protected int PortID_Left, PortID_Right;

	private MySwitch mySwitch;


	public override void EntityAwake()
	{
		TWillText = transform.GetChildByName("Will").GetComponent<Text>();
		TNowText = transform.GetChildByName("Now").GetComponent<Text>();
		knob = GetComponentInChildren<MyKnob>();
		mySwitch = transform.FindComponent_DFS<MySwitch>("MySwitch");

		// 默认启动时开机，读档可覆盖该设置
		mySwitch.IsOn = true;
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
		TWillText.text = TWill.ToString();
		TNowText.text = TNow.ToString();
	}

	void FixedUpdate()
	{
		if (TWill > TNow)
		{
			TNow += 0.1;
		}
		else
		{
			TWill += 0.1;
		}
		RValue = 100 * TNow;
		CircuitCalculator.NeedCalculate = true;
	}

	public override void LoadElement() => CircuitCalculator.UF.Union(PortID_Left, PortID_Right);


	public override void SetElement(int entityID)
	{
		CircuitCalculator.SpiceEntities.Add(new Resistor(entityID.ToString(), PortID_Left.ToString(), PortID_Right.ToString(), RValue));
	}

	public override EntityData Save() => new SimpleEntityData<Thermistor>(this);
}

