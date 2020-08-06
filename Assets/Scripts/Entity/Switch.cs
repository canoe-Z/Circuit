using SpiceSharp.Components;
using System.Collections.Generic;
using UnityEngine;

public class Switch : EntityBase
{
	private int state = 1;
	private MySlider mySlider = null;
	private GameObject connector = null;
	private int PortID_L, PortID_M, PortID_R;

	public override void EntityAwake()
	{
		mySlider = gameObject.GetComponentInChildren<MySlider>();
		mySlider.SetSliderPos(0.5f);
		
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
	}

	void Start()
	{
		mySlider.SliderEvent += UpdateSlider;
		UpdateSlider();

		PortID_L = ChildPorts[0].ID;
		PortID_M = ChildPorts[1].ID;
		PortID_R = ChildPorts[2].ID;
	}

	// 开关的状态有三种
	void UpdateSlider()
	{
		if (mySlider.SliderPos > 0.8f) state = 2;			//R，右接线柱接通
		else if (mySlider.SliderPos < 0.2f) state = 0;		//L，左接线柱接通
		else state = 1;										//M，不接通
		connector.transform.LookAt(mySlider.gameObject.transform);
	}

	// 开关一定要接中间才能激活
	// 否则当左/右端口单独被激活时，其余的那个端口就会和中间建立实质的连接，而这可能是不接地的，会导致仿真错误
	// 注意：将某个接线柱作为“中转”而不实际使用这个元件，也会导致元件被实际激活，如果元件不能保证内部连接的完备性，在极端状况下就可能出错
	// 完备性：指元件内部任意两个端口永远连通，开关不满足完备性，所以需要特殊处理
	public override bool IsConnected() => ChildPorts[1].IsConnected;

	public override void LoadElement()
	{
		//得到端口ID
		if (state == 2)
		{
			CircuitCalculator.UF.Union(PortID_R, PortID_M);
		}
		else if (state == 0)
		{
			CircuitCalculator.UF.Union(PortID_L, PortID_M);
		}
	}

	public override void SetElement()//得到约束方程
	{
		int EntityID = CircuitCalculator.EntityNum;

		if (state == 2)
		{
			CircuitCalculator.SpiceEntities.Add(new VoltageSource(string.Concat(EntityID.ToString(), "_", PortID_R), PortID_R.ToString(), PortID_M.ToString(), 0));
		}
		else if (state == 0)
		{
			CircuitCalculator.SpiceEntities.Add(new VoltageSource(string.Concat(EntityID.ToString(), "_", PortID_L), PortID_L.ToString(), PortID_M.ToString(), 0));
		}
	}

	public static GameObject Create(float? sliderPos = null, Float3 pos = null, Float4 angle = null, List<int> IDList = null)
	{
		Switch _switch = BaseCreate<Switch>(pos, angle, IDList);
		if (sliderPos != null) _switch.mySlider.SetSliderPos(sliderPos.Value);
		return _switch.gameObject;
	}

	public override EntityData Save() => new SwitchData(mySlider.SliderPos, transform.position, transform.rotation, ChildPortID);
}

[System.Serializable]
public class SwitchData : EntityData
{
	private readonly float sliderPos;
	public SwitchData(float sliderPos, Vector3 pos, Quaternion angle, List<int> IDList) : base(pos, angle, IDList)
	{
		this.sliderPos = sliderPos;
	}

	public override void Load() => Switch.Create(sliderPos, pos, angle, IDList);
}
