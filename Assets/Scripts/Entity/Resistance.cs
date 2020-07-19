using System.Collections.Generic;
using SpiceSharp.Components;
using UnityEngine;
using UnityEngine.UI;

public class Resistance : EntityBase, ISave
{
	public double Rnum = 120;
	public Text num;
	override public void EntityAwake()
	{
		FindCircuitPort();
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
		int LeftPortID = ChildPorts[0].ID;
		int RightPortID = ChildPorts[1].ID;
		CircuitCalculator.UF.Union(LeftPortID, RightPortID);
	}

	override public void SetElement()
	{
		//获取元件ID作为元件名称
		int LeftPortID = ChildPorts[0].ID;
		int RightPortID = ChildPorts[1].ID;
		int EntityID = CircuitCalculator.EntityNum;
		CircuitCalculator.SpiceEntities.Add(new Resistor(EntityID.ToString(), LeftPortID.ToString(), RightPortID.ToString(), Rnum));
		CircuitCalculator.SpicePorts.Add(ChildPorts[0]); 
		CircuitCalculator.SpicePorts.Add(ChildPorts[1]);
	}

	public ILoad Save()
	{
		return new ResistanceData(Rnum, gameObject.transform.position, ChildPortID);
	}
}

[System.Serializable]
public class ResistanceData : EntityBaseData, ILoad
{
	private readonly double rnum;
	public ResistanceData(double rnum,Vector3 pos, List<int> id) : base(pos, id) 
	{
		this.rnum = rnum;
	}

	override public void Load()
	{
		Resistance resistance = EntityCreator.CreateEntity<Resistance>(posfloat, IDList);
		resistance.Rnum = rnum;
	}
}