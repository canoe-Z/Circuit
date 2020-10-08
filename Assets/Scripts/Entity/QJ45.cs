using SpiceSharp.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QJ45 : EntityBase, ICalculatorUpdate, ISource
{
	private double Ra;                      // 比例臂电阻
	private double Rb = 1000;               // 比例臂电阻
	private double rate;                    // 比例臂
	private double Rn;                      // 标准电阻，比较臂

	private bool isExternalG = false;
	private bool isExternalE = false;


	private int PortID_X_0, PortID_X_1;

	public MyKnob[] knobs;
	public MyButton[] buttons;
	private int buttonOnID = -1;            // 唯一启用的按钮（0，1，2）
	private MyPin myPin;
	private int entityID;

	public override void EntityAwake()
	{
		myPin = GetComponentInChildren<MyPin>();

		// 和元件自身属性相关的初始化要放在Awake()中，实例化后可能改变
		// 必须手动初始化Pin来保证Pin的初始化顺序
		myPin.PinAwake();
		myPin.CloseText();
		myPin.SetString("检流计", 150);

		// 0-3为Rn调节旋钮
		for (var i = 0; i != 5; i++)
		{
			knobs[i].Devide = 11;
		}

		// 4为比例臂切换
		knobs[4].Devide = 8;
	}

	void Start()
	{
		// 第一次执行初始化，此后受事件控制
		knobs.ToList().ForEach(x => x.KnobEvent += UpdateKnob);
		UpdateKnob();

		// CalculatorUpdate()统一在Start()中执行，保证在实例化并写入元件自身属性完毕后执行
		CircuitCalculator.CalculateEvent += CalculatorUpdate;
		CalculatorUpdate();

		// 更新按钮状态
		for (var i = 0; i < buttons.Length; i++)
		{
			int k = i;
			buttons[i].IsOn = i == buttonOnID;
			buttons[i].ButtonEvent += () => UpdateOneButton(k);
			UpdateOneButton(k);
		}

		// 待测电阻
		PortID_X_0 = ChildPorts[0].ID;
		PortID_X_1 = ChildPorts[1].ID;
	}

	private void UpdateOneButton(int buttonID)
	{
		// 同时只能开启一个
		if (buttons[buttonID].IsOn)
		{
			buttonOnID = buttonID;
			for (var i = 0; i < buttons.Length; i++)
			{
				if (i != buttonID)
				{
					if (buttons[i].IsOn)
					{
						buttons[i].IsOn = false;
					}
				}
			}
		}
		else
		{
			if (!buttons.Select(x => x.IsOn).Contains(true))
			{
				buttonOnID = -1;
			}
		}
	}

	private void UpdateKnob()
	{
		// 计算Rn，0为高位（e3）,3为低位（e0)
		Rn = 0;
		for (var i = 0; i != 4; i++)
		{
			Rn += knobs[i].KnobPos_int * Math.Pow(10, -(i - 3));
		}

		// 切换比例臂
		switch (knobs[4].KnobPos_int)
		{
			case 0:
				rate = 1.0 / 1000;
				break;
			case 1:
				rate = 1.0 / 100;
				break;
			case 2:
				rate = 1.0 / 10;
				break;
			case 3:
				rate = 1.0 / 9;
				break;
			case 4:
				rate = 1.0 / 4;
				break;
			case 5:
				rate = 1.0 / 1;
				break;
			case 6:
				rate = 10.0 / 1;
				break;
			case 7:
				rate = 100.0 / 1;
				break;
		}
		Ra = rate * Rb;
	}

	public override void LoadElement()
	{
		CircuitCalculator.UF.Union(PortID_X_0, PortID_X_1);
	}


	private string GetName(string shortName) => string.Concat(entityID.ToString(), shortName);

	public override void SetElement(int entityID)
	{
		this.entityID = entityID;
		CircuitCalculator.SpiceEntities.Add(new Resistor(GetName("Rb"), GetName("A"), GetName("C"), Rb));
		CircuitCalculator.SpiceEntities.Add(new Resistor(GetName("Ra"), GetName("A"), GetName("D"), Ra));
		CircuitCalculator.SpiceEntities.Add(new Resistor(GetName("Rn"), GetName("B"), GetName("C"), Rn));

		CircuitCalculator.SpiceEntities.Add(new VoltageSource(GetName("Rx_0"), GetName("B"), PortID_X_0.ToString(), 0));
		CircuitCalculator.SpiceEntities.Add(new VoltageSource(GetName("Rx_1"), GetName("D"), PortID_X_1.ToString(), 0));

		if(!isExternalG)
		{
			// 内置检流计
			CircuitCalculator.SpiceEntities.Add(new Resistor(GetName("G"), GetName("C"), GetName("D"), 100));
			CircuitCalculator.InnerSpicePorts.Add(GetName("C"), -1);
			CircuitCalculator.InnerSpicePorts.Add(GetName("D"), -1);
		}

		if(!isExternalE)
		{
			// 内置4.5V电源
			CircuitCalculator.SpiceEntities.Add(new VoltageSource(GetName("E"), GetName("A"), GetName("B"), 4.5));
		}
	}

	public override EntityData Save() => new QJ45Data(this);

	public void CalculatorUpdate()
	{
		double maxI = 1e-6;
		if(IsConnected())
		{
			double pos = (CircuitCalculator.InnerSpicePorts[GetName("C")] - CircuitCalculator.InnerSpicePorts[GetName("D")]) / maxI;
			myPin.SetPos(0.5f + pos);
		}
		else
		{
			myPin.SetPos(0.5f);
		}
	}

	public void GroundCheck()
	{
		if (IsConnected() && !isExternalE)
		{
			CircuitCalculator.UF.Union(PortID_X_0, 0);
		}
	}

	[System.Serializable]
	public class QJ45Data : EntityData
	{
		private readonly List<int> knobRotIntList;
		private readonly int buttonOnID;

		public QJ45Data(QJ45 qj45)
		{
			baseData = new EntityBaseData(qj45);
			knobRotIntList = qj45.knobs.Select(x => x.KnobPos_int).ToList();
			buttonOnID = qj45.buttonOnID;
		}

		public override void Load()
		{
			QJ45 qj45 = BaseCreate<QJ45>(baseData);
			for (var i = 0; i < knobRotIntList.Count; i++)
			{
				// 此处尚未订阅事件，设置旋钮位置不会调用UpdateKnob()
				qj45.knobs[i].SetKnobRot(knobRotIntList[i]);
			}
			qj45.buttonOnID = buttonOnID;
		}
	}
}
