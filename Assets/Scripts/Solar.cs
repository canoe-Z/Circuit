using System;
using System.Numerics;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using SpiceSharp.Circuits;

public class Solar : MonoBehaviour
{
	double IscMax = 0.06;
	double Isc;
	public int GND,P;
	public NormItem bodyItem;
	public MySlider[] sliders = new MySlider[1];
	public int EntityID;
	void Start()
	{
		bodyItem = this.gameObject.GetComponent<NormItem>();
		MySlider[] slidersDisorder = this.gameObject.GetComponentsInChildren<MySlider>();
		sliders[slidersDisorder[0].SliderID] = slidersDisorder[0];
	}


	void Update()
	{
		Isc = sliders[0].SliderPos * IscMax;
	}
	//电路相关
	public bool IsConnected()//判断是否有一端连接，避免浮动节点
	{
		if (bodyItem.childsPorts[0].Connected == 1 || bodyItem.childsPorts[1].Connected == 1)
		{
			return true;
		}
		else
		{
			return false;
		}
	}
	public void LoadElement()//添加元件
	{
		GND = bodyItem.childsPorts[0].PortID_Global;
		P = bodyItem.childsPorts[1].PortID_Global;
		CircuitcalCulator.UF.Union(GND, P);
	}

	//二极管模型
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

	public void SetElement()//添加元件
	{
		int EntityID = CircuitcalCulator.EntityNum;
		GND = bodyItem.childsPorts[0].PortID_Global;
		P = bodyItem.childsPorts[1].PortID_Global;
		//获取端口ID并完成内部连接
		Debug.LogError("短路电流为" + Isc);
		CircuitcalCulator.entities.Add(new CurrentSource(string.Concat(EntityID, "_S"), "S+", GND.ToString(), Isc));
		CircuitcalCulator.entities.Add(new Diode(string.Concat(EntityID, "_D"), GND.ToString(), "S+", "1N4007"));
		CircuitcalCulator.entities.Add(CreateDiodeModel("1N4007", "Is=1.09774e-8 Rs=0.0414388 N=1.78309 Cjo=2.8173e-11 M=0.318974 tt=9.85376e-6 Kf=0 Af=1"));
		CircuitcalCulator.entities.Add(new Resistor(string.Concat(EntityID, "_R1"), "S+", GND.ToString(), 10000));
		CircuitcalCulator.entities.Add(new Resistor(string.Concat(EntityID, "_R2"), P.ToString(), "S+", 0.5));
		CircuitcalCulator.ports.Add(bodyItem.childsPorts[0]);
		CircuitcalCulator.ports.Add(bodyItem.childsPorts[1]);
	}
}
