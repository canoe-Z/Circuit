using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;
using System;
using System.Numerics;
using System.Text.RegularExpressions;

/// <summary>
/// TODO:晶体管
/// </summary>
public class Triode : EntityBase
{
	private int PortID_b, PortID_c, PortID_e;

	public override void EntityAwake() { }

	void Start()
	{
		PortID_b = ChildPorts[0].ID;
		PortID_c = ChildPorts[1].ID;
		PortID_e = ChildPorts[2].ID;
	}


	public override void LoadElement()
	{
		CircuitCalculator.UF.Union(PortID_b, PortID_e);
		CircuitCalculator.UF.Union(PortID_b, PortID_c);
	}

	// 构建晶体管模型
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

	private BipolarJunctionTransistorModel CreateBJTModel(string name, string parameters)
	{
		var bjtmodel = new BipolarJunctionTransistorModel(name);
		ApplyParameters(bjtmodel, parameters);
		return bjtmodel;
	}

	public override void SetElement(int entityID)
	{
		PortID_b = ChildPorts[0].ID;
		PortID_c = ChildPorts[1].ID;
		PortID_e = ChildPorts[2].ID;

		CircuitCalculator.SpiceEntities.Add(new BipolarJunctionTransistor(string.Concat(entityID, "_D"),
			PortID_c.ToString(), PortID_b.ToString(), PortID_e.ToString(), "0", "mjd44h11"));
		CircuitCalculator.SpiceEntities.Add(
			CreateBJTModel("mjd44h11", string.Join(" ",
					"IS = 1.45468e-14 BF = 135.617 NF = 0.85 VAF = 10",
					"IKF = 5.15565 ISE = 2.02483e-13 NE = 3.99964 BR = 13.5617",
					"NR = 0.847424 VAR = 100 IKR = 8.44427 ISC = 1.86663e-13",
					"NC = 1.00046 RB = 1.35729 IRB = 0.1 RBM = 0.1",
					"RE = 0.0001 RC = 0.037687 XTB = 0.90331 XTI = 1",
					"EG = 1.20459 CJE = 3.02297e-09 VJE = 0.649408 MJE = 0.351062",
					"TF = 2.93022e-09 XTF = 1.5 VTF = 1.00001 ITF = 0.999997",
					"CJC = 3.0004e-10 VJC = 0.600008 MJC = 0.409966 XCJC = 0.8",
					"FC = 0.533878 CJS = 0 VJS = 0.75 MJS = 0.5",
					"TR = 2.73328e-08 PTF = 0 KF = 0 AF = 1")));
	}

	public override EntityData Save() => new SimpleEntityData<Triode>(this);
}
