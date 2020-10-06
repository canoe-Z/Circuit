using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharp.Components;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public class UJ25 : EntityBase
{
	private readonly int knobNum = 13;      // 含有的旋钮个数
	private readonly int buttonNum = 3;     // 含有的按钮个数
	private int buttonOnID = 2;             // 唯一启用的按钮（0，1，2）

	private double Rab;                     // 与待测电源并联的示零电阻，用户调节时，Rp将反方向变化
	private double Rcd;                     // 与标准电源并联的电阻，值根据En的后两位确定
	private double Rp;                      // 用标准电源校准时调节的电阻
	private double Rabp;					// 步骤2中使用的辅助电阻

	private int PortID_E_G, PortID_E_V;
	private int PortID_G_G, PortID_G_V;
	private int PortID_En_G, PortID_En_V;
	private int PortID_X1_G, PortID_X1_V;
	private int PortID_X2_G, PortID_X2_V;


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

		// 标准
		PortID_En_G = ChildPorts[2].ID;
		PortID_En_V = ChildPorts[3].ID;

		// 未知1
		PortID_X1_G = ChildPorts[4].ID;
		PortID_X1_V = ChildPorts[5].ID;

		// 未知2
		PortID_X2_G = ChildPorts[6].ID;
		PortID_X2_V = ChildPorts[7].ID;

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
		Rcd = 0;
		for (var i = 0; i != 6; i++)
		{
			Rcd += knobs[i].KnobPos_int * Math.Pow(10, -(i - 3));
			RcdTexts[i].text = knobs[i].KnobPos_int.ToString();
		}

		// 计算Rab和Rp，Rab最大值为10191.0
		double RabOffSet = knobs[11].KnobPos_int * 1 + knobs[12].KnobPos_int * 0.1;
		Rab = 10180.0 + RabOffSet;

		// 用户调节Rp示0，完成标准化，实际可能没有完成
		// 可调范围9000-24000
		Rp = (24000 - 9000) * knobs[6].KnobPos * (1 + 1e-1 * knobs[7].KnobPos) * (1 + 1e-2 * knobs[8].KnobPos) * (1 + 1e-3 * knobs[9].KnobPos)
			+ 9000 - RabOffSet;

		Rabp = Rab + Rp - Rcd;
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
		switch (uj25Mode)
		{
			case UJ25Mode.n:
				if (IsConnected(1))
				{
					// 三个元件互相连通
					CircuitCalculator.UF.ListUnion(new List<int> { PortID_E_G, PortID_G_G, PortID_G_V, PortID_E_V, PortID_En_G, PortID_En_V });
				}
				break;

			case UJ25Mode.x1:
				if (IsConnected(2))
				{
					CircuitCalculator.UF.ListUnion(new List<int> { PortID_E_G, PortID_G_G, PortID_G_V, PortID_E_V, PortID_X1_G, PortID_X1_V });

				}
				break;

			case UJ25Mode.x2:
				if (IsConnected(3))
				{
					CircuitCalculator.UF.ListUnion(new List<int> { PortID_E_G, PortID_G_G, PortID_G_V, PortID_E_V, PortID_X2_G, PortID_X2_V });
				}
				break;

			default:
				break;
		}
	}


	public override void SetElement(int entityID)
	{
		string GetName(string shortName) => string.Concat(entityID.ToString(), shortName);

		switch (uj25Mode)
		{
			case UJ25Mode.n:
				if (IsConnected(1))
				{
					CircuitCalculator.SpiceEntities.Add(new Resistor(
						GetName("Rab"),
						PortID_E_V.ToString(),
						GetName("PortG_G"),
						Rab));

					CircuitCalculator.SpiceEntities.Add(new Resistor(
						GetName("Rp"),
						GetName("PortG_G"),
						PortID_E_G.ToString(),
						Rp));

					CircuitCalculator.SpiceEntities.Add(new VoltageSource(
						GetName("En-E"),
						PortID_En_V.ToString(),
						PortID_E_V.ToString(),
						0));

					CircuitCalculator.SpiceEntities.Add(new VoltageSource(
						GetName("En-G"),
						PortID_En_G.ToString(),
						GetName("PortG_V"),
						0));
				}
				break;

			case UJ25Mode.x1:
				if (IsConnected(2))
				{
					CircuitCalculator.SpiceEntities.Add(new Resistor(
						GetName("Rcd"),
						PortID_E_V.ToString(),
						GetName("PortG_G"),
						Rcd));

					CircuitCalculator.SpiceEntities.Add(new Resistor(
						GetName("Rabp"),
						GetName("PortG_G"),
						PortID_E_G.ToString(),
						Rabp));

					CircuitCalculator.SpiceEntities.Add(new VoltageSource(
						GetName("X1-E"),
						PortID_X1_V.ToString(),
						PortID_E_V.ToString(),
						0));

					CircuitCalculator.SpiceEntities.Add(new VoltageSource(
						GetName("X1-G"),
						PortID_X1_G.ToString(),
						GetName("PortG_V"),
						0));
				}
				break;

			case UJ25Mode.x2:
				if (IsConnected(3))
				{
					CircuitCalculator.SpiceEntities.Add(new Resistor(
					GetName("Rcd"),
					PortID_E_V.ToString(),
					GetName("PortG_G"),
					Rcd));

					CircuitCalculator.SpiceEntities.Add(new Resistor(
						GetName("Rabp"),
						GetName("PortG_G"),
						PortID_E_G.ToString(),
						Rabp));

					CircuitCalculator.SpiceEntities.Add(new VoltageSource(
						GetName("X1-E"),
						PortID_X2_V.ToString(),
						PortID_E_V.ToString(),
						0));

					CircuitCalculator.SpiceEntities.Add(new VoltageSource(
						GetName("X1-G"),
						PortID_X2_G.ToString(),
						GetName("PortG_V"),
						0));
				}
				break;

			default:
				break;
		}

			switch (buttonOnID)
			{
				case 0: // 粗
					CircuitCalculator.SpiceEntities.Add(new Resistor(
						GetName("G_G"),
						PortID_G_G.ToString(),
						GetName("PortG_G"),
						20000));

					CircuitCalculator.SpiceEntities.Add(new VoltageSource(
						GetName("G_V"),
						PortID_G_V.ToString(),
						GetName("PortG_V"),
						0));
					break;

				case 1: // 细
					CircuitCalculator.SpiceEntities.Add(new VoltageSource(
						GetName("G_G"),
						PortID_G_G.ToString(),
						GetName("PortG_G"),
						0));

					CircuitCalculator.SpiceEntities.Add(new VoltageSource(
						GetName("G_V"),
						PortID_G_V.ToString(),
						GetName("PortG_V"),
						0));
					break;

				default:
					break;
			}
	}

	public override EntityData Save() => new UJ25Data(this);

	[System.Serializable]
	public class UJ25Data : EntityData
	{
		private readonly List<KnobData> knobDataList = new List<KnobData>();
		private int buttonOnID;

		public UJ25Data(UJ25 uj25)
		{
			baseData = new EntityBaseData(uj25);
			foreach (var knob in uj25.knobs)
			{
				knobDataList.Add(new KnobData(knob.KnobPos, knob.KnobPos_int));
			}
			buttonOnID = uj25.buttonOnID;
		}

		public override void Load()
		{
			UJ25 uj25 = BaseCreate<UJ25>(baseData);
			for (var i = 0; i < knobDataList.Count; i++)
			{
				// 此处尚未订阅事件，设置旋钮位置不会调用UpdateKnob()
				if (uj25.knobs[i].Devide == -1)
				{
					uj25.knobs[i].SetKnobRot(knobDataList[i].KnobPos);
				}
				else
				{
					uj25.knobs[i].SetKnobRot(knobDataList[i].KnobPos_int);
				}
			}
			uj25.buttonOnID = buttonOnID;
		}
	}
}
