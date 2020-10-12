using SpiceSharp.Components;
using SpiceSharp.Components.Bipolars;
using System;
using UnityEngine.UI;

/// <summary>
/// 热敏电阻
/// </summary>
public class Thermistor : EntityBase, ICalculatorUpdate,IShow
{
	//组件
	public MyKnob knob;
	public Text txtWill;
	public Text txtNow;
	public MySwitch mySwitch;
	public override void EntityAwake()
	{
		knob.Devide = -1;
		knob.CanLoop = false;
		knob.SetKnobRot((float)(willT - willTMin) / (float)(willTMax - willTMin));

		mySwitch.IsOn = false;
	}

	//电路
	double resistance;
	protected int PortID_Left, PortID_Right;

	//渐近线
	double willT = MySettings.roomTemperature;
	double nowT = MySettings.roomTemperature;
	const double willTMax = 100;
	const double willTMin = 0;
	const double kPerSecond = 0.1;//每秒上升的比例
	float deltaTime = 0;
	void Update()
	{
		double realWillT = willT;//备份一下

		if (!mySwitch.IsOn)//如果关机
		{
			realWillT = MySettings.roomTemperature;//温度变为室温
		}


		//算出每帧上升比例
		double bili = kPerSecond * UnityEngine.Time.deltaTime;
		if (bili > 1) bili = 1;

		//将当前温度调节至目标温度
		nowT += (realWillT - nowT) * bili;

		if (mySwitch.IsOn)//如果开机
		{
			//更新显示
			txtWill.text = "目标温度：" + realWillT.ToString("0.00") + "℃";
			txtNow.text = "当前温度：" + nowT.ToString("0.00") + "℃";
		}
		else
		{
			txtWill.text = "";
			txtNow.text = "";
		}


		deltaTime += UnityEngine.Time.deltaTime;
		if (deltaTime > MySettings.hotRInterval)//触发
		{

			//电路计算
			resistance = ResistanceOf(nowT);
			CircuitCalculator.NeedCalculateByConnection = true;
			//触发结束
			deltaTime = 0;
		}
	}
	double ResistanceOf(double T)//温度转电阻
	{
		return 30 * Math.Pow(10, -Math.Log10(30) / 99 * T);
	}


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

	public void MyShowString()
	{
		DisplayController.myTipsToShow = "热敏电阻\n当前真实阻值：" + resistance.ToString("0.000000");
	}
}

