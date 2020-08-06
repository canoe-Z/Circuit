﻿using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEngine;
using SpiceSharp.Components;
using SpiceSharp.Circuits;
using UnityEngine.UI;

public class Solar : EntityBase, ISource
{
	private const double _IscMax = 0.06;
	private double Isc;
	private int PortID_G, PortID_V;

	public MySlider MySlider { get; set; }
	private Text sloarText;

	public override void EntityAwake()
	{
		MySlider = transform.FindComponent_DFS<MySlider>("Slider");
		sloarText = transform.FindComponent_DFS<Text>("Text");
	}

	void Start()
	{
		// 第一次执行初始化，此后受事件控制
		MySlider.SliderEvent += UpdateSlider;
		UpdateSlider();

		PortID_G = ChildPorts[0].ID;
		PortID_V = ChildPorts[1].ID;
	}

	void UpdateSlider()
	{
		float fm = 6 - 5 * MySlider.SliderPos;    // 会被平方的分母
		float lightStrength = 1 / (fm * fm);    // 这东西最小值1/36，最大值1
		Isc = lightStrength * _IscMax;

		//	更新光照强度的数值
		sloarText.text = EntityText.GetText(lightStrength * 1000, 1000.00, 2);
	}

	public override void LoadElement()
	{
		CircuitCalculator.UF.Union(PortID_G, PortID_V);
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

	public override void SetElement()
	{
		int EntityID = CircuitCalculator.EntityNum;
		PortID_G = ChildPorts[0].ID;
		PortID_V = ChildPorts[1].ID;

		Debug.Log("短路电流为" + Isc);
		CircuitCalculator.SpiceEntities.Add(new CurrentSource(string.Concat(EntityID, "_S"), "S+", PortID_G.ToString(), Isc));
		CircuitCalculator.SpiceEntities.Add(new Diode(string.Concat(EntityID, "_D"), PortID_G.ToString(), "S+", "1N4007"));
		CircuitCalculator.SpiceEntities.Add(CreateDiodeModel("1N4007", "Is=1.09774e-8 Rs=0.0414388 N=1.78309 Cjo=2.8173e-11 M=0.318974 tt=9.85376e-6 Kf=0 Af=1"));
		CircuitCalculator.SpiceEntities.Add(new Resistor(string.Concat(EntityID, "_R1"), "S+", PortID_G.ToString(), 10000));
		CircuitCalculator.SpiceEntities.Add(new Resistor(string.Concat(EntityID, "_R2"), PortID_V.ToString(), "S+", 0.5));
	}

	public void GroundCheck()
	{
		if (IsConnected())
		{
			if (!CircuitCalculator.UF.Connected(PortID_G, 0))
			{
				CircuitCalculator.UF.Union(PortID_G, 0);
				CircuitCalculator.GNDLines.Add(new GNDLine(PortID_G));
			}
		}
	}

	public static GameObject Create(float? sliderPos = null, Float3 pos = null, Float4 angle = null, List<int> IDlist = null)
	{
		Solar solar = BaseCreate<Solar>(pos, angle, IDlist);
		if (sliderPos != null) solar.MySlider.SetSliderPos(sliderPos.Value);
		return solar.gameObject;
	}

	public override EntityData Save() => new SolarData(MySlider.SliderPos, transform.position, transform.rotation, ChildPortID);
}

[System.Serializable]
public class SolarData : EntityData
{
	private readonly float sliderPos;

	public SolarData(float sliderPos, Vector3 pos, Quaternion angle, List<int> IDList) : base(pos, angle, IDList)
	{
		this.sliderPos = sliderPos;
	}

	public override void Load() => Solar.Create(sliderPos, pos, angle, IDList);
}