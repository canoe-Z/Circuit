using SpiceSharp.Components;
using UnityEngine;

public class DigtalVoltmeter : EntityBase,ISave
{
	public double R = 15000;
	override public void EntityStart()
	{
		FindCircuitPort();
	}

	//电路相关
	override public bool IsConnected()//判断是否有一端连接，避免浮动节点
	{
		if (ChildPorts[0].Connected == 1 || ChildPorts[1].Connected == 1 || ChildPorts[2].Connected == 1)
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
		int mV = ChildPorts[1].ID;
		int V = ChildPorts[2].ID;
		CircuitCalculator.UF.Union(GND, mV);
		CircuitCalculator.UF.Union(GND, V);
	}

	override public void SetElement()//添加元件
	{
		int EntityID = CircuitCalculator.EntityNum;
		int GND = ChildPorts[0].ID;
		int mV = ChildPorts[1].ID;
		int V = ChildPorts[2].ID;
		//获取端口ID并完成内部连接
		CircuitCalculator.SpiceEntities.Add(new Resistor(string.Concat(EntityID, "_mV"), GND.ToString(), mV.ToString(), R));
		CircuitCalculator.SpiceEntities.Add(new Resistor(string.Concat(EntityID, "_V"), GND.ToString(), V.ToString(), R));
		CircuitCalculator.SpicePorts.Add(ChildPorts[0]);
		CircuitCalculator.SpicePorts.Add(ChildPorts[1]);
		CircuitCalculator.SpicePorts.Add(ChildPorts[2]);
	}

	public static void CreateDigtalVoltmeter(float x,float y,float z, int id1, int id2, int id3)
	{
		GameObject digtalVoltmeter = (GameObject)Resources.Load("DigtalVoltmeter");
		Quaternion b = new Quaternion(0, 0, 0, 0);
		GameObject newv = Instantiate(digtalVoltmeter, new Vector3(x, y, z),b);
		newv.GetComponent<DigtalVoltmeter>().ChildPorts[0].ID = id1;
		newv.GetComponent<DigtalVoltmeter>().ChildPorts[1].ID = id2;
		newv.GetComponent<DigtalVoltmeter>().ChildPorts[2].ID = id3;
	}
	public ILoad Save()
	{
		return new DigtalVoltmeterData(gameObject.transform.position, ChildPorts[0].ID, ChildPorts[1].ID, ChildPorts[2].ID);
	}
}

[System.Serializable]
public class DigtalVoltmeterData : ILoad
{
	int ID1, ID2, ID3;
	public float X, Y, Z;

	public DigtalVoltmeterData(Vector3 pos,int id1,int id2,int id3)
	{
		ID1 = id1;
		ID2 = id2;
		ID3 = id3;
		X = pos.x;
		Y = pos.y;
		Z = pos.z;
	}

	public void Load()
	{
		DigtalVoltmeter.CreateDigtalVoltmeter(X,Y,Z,ID1,ID2,ID3);
	}
}