﻿using UnityEngine;
using SpiceSharp.Components;

public class Switch : EntityBase
{
	public int state = 1;
	public MySlider mySlider = null;
	GameObject connector = null;
	// Start is called before the first frame update
	override public void EntityStart()
	{
		FindCircuitPort();
		//滑块
		mySlider = this.gameObject.GetComponentInChildren<MySlider>();
		int childNum = this.transform.childCount;
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

	// 开关的状态有三种
	void Update()
    {
		if (mySlider.SliderPos > 0.8f) state = 2; //R
		else if (mySlider.SliderPos < 0.2f) state = 0; //L
		else state = 1; //M

		connector.transform.LookAt(mySlider.gameObject.transform);
    }

	//电路相关
	override public bool IsConnected()//判断是否有一端连接，避免浮动节点(对于开关中间必连）
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
	override public void LoadElement()
	{
		//得到端口ID
		int L, M, R;
		L = ChildPorts[0].PortID;
		M = ChildPorts[1].PortID;
		R = ChildPorts[2].PortID;
		if (state == 2)
		{
			CircuitCalculator.UF.Union(R, M);
		}
		else if (state == 0)
		{
			CircuitCalculator.UF.Union(L, M);
		}
	}
	override public void SetElement()//得到约束方程
	{
		//获取元件ID作为元件名称
		int EntityID = CircuitCalculator.EntityNum;
		//得到端口ID
		int L, M, R;
		L = ChildPorts[0].PortID;
		M = ChildPorts[1].PortID;
		R = ChildPorts[2].PortID;
		if (state == 2)
		{
			CircuitCalculator.SpiceEntities.Add(new VoltageSource(string.Concat(EntityID.ToString(), "_", R), R.ToString(), M.ToString(), 0));
		}
		else if (state == 0)
		{
			CircuitCalculator.SpiceEntities.Add(new VoltageSource(string.Concat(EntityID.ToString(), "_", L), L.ToString(), M.ToString(), 0));
		}
	}
}