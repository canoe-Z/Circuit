using System.Collections.Generic;
using UnityEngine;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;

public class Voltmeter : MonoBehaviour
{
	public double MaxU0 = 1.5;
	public double MaxU1 = 5;
	public double MaxU2 = 15;
	public double R0 = 1500;
	public double R1 = 5000;
	public double R2 = 15000;


	GameObject pin = null;
	float pinPos = 0;//1单位1分米1600像素，750像素=0.46875，1500像素=0.9375，800爆表0.5
	public NormItem bodyItem = null;
	void Start()
    {
		bodyItem = this.gameObject.GetComponent<NormItem>();

		//找到指针
		int childNum = this.transform.childCount;
		for(int i = 0; i < childNum; i++)
		{
			if (transform.GetChild(i).name == "Pin")
			{
				pin = transform.GetChild(i).gameObject;
				return;
			}
		}
		Debug.LogError("电压表儿子没有pin");
    }

	// Update is called once per frame
	void Update()
	{
		//计算指针偏移量
		double GNDu = this.bodyItem.childsPorts[0].U;
		double doublePin = 0;
		doublePin += (this.bodyItem.childsPorts[1].U - GNDu) / MaxU0;
		doublePin += (this.bodyItem.childsPorts[2].U - GNDu) / MaxU1;
		doublePin += (this.bodyItem.childsPorts[3].U - GNDu) / MaxU2;
		doublePin -= 0.5;
		pinPos = (float)(doublePin * 0.9375);
		if (pinPos > 0.5) pinPos = 0.5f;
		else if (pinPos < -0.5) pinPos = -0.5f;
		Vector3 pos = pin.transform.localPosition;
		pos.z = pinPos;
		pin.transform.localPosition = pos;
	}

	//电路相关
	public bool IsConnected()//判断是否有一端连接，避免浮动节点
	{
		if (bodyItem.childsPorts[0].Connected == 1 || bodyItem.childsPorts[1].Connected == 1 || bodyItem.childsPorts[2].Connected == 1 || bodyItem.childsPorts[3].Connected == 1)
		{
			return true;
		}
		else
		{
			return false;
		}
	}
	public void LoadElement()//添加元件
	{
		int GND = bodyItem.childsPorts[0].PortID_Global;
		int V0 = bodyItem.childsPorts[1].PortID_Global;
		int V1 = bodyItem.childsPorts[2].PortID_Global;
		int V2 = bodyItem.childsPorts[3].PortID_Global;
		Global.MyCircuit.UF.Union(GND, V0);
		Global.MyCircuit.UF.Union(GND, V1);
		Global.MyCircuit.UF.Union(GND, V2);
	}
	public void SetElement()//添加元件
	{
		int EntityID = Global.MyCircuit.EntityNum;
		int GND = bodyItem.childsPorts[0].PortID_Global;
		int V0 = bodyItem.childsPorts[1].PortID_Global;
		int V1 = bodyItem.childsPorts[2].PortID_Global;
		int V2 = bodyItem.childsPorts[3].PortID_Global;
		//指定三个电阻的ID
		string[] ResistorID = new string[3];
		for (int i = 0; i < 3; i++)
		{
			ResistorID[i] = string.Concat(EntityID, "_", i);
		}
		//获取端口ID并完成内部连接
		Global.MyCircuit.entities.Add(new Resistor(ResistorID[0], GND.ToString(), V0.ToString(), R0));
		Global.MyCircuit.entities.Add(new Resistor(ResistorID[1], GND.ToString(), V1.ToString(), R1));
		Global.MyCircuit.entities.Add(new Resistor(ResistorID[2], GND.ToString(), V2.ToString(), R2));
		Global.MyCircuit.ports.Add(bodyItem.childsPorts[0]);
		Global.MyCircuit.ports.Add(bodyItem.childsPorts[1]);
		Global.MyCircuit.ports.Add(bodyItem.childsPorts[2]);
		Global.MyCircuit.ports.Add(bodyItem.childsPorts[3]);
	}
}
