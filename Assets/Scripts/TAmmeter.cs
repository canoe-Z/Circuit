using System.Collections.Generic;
using UnityEngine;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;

public class TAmmeter : MonoBehaviour
{
	public double R = 0.001;
	public NormItem bodyItem;
	void Start()
	{
		bodyItem = this.gameObject.GetComponent<NormItem>();
	}

	//电路相关
	public bool IsConnected()//判断是否有一端连接，避免浮动节点
	{
		if (bodyItem.childsPorts[0].Connected == 1 || bodyItem.childsPorts[1].Connected == 1 || bodyItem.childsPorts[2].Connected == 1)
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
		//获取端口ID并完成并查集连接
		int GND = bodyItem.childsPorts[0].PortID_Global;
		int mA = bodyItem.childsPorts[1].PortID_Global;
		int A = bodyItem.childsPorts[2].PortID_Global;
		Global.MyCircuit.UF.Union(GND, mA);
		Global.MyCircuit.UF.Union(GND, A);
	}
	public void SetElement()//得到约束方程
	{
		//获取元件ID作为元件名称
		int EntityID = Global.MyCircuit.EntityNum;
		int GND = bodyItem.childsPorts[0].PortID_Global;
		int mA = bodyItem.childsPorts[1].PortID_Global;
		int A = bodyItem.childsPorts[2].PortID_Global;
		//获取端口ID并完成内部连接
		Global.MyCircuit.entities.Add(new Resistor(string.Concat(EntityID, "_mA"), GND.ToString(), mA.ToString(), R));
		Global.MyCircuit.entities.Add(new Resistor(string.Concat(EntityID, "_A"), GND.ToString(), A.ToString(), R));
		Global.MyCircuit.ports.Add(bodyItem.childsPorts[0]);
		Global.MyCircuit.ports.Add(bodyItem.childsPorts[1]);
		Global.MyCircuit.ports.Add(bodyItem.childsPorts[2]);
		//电流表将其电流加入检测序列
		Global.MyCircuit.tammeter.Add(this);
	}
	public void Calculate()//计算自身电流
	{
		bodyItem.childsPorts[1].I = (bodyItem.childsPorts[1].U - bodyItem.childsPorts[0].U) / R;
		bodyItem.childsPorts[2].I = (bodyItem.childsPorts[2].U - bodyItem.childsPorts[0].U) / R;
	}
}
