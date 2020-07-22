using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEngine;
using SpiceSharp.Components;
using SpiceSharp.Circuits;
using UnityEngine.UI;

public class Solar : EntityBase, ISource
{
	readonly double IscMax = 0.06;
	double Isc;
	int GND, P;
	private MySlider slider;
	private Text sloarText;

	public override void EntityAwake()
	{
		slider = transform.FindComponent_DFS<MySlider>("Slider");
		sloarText = transform.FindComponent_DFS<Text>("Text");
	}

	void Update()
	{
		float fm = 6 - 5 * slider.SliderPos;//会被平方的分母
		float lightStrength = 1 / (fm * fm);//这东西最小值1/36，最大值1
		Isc = lightStrength * IscMax;
		//下面更新光照强度的数值
		sloarText.text = EntityText.GetText(lightStrength * 1000, 1000.00, 2);
	}

	override public bool IsConnected()
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
		int GND = ChildPorts[0].ID;
		int P = ChildPorts[1].ID;
		CircuitCalculator.UF.Union(GND, P);
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
				throw new Exception("Invalid assignment");
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

	override public void SetElement()
	{
		int EntityID = CircuitCalculator.EntityNum;
		GND = ChildPorts[0].ID;

		Debug.LogWarning("短路电流为" + Isc);
		CircuitCalculator.SpiceEntities.Add(new CurrentSource(string.Concat(EntityID, "_S"), "S+", GND.ToString(), Isc));
		CircuitCalculator.SpiceEntities.Add(new Diode(string.Concat(EntityID, "_D"), GND.ToString(), "S+", "1N4007"));
		CircuitCalculator.SpiceEntities.Add(CreateDiodeModel("1N4007", "Is=1.09774e-8 Rs=0.0414388 N=1.78309 Cjo=2.8173e-11 M=0.318974 tt=9.85376e-6 Kf=0 Af=1"));
		CircuitCalculator.SpiceEntities.Add(new Resistor(string.Concat(EntityID, "_R1"), "S+", GND.ToString(), 10000));
		CircuitCalculator.SpiceEntities.Add(new Resistor(string.Concat(EntityID, "_R2"), P.ToString(), "S+", 0.5));
		CircuitCalculator.SpicePorts.Add(ChildPorts[0]);
		CircuitCalculator.SpicePorts.Add(ChildPorts[1]);
	}

	public void GroundCheck()
	{
		if (IsConnected())
		{
			if (!CircuitCalculator.UF.Connected(GND, 0))
			{
				CircuitCalculator.UF.Union(GND, 0);
				CircuitCalculator.GNDLines.Add(new GNDLine(GND));
			}
		}
	}

	public override EntityData Save()
	{
		return new SolarData(transform.position, transform.rotation, ChildPortID);
	}
}

[System.Serializable]
public class SolarData : EntityData
{
	public SolarData(Vector3 pos, Quaternion angle, List<int> id) : base(pos, angle, id) { }

	override public void Load()
	{
		EntityCreator.CreateEntity<Solar>(posfloat, anglefloat, IDList);
	}
}

