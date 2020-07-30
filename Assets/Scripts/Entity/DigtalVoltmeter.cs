using SpiceSharp.Components;
using UnityEngine.UI;

public class DigtalVoltmeter : EntityBase
{
	public double R = 15000;
	private Text digtalDigtalVoltmeter;
	private int GND, mV, V;

	public override void EntityAwake()
	{
		digtalDigtalVoltmeter = transform.FindComponent_DFS<Text>("Text");
	}

	void Start()
	{
		GND = ChildPorts[0].ID;
		mV = ChildPorts[1].ID;
		V = ChildPorts[2].ID;
	}

	void Update()
	{
		// 数显
		double mV, V;
		if (ChildPorts[1].Connected == 1)
		{
			mV = (ChildPorts[1].U - ChildPorts[0].U) * 1000;
			digtalDigtalVoltmeter.text = EntityText.GetText(mV, 999.99, 2);
		}
		else if (ChildPorts[2].Connected == 1)
		{
			V = ChildPorts[2].U - ChildPorts[0].U;
			digtalDigtalVoltmeter.text = EntityText.GetText(V, 999.99, 2);
		}
		else
		{
			digtalDigtalVoltmeter.text = EntityText.GetText(0, 2);
		}
	}

	public override void LoadElement()
	{
		CircuitCalculator.UF.Union(GND, mV);
		CircuitCalculator.UF.Union(GND, V);
	}

	public override void SetElement()
	{
		int EntityID = CircuitCalculator.EntityNum;

		CircuitCalculator.SpiceEntities.Add(new Resistor(string.Concat(EntityID, "_mV"), GND.ToString(), mV.ToString(), R));
		CircuitCalculator.SpiceEntities.Add(new Resistor(string.Concat(EntityID, "_V"), GND.ToString(), V.ToString(), R));
		CircuitCalculator.SpicePorts.Add(ChildPorts[0]);
		CircuitCalculator.SpicePorts.Add(ChildPorts[1]);
		CircuitCalculator.SpicePorts.Add(ChildPorts[2]);
	}

	public override EntityData Save()
	{
		// 数字电压表属于简单元件（不需特殊值）
		return new SimpleEntityData<DigtalVoltmeter>(transform.position, transform.rotation, ChildPortID);
	}
}

