using System.Collections.Generic;
using UnityEngine;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;

public class SliderR : MonoBehaviour
{
	public double Rmax = 300;
	public double RL = 300;
	public double RR = 0;
	NormItem bodyItem;
	MySlider myslider;
	void Start()
    {
		bodyItem = this.gameObject.GetComponent<NormItem>();
		if (bodyItem == null) Debug.LogError("SliderRItem");
		myslider = this.gameObject.GetComponentInChildren<MySlider>();
		myslider.SliderPos = 1;
	}

    void Update()
    {
		this.RL = Rmax * myslider.SliderPos;
		this.RR = Rmax - RL;
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
	public void LoadElement()
	{
		//获取元件ID作为元件名称
		int EntityID = CircuitcalCulator.EntityNum;
		//获取端口ID并完成内部连接
		int TL, TR, L, R;
		TL = bodyItem.childsPorts[0].PortID_Global;
		TR = bodyItem.childsPorts[1].PortID_Global;
		L = bodyItem.childsPorts[2].PortID_Global;
		R = bodyItem.childsPorts[3].PortID_Global;
		CircuitcalCulator.UF.Union(TL, L);
		CircuitcalCulator.UF.Union(TL, R);
		CircuitcalCulator.UF.Union(TL, TR);
	}
	public void SetElement()
	{
		//获取元件ID作为元件名称
		int EntityID = CircuitcalCulator.EntityNum;
		//获取端口ID并完成内部连接
		int TL, TR, L, R;
		TL = bodyItem.childsPorts[0].PortID_Global;
		TR = bodyItem.childsPorts[1].PortID_Global;
		L = bodyItem.childsPorts[2].PortID_Global;
		R = bodyItem.childsPorts[3].PortID_Global;
		CircuitcalCulator.entities.Add(new Resistor(string.Concat(EntityID, "_L"), TL.ToString(), L.ToString(), RL));
		CircuitcalCulator.entities.Add(new Resistor(string.Concat(EntityID, "_R"), TL.ToString(), R.ToString(), RR));
		CircuitcalCulator.entities.Add(new VoltageSource(string.Concat(EntityID, "_T"), TL.ToString(), TR.ToString(), 0));
		CircuitcalCulator.ports.Add(bodyItem.childsPorts[0]);
		CircuitcalCulator.ports.Add(bodyItem.childsPorts[1]);
		CircuitcalCulator.ports.Add(bodyItem.childsPorts[2]);
		CircuitcalCulator.ports.Add(bodyItem.childsPorts[3]);

	}
}
