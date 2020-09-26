using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharp.Components;
using UnityEngine;
using UnityEngine.UI;

public class UJ25 : EntityBase
{
	private readonly int knobNum = 13;      // 含有的旋钮个数

	private double Rab;         // 与待测电源并联的示零电阻，用户调节时，Rp将反方向变化
	private double Rcd;         // 与标准电源并联的电阻，值根据En的后两位确定
	private double Rp;          // 用标准电源校准时调节的电阻
	private double Rg;          // 为检流计串联的电阻
	private double T = 25;      // 当前温度
	private double En;          // 当前温度下的标准电池电动势
	private const double E20 = 1.01860;     //20摄氏度下的标准电池电动势

	private int PortID_E_G, PortID_E_V;
	private int PortID_G_G, PortID_G_V;
	private int PortID_Normal_G, PortID_Normal_V;
	private int PortID_X1_G, PortID_X1_V;
	private int PortID_X2_G, PortID_X2_V;
	private int PortID_A, PortID_B, PortID_C, PortID_D, PortID_E;

	// 四种工作模式，对应N，断，X1，X2
	private enum UJ25Mode { n, disconnect, x1, x2 }
	UJ25Mode uj25Mode;

	private List<MyKnob> knobs;
	private List<Text> RcdTexts;

	public override void EntityAwake()
	{
		knobs = transform.FindComponentsInChildren<MyKnob>().OrderBy(x => x.name).ToList();
		if (knobs.Count != knobNum) Debug.LogError("旋钮个数不合法");

		RcdTexts = transform.FindComponentsInChildren<Text>().OrderBy(x => x.name).ToList();

		// 0-5为Rcd调节旋钮，6-7为Rab调节，8-11为Rp调节旋钮（连续），
		// 12为UJ25的切换开关

		// Rcd调节旋钮，最高位旋钮可以调节至18，其余旋钮调节至10
		knobs[0].Devide = 19;
		for (var i = 1; i != 6; i++)
		{
			knobs[i].Devide = 11;
		}

		// Rab调节旋钮，可调节至10
		for (var i = 6; i != 8; i++)
		{
			knobs[i].Devide = 10;
		}

		// UJ25挡位切换，共5挡
		knobs[12].Devide = 5;
	}

	void Start()
	{
		// 第一次执行初始化，此后受事件控制
		knobs.ForEach(x => x.KnobEvent += UpdateKnob);
		UpdateKnob();

		PortID_G_G = ChildPorts[0].ID;
		PortID_G_V = ChildPorts[1].ID;
		PortID_Normal_G = ChildPorts[2].ID;
		PortID_Normal_V = ChildPorts[3].ID;
		PortID_X1_G = ChildPorts[4].ID;
		PortID_X1_V = ChildPorts[5].ID;
		PortID_X2_G = ChildPorts[6].ID;
		PortID_X2_V = ChildPorts[7].ID;
	}

	double lastRcd;
	double lastRp;
	double Rabp;
	private void UpdateKnob()
	{
		if(uj25Mode== UJ25Mode.n)
		{
			// 计算Rab和Rp
			Rab = 1018 + knobs[6].KnobPos_int * 0.1 + knobs[7].KnobPos_int * 0.01;

			// 用户调节Rp示0，完成标准化，实际可能没有完成
			Rp = (2282 - 982) * knobs[6].KnobPos_int + 982;

			// 计算Rcd并显示,5为高位旋钮(e2),0为最低位(e-3)
			Rcd = 0;
			for (var i = 0; i != 6; i++)
			{
				Rcd += knobs[i].KnobPos_int * Math.Pow(10, i - 3);
				RcdTexts[i].text = knobs[5 - i].KnobPos_int.ToString();
			}
			lastRcd = Rcd;

			// 即使用户设置了Rcd也令其为0
			Rcd = 0;
		}
		else if(uj25Mode==UJ25Mode.x1 || uj25Mode == UJ25Mode.x2)
		{
			// 在用户切到X1或X2时记录 Rabp = Rab + Rp
			// 计算Rcd并显示,5为高位旋钮(e2),0为最低位(e-3)
			Rcd = 0;
			for (var i = 0; i != 6; i++)
			{
				Rcd += knobs[i].KnobPos_int * Math.Pow(10, i - 3);
				RcdTexts[i].text = knobs[5 - i].KnobPos_int.ToString();
			}
			double offsetRcd = Rcd - lastRcd;
			lastRcd = Rcd;

			Rp = (2282 - 982) * knobs[6].KnobPos_int + 982;
			double offsetRp = Rp - lastRp;
			lastRp = Rp;
			Rabp = Rabp - offsetRcd + offsetRp;
			if (Rabp < 0) Rabp = 0;
		}

		// 标准电池温度修正公式
		En = E20 - 3.99e-5 * (T - 20) - 0.94e-6 * Math.Pow(T - 20, 2.0)
			+ 9e-9 * Math.Pow(T - 20, 3.0);
	}

	public override void LoadElement()
	{
		throw new System.NotImplementedException();
	}


	public override void SetElement(int entityID)
	{
		string GetName(string shortName)
		{
			return string.Concat(entityID.ToString(), shortName);
		}

		switch (uj25Mode)
		{
			case UJ25Mode.n:
				// Rab
				CircuitCalculator.SpiceEntities.Add(new Resistor(
					GetName("Rab"),
					PortID_A.ToString(),
					PortID_B.ToString(),
					Rab));

				// 不接Rcd
				// Rp
				CircuitCalculator.SpiceEntities.Add(new Resistor(
					GetName("Rab"),
					PortID_B.ToString(),
					PortID_E.ToString(),
					Rp));
				break;
			case UJ25Mode.disconnect:
				// 外部连接均断路
				break;
			case UJ25Mode.x1:
				// En，Ex1两端口断路
				// Rabp
				CircuitCalculator.SpiceEntities.Add(new Resistor(
					GetName("Rab"),
					PortID_A.ToString(),
					PortID_B.ToString(),
					Rab));

				// Rcd
				CircuitCalculator.SpiceEntities.Add(new Resistor(
					GetName("Rcd"),
					PortID_B.ToString(),
					PortID_D.ToString(),
					Rcd));

				// Rp
				CircuitCalculator.SpiceEntities.Add(new Resistor(
					GetName("Rp"),
					PortID_D.ToString(),
					PortID_E.ToString(),
					Rp));
				break;

			case UJ25Mode.x2:
				break;
			default:
				break;
		}
	}

	public override EntityData Save() => new UJ25Data(this);

	[System.Serializable]
	public class UJ25Data : EntityData
	{
		private readonly List<int> knobRotIntList = new List<int>();

		public UJ25Data(UJ25 uj25)
		{
			baseData = new EntityBaseData(uj25);
			uj25.knobs.ForEach(x => knobRotIntList.Add(x.KnobPos_int));
		}

		public override void Load()
		{
			UJ25 uj25 = BaseCreate<UJ25>(baseData);
			for (var i = 0; i < knobRotIntList.Count; i++)
			{
				// 此处尚未订阅事件，设置旋钮位置不会调用UpdateKnob()
				uj25.knobs[i].SetKnobRot(knobRotIntList[i]);
			}
		}
	}
}
