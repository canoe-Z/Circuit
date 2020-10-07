using SpiceSharp.Components;
using SpiceSharp.Components.Bipolars;
using UnityEngine.UI;

/// <summary>
/// 热敏电阻
/// </summary>
public class Thermistor : EntityBase, ICalculatorUpdate
{
	//组件
	public MyKnob knob;
	public Text txtWill;
	public Text txtNow;
	public override void EntityAwake()
	{
		mySwitch = transform.FindComponent_DFS<MySwitch>("MySwitch");
		// 默认启动时开机，读档可覆盖该设置
		mySwitch.IsOn = true;

		knob.Devide = -1;
		knob.CanLoop = false;
	}

	//电路
	double resistance;
	protected int PortID_Left, PortID_Right;

	//渐近线
	double willT = 90;
	double nowT = 30;
	const double willTMax = 100;
	const double willTMin = 0;
	const double kPerSecond = 0.5;//每秒上升的比例
	float deltaTime = 0;
	void Update()
	{
		deltaTime += UnityEngine.Time.deltaTime;
		if (deltaTime > MySettings.hotRInterval)//触发
		{
			//算出比例
			double bili = kPerSecond * deltaTime;
			if (bili > 1) bili = 1;

			//将当前温度调节至目标温度
			nowT += (willT - nowT) * bili;

			//电路计算
			resistance = ResistanceOf(nowT);
			CircuitCalculator.NeedCalculateByConnection = true;
			//触发结束
			deltaTime = 0;
		}
	}
	double ResistanceOf(double T)//温度转电阻
	{
		return T;
	}



	private MySwitch mySwitch;
	void UpdateKnob()
	{
		willT = (willTMax - willTMin) * knob.KnobPos + willTMin;
	}
	void Start()
	{
		// CalculatorUpdate()统一在Start()中执行，保证在实例化并写入元件自身属性完毕后执行
		CircuitCalculator.CalculateEvent += CalculatorUpdate;

		CalculatorUpdate();
		PortID_Left = ChildPorts[0].ID;
		PortID_Right = ChildPorts[1].ID;

		knob.KnobEvent += UpdateKnob;
		UpdateKnob();
	}

	public void CalculatorUpdate()//电路计算之后
	{
		txtWill.text = willT.ToString("000.00");
		txtNow.text = nowT.ToString("000.00");
	}

	public override void LoadElement()
	{
		CircuitCalculator.UF.Union(PortID_Left, PortID_Right);
	}


	public override void SetElement(int entityID)
	{
		CircuitCalculator.SpiceEntities.Add(new Resistor(entityID.ToString(), PortID_Left.ToString(), PortID_Right.ToString(), resistance));
	}

	public override EntityData Save() => new SimpleEntityData<Thermistor>(this);
}

