﻿using SpiceSharp.Components;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static ThreeSource;

public class ThreeSource : EntityBase, ISource
{
	private int sourceNum;										// 含有的独立电源个数
	private int knobNum;                                        // 含有的旋钮个数
	private double[] _EMax = new double[3];						// 最大值，对于固定电源则为固定值
	private readonly int[] G = new int[3];                      // 存放独立电源负极的端口ID
	private readonly int[] V = new int[3];                      // 存放独立电源正极的端口ID
	private readonly double[] E = new double[3];                // 电压数组
	private readonly double[] R = new double[3];                // 内阻数组

	private List<MyKnob> knobs;
	private List<Text> texts;

	public enum SourceMode { three, one, twoOfThree }
	private SourceMode sourceMode;

	public override void EntityAwake()
	{
		// 根据画布的名字区分三种不同的电源
		// 0：三路可调，1：单路，2：双路可调
		Canvas canvas = GetComponentInChildren<Canvas>();

		if (int.TryParse(canvas.name, out int res))
		{
			if (res == 0)
			{
				sourceMode = SourceMode.three;
				sourceNum = 3;
				knobNum = 3;
			}
			else if (res == 1)
			{
				sourceMode = SourceMode.one;
				sourceNum = 1;
				knobNum = 1;
			}
			else if (res == 2)
			{
				sourceMode = SourceMode.twoOfThree;
				sourceNum = 3;
				knobNum = 2;
			}
		}
		else
		{
			Debug.LogError("画布名称转换失败");
		}

		// 获取旋钮和文本的引用
		knobs = transform.FindComponentsInChildren<MyKnob>().OrderBy(x => x.name).ToList();
		if (knobs.Count != knobNum) Debug.LogError("旋钮个数不合法");

		texts = transform.FindComponentsInChildren<Text>().OrderBy(x => x.name).ToList();
		if (texts.Count != sourceNum) Debug.LogError("文本个数不合法");

		// 限制旋钮旋转极限角度
		knobs.ForEach(x => x.AngleRange = 337.5f);
	}

	void Start()
	{
		// 第一次执行初始化，此后受事件控制
		knobs.ForEach(x => x.KnobEvent += UpdateKnob);
		UpdateKnob();

		for (var i = 0; i < sourceNum; i++)
		{
			G[i] = ChildPorts[2 * i + 1].ID;
			V[i] = ChildPorts[2 * i].ID;
		}
	}

	void UpdateKnob()
	{
		for (var i = 0; i < sourceNum; i++)
		{
			if (i < knobNum)
			{
				E[i] = knobs[i].KnobPos * _EMax[i];
				texts[i].text = E[i].ToString("00.00");
			}
			else
			{
				E[i] = _EMax[i];
				texts[i].text = ((int)E[i]).ToString();
			}
		}
	}

	/// <summary>
	/// 判断单个独立电源的连接状态
	/// </summary>
	/// <param name="n"></param>
	/// <returns></returns>
	public bool IsConnected(int n) => ChildPorts[2 * n + 1].IsConnected || ChildPorts[2 * n].IsConnected;

	/// <summary>
	/// 预连接单个电源
	/// </summary>
	/// <param name="n"></param>
	public void LoadElement(int n) => CircuitCalculator.UF.Union(G[n], V[n]);

	/// <summary>
	/// 预连接连接状态为真的独立电源
	/// </summary>
	override public void LoadElement()
	{
		for (int j = 0; j < sourceNum; j++)
		{
			if (IsConnected(j))
			{
				LoadElement(j);
			}
		}
	}

