﻿using SpiceSharp.Components;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 滑动变阻器
/// </summary>
public class SliderR : EntityBase
{
	private double RMax;
	private double RLeft, RRight;
	private MySlider mySlider;
	private int PortID_TL, PortID_TR, PortID_L, PortID_R;

	public override void EntityAwake()
	{
		mySlider = gameObject.GetComponentInChildren<MySlider>();
		// 注意滑变滑块的初始位置在最右边
		mySlider.SetSliderPos(1);
	}

	void Start()
	{
		// 第一次执行初始化，此后受事件控制
		mySlider.SliderEvent += UpdateSlider;
		UpdateSlider();

		PortID_TL = ChildPorts[0].ID;
		PortID_TR = ChildPorts[1].ID;
		PortID_L = ChildPorts[2].ID;
		PortID_R = ChildPorts[3].ID;
	}

	void UpdateSlider()
	{
		RLeft = RMax * mySlider.SliderPos;
		RRight = RMax - RLeft;
	}

	public override void LoadElement()
	{
		CircuitCalculator.UF.ListUnion(new List<int> { PortID_L, PortID_R, PortID_TL, PortID_TR });
	}

	public override void SetElement(int entityID)
	{
		if (Math.Abs(RLeft) < 0.001)
		{
			RLeft = 0.001;
		}
		if (Math.Abs(RRight) < 0.001)
		{
			RRight = 0.001;
		}
		CircuitCalculator.SpiceEntities.Add(new Resistor(string.Concat(entityID, "_L"), PortID_TL.ToString(), PortID_L.ToString(), RLeft));
		CircuitCalculator.SpiceEntities.Add(new Resistor(string.Concat(entityID, "_R"), PortID_TL.ToString(), PortID_R.ToString(), RRight));
		CircuitCalculator.SpiceEntities.Add(new VoltageSource(string.Concat(entityID, "_T"), PortID_TL.ToString(), PortID_TR.ToString(), 0));
	}

	public static GameObject Create(double RMax)
	{
		return BaseCreate<SliderR>().Set(RMax).gameObject;
	}

	public SliderR Set(double RMax)
	{
		this.RMax = RMax;
		return this;
	}

	public override EntityData Save() => new SliderRData(this);

	[System.Serializable]
	public class SliderRData : EntityData
	{
		private readonly double RMax;
		private readonly float sliderPos;

		public SliderRData(SliderR sliderR)
		{
			baseData = new EntityBaseData(sliderR);
			RMax = sliderR.RMax;
			sliderPos = sliderR.mySlider.SliderPos;
		}

		public override void Load()
		{
			SliderR sliderR = BaseCreate<SliderR>(baseData).Set(RMax);
			sliderR.mySlider.SetSliderPos(sliderPos);
		}
	}
}