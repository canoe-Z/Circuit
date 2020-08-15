using SpiceSharp.Components;
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
		CircuitCalculator.UF.Union(PortID_TL, PortID_L);
		CircuitCalculator.UF.Union(PortID_TL, PortID_R);
		CircuitCalculator.UF.Union(PortID_TL, PortID_TR);
	}

	public override void SetElement(int entityID)
	{
		CircuitCalculator.SpiceEntities.Add(new Resistor(string.Concat(entityID, "_L"), PortID_TL.ToString(), PortID_L.ToString(), RLeft));
		CircuitCalculator.SpiceEntities.Add(new Resistor(string.Concat(entityID, "_R"), PortID_TL.ToString(), PortID_R.ToString(), RRight));
		CircuitCalculator.SpiceEntities.Add(new VoltageSource(string.Concat(entityID, "_T"), PortID_TL.ToString(), PortID_TR.ToString(), 0));
	}

	public static GameObject Create(double RMax)
	{
		return Set(BaseCreate<SliderR>(), RMax).gameObject;
	}

	public static SliderR Set(SliderR sliderR, double RMax)
	{
		sliderR.RMax = RMax;
		return sliderR;
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
			SliderR sliderR = Set(BaseCreate<SliderR>(baseData), RMax);
			sliderR.mySlider.SetSliderPos(sliderPos);
		}
	}
}