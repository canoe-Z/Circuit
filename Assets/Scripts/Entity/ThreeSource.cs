using SpiceSharp.Components;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ThreeSource : EntityBase, ISource
{
	private const int sourceNum = 3;										// 含有的独立电源个数
	private const int knobNum = 2;                                          // 含有的旋钮个数
	private const int textNum = 2;                                          // 含有的Text个数
	private const double _E0MAX = 15;										// 电源0最大值
	private const double _E1MAX = 15;										// 电源1最大值

	private readonly int[] G = new int[sourceNum];							// 存放独立电源负极的端口ID
	private readonly int[] V = new int[sourceNum];							// 存放独立电源正极的端口ID
	private readonly double[] E = new double[sourceNum] { 15, 15, 5 };      // 电压数组
	private readonly double[] R = new double[sourceNum] { 0.1, 0.1, 0.1 };  // 内阻数组

	public List<MyKnob> Knobs;
	public List<Text> Texts;

	public override void EntityAwake()
	{
		Knobs = transform.FindComponentsInChildren<MyKnob>();
		if (Knobs.Count != knobNum) Debug.LogError("旋钮个数不合法");
		Knobs.Sort((x, y) => { return x.name.CompareTo(y.name); });

		Texts = transform.FindComponentsInChildren<Text>();
		if (Knobs.Count != textNum) Debug.LogError("旋钮个数不合法");
		Texts.Sort((x, y) => { return x.name.CompareTo(y.name); });

		Knobs.ForEach(x => { x.AngleRange = 337.5f; x.KnobEvent += UpdateKnob; });

		// 更新初值
		UpdateKnob();
	}

	void UpdateKnob()
	{
		E[0] = Knobs[0].KnobPos * _E0MAX;
		E[1] = Knobs[1].KnobPos * _E1MAX;
		Texts[0].text = E[0].ToString("00.00");
		Texts[1].text = E[1].ToString("00.00");
	}

	/// <summary>
	/// 判断单个独立电源的连接状态
	/// </summary>
	/// <param name="n"></param>
	/// <returns></returns>
	public bool IsConnected(int n)
	{
		if (ChildPorts[2 * n + 1].Connected == 1 || ChildPorts[2 * n].Connected == 1)
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	/// <summary>
	/// 判断电源的连接状态，有单个电源连接即返回真
	/// </summary>
	/// <returns></returns>
	override public bool IsConnected()
	{
		bool _isConnected = false;
		for (int j = 0; j < 3; j++)
		{
			if (IsConnected(j))
			{
				_isConnected = true;
			}
		}
		return _isConnected;
	}

	/// <summary>
	/// 预连接单个电源
	/// </summary>
	/// <param name="n"></param>
	public void LoadElement(int n)
	{
		G[n] = ChildPorts[2 * n + 1].ID;
		V[n] = ChildPorts[2 * n].ID;
		CircuitCalculator.UF.Union(G[n], V[n]);
	}

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
		G[n] = ChildPorts[2 * n + 1].ID;
		V[n] = ChildPorts[2 * n].ID;
		CircuitCalculator.SpiceEntities.Add(new VoltageSource(string.Concat(EntityID, "_", n), V[n].ToString(), string.Concat(EntityID, "_rPort", n), E[n]));
		CircuitCalculator.SpiceEntities.Add(new Resistor(string.Concat(EntityID.ToString(), "_r", n), string.Concat(EntityID, "_rPort", n), G[n].ToString(), R[n]));
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

	public override EntityData Save()
	{
		return new SourceData(Knobs, transform.position, transform.rotation, ChildPortID);
	}
}

/// <summary>
/// 存档数据
/// </summary>
[System.Serializable]
public class SourceData : EntityData
{
	private readonly List<float> knobPosList = new List<float>();

	public SourceData(List<MyKnob> knobs, Vector3 pos, Quaternion angle, List<int> IDList) : base(pos, angle, IDList)
	{
		knobs.ForEach(x => knobPosList.Add(x.KnobPos));
	}

	override public void Load()
	{
		ThreeSource threeSource = EntityCreator.CreateEntity<ThreeSource>(posfloat, anglefloat, IDList);
		for (var i = 0; i < knobPosList.Count; i++)
		{
			// 此处不再需要更新值，ChangeKnobRot方法会发送更新值的消息给元件
			threeSource.Knobs[i].SetKnobRot(knobPosList[i]);
		}
	}
}