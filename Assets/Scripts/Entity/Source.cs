﻿using SpiceSharp.Components;
using System.Collections.Generic;
using UnityEngine;

public class Source : EntityBase, ISource
{
	private const int sourceNum = 3;                                //含有的独立电源个数
	private const int sliderNum = 2;                                //含有的滑块个数
	private const double _E0MAX = 30;                               //电源0最大值
	private const double _E1MAX = 30;                               //电源1最大值

	public int[] G = new int[sourceNum];                            //存放独立电源负极的端口ID
	public int[] V = new int[sourceNum];                            //存放独立电源正极的端口ID
	public double[] E = new double[sourceNum] { 30, 30, 5 };        //电压数组
	public double[] R = new double[sourceNum] { 0.1, 0.1, 0.1 };    //内阻数组

	public List<MySlider> Sliders { get; set; } = new List<MySlider>();

	public override void EntityAwake()
	{
		Sliders = transform.FindComponentsInChildren<MySlider>();
		if (Sliders.Count != sliderNum) Debug.LogError("滑块个数不合法");

		// 用滑块在编辑器中的名称排序
		Sliders.Sort((x, y) => { return x.name.CompareTo(y.name); });
	}

	public void Update()
	{
		E[0] = Sliders[0].SliderPos * _E0MAX;
		E[1] = Sliders[1].SliderPos * _E1MAX;
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
		return new SourceData(Sliders, transform.position, transform.rotation, ChildPortID);
	}
}

/// <summary>
/// 存档数据
/// </summary>
[System.Serializable]
public class SourceData : EntityData
{
	private readonly List<float> sliderPosList = new List<float>();

	public SourceData(List<MySlider> sliders, Vector3 pos, Quaternion angle, List<int> IDList) : base(pos, angle, IDList)
	{
		foreach (MySlider slider in sliders)
		{
			sliderPosList.Add(slider.SliderPos);
		}
	}

	override public void Load()
	{
		Source source = EntityCreator.CreateEntity<Source>(posfloat, anglefloat, IDList);
		for (var i = 0; i < sliderPosList.Count; i++)
		{
			source.Sliders[i].ChangeSliderPos(sliderPosList[i]);
		}
		// 对于包含滑块的情况，要在此处更新值，否则读档后首次计算的结果有误
		source.Update();
	}
}