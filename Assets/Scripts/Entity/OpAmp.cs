using SpiceSharp.Components;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// TODO:运算放大器
/// </summary>
public class OpAmp : EntityBase
{
	private int PortID_N, PortID_P, PortID_O;

	public CircuitPort outPort, veePort, vccPort;
	//public OpAmpOutPort outPort = new OpAmpOutPort();
	public override void EntityAwake() { }

	public bool NeedReset = false;
	public double LastVcc, LastVee;

	void Start()
	{
		PortID_N = ChildPorts[0].ID;
		PortID_P = ChildPorts[1].ID;

		veePort = ChildPorts[2];
		vccPort = ChildPorts[3];
		outPort = ChildPorts[4];
		PortID_O = ChildPorts[4].ID;
	}


	public override void LoadElement()
	{
		CircuitCalculator.UF.ListUnion(new List<int> { PortID_N, PortID_P, PortID_O });
	}

	public override void SetElement(int entityID)
	{
		// 伪装Vcc和Vee的连接，读取其电压作为上下限

		if(!NeedReset)
        {
			// 运放
        }
		else
        {
			// 输出上下限电压
        }
		throw new System.NotImplementedException();
	}

	public void SaveLimit()
    {
		LastVcc = vccPort.U;
		LastVee = veePort.U;
	}

	public bool IsOverFlow()
	{
		return outPort.U > vccPort.U || outPort.U < veePort.U;
    }

	public override EntityData Save()
	{
		throw new System.NotImplementedException();
	}
}

public class OpAmpOutPort
{
	public OpAmp father;
	public double U;
}