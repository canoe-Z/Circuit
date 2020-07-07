using System.Collections.Generic;
using UnityEngine;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;

public class SourceStand : EntityBase, INormal
{
	public double E = 1.5f;
	public double R = 100;
	public int G, V;
	public int EntityID;
	void Start()
	{
		FindCircuitPort();
	}

	//下面是电路相关的
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
		//获取端口ID并完成并查集连接
		G = childsPorts[0].PortID_Global;
		V = childsPorts[1].PortID_Global;
		CircuitCalculator.UF.Union(G, V);
	}
	public void SetElement()
	{
		//获取元件ID作为元件名称
		EntityID = CircuitCalculator.EntityNum;
		//获取端口ID并完成内部连接
		G = childsPorts[0].PortID_Global;
		V = childsPorts[1].PortID_Global;
		CircuitCalculator.entities.Add(new VoltageSource(EntityID.ToString(), V.ToString(), string.Concat(EntityID.ToString(), "_rPort"), E));
		CircuitCalculator.entities.Add(new Resistor(string.Concat(EntityID.ToString(), "_r"), string.Concat(EntityID.ToString(), "_rPort"), G.ToString(), R));
		//默认不接地，连接到电路中使用，如果电路中没有形成对0的通路，将其接地
	}
}
