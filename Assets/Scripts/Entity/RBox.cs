using UnityEngine;
using SpiceSharp.Components;

public class RBox : EntityBase
{
	public double R_99999 = 0;
	public double R_99 = 0;
	public double R_09 = 0;
	MySlider[] sliders = new MySlider[6];
	void Start()
	{
		FindCircuitPort();
		MySlider[] slidersDisorder = this.gameObject.GetComponentsInChildren<MySlider>();
		for (int i = 0; i < 6; i++)
		{
			sliders[slidersDisorder[i].SliderID] = slidersDisorder[i];
			sliders[i].Devide = 10;//分成10份
		}
	}

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
	override public bool IsConnected()//判断是否有一端连接，避免浮动节点
	{
		if (childsPorts[0].Connected == 1 || childsPorts[1].Connected == 1 || childsPorts[2].Connected == 1 || childsPorts[3].Connected == 1)
		{
			return true;
		}
		else
		{
			return false;
		}
	}
	//电路相关
	override public void LoadElement()
	{
		//获取端口ID并完成并查集连接
		int G, R999, R99, R9;
		G = childsPorts[0].PortID_Global;
		R999 = childsPorts[3].PortID_Global;//顺序翻转
		R99 = childsPorts[2].PortID_Global;
		R9 = childsPorts[1].PortID_Global;
		CircuitCalculator.UF.Union(G, R9);
		CircuitCalculator.UF.Union(G, R99);
		CircuitCalculator.UF.Union(G, R999);
	}
	override public void SetElement()
	{
		//获取元件ID作为元件名称
		int EntityID = CircuitCalculator.EntityNum;
		int G, R999, R99, R9;
		G = childsPorts[0].PortID_Global;
		R999 = childsPorts[3].PortID_Global;//顺序翻转
		R99 = childsPorts[2].PortID_Global;
		R9 = childsPorts[1].PortID_Global;
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
		CircuitCalculator.ports.Add(childsPorts[0]);
		CircuitCalculator.ports.Add(childsPorts[1]);
		CircuitCalculator.ports.Add(childsPorts[2]);
		CircuitCalculator.ports.Add(childsPorts[3]);
	}
}
