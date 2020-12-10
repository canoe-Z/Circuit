using SpiceSharp.Components;
using UnityEngine;

/// <summary>
/// 电流计
/// </summary>
public class Gmeter : EntityBase, ICalculatorUpdate
{
	private double MaxI;
	private double R;
	private MyKnob myKnob;
	private MyPin myPin;
	private bool notChangeMyPinPos = false;         // 不改变MyPin的位置
	private int PortID_Left, PortID_Right;

	public override void EntityAwake()
	{
		myPin = GetComponentInChildren<MyPin>();

		// 和元件自身属性相关的初始化要放在Awake()中，实例化后可能改变
		// 必须手动初始化Pin来保证Pin的初始化顺序
		myPin.PinAwake();
		myPin.CloseText();
		myPin.SetString("检流计", 150);

		myKnob = GetComponentInChildren<MyKnob>();
		myKnob.Devide = 8;
	}

	void Start()
	{
		// 第一次执行初始化，此后受事件控制
		myKnob.KnobEvent += UpdateKnob;
		UpdateKnob();

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
		if (notChangeMyPinPos)  // 强制0.5f
		{
			myPin.SetPos(0.5f);
		}
		else
		{
			double doublePin = ChildPorts[0].I / MaxI * 2;
			myPin.SetPos(doublePin + 0.5f);
		}
	}

	private void UpdateKnob()
	{
		// 更新参数
		switch (myKnob.KnobPos_int)
		{
			case 0: //表头保护
				notChangeMyPinPos = true;
				MaxI = 10000;   //你强任你强，我就是不动
				R = 1e-8;
				break;
			case 1: //调零
				notChangeMyPinPos = true;
				MaxI = 10000;
				R = 1e-8;
				break;
			case 2: //1uA
				notChangeMyPinPos = false;
				MaxI = 1e-6;
				R = 1;
				break;
			case 3: //300nA
				notChangeMyPinPos = false;
				MaxI = 300 * 1e-9;
				R = 3;
				break;
			case 4: //100nA
				notChangeMyPinPos = false;
				MaxI = 100 * 1e-9;
				R = 10;
				break;
			case 5: //30nA
				notChangeMyPinPos = false;
				MaxI = 30 * 1e-9;
				R = 30;
				break;
			case 6: //10nA
				notChangeMyPinPos = false;
				MaxI = 10 * 1e-9;
				R = 100;
				break;
			case 7: //3nA
				notChangeMyPinPos = false;
				MaxI = 3 * 1e-9;
				R = 300;
				break;
			default:
				Debug.LogError("总之就是有个bug");
				break;
		}
	}

	public override void LoadElement() => CircuitCalculator.UF.Union(PortID_Left, PortID_Right);

	public override void SetElement(int entityID)
	{
		CircuitCalculator.SpiceEntities.Add(new Resistor(entityID.ToString(), PortID_Left.ToString(), PortID_Right.ToString(), R));
		CircuitCalculator.SpicePorts.AddRange(ChildPorts);
	}

	public override EntityData Save() => new GmeterData(this);

	[System.Serializable]
	public class GmeterData : EntityData
	{
		private readonly int knobPos_int;
		private readonly float randomFloat;
		public GmeterData(Gmeter gmeter)
		{
			baseData = new EntityBaseData(gmeter);
			knobPos_int = gmeter.myKnob.KnobPos_int;
			randomFloat = gmeter.myPin.knobZero.KnobPos;
		}

		public override void Load()
		{
			Gmeter gmeter = BaseCreate<Gmeter>(baseData);
			gmeter.myKnob.SetKnobRot(knobPos_int);
			gmeter.myPin.knobZero.SetKnobRot(randomFloat);
		}
	}
}