	/// <summary>
	/// 加载单个独立电源
	/// </summary>
	/// <param name="n"></param>
	public void SetElement(int n)
	{
		int EntityID = CircuitCalculator.EntityNum;
		CircuitCalculator.SpiceEntities.Add(new VoltageSource(string.Concat(EntityID, "_", n), V[n].ToString(), string.Concat(EntityID, "_rPort", n), E[n]));
		CircuitCalculator.SpiceEntities.Add(new Resistor(string.Concat(EntityID.ToString(), "_r", n), string.Concat(EntityID, "_rPort", n), G[n].ToString(), R[n]));

		CircuitCalculator.SpicePorts.Add(ChildPorts[2 * n]);
		CircuitCalculator.SpicePorts.Add(ChildPorts[2 * n + 1]);
	}

	/// <summary>
	/// 加载连接状态为真的独立电源
	/// </summary>
	override public void SetElement()
	{
		for (int j = 0; j < sourceNum; j++)
		{
			if (IsConnected(j))
			{
				SetElement(j);
			}
		}
	}

	/// <summary>
	/// 接地检测，如果电源的负极不和地相连，则在此创建一条接地线
	/// </summary>
	public void GroundCheck()
	{
		for (int j = 0; j < sourceNum; j++)
		{
			if (IsConnected(j))
			{
				if (!CircuitCalculator.UF.Connected(G[j], 0))
				{
					CircuitCalculator.UF.Union(G[j], 0);
					CircuitCalculator.GNDLines.Add(new GNDLine(G[j]));
				}
			}
		}
	}

	private static ThreeSource ThreeSourceInstantiate(SourceMode sourceMode, Float3 pos, Float4 angle, List<int> IDlist)
	{
		pos = pos ?? Float3.zero;
		angle = angle ?? Float4.identity;

		// 加载预制体
		GameObject SourceObject;
		switch (sourceMode)
		{
			case SourceMode.one:
				SourceObject = (GameObject)Resources.Load("ThreeSource1");
				break;
			case SourceMode.three:
				SourceObject = (GameObject)Resources.Load("ThreeSource");
				break;
			case SourceMode.twoOfThree:
				SourceObject = (GameObject)Resources.Load("ThreeSource2");
				break;
			default:
				SourceObject = null;
				break;
		}

		ThreeSource threeSource = Instantiate(SourceObject, pos.ToVector3(), angle.ToQuaternion()).GetComponent<ThreeSource>();

		// 注入ID
		if (IDlist != null) SetEntityID(threeSource, IDlist);
		return threeSource;
	}

	public static GameObject Create(SourceMode sourceMode = SourceMode.three,
		List<double> _EMaxList = null, List<float> knobPosList = null, Float3 pos = null, Float4 angle = null, List<int> IDList = null)
	{
		ThreeSource threeSource = ThreeSourceInstantiate(sourceMode, pos, angle, IDList);

		if (_EMaxList != null)
		{
			for (var i = 0; i < threeSource.sourceNum; i++)
			{
				threeSource._EMax[i] = _EMaxList[i];
			}
		}

		if (knobPosList != null)
		{
			for (var i = 0; i < knobPosList.Count; i++)
			{
				// 此处不再需要更新值，在Start()中统一更新
				threeSource.knobs[i].SetKnobRot(knobPosList[i]);
			}
		}

		return threeSource.gameObject;
	}

	public override EntityData Save() => new SourceData(sourceMode, _EMax, knobs, transform.position, transform.rotation, ChildPortID);
}

/// <summary>
/// 存档数据
/// </summary>
[System.Serializable]
public class SourceData : EntityData
{
	private readonly SourceMode sourceMode;
	private readonly List<float> knobPosList = new List<float>();
	private readonly List<double> _EMaxList;

	public SourceData(SourceMode sourceMode, double[] _EMax, List<MyKnob> knobs, Vector3 pos, Quaternion angle, List<int> IDList) : base(pos, angle, IDList)
	{
		this.sourceMode = sourceMode;
		_EMaxList = _EMax.ToList();
		knobs.ForEach(x => knobPosList.Add(x.KnobPos));
	}

	public override void Load() => Create(sourceMode, _EMaxList, knobPosList, pos, angle, IDList);
}