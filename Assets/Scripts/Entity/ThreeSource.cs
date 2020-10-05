using SpiceSharp.Components;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 三模三路电源
/// </summary>
public class ThreeSource : EntityBase, ISource
{
	private int sourceNum;                                      // 含有的独立电源个数
	private int knobNum;                                        // 含有的旋钮个数
	private readonly double[] EMax = new double[3];             // 最大值，对于固定电源则为固定值
	private readonly int[] G = new int[3];                      // 存放独立电源负极的端口ID
	private readonly int[] V = new int[3];                      // 存放独立电源正极的端口ID
	private readonly double[] E = new double[3];                // 电压数组
	private readonly double[] R = new double[3];                // 内阻数组

	private MySwitch mySwitch;
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

		// 获取开关，旋钮和文本的引用
		mySwitch = transform.FindComponent_DFS<MySwitch>("MySwitch");

		knobs = transform.FindComponentsInChildren<MyKnob>().OrderBy(x => x.name).ToList();
		if (knobs.Count != knobNum) Debug.LogError("旋钮个数不合法");

		texts = transform.FindComponentsInChildren<Text>().OrderBy(x => x.name).ToList();
		if (texts.Count != sourceNum) Debug.LogError("文本个数不合法");

		// 默认启动时开机，读档可覆盖该设置
		mySwitch.IsOn = true;

		// 限制旋钮旋转极限角度
		knobs.ForEach(x => x.AngleRange = 337.5f);
	}

	void Start()
	{
		// 第一次执行初始化，此后受事件控制
		mySwitch.SwitchEvent += ChangePower;
		knobs.ForEach(x => x.KnobEvent += UpdateKnob);
		ChangePower();

		for (var i = 0; i < sourceNum; i++)
		{
			G[i] = ChildPorts[2 * i + 1].ID;
			V[i] = ChildPorts[2 * i].ID;
		}
	}

	/// <summary>
	/// 根据旋钮当前位置更新电压与显示
	/// </summary>
	private void UpdateKnob()
	{
		// 关机时旋钮可以调整，但是不更新电源
		if (!mySwitch.IsOn) return;

		for (var i = 0; i < sourceNum; i++)
		{
			if (i < knobNum)
			{
				E[i] = knobs[i].KnobPos * EMax[i];
				texts[i].text = E[i].ToString("00.00");
			}
			else
			{
				E[i] = EMax[i];
				texts[i].text = ((int)E[i]).ToString();
			}
		}
	}

	/// <summary>
	/// 根据MySwtich状态开启/关闭
	/// </summary>
	private void ChangePower()
	{
		// 开机
		if (mySwitch.IsOn)
		{
			// 根据旋钮位置更新电源状态
			UpdateKnob();
		}
		// 关机
		else
		{
			// 电压置零，显示置零，旋钮位置不变
			for (var i = 0; i < sourceNum; i++)
			{
				E[i] = 0;
				texts[i].text = E[i].ToString("00.00");
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
	public override void LoadElement()
	{
		for (int j = 0; j < sourceNum; j++)
		{
			if (IsConnected(j)) LoadElement(j);
		}
	}

	/// <summary>
	/// 加载单个独立电源
	/// </summary>
	/// <param name="n"></param>
	public void SetElement(int n, int entityID)
	{
		CircuitCalculator.SpiceEntities.Add(new VoltageSource(string.Concat(entityID, "_", n), V[n].ToString(), string.Concat(entityID, "_rPort", n), E[n]));
		CircuitCalculator.SpiceEntities.Add(new Resistor(string.Concat(entityID.ToString(), "_r", n), string.Concat(entityID, "_rPort", n), G[n].ToString(), R[n]));

		CircuitCalculator.SpicePorts.Add(ChildPorts[2 * n]);
		CircuitCalculator.SpicePorts.Add(ChildPorts[2 * n + 1]);
	}

	/// <summary>
	/// 加载连接状态为真的独立电源
	/// </summary>
	public override void SetElement(int entityID)
	{
		for (int j = 0; j < sourceNum; j++)
		{
			if (IsConnected(j)) SetElement(j, entityID);
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

	public static string GetPrefabName(SourceMode sourceMode)
	{
		switch (sourceMode)
		{
			case SourceMode.one:
				return "ThreeSource1";
			case SourceMode.three:
				return "ThreeSource";
			case SourceMode.twoOfThree:
				return "ThreeSource2";
			default:
				return null;
		}
	}

	public static GameObject Create(SourceMode sourceMode, List<double> EMaxList)
	{
		return BaseCreate<ThreeSource>(prefabName: GetPrefabName(sourceMode)).Set(EMaxList).gameObject;
	}

	private ThreeSource Set(List<double> EMaxList)
	{
		for (var i = 0; i < sourceNum; i++)
		{
			EMax[i] = EMaxList[i];
		}
		return this;
	}

	public override EntityData Save() => new SourceData(this);

	/// <summary>
	/// 存档数据
	/// </summary>
	[System.Serializable]
	public class SourceData : EntityData
	{
		private readonly SourceMode sourceMode;
		private readonly List<double> EMaxList;
		private readonly bool isOn;
		private readonly List<float> knobPosList = new List<float>();

		public SourceData(ThreeSource threeSource)
		{
			sourceMode = threeSource.sourceMode;
			baseData = new EntityBaseData(threeSource);
			EMaxList = threeSource.EMax.ToList();
			isOn = threeSource.mySwitch.IsOn;
			threeSource.knobs.ForEach(x => knobPosList.Add(x.KnobPos));
		}

		public override void Load()
		{
			ThreeSource threeSource = BaseCreate<ThreeSource>(baseData, GetPrefabName(sourceMode)).Set(EMaxList);

			threeSource.mySwitch.IsOn = isOn;
			for (var i = 0; i < knobPosList.Count; i++)
			{
				// 此处不再需要更新值，在Start()中统一更新
				threeSource.knobs[i].SetKnobRot(knobPosList[i]);
			}
		}
	}
}


