using SpiceSharp.Components;
using System.Collections.Generic;
using UnityEngine;

public class SliderR : EntityBase
{
	public double rMax;
	private double rLeft, rRight;
	public MySlider MySlider { get; set; }
	private int TL, TR, L, R;

	public override void EntityAwake()
	{
		MySlider = gameObject.GetComponentInChildren<MySlider>();
		// 注意滑变滑块的初始位置在最右边
		MySlider.SetSliderPos(1);
		MySlider.SliderEvent += UpdateSlider;
		UpdateSlider();
	}

	void UpdateSlider()
	{
		rLeft = rMax * MySlider.SliderPos;
		rRight = rMax - rLeft;
	}

	void Start()
	{
		TL = ChildPorts[0].ID;
		TR = ChildPorts[1].ID;
		L = ChildPorts[2].ID;
		R = ChildPorts[3].ID;
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
		return new SliderRData(rMax, MySlider.SliderPos, transform.position, transform.rotation, ChildPortID);
	}

	public static SliderR Create(double rMax)
	{
		SliderR sliderR = EntityCreator.CreateEntity<SliderR>();
		sliderR.rMax = rMax;
		return sliderR;
	}
}

[System.Serializable]
public class SliderRData : EntityData
{
	private readonly double rMax;
	private readonly float sliderPos;

	public SliderRData(double rMax, float sliderPos, Vector3 pos, Quaternion angle, List<int> id) : base(pos, angle, id)
	{
		this.rMax = rMax;
		this.sliderPos = sliderPos;
	}

	override public void Load()
	{
		SliderR sliderR = EntityCreator.CreateEntity<SliderR>(posfloat, anglefloat, IDList);
		sliderR.rMax = rMax;
		sliderR.MySlider.SetSliderPos(sliderPos);
	}
}
