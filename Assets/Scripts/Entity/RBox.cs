using System.Collections.Generic;
using System.Linq;
using SpiceSharp.Components;
using UnityEngine;

/// <summary>
/// 电阻箱
/// </summary>
public class RBox : EntityBase
{
	private readonly int knobNum = 6;                           // 含有的旋钮个数

	// 对应99999,99,9
	private readonly double[] R = new double[3];                // 不同挡位下的内阻
	private readonly double[] tolerance = new double[3];        // 不同挡位下的误差限
	private double[] nominal = new double[3];					// 不同挡位下包含误差的内阻

	private int PortID_G, PortID_R999, PortID_R99, PortID_R9;
	private List<MyKnob> knobs;

	private bool isLoad = false;

	public override void EntityAwake()
	{
		knobs = transform.FindComponentsInChildren<MyKnob>().OrderBy(x => x.name).ToList();
		if (knobs.Count != knobNum) Debug.LogError("旋钮个数不合法");
		knobs.ForEach(x => x.Devide = 10);

		// 第一次执行初始化，此后受事件控制
		knobs.ForEach(x => x.KnobEvent += UpdateKnob);
		UpdateKnob();
	}

	void Start()
	{
		PortID_G = ChildPorts[0].ID;
		PortID_R9 = ChildPorts[1].ID;
		PortID_R99 = ChildPorts[2].ID;
		PortID_R999 = ChildPorts[3].ID;
	}

	void Update()
	{
		Debug.Log("准确值" + R[1].ToString());
		Debug.Log("误差限" + tolerance[1].ToString());
		Debug.Log("模糊值" + nominal[1].ToString());
	}

	private void UpdateKnob()
	{
		// 不确定度相关
		// ZX-21型电阻箱铭牌数据
		// X10000	X1000	X100	X10		X1		X0.1
		// 1000		1000	1000	2000	5000	50000	*10e-6
		// R0 = 20m
		// 按均匀分布处理
		for (var i = 0; i < 3; i++)
		{
			tolerance[i] = 0;
		}

		int total = 0;
		for (int i = 0; i < knobNum; i++)
		{
			total *= 10;
			total += knobs[i].KnobPos_int;

			// 5为最低位旋钮，计算误差限
			switch (i)
			{
				case 0:
					tolerance[0] += 10000 * knobs[i].KnobPos_int * 1000 * 1e-6;
					break;
				case 1:
					tolerance[0] += 1000 * knobs[i].KnobPos_int * 1000 * 1e-6;
					break;
				case 2:
					tolerance[0] += 100 * knobs[i].KnobPos_int * 1000 * 1e-6;
					break;
				case 3:
					tolerance[0] += 10 * knobs[i].KnobPos_int * 2000 * 1e-6;
					break;
				case 4:
					tolerance[0] += knobs[i].KnobPos_int * 5000 * 1e-6;
					tolerance[1] += knobs[i].KnobPos_int * 5000 * 1e-6;
					break;
				case 5:
					tolerance[0] += 0.1 * knobs[i].KnobPos_int * 50000 * 1e-6;
					tolerance[1] += 0.1 * knobs[i].KnobPos_int * 50000 * 1e-6;
					tolerance[2] += 0.1 * knobs[i].KnobPos_int * 50000 * 1e-6;
					break;
				default:
					break;
			}
		}

		// 误差限中的R0
		for (var i = 0; i < 3; i++)
		{
			tolerance[i] += 0.02;
		}

		R[0] = total / (float)10;
		R[1] = total % 100 / (float)10;
		R[2] = total % 10 / (float)10;

		// 对于读档，沿用读档内的模糊值，而不是生成新的
		if (isLoad)
		{
			isLoad = false;
		}
		else
		{
			for (var i = 0; i < 3; i++)
			{
				nominal[i] = R[i] + tolerance[i] * Random.Range(-1f, 1f);
				nominal[i] = System.Math.Abs(nominal[i]);
			}
		}
	}

	public override void LoadElement()
	{
		CircuitCalculator.UF.Union(PortID_G, PortID_R9);
		CircuitCalculator.UF.Union(PortID_G, PortID_R99);
		CircuitCalculator.UF.Union(PortID_G, PortID_R999);
	}

	public override void SetElement(int entityID)
	{
		CircuitCalculator.SpiceEntities.Add(new Resistor(string.Concat(entityID, "_", 0),
			PortID_G.ToString(), PortID_R999.ToString(), nominal[0]));
		CircuitCalculator.SpiceEntities.Add(new Resistor(string.Concat(entityID, "_", 1),
			PortID_G.ToString(), PortID_R99.ToString(), nominal[1]));
		CircuitCalculator.SpiceEntities.Add(new Resistor(string.Concat(entityID, "_", 2),
			PortID_G.ToString(), PortID_R9.ToString(), nominal[2]));
	}

	public override EntityData Save() => new RboxData(this);

	[System.Serializable]
	public class RboxData : EntityData
	{
		private readonly List<int> knobRotIntList = new List<int>();
		private readonly double[] nominal;

		public RboxData(RBox RBox)
		{
			baseData = new EntityBaseData(RBox);
			RBox.knobs.ForEach(x => knobRotIntList.Add(x.KnobPos_int));
			nominal = RBox.nominal;
			Debug.LogError("模糊值" + nominal[1].ToString());
		}

		public override void Load()
		{
			RBox RBox = BaseCreate<RBox>(baseData);
			for (var i = 0; i < knobRotIntList.Count; i++)
			{
				// 此处尚未订阅事件，设置旋钮位置不会调用UpdateKnob()
				RBox.knobs[i].SetKnobRot(knobRotIntList[i]);
			}
			RBox.isLoad = true;
			RBox.nominal = nominal;
			Debug.LogError("模糊值" + RBox.nominal[1].ToString());
		}
	}
}