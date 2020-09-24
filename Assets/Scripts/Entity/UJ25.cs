using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharp.Components;
using UnityEngine;

public class UJ25 : EntityBase
{
	private readonly int knobNum = 11;                           // 含有的旋钮个数

	private double Rab;
	private double Rcd;
	private double Rp;
	private double T;
	private double En;
	private const double E20 = 1.01860;

	private int PortID_G_G, PortID_G_V;
	private int PortID_Normal_G, PortID_Normal_V;
	private int PortID_X1_G, PortID_X1_V;
	private int PortID_X2_G, PortID_X2_V;

	private List<MyKnob> knobs;

	public override void EntityAwake()
	{
		knobs = transform.FindComponentsInChildren<MyKnob>().OrderBy(x => x.name).ToList();
		if (knobs.Count != knobNum) Debug.LogError("旋钮个数不合法");

		// 第一次执行初始化，此后受事件控制
		knobs.ForEach(x => x.KnobEvent += UpdateKnob);
		UpdateKnob();

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
		PortID_G_G = ChildPorts[0].ID;
		PortID_G_V = ChildPorts[1].ID;
		PortID_Normal_G = ChildPorts[2].ID;
		PortID_Normal_V = ChildPorts[3].ID;
		PortID_X1_G = ChildPorts[4].ID;
		PortID_X1_V = ChildPorts[5].ID;
		PortID_X2_G = ChildPorts[6].ID;
		PortID_X2_V = ChildPorts[7].ID;
	}

	private void UpdateKnob()
	{
		// 计算温度补偿电阻Rab
		Rab = 1018 + knobs[6].KnobPos_int * 0.1 + knobs[7].KnobPos_int * 0.01;

		// 计算Rcd,5为高位旋钮(e2),0为最低位(e-3)
		Rcd = 0;
		for (var i = 0; i != 6; i++)
		{
			Rcd += knobs[i].KnobPos_int * Math.Pow(10, i - 3);
		}

		// 标准电池温度修正公式
		En = E20 - 3.99e-5 * (T - 20) - 0.94e-6 * Math.Pow(T - 20, 2.0)
			+ 9e-9 * Math.Pow(T - 20, 3.0);

	}

	// Start is called before the first frame update


	// Update is called once per frame
	void Update()
	{

	}

	public override void LoadElement()
	{
		throw new System.NotImplementedException();
	}

	public override void SetElement(int entityID)
	{
		throw new System.NotImplementedException();
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
