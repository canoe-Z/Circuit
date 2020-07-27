using SpiceSharp.Components;
using System.Collections.Generic;
using UnityEngine;

public class SliderR : EntityBase
{
	public double rMax = 300;

	double rLeft = 300;
	double rRight = 0;

	public MySlider MySlider { get; set; }

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

	public override void LoadElement()
	{
		int TL = ChildPorts[0].ID;
		int TR = ChildPorts[1].ID;
		int L = ChildPorts[2].ID;
		int R = ChildPorts[3].ID;

		CircuitCalculator.UF.Union(TL, L);
		CircuitCalculator.UF.Union(TL, R);
		CircuitCalculator.UF.Union(TL, TR);
	}

	public override void SetElement()
	{
		int EntityID = CircuitCalculator.EntityNum;
		int TL = ChildPorts[0].ID;
		int TR = ChildPorts[1].ID;
		int L = ChildPorts[2].ID;
		int R = ChildPorts[3].ID;

		CircuitCalculator.SpiceEntities.Add(new Resistor(string.Concat(EntityID, "_L"), TL.ToString(), L.ToString(), rLeft));
		CircuitCalculator.SpiceEntities.Add(new Resistor(string.Concat(EntityID, "_R"), TL.ToString(), R.ToString(), rRight));
		CircuitCalculator.SpiceEntities.Add(new VoltageSource(string.Concat(EntityID, "_T"), TL.ToString(), TR.ToString(), 0));
	}

	public override EntityData Save()
	{
		return new SliderRData(rMax, MySlider.SliderPos, transform.position, transform.rotation, ChildPortID);
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
