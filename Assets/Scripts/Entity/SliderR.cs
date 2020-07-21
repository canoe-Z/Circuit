using SpiceSharp.Components;
using System.Collections.Generic;
using UnityEngine;

public class SliderR : EntityBase
{
	/// <summary>
	/// 可以被随意C的数据
	/// </summary>
	public double Rmax = 300;
	double RL = 300;
	double RR = 0;
	public MySlider myslider;
	public override void EntityAwake()
	{
		myslider = gameObject.GetComponentInChildren<MySlider>();
		myslider.SliderPos = 1;
	}

	public void Update()
	{
		RL = Rmax * myslider.SliderPos;
		RR = Rmax - RL;
	}

	// 电路相关
	// 判断是否有一端连接，避免浮动节点
	override public bool IsConnected()
	{
		if (ChildPorts[0].Connected == 1 || ChildPorts[1].Connected == 1 || ChildPorts[2].Connected == 1 || ChildPorts[3].Connected == 1)
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	override public void LoadElement()
	{
		// 获取端口ID并完成内部连接
		int TL, TR, L, R;
		TL = ChildPorts[0].ID;
		TR = ChildPorts[1].ID;
		L = ChildPorts[2].ID;
		R = ChildPorts[3].ID;
		CircuitCalculator.UF.Union(TL, L);
		CircuitCalculator.UF.Union(TL, R);
		CircuitCalculator.UF.Union(TL, TR);
	}

	override public void SetElement()
	{
		// 获取元件ID作为元件名称
		int EntityID = CircuitCalculator.EntityNum;
		// 获取端口ID并完成内部连接
		int TL, TR, L, R;
		TL = ChildPorts[0].ID;
		TR = ChildPorts[1].ID;
		L = ChildPorts[2].ID;
		R = ChildPorts[3].ID;
		CircuitCalculator.SpiceEntities.Add(new Resistor(string.Concat(EntityID, "_L"), TL.ToString(), L.ToString(), RL));
		CircuitCalculator.SpiceEntities.Add(new Resistor(string.Concat(EntityID, "_R"), TL.ToString(), R.ToString(), RR));
		CircuitCalculator.SpiceEntities.Add(new VoltageSource(string.Concat(EntityID, "_T"), TL.ToString(), TR.ToString(), 0));
		CircuitCalculator.SpicePorts.Add(ChildPorts[0]);
		CircuitCalculator.SpicePorts.Add(ChildPorts[1]);
		CircuitCalculator.SpicePorts.Add(ChildPorts[2]);
		CircuitCalculator.SpicePorts.Add(ChildPorts[3]);
	}

	public override EntityData Save()
	{
		return new SliderRData(Rmax, myslider.SliderPos,gameObject.transform.position, ChildPortID);
	}
}

[System.Serializable]
public class SliderRData : EntityData
{
	private readonly double rmax;
	private readonly float sliderpos;
	public SliderRData(double rmax,float sliderpos,Vector3 pos, List<int> id) : base(pos, id) 
	{
		this.rmax = rmax;
		this.sliderpos = sliderpos;
	}

	override public void Load()
	{
		SliderR sliderR = EntityCreator.CreateEntity<SliderR>(posfloat, IDList);
		sliderR.Rmax = rmax;
		sliderR.myslider.ChangeSliderPos(sliderpos);
		sliderR.Update();
	}
}
