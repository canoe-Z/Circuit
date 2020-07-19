﻿using System.Collections.Generic;
using SpiceSharp.Components;
using UnityEngine;

public class DigtalAmmeter : EntityBase, IAmmeter, ISave
{
	public double R = 0.001;
	override public void EntityAwake()
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

	override public void LoadElement()
	{
		//获取端口ID并完成并查集连接
		int GND = ChildPorts[0].ID;
		int mA = ChildPorts[1].ID;
		int A = ChildPorts[2].ID;
		CircuitCalculator.UF.Union(GND, mA);
		CircuitCalculator.UF.Union(GND, A);
	}

	override public void SetElement()//得到约束方程
	{
		//获取元件ID作为元件名称
		int EntityID = CircuitCalculator.EntityNum;
		int GND = ChildPorts[0].ID;
		int mA = ChildPorts[1].ID;
		int A = ChildPorts[2].ID;
		//获取端口ID并完成内部连接
		CircuitCalculator.SpiceEntities.Add(new Resistor(string.Concat(EntityID, "_mA"), GND.ToString(), mA.ToString(), R));
		CircuitCalculator.SpiceEntities.Add(new Resistor(string.Concat(EntityID, "_A"), GND.ToString(), A.ToString(), R));
		CircuitCalculator.SpicePorts.Add(ChildPorts[0]);
		CircuitCalculator.SpicePorts.Add(ChildPorts[1]);
		CircuitCalculator.SpicePorts.Add(ChildPorts[2]);
	}
	public void CalculateCurrent()//计算自身电流
	{
		ChildPorts[1].I = (ChildPorts[1].U - ChildPorts[0].U) / R;
		ChildPorts[2].I = (ChildPorts[2].U - ChildPorts[0].U) / R;
	}

	public ILoad Save()
	{
		return new DigtalAmmeterData(gameObject.transform.position, ChildPortID);
	}
}

[System.Serializable]
public class DigtalAmmeterData : EntityBaseData, ILoad
{
	public DigtalAmmeterData(Vector3 pos, List<int> id) : base(pos, id) { }

	override public void Load()
	{
		EntityCreator.CreateEntity<DigtalAmmeter>(posfloat, IDList);
	}
}