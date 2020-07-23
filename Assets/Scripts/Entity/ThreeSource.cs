using SpiceSharp.Components;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ThreeSource : EntityBase, ISource
{
	private const int sourceNum = 3;										//含有的独立电源个数
	private const int sliderNum = 2;										//含有的滑块个数
	private const double _E0MAX = 15;										//电源0最大值
	private const double _E1MAX = 15;										//电源1最大值

	private readonly int[] G = new int[sourceNum];							//存放独立电源负极的端口ID
	private readonly int[] V = new int[sourceNum];							//存放独立电源正极的端口ID
	private readonly double[] E = new double[sourceNum] { 15, 15, 5 };      //电压数组
	private readonly double[] R = new double[sourceNum] { 0.1, 0.1, 0.1 };  //内阻数组

	public List<MyKnob> knobs;//编辑器去挂
	public List<Text> texts;

	public override void EntityAwake()
	{
		//编辑器去挂

		/*sliders = transform.FindComponentsInChildren<MySlider>();
		if (sliders.Count != sliderNum) Debug.LogError("滑块个数不合法");

		// 用滑块在编辑器中的名称排序
		sliders.Sort((x, y) => { return x.name.CompareTo(y.name); });

		foreach(MySlider slider in sliders)
		{
			slider.SliderEvent += UpdateSlider;
		}
		
		// 更新初值
		UpdateSlider();*/
	}

	void Update()
	{
		E[0] = knobs[0].KnobPos * _E0MAX;
		E[1] = knobs[1].KnobPos * _E1MAX;
		texts[0].text = E[0].ToString("00.00");
		texts[1].text = E[1].ToString("00.00");
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
		return new SourceData(knobs, transform.position, transform.rotation, ChildPortID);
	}
}

/// <summary>
/// 存档数据
/// </summary>
[System.Serializable]
public class SourceData : EntityData
{
	private readonly List<float> sliderPosList = new List<float>();

	public SourceData(List<MyKnob> knobs, Vector3 pos, Quaternion angle, List<int> IDList) : base(pos, angle, IDList)
	{
		foreach (MyKnob knob in knobs)
		{
			sliderPosList.Add(knob.KnobPos);
		}
	}

	override public void Load()
	{
		ThreeSource source = EntityCreator.CreateEntity<ThreeSource>(posfloat, anglefloat, IDList);
		for (var i = 0; i < sliderPosList.Count; i++)
		{
			// 此处不再需要更新值，SliderPos的Set方法会发送更新值的消息给元件
			source.knobs[i].ChangeKnobRot(sliderPosList[i]);
		}
	}
}