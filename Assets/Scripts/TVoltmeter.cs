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
		CircuitCalculator.UF.Union(GND, mV);
		CircuitCalculator.UF.Union(GND, V);
	}
	public void SetElement()//添加元件
	{
		int EntityID = CircuitCalculator.EntityNum;
		int GND = bodyItem.childsPorts[0].PortID_Global;
		int mV = bodyItem.childsPorts[1].PortID_Global;
		int V = bodyItem.childsPorts[2].PortID_Global;
		//获取端口ID并完成内部连接
		CircuitCalculator.entities.Add(new Resistor(string.Concat(EntityID, "_mV"), GND.ToString(), mV.ToString(), R));
		CircuitCalculator.entities.Add(new Resistor(string.Concat(EntityID, "_V"), GND.ToString(), V.ToString(), R));
		CircuitCalculator.ports.Add(bodyItem.childsPorts[0]);
		CircuitCalculator.ports.Add(bodyItem.childsPorts[1]);
		CircuitCalculator.ports.Add(bodyItem.childsPorts[2]);
	}
}
