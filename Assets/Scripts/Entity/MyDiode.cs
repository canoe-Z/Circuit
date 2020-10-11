using System;
using System.Text.RegularExpressions;
using SpiceSharp.Components;
using SpiceSharp.Entities;

/// <summary>
/// 二极管
/// </summary>
public class MyDiode : EntityBase
{
	private int PortID_G, PortID_V;

	public override void EntityAwake() { }

	void Start()
	{
		PortID_G = ChildPorts[0].ID;
		PortID_V = ChildPorts[1].ID;
	}

	public override void LoadElement() => CircuitCalculator.UF.Union(PortID_G, PortID_V);

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
		PortID_G = ChildPorts[0].ID;
		PortID_V = ChildPorts[1].ID;

		CircuitCalculator.SpiceEntities.Add(new Diode(string.Concat(entityID, "_D"),
			PortID_V.ToString(), PortID_G.ToString(), "MyZener"));
		/*
		CircuitCalculator.SpiceEntities.Add(
			CreateDiodeModel("1N4007", "Is=1.09774e-8 Rs=0.0414388 N=1.78309 Cjo=2.8173e-11 M=0.318974 tt=9.85376e-6 Kf=0 Af=1"));
		*/
		//.MODEL IdealD D (Is=1e-14 Rs=0.0 N=1.0 Tt=0.0 Cjo=0.0 Vj=1.0 ....)
		//CircuitCalculator.SpiceEntities.Add(
			//CreateDiodeModel("MyZener", "Is=1e-20 Rs=100000 N=1.0 Cjo=1e-20 M=0.0 tt=1e-20 bv=12 ibv=1e-20 vj=100"));
	}

	public override EntityData Save() => new SimpleEntityData<MyDiode>(this);
}