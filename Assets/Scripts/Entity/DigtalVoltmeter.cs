using SpiceSharp.Components;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DigtalVoltmeter : EntityBase, ICalculatorUpdate
{
	private readonly double R = 15000;
	private MySwitch mySwitch;
	private Text digtalDigtalVoltmeter;
	private int PortID_GND, PortID_mV, PortID_V;
	private bool isOnTest = false;

	public override void EntityAwake()
	{
		digtalDigtalVoltmeter = transform.FindComponent_DFS<Text>("Text");
	}

	void Start()
	{
		// CalculatorUpdate()统一在Start()中执行，保证在实例化并写入元件自身属性完毕后执行
		CircuitCalculator.CalculateEvent += CalculatorUpdate;
		CalculatorUpdate();

		PortID_GND = ChildPorts[0].ID;
		PortID_mV = ChildPorts[1].ID;
		PortID_V = ChildPorts[2].ID;
	}

	public void CalculatorUpdate()
	{
		if (ChildPorts[1].IsConnected)
		{
			double mV = (ChildPorts[1].U - ChildPorts[0].U) * 1000;
			digtalDigtalVoltmeter.text = EntityText.GetText(mV, 999.99, 2);
		}
		else if (ChildPorts[2].IsConnected)
		{
			double V = ChildPorts[2].U - ChildPorts[0].U;
			digtalDigtalVoltmeter.text = EntityText.GetText(V, 999.99, 2);
		}
		else
		{
			digtalDigtalVoltmeter.text = EntityText.GetText(0, 2);
		}

		if (!isOnTest)
		{
			digtalDigtalVoltmeter.text = EntityText.GetText(0, 2);
		}
	}

	public override void LoadElement() => CircuitCalculator.UF.ListUnion(new List<(int, int)> { (PortID_GND, PortID_mV), (PortID_GND, PortID_V) });

	public override void SetElement(int entityID)
	{
		//TODO:开关
		if (isOnTest)
		{
			CircuitCalculator.SpiceEntities.Add(new Resistor(string.Concat(entityID, "_mV"), PortID_GND.ToString(), PortID_mV.ToString(), R));
			CircuitCalculator.SpiceEntities.Add(new Resistor(string.Concat(entityID, "_V"), PortID_GND.ToString(), PortID_V.ToString(), R));

			CircuitCalculator.SpicePorts.AddRange(ChildPorts);
		}
		else
		{
			Debug.LogWarning("开关关闭");
			/*
			CircuitCalculator.SpiceEntities.Add(new CurrentSource(string.Concat(entityID, "_Off_mV"), PortID_GND.ToString(), PortID_mV.ToString(), 0));
			CircuitCalculator.SpiceEntities.Add(new CurrentSource(string.Concat(entityID, "_Off_V"), PortID_GND.ToString(), PortID_V.ToString(), 0));
			*/
			CircuitCalculator.SpiceEntities.Add(new Resistor(string.Concat(entityID, "_Off_mV"), PortID_GND.ToString(), PortID_mV.ToString(), 1e12));
			CircuitCalculator.SpiceEntities.Add(new Resistor(string.Concat(entityID, "_Off_V"), PortID_GND.ToString(), PortID_V.ToString(), 1e12));
		}
	}

	// 数字电压表属于简单元件（不需特殊值）
	public override EntityData Save() => new SimpleEntityData<DigtalVoltmeter>(transform.position, transform.rotation, ChildPortID);
}

