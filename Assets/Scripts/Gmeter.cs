using System.Collections.Generic;
using UnityEngine;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
public class Gmeter : MonoBehaviour
{
	double MaxI = 0.001;
	double R = 10;
	GameObject pin = null;
	float pinPos = 0;//1单位1分米1600像素，750像素=0.46875，1500像素=0.9375，800爆表0.5
	public NormItem bodyItem;
	public MySlider mySlider = null;
	//public int LeftPortID, RightPortID;
	//public int EntityID;
	// Start is called before the first frame update
	void Start()
    {
		//滑块
		mySlider = this.gameObject.GetComponentInChildren<MySlider>();
		//if (mySlider == null) Debug.Log("Oh");
		mySlider.Devide = 5;

		bodyItem = this.gameObject.GetComponent<NormItem>();

		int childNum = this.transform.childCount;
		for (int i = 0; i < childNum; i++)
		{
			if (transform.GetChild(i).name == "Pin")
			{
				pin = transform.GetChild(i).gameObject;
				return;
			}
		}
		Debug.LogError("电流计儿子没有pin");
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
		double doublePin = (this.bodyItem.childsPorts[0].I) / MaxI;
		//doublePin -= 0.5;
		pinPos = (float)(doublePin * 0.9375);
		if (pinPos > 0.5) pinPos = 0.5f;
		else if (pinPos < -0.5) pinPos = -0.5f;

		Vector3 pos = pin.transform.localPosition;
		pos.z = pinPos;
		pin.transform.localPosition = pos;

		//ID实时更新
		//EntityID = CircuitcalCulator.EntityNum;
		//LeftPortID = bodyItem.childsPorts[0].PortID_Global;
		//RightPortID = bodyItem.childsPorts[1].PortID_Global;
	}

	//电路相关
	public bool IsConnected()//判断是否有一端连接，避免浮动节点
	{
		if (bodyItem.childsPorts[0].Connected == 1 || bodyItem.childsPorts[1].Connected == 1)
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
		LeftPortID = bodyItem.childsPorts[0].PortID_Global;
		RightPortID = bodyItem.childsPorts[1].PortID_Global;
		CircuitcalCulator.UF.Union(LeftPortID, RightPortID);
	}

	public void SetElement()
	{
		//获取元件ID作为元件名称
		int EntityID = CircuitcalCulator.EntityNum;
		int LeftPortID, RightPortID;
		LeftPortID = bodyItem.childsPorts[0].PortID_Global;
		RightPortID = bodyItem.childsPorts[1].PortID_Global;
		CircuitcalCulator.entities.Add(new Resistor(EntityID.ToString(), LeftPortID.ToString(), RightPortID.ToString(), R));
		CircuitcalCulator.ports.Add(bodyItem.childsPorts[0]);
		CircuitcalCulator.ports.Add(bodyItem.childsPorts[1]);
		//电位计将其电流加入检测序列
		CircuitcalCulator.gmeter.Add(this);
	}

	public void Calculate()//计算自身电流
	{
		bodyItem.childsPorts[0].I = (bodyItem.childsPorts[1].U - bodyItem.childsPorts[0].U) / R;
	}
}