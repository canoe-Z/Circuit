using SpiceSharp.Components;
using SpiceSharp.Entities;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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


	// 构建二极管模型
	protected void ApplyParameters(Entity entity, string definition)
	{
		// Get all assignments
		definition = Regex.Replace(definition, @"\s*\=\s*", "=");
		var assignments = definition.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
		foreach (var assignment in assignments)
		{
			// Get the name and value
			var parts = assignment.Split('=');
			if (parts.Length != 2)
				throw new System.Exception("Invalid assignment");
			var name = parts[0].ToLower();
			var value = double.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);

			// Set the entity parameter
			entity.SetParameter(name, value);
		}
	}
	private DiodeModel CreateDiodeModel(string name, string parameters)
	{
		var dm = new DiodeModel(name);
		ApplyParameters(dm, parameters);
		return dm;
	}


	public override void SetElement(int entityID)
	{
		// 伪装Vcc和Vee的连接，读取其电压作为上下限

		PortID_G = ChildPorts[0].ID;
		PortID_V = ChildPorts[1].ID;

		CircuitCalculator.SpiceEntities.AddRange(new List<Entity>
		{
			CreateDiodeModel("Solar_1N4007", "Is=1.09774e-8 Rs=0.0414388 N=1.78309 Cjo=2.8173e-11 M=0.318974 tt=9.85376e-6 Kf=0 Af=1"),
			new CurrentSource(string.Concat(entityID, "_S"), "S+", PortID_G.ToString(), Isc),
			new Diode(string.Concat(entityID, "_D"), PortID_G.ToString(), "S+", "Solar_1N4007"),
			new Resistor(string.Concat(entityID, "_R1"), "S+", PortID_G.ToString(), 10000),
			new Resistor(string.Concat(entityID, "_R2"), PortID_V.ToString(), "S+", 0.5)
		});

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