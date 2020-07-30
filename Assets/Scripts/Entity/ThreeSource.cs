using SpiceSharp.Components;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ThreeSource : EntityBase, ISource
{
	private const int sourceNum = 3;                                        // 含有的独立电源个数
	private const int knobNum = 3;                                          // 含有的旋钮个数
	private const int textNum = 3;                                          // 含有的Text个数
	private const double _E0MAX = 15;                                       // 电源0最大值
	private const double _E1MAX = 15;                                       // 电源1最大值
	private const double _E2MAX = 15;                                       // 电源2最大值
	private readonly int[] G = new int[sourceNum];                          // 存放独立电源负极的端口ID
	private readonly int[] V = new int[sourceNum];                          // 存放独立电源正极的端口ID
	private readonly double[] E = new double[sourceNum] { 15, 15, 5 };      // 电压数组
	private readonly double[] R = new double[sourceNum] { 0.1, 0.1, 0.1 };  // 内阻数组

	[HideInInspector]//编辑器隐藏
	public List<MyKnob> Knobs;
	[HideInInspector]//编辑器隐藏
	public List<Text> Texts;
	public enum SourceMode
	{
		three = 0,
		one = 1,
		twoOfThree = 2
	}
	SourceMode sourceMode = SourceMode.three;

	public override void EntityAwake()
	{
		Knobs = transform.FindComponentsInChildren<MyKnob>().OrderBy(x => x.name).ToList();
		if (Knobs.Count != knobNum) Debug.LogError("旋钮个数不合法");

		Texts = transform.FindComponentsInChildren<Text>().OrderBy(x => x.name).ToList();
		if (Knobs.Count != textNum) Debug.LogError("文本个数不合法");

		Knobs.ForEach(x => { x.AngleRange = 337.5f; x.KnobEvent += UpdateKnob; });

		//根据画布的名字区分三种不同的电源
		//0：三路可调，1：单路，2：双路可调
		Canvas canvas = GetComponentInChildren<Canvas>();
		if (int.TryParse(canvas.name, out int res))
		{
			if (res == 0) sourceMode = SourceMode.three;//现在进行强制类型转换可能会造成莫名其妙的bug
			else if (res == 1) sourceMode = SourceMode.one;
			else if (res == 2) sourceMode = SourceMode.twoOfThree;
		}
		else
		{
			Debug.LogError("转换失败");
		}

		// 更新初值
		UpdateKnob();
	}

	void UpdateKnob()
	{
		switch (sourceMode)
		{
			case SourceMode.one:
				E[0] = Knobs[0].KnobPos * _E0MAX;
				Texts[0].text = E[0].ToString("00.00");
				break;
			case SourceMode.three:
				E[0] = Knobs[0].KnobPos * _E0MAX;
				E[1] = Knobs[1].KnobPos * _E1MAX;
				E[2] = Knobs[2].KnobPos * _E2MAX;
				Texts[0].text = E[0].ToString("00.00");
				Texts[1].text = E[1].ToString("00.00");
				Texts[2].text = E[2].ToString("00.00");
				break;
			case SourceMode.twoOfThree:
				E[0] = Knobs[0].KnobPos * _E0MAX;
				E[1] = Knobs[1].KnobPos * _E1MAX;
				E[2] = 5;
				Texts[0].text = E[0].ToString("00.00");
				Texts[1].text = E[1].ToString("00.00");
				Texts[2].text = "5";
				break;
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