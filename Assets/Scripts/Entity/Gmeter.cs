using System.Collections.Generic;
using SpiceSharp.Components;
using UnityEngine;

public class Gmeter : EntityBase, IAmmeter , ISave
{
	double MaxI = 0.001;
	double R = 10;
	GameObject pin = null;
	float pinPos = 0;//1单位1分米1600像素，750像素=0.46875，1500像素=0.9375，800爆表0.5
	public MySlider mySlider = null;
	override public void EntityAwake()
	{
		FindCircuitPort();
		FindPin();
		//滑块
		mySlider = this.gameObject.GetComponentInChildren<MySlider>();
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
		double doublePin = (ChildPorts[0].I) / MaxI;
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

	override public bool IsConnected()//判断是否有一端连接，避免浮动节点
	{
		if (ChildPorts[0].Connected == 1 || ChildPorts[1].Connected == 1)
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
		int LeftPortID, RightPortID;
		LeftPortID = ChildPorts[0].ID;
		RightPortID = ChildPorts[1].ID;
		CircuitCalculator.UF.Union(LeftPortID, RightPortID);
	}

	override public void SetElement()
	{
		//获取元件ID作为元件名称
		int EntityID = CircuitCalculator.EntityNum;
		int LeftPortID, RightPortID;
		LeftPortID = ChildPorts[0].ID;
		RightPortID = ChildPorts[1].ID;
		CircuitCalculator.SpiceEntities.Add(new Resistor(EntityID.ToString(), LeftPortID.ToString(), RightPortID.ToString(), R));
		CircuitCalculator.SpicePorts.Add(ChildPorts[0]);
		CircuitCalculator.SpicePorts.Add(ChildPorts[1]);
		//电位计将其电流加入检测序列
		CircuitCalculator.Ammeters.Add(this);
	}

	public void CalculateCurrent()//计算自身电流
	{
		ChildPorts[0].I = (ChildPorts[1].U - ChildPorts[0].U) / R;
	}

	public ILoad Save()
	{
		return new GmeterData(gameObject.transform.position, ChildPortID);
	}
}

[System.Serializable]
public class GmeterData : EntityBaseData, ILoad
{
	public GmeterData(Vector3 pos, List<int> id) : base(pos, id) { }

	override public void Load()
	{
		EntityCreator.CreateEntity<Gmeter>(posfloat, IDList);
	}
}
