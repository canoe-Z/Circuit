﻿using System;
using System.Collections.Generic;
using System.Linq;

public class QJ45 : EntityBase
{
	private double Ra;                      // 比例臂电阻
	private double Rb;                      // 比例臂电阻
	private double Rn;                      // 标准电阻，比较臂

	private int PortID_X_0, PortID_X_1;

	public MyKnob[] knobs;
	public MyButton[] buttons;
	private int buttonOnID = -1;            // 唯一启用的按钮（0，1，2）
	public override void EntityAwake()
	{
		// 0-3为Rn调节旋钮
		for (var i = 0; i != 4; i++)
		{
			knobs[i].Devide = 10;
		}
	}

	void Start()
	{
		// TODO: 第一次执行初始化，此后受事件控制
		knobs.ToList().ForEach(x => x.KnobEvent += UpdateKnob);
		UpdateKnob();

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

	public void Update()
	{
	}

	private void UpdateKnob()
	{
		// 计算Rn，0为高位（e3）,3为低位（e0)
		Rn = 0;
		for (var i = 0; i != 4; i++)
		{
			Rn += knobs[i].KnobPos_int * Math.Pow(10, -(i - 3));
		}
	}

	public override void LoadElement()
	{
		CircuitCalculator.UF.Union(PortID_X_0, PortID_X_1);
	}


	public override void SetElement(int entityID)
	{
	}

	public override EntityData Save() => new QJ45Data(this);

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
