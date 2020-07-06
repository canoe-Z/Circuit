﻿using UnityEngine;
using SpiceSharp.Components;

public class Source : MonoBehaviour
{
	public double R0 = 0.1;
	public double E0Max = 30;
	public double R1 = 0.1;
	public double E1Max = 30;
	public double R2 = 0.1;
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
		CircuitcalCulator.UF.Union(G[n], V[n]);
	}
	public void SetElement(int n)
	{
		EntityID = CircuitcalCulator.EntityNum;
		G[n] = bodyItem.childsPorts[2 * n + 1].PortID_Global;
		V[n] = bodyItem.childsPorts[2 * n].PortID_Global;
		CircuitcalCulator.entities.Add(new VoltageSource(string.Concat(EntityID, "_", n), V[n].ToString(), string.Concat(EntityID, "_rPort", n), E[n]));
		CircuitcalCulator.entities.Add(new Resistor(string.Concat(EntityID.ToString(), "_r", n), string.Concat(EntityID, "_rPort", n), G[n].ToString(), R[n]));
	}
}
