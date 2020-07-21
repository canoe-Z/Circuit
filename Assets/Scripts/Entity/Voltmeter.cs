using SpiceSharp.Components;
using UnityEngine;

public class Voltmeter : EntityBase, IAwake
{
	public double MaxU0 = 1.5;
	public double MaxU1 = 5;
	public double MaxU2 = 15;

	public double R0 = 1500;
	public double R1 = 5000;
	public double R2 = 15000;

	GameObject pin = null;
	float pinPos = 0;//1单位1分米1600像素，750像素=0.46875，1500像素=0.9375，800爆表0.5

	public void EntityAwake()
	{
		FindPin();
	}

	void Update()
	{
		//计算指针偏移量
		double GNDu = ChildPorts[0].U;
		double doublePin = 0;
		doublePin += (ChildPorts[1].U - GNDu) / MaxU0;
		doublePin += (ChildPorts[2].U - GNDu) / MaxU1;
		doublePin += (ChildPorts[3].U - GNDu) / MaxU2;
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
		if (ChildPorts[0].Connected == 1 || ChildPorts[1].Connected == 1 || ChildPorts[2].Connected == 1 || ChildPorts[3].Connected == 1)
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	override public void LoadElement()//添加元件
	{
		int GND = ChildPorts[0].ID;
		int V0 = ChildPorts[1].ID;
		int V1 = ChildPorts[2].ID;
		int V2 = ChildPorts[3].ID;
		CircuitCalculator.UF.Union(GND, V0);
		CircuitCalculator.UF.Union(GND, V1);
		CircuitCalculator.UF.Union(GND, V2);
	}

	override public void SetElement()//添加元件
	{
		int EntityID = CircuitCalculator.EntityNum;
		int GND = ChildPorts[0].ID;
		int V0 = ChildPorts[1].ID;
		int V1 = ChildPorts[2].ID;
		int V2 = ChildPorts[3].ID;
		//指定三个电阻的ID
		string[] ResistorID = new string[3];
		for (int i = 0; i < 3; i++)
		{
			ResistorID[i] = string.Concat(EntityID, "_", i);
		}
		//获取端口ID并完成内部连接
		CircuitCalculator.SpiceEntities.Add(new Resistor(ResistorID[0], GND.ToString(), V0.ToString(), R0));
		CircuitCalculator.SpiceEntities.Add(new Resistor(ResistorID[1], GND.ToString(), V1.ToString(), R1));
		CircuitCalculator.SpiceEntities.Add(new Resistor(ResistorID[2], GND.ToString(), V2.ToString(), R2));
		CircuitCalculator.SpicePorts.Add(ChildPorts[0]);
		CircuitCalculator.SpicePorts.Add(ChildPorts[1]);
		CircuitCalculator.SpicePorts.Add(ChildPorts[2]);
		CircuitCalculator.SpicePorts.Add(ChildPorts[3]);
	}

	public override EntityData Save()
	{
		return new SimpleEntityData<Voltmeter>(gameObject.transform.position, ChildPortID);
	}
}