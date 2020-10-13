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
	int PortID_Fu, PortID_Zheng, PortID_O, PortID_Vcc, PortID_Vee;
	double inResis = 1e5;//输入电阻
	double outResis = 10;//输出电阻



	double outInResis = 1e10;//输出电阻(内部)
	double loadResis = 100;//上下的电阻，vcc和vee之间的
	double gain = 1e5;

	public override void EntityAwake() { }

	void Start()
	{
		PortID_Fu = ChildPorts[0].ID;//-
		PortID_Zheng = ChildPorts[1].ID;//+
		PortID_Vee = ChildPorts[2].ID;
		PortID_Vcc = ChildPorts[3].ID;
		PortID_O = ChildPorts[4].ID;
	}


	public override void LoadElement()
	{
		CircuitCalculator.UF.ListUnion(new List<int> { PortID_Fu, PortID_Zheng, PortID_O, PortID_Vcc, PortID_Vee });
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
		CircuitCalculator.SpiceEntities.AddRange(new List<Entity>
		{
			CreateDiodeModel("Opamp_1N4007", "Is=1.09774e-8 Rs=0.0414388 N=1.78309 Cjo=2.8173e-11 M=0.318974 tt=9.85376e-6 Kf=0 Af=1"),
			//CreateDiodeModel("Lixiang","Is=1e-24 N=1e-24 Rs=1e-24 Cjo=0 BV=10000 IBV=0 Vj=1e-8"),
			new Resistor(string.Concat(entityID, "_RinZheng"),PortID_Zheng.ToString(),string.Concat(entityID, "nodeGND"), inResis/2),
			new Resistor(string.Concat(entityID, "_RinFu"),PortID_Fu.ToString(),string.Concat(entityID, "nodeGND"), inResis/2),
			//输入端放大
			new VoltageControlledVoltageSource(string.Concat(entityID, "_Uctrl"),
			string.Concat(entityID, "nodeOutNB"),string.Concat(entityID, "nodeGND"),
			PortID_Zheng.ToString(),PortID_Fu.ToString(),gain),

			//Vcc和Vee之间加两个电阻得到地
			new Resistor(string.Concat(entityID, "_Rvcc"),PortID_Vcc.ToString(),string.Concat(entityID, "nodeGND"),loadResis/2),
			new Resistor(string.Concat(entityID, "_Rvee"),PortID_Vee.ToString(),string.Concat(entityID, "nodeGND"),loadResis/2),

			//
			//new VoltageSource(string.Concat(entityID,"ada"),string.Concat(entityID, "nodeGND"),PortID_Vee.ToString(),3),

			////将输入电压变为nodeVcc和nodeVee
			//new VoltageControlledVoltageSource(string.Concat(entityID, "_Uctrlvcc"),
			//string.Concat(entityID, "nodeVccNB"),string.Concat(entityID, "nodeGND"),
			//PortID_Vcc.ToString(),string.Concat(entityID, "nodeGND"),1),
			//new VoltageControlledVoltageSource(string.Concat(entityID, "_Uctrlvee"),
			//string.Concat(entityID, "nodeVeeNB"),string.Concat(entityID, "nodeGND"),
			//PortID_Vee.ToString(),string.Concat(entityID, "nodeGND"),1),


			//二极管钳位
			new Diode(string.Concat(entityID, "_D1"), PortID_Vee.ToString(), string.Concat(entityID, "nodeOutWake"), "Opamp_1N4007"),
			new Diode(string.Concat(entityID, "_D2"), string.Concat(entityID, "nodeOutWake"), PortID_Vcc.ToString(), "Opamp_1N4007"),

			//弱输出电阻
			new Resistor(string.Concat(entityID, "_Rout"),string.Concat(entityID, "nodeOutNB"),string.Concat(entityID, "nodeOutWake"),outInResis),

			new VoltageControlledVoltageSource(string.Concat(entityID, "_Uctrlout"),
			string.Concat(entityID, "nodeOutWakeNB"),string.Concat(entityID, "nodeGND"),
			string.Concat(entityID, "nodeOutWake"),string.Concat(entityID, "nodeGND"),1),
			
			//真输出电阻
			new Resistor(string.Concat(entityID, "_RoutReal"),PortID_O.ToString(),string.Concat(entityID, "nodeOutWakeNB"),outResis),
		}) ;
	}

	public override EntityData Save() => new SimpleEntityData<OpAmp>(this);
}

public class OpAmpOutPort
{
	public OpAmp father;
	public double U;
}