using SpiceSharp.Components;
using UnityEngine.UI;

public class Resistance : EntityBase
{
	public double Rnum = 120;
	public Text num;
	int LeftPortID, RightPortID;
	override public void EntityStart()
	{
		FindCircuitPort();
		LeftPortID = ChildPorts[0].PortID;
		RightPortID = ChildPorts[1].PortID;
	}

	private void Update()
	{
		if (num) num.text = Rnum.ToString();
	}

	//电路相关
	override public bool IsConnected()//判断是否有一端连接，避免浮动节点
	{
		if (ChildPorts[0].Connected == 1 || ChildPorts[1].Connected == 1)
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
		CircuitCalculator.UF.Union(LeftPortID, RightPortID);
	}
	override public void SetElement()
	{
		//获取元件ID作为元件名称
		int EntityID = CircuitCalculator.EntityNum;
		CircuitCalculator.SpiceEntities.Add(new Resistor(EntityID.ToString(), LeftPortID.ToString(), RightPortID.ToString(), Rnum));
		/*
		CircuitCalculator.ports.Add(childsPorts[0]); 
		CircuitCalculator.ports.Add(childsPorts[1]);
		*/
	}
}