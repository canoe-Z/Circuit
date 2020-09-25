﻿using SpiceSharp.Components;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DigtalVoltmeter : EntityBase, ICalculatorUpdate
{
	private readonly double R = 15000;
	private int PortID_GND, PortID_mV, PortID_V;

	private bool isLoad = false;
	private double mV, V;
	private double tolerance_mV, tolerance_V;
	private double nominal_mV, nominal_V;

	private Text digtalDigtalVoltmeter;
	private MySwitch mySwitch;

	public override void EntityAwake()
	{
		digtalDigtalVoltmeter = transform.FindComponent_DFS<Text>("Text");
		mySwitch = transform.FindComponent_DFS<MySwitch>("MySwitch");

		// 默认启动时开机，读档可覆盖该设置
		mySwitch.IsOn = true;
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
			// 更新真实值
			mV = (ChildPorts[1].U - ChildPorts[0].U) * 1000;
			// 存档沿用误差值
			if (isLoad)
			{
				isLoad = false;
				digtalDigtalVoltmeter.text = EntityText.GetText(nominal_mV, 999.99, 2);
			}
			// 否则计算误差限，使用随机生成的误差值
			else
			{
				tolerance_mV = 0.02 * 0.01 * mV + 0.001 * 2;
				nominal_mV = mV + tolerance_mV * Random.Range(-1f, 1f);
				digtalDigtalVoltmeter.text = EntityText.GetText(nominal_mV, 999.99, 2);
			}
		}
		else if (ChildPorts[2].IsConnected)
		{
			V = ChildPorts[2].U - ChildPorts[0].U;
			if (isLoad)
			{
				isLoad = false;
				digtalDigtalVoltmeter.text = EntityText.GetText(nominal_V, 999.99, 2);
			}
			else
			{
				tolerance_V = 0.02 * 0.01 * V + 0.001 * 2;
				nominal_V = V + tolerance_V * Random.Range(-1f, 1f);
				digtalDigtalVoltmeter.text = EntityText.GetText(nominal_V, 999.99, 2);
			}
		}
		else
		{
			digtalDigtalVoltmeter.text = "0.00";
		}

		// 开关变化引起电路重新计算，之后调用该部分
		if (!mySwitch.IsOn)
		{
			digtalDigtalVoltmeter.text = "0.00";
		}
	}

	public override void LoadElement() => CircuitCalculator.UF.ListUnion(new List<(int, int)> { (PortID_GND, PortID_mV), (PortID_GND, PortID_V) });

	public override void SetElement(int entityID)
	{
		if (mySwitch.IsOn)
		{
			CircuitCalculator.SpiceEntities.Add(new Resistor(string.Concat(entityID, "_Off_mV"), PortID_GND.ToString(), PortID_mV.ToString(), R));
			CircuitCalculator.SpiceEntities.Add(new Resistor(string.Concat(entityID, "_Off_V"), PortID_GND.ToString(), PortID_V.ToString(), R));

			CircuitCalculator.SpicePorts.AddRange(ChildPorts);
		}
	}

	public override EntityData Save() => new DigtalVoltmeterData(this);

	[System.Serializable]
	public class DigtalVoltmeterData : EntityData
	{
		private readonly bool isOn;
		private readonly double nominal_mV, nominal_V;


		public DigtalVoltmeterData(DigtalVoltmeter digtalVoltmeter)
		{
			baseData = new EntityBaseData(digtalVoltmeter);
			isOn = digtalVoltmeter.mySwitch.IsOn;
			nominal_mV = digtalVoltmeter.nominal_mV;
			nominal_V = digtalVoltmeter.nominal_V;
		}

		public override void Load()
		{
			DigtalVoltmeter digtalVoltmeter = BaseCreate<DigtalVoltmeter>(baseData);
			digtalVoltmeter.mySwitch.IsOn = isOn;
			digtalVoltmeter.isLoad = true;
			digtalVoltmeter.nominal_mV = nominal_mV;
			digtalVoltmeter.nominal_V = nominal_V;
		}
	}
}

