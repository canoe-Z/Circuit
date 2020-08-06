using SpiceSharp.Components;
using System.Collections.Generic;
using UnityEngine;

public class SliderR : EntityBase
{
	private double rMax;
	private double rLeft, rRight;
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
		rLeft = rMax * mySlider.SliderPos;
		rRight = rMax - rLeft;
	}

	public override void LoadElement()
	{
		CircuitCalculator.UF.Union(PortID_TL, PortID_L);
		CircuitCalculator.UF.Union(PortID_TL, PortID_R);
		CircuitCalculator.UF.Union(PortID_TL, PortID_TR);
	}

	public override void SetElement()
	{
		int EntityID = CircuitCalculator.EntityNum;

		CircuitCalculator.SpiceEntities.Add(new Resistor(string.Concat(EntityID, "_L"), PortID_TL.ToString(), PortID_L.ToString(), rLeft));
		CircuitCalculator.SpiceEntities.Add(new Resistor(string.Concat(EntityID, "_R"), PortID_TL.ToString(), PortID_R.ToString(), rRight));
		CircuitCalculator.SpiceEntities.Add(new VoltageSource(string.Concat(EntityID, "_T"), PortID_TL.ToString(), PortID_TR.ToString(), 0));
	}

	public override EntityData Save()
	{
		return new SliderRData(rMax, mySlider.SliderPos, transform.position, transform.rotation, ChildPortID);
	}

	public static GameObject Create(double rMax, float? sliderPos = null, Float3 pos = null, Float4 angle = null, List<int> IDList = null)
	{
		SliderR sliderR = BaseCreate<SliderR>(pos, angle, IDList);
		sliderR.rMax = rMax;
		if (sliderPos != null) sliderR.mySlider.SetSliderPos(sliderPos.Value);
		return sliderR.gameObject;
	}
}

[System.Serializable]
public class SliderRData : EntityData
{
	private readonly double rMax;
	private readonly float sliderPos;

	public SliderRData(double rMax, float sliderPos, Vector3 pos, Quaternion angle, List<int> IDList) : base(pos, angle, IDList)
	{
		this.rMax = rMax;
		this.sliderPos = sliderPos;
	}

	override public void Load() => SliderR.Create(rMax, sliderPos, pos, angle, IDList);
}
