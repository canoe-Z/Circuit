using SpiceSharp.Components;
using System.Collections.Generic;
using UnityEngine;

public class Switch : EntityBase
{
	public int state = 1;
	public MySlider mySlider = null;
	private GameObject connector = null;

	public override void EntityAwake()
	{
		mySlider = gameObject.GetComponentInChildren<MySlider>();
		mySlider.SetSliderPos(0.5f);
		mySlider.SliderEvent += UpdateSlider;

		int childNum = transform.childCount;
		for (int i = 0; i < childNum; i++)
		{
			if (transform.GetChild(i).name == "Connector")
			{
				connector = transform.GetChild(i).gameObject;
				return;
			}
		}
		Debug.LogError("开关儿子没有拉杆");

		UpdateSlider();
	}

	// 开关的状态有三种
	void UpdateSlider()
	{
		if (mySlider.SliderPos > 0.8f) state = 2; //R
		else if (mySlider.SliderPos < 0.2f) state = 0; //L
		else state = 1; //M
		connector.transform.LookAt(mySlider.gameObject.transform);
	}

	// 开关一定要接中间才能激活
	// 否则当左/右端口单独被激活时，其余的那个端口就会和中间建立实质的连接，而这可能是不接地的，会导致仿真错误
	// 注意：将某个接线柱作为“中转”而不实际使用这个元件，也会导致元件被实际激活，如果元件不能保证内部连接的完备性，在极端状况下就可能出错
	// 完备性：指元件内部任意两个端口永远连通，开关不满足完备性，所以需要特殊处理
	public override bool IsConnected()
	{
		if (ChildPorts[1].Connected == 1)
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	public override void LoadElement()
	{
		//得到端口ID
		int L, M, R;
		L = ChildPorts[0].ID;
		M = ChildPorts[1].ID;
		R = ChildPorts[2].ID;
		if (state == 2)
		{
			CircuitCalculator.UF.Union(R, M);
		}
		else if (state == 0)
		{
			CircuitCalculator.UF.Union(L, M);
		}
	}

	public override void SetElement()//得到约束方程
	{
		//获取元件ID作为元件名称
		int EntityID = CircuitCalculator.EntityNum;
		//得到端口ID
		int L, M, R;
		L = ChildPorts[0].ID;
		M = ChildPorts[1].ID;
		R = ChildPorts[2].ID;
		if (state == 2)
		{
			CircuitCalculator.SpiceEntities.Add(new VoltageSource(string.Concat(EntityID.ToString(), "_", R), R.ToString(), M.ToString(), 0));
		}
		else if (state == 0)
		{
			CircuitCalculator.SpiceEntities.Add(new VoltageSource(string.Concat(EntityID.ToString(), "_", L), L.ToString(), M.ToString(), 0));
		}
	}

	public override EntityData Save()
	{
		return new SwitchData(mySlider.SliderPos, transform.position, transform.rotation, ChildPortID);
	}
}

[System.Serializable]
public class SwitchData : EntityData
{
	private readonly float sliderpos;
	public SwitchData(float sliderpos, Vector3 pos, Quaternion angle, List<int> id) : base(pos, angle, id)
	{
		this.sliderpos = sliderpos;
	}

	override public void Load()
	{
		Switch _switch = EntityCreator.CreateEntity<Switch>(posfloat, anglefloat, IDList);
		_switch.mySlider.SetSliderPos(sliderpos);
	}
}
