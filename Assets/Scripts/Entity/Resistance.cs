using System.Collections.Generic;
using SpiceSharp.Components;
using UnityEngine;
using UnityEngine.UI;

public class Resistance : EntityBase
{
	public double Value = 120;
	public Text resistanceText;

	public override void EntityAwake()
	{
		resistanceText = transform.FindComponent_DFS<Text>("Text");
	}

	void Start()
	{
		// 不能在Awake(）中执行，Awake()之后还可能修改阻值
		if (resistanceText) resistanceText.text = Value.ToString();
	}

	// 电路相关
	public override bool IsConnected()//判断是否有一端连接，避免浮动节点
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

	public override void LoadElement()
	{
		int LeftPortID = ChildPorts[0].ID;
		int RightPortID = ChildPorts[1].ID;
		CircuitCalculator.UF.Union(LeftPortID, RightPortID);
	}

	public override void SetElement()
	{
		//获取元件ID作为元件名称
		int LeftPortID = ChildPorts[0].ID;
		int RightPortID = ChildPorts[1].ID;
		int EntityID = CircuitCalculator.EntityNum;
		CircuitCalculator.SpiceEntities.Add(new Resistor(EntityID.ToString(), LeftPortID.ToString(), RightPortID.ToString(), Value));
		CircuitCalculator.SpicePorts.Add(ChildPorts[0]); 
		CircuitCalculator.SpicePorts.Add(ChildPorts[1]);
	}

	public override EntityData Save()
	{
		return new ResistanceData(Value, gameObject.transform.position, ChildPortID);
	}
}

[System.Serializable]
public class ResistanceData : EntityData
{
	private readonly double rnum;
	public ResistanceData(double rnum,Vector3 pos, List<int> id) : base(pos, id) 
	{
		this.rnum = rnum;
	}

	override public void Load()
	{
		Resistance resistance = EntityCreator.CreateEntity<Resistance>(posfloat, IDList);
		resistance.Value = rnum;
	}
}