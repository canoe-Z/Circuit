using System.Collections.Generic;
using UnityEngine;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;

public class TVoltmeter : MonoBehaviour
{
	public double R = 15000;
	public NormItem bodyItem = null;
	void Start()
	{
		bodyItem = this.gameObject.GetComponent<NormItem>();
	}

	//电路相关
	public bool IsConnected()//判断是否有一端连接，避免浮动节点
	{
		if (bodyItem.childsPorts[0].Connected == 1 || bodyItem.childsPorts[1].Connected == 1 || bodyItem.childsPorts[2].Connected == 1 )
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
		int mV = bodyItem.childsPorts[1].PortID_Global;
		int V = bodyItem.childsPorts[2].PortID_Global;
		Global.MyCircuit.UF.Union(GND, mV);
		Global.MyCircuit.UF.Union(GND, V);
	}
	public void SetElement()//添加元件
	{
		int EntityID = Global.MyCircuit.EntityNum;
		int GND = bodyItem.childsPorts[0].PortID_Global;
		int mV = bodyItem.childsPorts[1].PortID_Global;
		int V = bodyItem.childsPorts[2].PortID_Global;
		//获取端口ID并完成内部连接
		Global.MyCircuit.entities.Add(new Resistor(string.Concat(EntityID, "_mV"), GND.ToString(), mV.ToString(), R));
		Global.MyCircuit.entities.Add(new Resistor(string.Concat(EntityID, "_V"), GND.ToString(), V.ToString(), R));
		Global.MyCircuit.ports.Add(bodyItem.childsPorts[0]);
		Global.MyCircuit.ports.Add(bodyItem.childsPorts[1]);
		Global.MyCircuit.ports.Add(bodyItem.childsPorts[2]);
	}
}
