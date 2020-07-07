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
		int EntityID = CircuitCalculator.EntityNum;
		//获取端口ID并完成内部连接
		int TL, TR, L, R;
		TL = bodyItem.childsPorts[0].PortID_Global;
		TR = bodyItem.childsPorts[1].PortID_Global;
		L = bodyItem.childsPorts[2].PortID_Global;
		R = bodyItem.childsPorts[3].PortID_Global;
		CircuitCalculator.UF.Union(TL, L);
		CircuitCalculator.UF.Union(TL, R);
		CircuitCalculator.UF.Union(TL, TR);
	}
	public void SetElement()
	{
		//获取元件ID作为元件名称
		int EntityID = CircuitCalculator.EntityNum;
		//获取端口ID并完成内部连接
		int TL, TR, L, R;
		TL = bodyItem.childsPorts[0].PortID_Global;
		TR = bodyItem.childsPorts[1].PortID_Global;
		L = bodyItem.childsPorts[2].PortID_Global;
		R = bodyItem.childsPorts[3].PortID_Global;
		CircuitCalculator.entities.Add(new Resistor(string.Concat(EntityID, "_L"), TL.ToString(), L.ToString(), RL));
		CircuitCalculator.entities.Add(new Resistor(string.Concat(EntityID, "_R"), TL.ToString(), R.ToString(), RR));
		CircuitCalculator.entities.Add(new VoltageSource(string.Concat(EntityID, "_T"), TL.ToString(), TR.ToString(), 0));
		CircuitCalculator.ports.Add(bodyItem.childsPorts[0]);
		CircuitCalculator.ports.Add(bodyItem.childsPorts[1]);
		CircuitCalculator.ports.Add(bodyItem.childsPorts[2]);
		CircuitCalculator.ports.Add(bodyItem.childsPorts[3]);

	}
}
