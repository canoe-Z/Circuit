using System.Collections.Generic;
using UnityEngine;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;

public class Source : MonoBehaviour
{
	public double R0 = 0.1;
	double E0 = 5;
	public double E0Max = 30;
	public double R1 = 0.1;
	double E1 = 5;
	public double E1Max = 30;
	public double R2 = 0.1;
	readonly double E2 = 5;
	NormItem bodyItem;
	MySlider[] sliders = new MySlider[2];
	public int[] G = new int[3];
	public int[] V = new int[3];
	public double[] E = new double[3] { 30,30,5 };
	public double[] R = new double[3] { 0.1, 0.1, 0.1 };
	public int EntityID;
	void Start()
    {
		bodyItem = this.gameObject.GetComponent<NormItem>();
		MySlider[] slidersDisorder = this.gameObject.GetComponentsInChildren<MySlider>();
		sliders[slidersDisorder[0].SliderID] = slidersDisorder[0];
		sliders[slidersDisorder[1].SliderID] = slidersDisorder[1];
	}

  
    void Update()
    {
		E[0] = sliders[0].SliderPos * E0Max;
		E[1] = sliders[1].SliderPos * E1Max;
	}
	//电路相关
	public bool IsConnected(int n)
	{
		if (bodyItem.childsPorts[2 * n + 1].Connected == 1 || bodyItem.childsPorts[2 * n].Connected == 1)
		{
			return true;
		}
		else
		{
			return false;
		}
	}
	public void LoadElement(int n)
	{
		G[n] = bodyItem.childsPorts[2 * n + 1].PortID_Global;
		V[n] = bodyItem.childsPorts[2 * n].PortID_Global;
		Global.MyCircuit.UF.Union(G[n], V[n]);
	}
	public void SetElement(int n)
	{
		EntityID = Global.MyCircuit.EntityNum;
		G[n] = bodyItem.childsPorts[2 * n + 1].PortID_Global;
		V[n] = bodyItem.childsPorts[2 * n].PortID_Global;
		Global.MyCircuit.entities.Add(new VoltageSource(string.Concat(EntityID, "_", n), V[n].ToString(), string.Concat(EntityID, "_rPort", n), E[n]));
		Global.MyCircuit.entities.Add(new Resistor(string.Concat(EntityID.ToString(), "_r", n), string.Concat(EntityID, "_rPort", n), G[n].ToString(), R[n]));
	}
	/*
	public void LoadElement()//得到约束方程
	{
		//获取端口ID并完成并查集连接
		V0 = bodyItem.childsPorts[0].PortID_Global;
		G0 = bodyItem.childsPorts[1].PortID_Global;
		V1 = bodyItem.childsPorts[2].PortID_Global;
		G1 = bodyItem.childsPorts[3].PortID_Global;
		V2 = bodyItem.childsPorts[4].PortID_Global;
		G2 = bodyItem.childsPorts[5].PortID_Global;
		Global.MyCircuit.UF.Union(V0, G0);
		Global.MyCircuit.UF.Union(V1, G1);
		Global.MyCircuit.UF.Union(V2, G2);
		Global.MyCircuit.UF.Union(0, G0);
		Global.MyCircuit.UF.Union(0, G1);
		Global.MyCircuit.UF.Union(0, G2);
		Global.MyCircuit.UF.Union(0, V0);
		Global.MyCircuit.UF.Union(0, V1);
		Global.MyCircuit.UF.Union(0, V2);
	}
	public void SetElement()//得到约束方程
	{
		//获取元件ID作为元件名称
		int EntityID = Global.MyCircuit.EntityNum;
		//获取端口ID并完成内部连接
		int V0 = bodyItem.childsPorts[0].PortID_Global;
		int G0 = bodyItem.childsPorts[1].PortID_Global;
		int V1 = bodyItem.childsPorts[2].PortID_Global;
		int G1 = bodyItem.childsPorts[3].PortID_Global;
		int V2 = bodyItem.childsPorts[4].PortID_Global;
		int G2 = bodyItem.childsPorts[5].PortID_Global;
		//指定三个电压源的ID
		string[] SourceID = new string[3];
		for (int i = 0; i < 3; i++)
		{
			SourceID[i] = string.Concat(EntityID,"_",i);
		}
		Global.MyCircuit.entities.Add(new VoltageSource(SourceID[0], V0.ToString(), "r0", E0));
		Global.MyCircuit.entities.Add(new VoltageSource(SourceID[1], V1.ToString(), "r1", E1));
		Global.MyCircuit.entities.Add(new VoltageSource(SourceID[2], V2.ToString(), "r2", E2));
		//封装电阻
		//Global.MyCircuit.entities.Add(new VoltageSource(SourceID[2], V2.ToString(), G2.ToString(), E2));
		Global.MyCircuit.entities.Add(new Resistor(string.Concat(SourceID[0], "_r"), "r0", G0.ToString(), R0));
		Global.MyCircuit.entities.Add(new Resistor(string.Concat(SourceID[1], "_r"), "r1", G1.ToString(), R1));
		Global.MyCircuit.entities.Add(new Resistor(string.Concat(SourceID[2], "_r"), "r2", G2.ToString(), R2));
		Global.MyCircuit.entities.Add(new VoltageSource(string.Concat(SourceID[0], "_GND"), G0.ToString(), "0", 0));
		Global.MyCircuit.entities.Add(new VoltageSource(string.Concat(SourceID[1], "_GND"), G1.ToString(), "0", 0));
		Global.MyCircuit.entities.Add(new VoltageSource(string.Concat(SourceID[2], "_GND"), G2.ToString(), "0", 0));
	}
	*/
}
