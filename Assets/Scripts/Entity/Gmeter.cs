﻿using UnityEngine;
using SpiceSharp.Components;
public class Gmeter : EntityBase, INormal
{
	double MaxI = 0.001;
	double R = 10;
	GameObject pin = null;
	float pinPos = 0;//1单位1分米1600像素，750像素=0.46875，1500像素=0.9375，800爆表0.5
	public MySlider mySlider = null;
	void Start()
    {
		FindCircuitPort();
		FindPin();
		//滑块
		mySlider = this.gameObject.GetComponentInChildren<MySlider>();
		//if (mySlider == null) Debug.Log("Oh");
		mySlider.Devide = 5;
	}

    void Update()
	{
		//量程
		this.MaxI = 0.1;
		this.R = 10;
		for(int i = 0; i < mySlider.SliderPos_int; i++)
		{
			MaxI *= 0.01;
			R *= 2;
		}

		//示数
		double doublePin = (childsPorts[0].I) / MaxI;
		//doublePin -= 0.5;
		pinPos = (float)(doublePin * 0.9375);
		if (pinPos > 0.5) pinPos = 0.5f;
		else if (pinPos < -0.5) pinPos = -0.5f;

		Vector3 pos = pin.transform.localPosition;
		pos.z = pinPos;
		pin.transform.localPosition = pos;

	}

	//电路相关
	public void FindPin()
	{
		int childNum = transform.childCount;
		for (int i = 0; i < childNum; i++)
		{
			if (transform.GetChild(i).name == "Pin")
			{
				pin = transform.GetChild(i).gameObject;
				return;
			}
		}
	}

	public bool IsConnected()//判断是否有一端连接，避免浮动节点
	{
		if (childsPorts[0].Connected == 1 || childsPorts[1].Connected == 1)
		{
			return true;
		}
		else
		{
			return false;
		}
	}
	public void LoadElement()
	{
		int LeftPortID, RightPortID;
		LeftPortID = childsPorts[0].PortID_Global;
		RightPortID = childsPorts[1].PortID_Global;
		CircuitCalculator.UF.Union(LeftPortID, RightPortID);
	}

	public void SetElement()
	{
		//获取元件ID作为元件名称
		int EntityID = CircuitCalculator.EntityNum;
		int LeftPortID, RightPortID;
		LeftPortID = childsPorts[0].PortID_Global;
		RightPortID = childsPorts[1].PortID_Global;
		CircuitCalculator.entities.Add(new Resistor(EntityID.ToString(), LeftPortID.ToString(), RightPortID.ToString(), R));
		CircuitCalculator.ports.Add(childsPorts[0]);
		CircuitCalculator.ports.Add(childsPorts[1]);
		//电位计将其电流加入检测序列
		CircuitCalculator.gmeter.Add(this);
	}

	public void Calculate()//计算自身电流
	{
		childsPorts[0].I = (childsPorts[1].U - childsPorts[0].U) / R;
	}
}