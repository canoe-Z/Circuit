using SpiceSharp.Components;
using System.Collections.Generic;
using UnityEngine;

public class SliderR : EntityBase
{
	private double rMax;
	private double rLeft, rRight;
	private MySlider mySlider;
	private int TL, TR, L, R;

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

		TL = ChildPorts[0].ID;
		TR = ChildPorts[1].ID;
		L = ChildPorts[2].ID;
		R = ChildPorts[3].ID;
	}

	void UpdateSlider()
	{
		rLeft = rMax * mySlider.SliderPos;
		rRight = rMax - rLeft;
	}

	public override void LoadElement()
	{
		CircuitCalculator.UF.Union(TL, L);
		CircuitCalculator.UF.Union(TL, R);
		CircuitCalculator.UF.Union(TL, TR);
	}

	public override void SetElement()
	{
		int EntityID = CircuitCalculator.EntityNum;

		CircuitCalculator.SpiceEntities.Add(new Resistor(string.Concat(EntityID, "_L"), TL.ToString(), L.ToString(), rLeft));
		CircuitCalculator.SpiceEntities.Add(new Resistor(string.Concat(EntityID, "_R"), TL.ToString(), R.ToString(), rRight));
		CircuitCalculator.SpiceEntities.Add(new VoltageSource(string.Concat(EntityID, "_T"), TL.ToString(), TR.ToString(), 0));
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
