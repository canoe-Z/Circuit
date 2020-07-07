using System.Collections.Generic;
using UnityEngine;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;

public class RBox : MonoBehaviour
{
	public double R_99999 = 0;
	public double R_99 = 0;
	public double R_09 = 0;
	NormItem bodyItem;
	MySlider[] sliders = new MySlider[6];
	// Start is called before the first frame update
	void Start()
	{
		bodyItem = this.gameObject.GetComponent<NormItem>();
		MySlider[] slidersDisorder = this.gameObject.GetComponentsInChildren<MySlider>();
		//Debug.Log(sliders.Length);
		//sliders = slidersDisorder;
		for (int i = 0; i < 6; i++)
		{
			sliders[slidersDisorder[i].SliderID] = slidersDisorder[i];
			sliders[i].Devide = 10;//分成10份
		}
	}

    // Update is called once per frame
    void Update()
	{
		int total = 0;
		for(int i = 0; i < 6; i++)
		{
			total *= 10;
			total += sliders[i].SliderPos_int;
		}
		this.R_99999 = (float)total / (float)10;
		this.R_99 = (float)(total % 100) / (float)10;
		this.R_09 = (float)(total % 10) / (float)10;
	}
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
	//电路相关
	public void LoadElement()
	{
		//获取端口ID并完成并查集连接
		int G, R999, R99, R9;
		G = bodyItem.childsPorts[0].PortID_Global;
		R999 = bodyItem.childsPorts[3].PortID_Global;//顺序翻转
		R99 = bodyItem.childsPorts[2].PortID_Global;
		R9 = bodyItem.childsPorts[1].PortID_Global;
		CircuitCalculator.UF.Union(G, R9);
		CircuitCalculator.UF.Union(G, R99);
		CircuitCalculator.UF.Union(G, R999);
	}
	public void SetElement()
	{
		//获取元件ID作为元件名称
		int EntityID = CircuitCalculator.EntityNum;
		int G, R999, R99, R9;
		G = bodyItem.childsPorts[0].PortID_Global;
		R999 = bodyItem.childsPorts[3].PortID_Global;//顺序翻转
		R99 = bodyItem.childsPorts[2].PortID_Global;
		R9 = bodyItem.childsPorts[1].PortID_Global;
		//指定三个电阻的ID
		string[] ResistorID = new string[3];
		for (int i = 0; i < 3; i++)
		{
			ResistorID[i] = string.Concat(EntityID, "_", i);
		}
		//获取端口ID并完成内部连接
		CircuitCalculator.entities.Add(new Resistor(ResistorID[0], G.ToString(), R999.ToString(), R_99999));
		CircuitCalculator.entities.Add(new Resistor(ResistorID[1], G.ToString(), R99.ToString(), R_99));
		CircuitCalculator.entities.Add(new Resistor(ResistorID[2], G.ToString(), R9.ToString(), R_09));
		CircuitCalculator.ports.Add(bodyItem.childsPorts[0]);
		CircuitCalculator.ports.Add(bodyItem.childsPorts[1]);
		CircuitCalculator.ports.Add(bodyItem.childsPorts[2]);
		CircuitCalculator.ports.Add(bodyItem.childsPorts[3]);
	}
}
