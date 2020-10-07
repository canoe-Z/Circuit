using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharp.Components;
using UnityEngine;
using UnityEngine.UI;

public class QJ45 : EntityBase
{
	private readonly int knobNum = 13;      // 含有的旋钮个数
	private readonly int buttonNum = 3;     // 含有的按钮个数
	private int buttonOnID = 2;             // 唯一启用的按钮（0，1，2）

	private double Ra;						// 比例臂电阻
	private double Rb;						// 比例臂电阻
	private double Rn;						// 标准电阻，比较臂

	private int PortID_E_G, PortID_E_V;
	private int PortID_G_G, PortID_G_V;
	private int PortID_X_1, PortID_X_2;

	// 四种工作模式，对应N，断，X1，X2
	private enum UJ25Mode { n, disconnect, x1, x2 }
	UJ25Mode uj25Mode;

	private List<MyKnob> knobs;
	private List<MyButton> buttons;
	public Text[] RcdTexts = new Text[6];

	public override void EntityAwake()
	{
		knobs = transform.FindComponentsInChildren<MyKnob>().OrderBy(x => int.Parse(x.name)).ToList();
		if (knobs.Count != knobNum) Debug.LogError("旋钮个数不合法");

		// 0粗，1细，2短路
		buttons = transform.FindComponentsInChildren<MyButton>().OrderBy(x => int.Parse(x.name)).ToList();
		if (buttons.Count != buttonNum) Debug.LogError("按钮个数不合法");

		// 0-5为Rcd调节旋钮，6-9为Rp调节，10为模式切换旋钮，11-12为Rab调节，
		// Rcd调节旋钮，最高位旋钮可以调节至18，其余旋钮调节至10
		knobs[0].Devide = 19;
		for (var i = 1; i != 6; i++)
		{
			knobs[i].Devide = 11;
		}

		// UJ25挡位切换，共5挡
		knobs[10].AngleRange = 225;
		knobs[10].Devide = 5;
		knobs[10].IsChangeConnection = true;
		knobs[10].SetKnobRot(3);

		// Rab调节旋钮，可调节至10
		knobs[11].Devide = 11;
		knobs[12].Devide = 11;

		for (var i = 0; i != 6; i++)
		{
			RcdTexts[i].text = "0";
		}
	}

	void Start()
	{
		// 第一次执行初始化，此后受事件控制
		knobs.ForEach(x => x.KnobEvent += UpdateKnob);
		UpdateKnob();

		// 更新按钮状态
		for (var i = 0; i < buttons.Count; i++)
		{
			int k = i;
			buttons[i].IsOn = i == buttonOnID;
			buttons[i].ButtonEvent += () => UpdateOneButton(k);
			UpdateOneButton(k);
		}

		// 电计
		PortID_G_G = ChildPorts[0].ID;
		PortID_G_V = ChildPorts[1].ID;

		// 电源
		PortID_E_G = ChildPorts[8].ID;
		PortID_E_V = ChildPorts[9].ID;
	}

	private void UpdateOneButton(int buttonID)
	{
		// 同时只能开启一个
		if (buttons[buttonID].IsOn)
		{
			buttonOnID = buttonID;
			for (var i = 0; i < buttons.Count; i++)
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
				buttons[2].IsOn = true;
			}
		}
	}

	public void Update()
	{
		/*
		Debug.LogWarning("Rab为：" + Rab.ToString());
		Debug.LogWarning("Rcd为：" + Rcd.ToString());
		Debug.LogWarning("Rp为：" + Rp.ToString());
		Debug.LogWarning("Rabp为：" + Rabp.ToString());
		*/
	}

	private void UpdateKnob()
	{
		// 模式切换
		switch (knobs[10].KnobPos_int)
		{
			case 0:
				uj25Mode = UJ25Mode.x2;
				break;
			case 1:
				uj25Mode = UJ25Mode.disconnect;
				break;
			case 2:
				uj25Mode = UJ25Mode.x1;
				break;
			case 3:
				uj25Mode = UJ25Mode.disconnect;
				break;
			case 4:
				uj25Mode = UJ25Mode.n;
				break;
			default:
				uj25Mode = UJ25Mode.disconnect;
				break;
		}

		// 计算Rcd并显示,0为高位旋钮(电阻e3/对应电压e-1),5为最低位(e-2/对应e-6)
		Rn = 0;
		for (var i = 0; i != 6; i++)
		{
			Rn += knobs[i].KnobPos_int * Math.Pow(10, -(i - 3));
			RcdTexts[i].text = knobs[i].KnobPos_int.ToString();
		}
	}

	// 电计短路，模式为断开，未接电计或未接电源，不视为启用
	public override bool IsConnected()
	{
		return uj25Mode != UJ25Mode.disconnect && buttonOnID != 2 && IsConnected(0) && IsConnected(4);
	}

	/// <summary>
	/// 判断二端口连接状态
	/// </summary>
	/// <param name="n">0电计，1标准，2未知1，3未知2，4电源</param>
	/// <returns></returns>
	public bool IsConnected(int n) => ChildPorts[2 * n + 1].IsConnected && ChildPorts[2 * n].IsConnected;

	public override void LoadElement()
	{
	}


	public override void SetElement(int entityID)
	{
	}

	public override EntityData Save() => new QJ45Data(this);

	[System.Serializable]
	public class QJ45Data : EntityData
	{
		private readonly List<KnobData> knobDataList = new List<KnobData>();
		private int buttonOnID;

		public QJ45Data(QJ45 qj45)
		{
			baseData = new EntityBaseData(qj45);
			foreach (var knob in qj45.knobs)
			{
				knobDataList.Add(new KnobData(knob.KnobPos, knob.KnobPos_int));
			}
			buttonOnID = qj45.buttonOnID;
		}

		public override void Load()
		{
			QJ45 qj45 = BaseCreate<QJ45>(baseData);
			for (var i = 0; i < knobDataList.Count; i++)
			{
				// 此处尚未订阅事件，设置旋钮位置不会调用UpdateKnob()
				if (qj45.knobs[i].Devide == -1)
				{
					qj45.knobs[i].SetKnobRot(knobDataList[i].KnobPos);
				}
				else
				{
					qj45.knobs[i].SetKnobRot(knobDataList[i].KnobPos_int);
				}
			}
			qj45.buttonOnID = buttonOnID;
		}
	}
}
