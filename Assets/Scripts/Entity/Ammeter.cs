using UnityEngine;
using SpiceSharp.Components;

public class Ammeter : EntityBase
{
	public double MaxI0 = 0.05;
	public double MaxI1 = 0.1;
	public double MaxI2 = 0.5;
	public double R0 = 2;
	public double R1 = 1;
	public double R2 = 0.2;
	GameObject pin = null;
	float pinPos = 0;//1单位1分米1600像素，750像素=0.46875，1500像素=0.9375，800爆表0.5

	void Start()
	{
		FindCircuitPort();
		FindPin();
	}
	// Update is called once per frame
	void Update()
	{
		double doublePin = 0;
		doublePin += (childsPorts[1].I) / MaxI0;
		doublePin += (childsPorts[2].I) / MaxI1;
		doublePin += (childsPorts[3].I) / MaxI2;
		doublePin -= 0.5;
		pinPos = (float)(doublePin * 0.9375);
		if (pinPos > 0.5) pinPos = 0.5f;
		else if (pinPos < -0.5) pinPos = -0.5f;

		Vector3 pos = pin.transform.localPosition;
		pos.z = pinPos;
		pin.transform.localPosition = pos;
	}
	public void FindPin()
	{
		int childNum = transform.childCount;
		for (int i = 0; i < childNum; i++)
		{
			if (transform.GetChild(i).name == "Pin")
			{
				pin = transform.GetChild(i).gameObject;
				return;
			}
		}
	}

	//电路相关
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
	override public void LoadElement()
	{
		//获取端口ID并完成并查集连接
		int GND = childsPorts[0].PortID_Global;
		int V0 = childsPorts[1].PortID_Global;
		int V1 = childsPorts[2].PortID_Global;
		int V2 = childsPorts[3].PortID_Global;
		CircuitCalculator.UF.Union(GND, V0);
		CircuitCalculator.UF.Union(GND, V1);
		CircuitCalculator.UF.Union(GND, V2);
	}
	override public void SetElement()//得到约束方程
	{
		//获取元件ID作为元件名称
		int EntityID = CircuitCalculator.EntityNum;
		int GND = childsPorts[0].PortID_Global;
		int V0 = childsPorts[1].PortID_Global;
		int V1 = childsPorts[2].PortID_Global;
		int V2 = childsPorts[3].PortID_Global;
		//指定三个电阻的ID
		string[] ResistorID = new string[3];
		for (int i = 0; i < 3; i++)
		{
			ResistorID[i] = string.Concat(EntityID, "_", i);
		}
		//获取端口ID并完成内部连接
		CircuitCalculator.entities.Add(new Resistor(ResistorID[0], GND.ToString(), V0.ToString(), R0));
		CircuitCalculator.entities.Add(new Resistor(ResistorID[1], GND.ToString(), V1.ToString(), R1));
		CircuitCalculator.entities.Add(new Resistor(ResistorID[2], GND.ToString(), V2.ToString(), R2));
		CircuitCalculator.ports.Add(childsPorts[0]);
		CircuitCalculator.ports.Add(childsPorts[1]);
		CircuitCalculator.ports.Add(childsPorts[2]);
		CircuitCalculator.ports.Add(childsPorts[3]);
		//电流表将其电流加入检测序列
		CircuitCalculator.ammeter.Add(this);
	}
	public void Calculate()//计算自身电流
	{
		childsPorts[1].I = (childsPorts[1].U - childsPorts[0].U) / R0;
		childsPorts[2].I = (childsPorts[2].U - childsPorts[0].U) / R1;
		childsPorts[3].I = (childsPorts[3].U - childsPorts[0].U) / R2;
	}
}
