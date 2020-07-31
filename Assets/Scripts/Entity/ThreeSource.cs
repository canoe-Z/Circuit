using SpiceSharp.Components;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ThreeSource : EntityBase, ISource
{
	private int sourceNum;										// 含有的独立电源个数
	private int knobNum;										// 含有的旋钮个数
	public double[] EMax { get; set; } = new double[3];			// 最大值，对于固定电源则为固定值

	private readonly int[] G = new int[3];						// 存放独立电源负极的端口ID
	private readonly int[] V = new int[3];                      // 存放独立电源正极的端口ID
	private readonly double[] E = new double[3];                // 电压数组
	private readonly double[] R = new double[3];                // 内阻数组

	public List<MyKnob> Knobs { get; set; }
	public List<Text> Texts { get; set; }

	public override void EntityAwake()
	{
		// 根据画布的名字区分三种不同的电源
		// 0：三路可调，1：单路，2：双路可调
		Canvas canvas = GetComponentInChildren<Canvas>();

		if (int.TryParse(canvas.name, out int res))
		{
			if (res == 0)
			{
				sourceNum = 3;
				knobNum = 3;
			}
			else if (res == 1)
			{
				sourceNum = 1;
				knobNum = 1;
			}
			else if (res == 2)
			{
				sourceNum = 3;
				knobNum = 2;
			}
		}
		else
		{
			Debug.LogError("画布名称转换失败");
		}

		// 获取旋钮和文本的引用
		Knobs = transform.FindComponentsInChildren<MyKnob>().OrderBy(x => x.name).ToList();
		if (Knobs.Count != knobNum) Debug.LogError("旋钮个数不合法");

		Texts = transform.FindComponentsInChildren<Text>().OrderBy(x => x.name).ToList();
		if (Texts.Count != sourceNum) Debug.LogError("文本个数不合法");

		// 旋钮初始化
		Knobs.ForEach(x => { x.AngleRange = 337.5f; x.KnobEvent += UpdateKnob; });

		//以下初始化
		EMax[0] = 15;
		EMax[1] = 15;
		EMax[2] = 5;

		// 更新初值
		UpdateKnob();
	}

	void UpdateKnob()
	{
		for (var i = 0; i < sourceNum; i++)
		{
			if (i < knobNum)
			{
				E[i] = Knobs[i].KnobPos * EMax[i];
				Texts[i].text = E[i].ToString("00.00");
			}
			else
			{
				E[i] = EMax[i];
				Texts[i].text = ((int)E[i]).ToString();
			}
		}
	}

	void Start()
	{
		for (var i = 0; i < sourceNum; i++)
		{
			G[i] = ChildPorts[2 * i + 1].ID;
			V[i] = ChildPorts[2 * i].ID;
		}
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
	/// 预连接单个电源
	/// </summary>
	/// <param name="n"></param>
	public void LoadElement(int n)
	{
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