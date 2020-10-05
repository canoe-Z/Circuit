﻿using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharp.Components;
using UnityEngine;
using UnityEngine.UI;

public class UJ25 : EntityBase
{
	private readonly int knobNum = 13;      // 含有的旋钮个数

	private double Rab;						// 与待测电源并联的示零电阻，用户调节时，Rp将反方向变化
	private double Rcd;						// 与标准电源并联的电阻，值根据En的后两位确定
	private double Rp;						// 用标准电源校准时调节的电阻

	private int PortID_E_G, PortID_E_V;
	private int PortID_G_G, PortID_G_V;
	private int PortID_En_G, PortID_En_V;
	private int PortID_X1_G, PortID_X1_V;
	private int PortID_X2_G, PortID_X2_V;

	private double lastRcd, lastRp, Rabp;

	// 四种工作模式，对应N，断，X1，X2
	private enum UJ25Mode { n, disconnect, x1, x2 }
	UJ25Mode uj25Mode;

	private List<MyKnob> knobs;
	private List<Text> RcdTexts;

	public override void EntityAwake()
	{
		knobs = transform.FindComponentsInChildren<MyKnob>().OrderBy(x => int.Parse(x.name)).ToList();
		if (knobs.Count != knobNum) Debug.LogError("旋钮个数不合法");

		RcdTexts = transform.FindComponentsInChildren<Text>().OrderBy(x => int.Parse(x.name)).ToList();

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
	}

	private void UpdateKnob()
	{
		if (uj25Mode == UJ25Mode.n)
		{
			// 计算Rab和Rp
			Rab = 1018 + knobs[6].KnobPos_int * 0.1 + knobs[7].KnobPos_int * 0.01;

			// 用户调节Rp示0，完成标准化，实际可能没有完成
			// 可调范围982-2282
			Rp = (2282 - 982) * knobs[8].KnobPos_int + 982;

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
		else if (uj25Mode == UJ25Mode.x1 || uj25Mode == UJ25Mode.x2)
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
	}

	// UJ25的电源端连接时视为已连接
	public override bool IsConnected()
	{
		if (ChildPorts[8].IsConnected || ChildPorts[9].IsConnected)
		{
			return true;
		}
		else
		{
			return false;
		}
	}


	public override void LoadElement()
	{
		switch (uj25Mode)
		{
			case UJ25Mode.n:
				CircuitCalculator.UF.Union(PortID_E_G, PortID_E_V);
				CircuitCalculator.UF.Union(PortID_En_G, PortID_En_V);
				CircuitCalculator.UF.Union(PortID_G_G, PortID_G_V);
				break;

			case UJ25Mode.x1:
				CircuitCalculator.UF.Union(PortID_E_G, PortID_E_V);
				CircuitCalculator.UF.Union(PortID_X1_G, PortID_X1_V);
				CircuitCalculator.UF.Union(PortID_G_G, PortID_G_V);
				break;

			case UJ25Mode.x2:
				CircuitCalculator.UF.Union(PortID_E_G, PortID_E_V);
				CircuitCalculator.UF.Union(PortID_X2_G, PortID_X2_V);
				CircuitCalculator.UF.Union(PortID_G_G, PortID_G_V);
				break;

			default:
				break;
		}
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
				CircuitCalculator.SpiceEntities.Add(new Resistor(
					GetName("Rab"),
					PortID_E_V.ToString(),
					PortID_G_G.ToString(),
					Rab));

				CircuitCalculator.SpiceEntities.Add(new Resistor(
					GetName("Rp"),
					PortID_G_G.ToString(),
					PortID_E_G.ToString(),
					Rp));

				CircuitCalculator.SpiceEntities.Add(new VoltageSource(
					GetName("En_V"),
					PortID_En_V.ToString(),
					PortID_E_V.ToString(),
					0));
				CircuitCalculator.SpiceEntities.Add(new VoltageSource(
					GetName("En-G"),
					PortID_En_G.ToString(),
					PortID_G_V.ToString(),
					0));
				break;

			case UJ25Mode.x1:
				CircuitCalculator.SpiceEntities.Add(new Resistor(
					GetName("Rabp"),
					PortID_E_V.ToString(),
					PortID_G_G.ToString(),
					Rabp));

				CircuitCalculator.SpiceEntities.Add(new Resistor(
					GetName("Rcd"),
					PortID_G_G.ToString(),
					PortID_E_G.ToString(),
					Rcd));

				CircuitCalculator.SpiceEntities.Add(new VoltageSource(
					GetName("X1-G"),
					PortID_X1_V.ToString(),
					PortID_G_V.ToString(),
					0));

				CircuitCalculator.SpiceEntities.Add(new VoltageSource(
					GetName("X1-E"),
					PortID_X1_G.ToString(),
					PortID_E_G.ToString(),
					0));
				break;

			case UJ25Mode.x2:
				CircuitCalculator.SpiceEntities.Add(new Resistor(
					GetName("Rabp"),
					PortID_E_V.ToString(),
					PortID_G_G.ToString(),
					Rabp));

				CircuitCalculator.SpiceEntities.Add(new Resistor(
					GetName("Rcd"),
					PortID_G_G.ToString(),
					PortID_E_G.ToString(),
					Rcd));

				CircuitCalculator.SpiceEntities.Add(new VoltageSource(
					GetName("X2-G"),
					PortID_X2_V.ToString(),
					PortID_G_V.ToString(),
					0));

				CircuitCalculator.SpiceEntities.Add(new VoltageSource(
					GetName("X2-E"),
					PortID_X2_G.ToString(),
					PortID_E_G.ToString(),
					0));
				break;

			default:
				break;
		}
	}

	public static GameObject Create()
	{
		return BaseCreate<UJ25>().Set().gameObject;
	}

	private UJ25 Set()
	{
		return this;
	}

	public override EntityData Save() => new UJ25Data(this);

	[System.Serializable]
	public class UJ25Data : EntityData
	{
		private readonly List<int> knobRotIntList = new List<int>();

		public UJ25Data(UJ25 uj25)
		{
			baseData = new EntityBaseData(uj25);
			//uj25.knobs.ForEach(x => knobRotIntList.Add(x.KnobPos_int));
		}

		public override void Load()
		{
			/*
			UJ25 uj25 = BaseCreate<UJ25>(baseData);
			for (var i = 0; i < knobRotIntList.Count; i++)
			{
				// 此处尚未订阅事件，设置旋钮位置不会调用UpdateKnob()
				uj25.knobs[i].SetKnobRot(knobRotIntList[i]);
			}
			*/
		}
	}
}
