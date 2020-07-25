using SpiceSharp.Components;
using UnityEngine;
using UnityEngine.UI;

public class DigtalAmmeter : EntityBase, IAmmeter
{
	public double R = 0.001;
	private Text digtalAmmeterText;

	public override void EntityAwake()
	{
		digtalAmmeterText = transform.FindComponent_DFS<Text>("Text");
	}

	void Update()
	{
		double mA, A;
		if (ChildPorts[1].Connected == 1)
		{
			mA = ChildPorts[1].I * 1000;
			digtalAmmeterText.text = EntityText.GetText(mA, 999.99, 2);
		}
		else if (ChildPorts[2].Connected == 1)
		{
			A = ChildPorts[2].I;
			digtalAmmeterText.text = EntityText.GetText(A, 999.99, 2);
		}
		else
		{
			digtalAmmeterText.text = EntityText.GetText(0, 2);
		}
	}

	override public bool IsConnected()
	{
		if (ChildPorts[0].Connected == 1 || ChildPorts[1].Connected == 1 || ChildPorts[2].Connected == 1)
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
		int mA = ChildPorts[1].ID;
		int A = ChildPorts[2].ID;

		CircuitCalculator.UF.Union(GND, mA);
		CircuitCalculator.UF.Union(GND, A);
	}

	override public void SetElement()
	{
		int EntityID = CircuitCalculator.EntityNum;
		int GND = ChildPorts[0].ID;
		int mA = ChildPorts[1].ID;
		int A = ChildPorts[2].ID;

		CircuitCalculator.SpiceEntities.Add(new Resistor(string.Concat(EntityID, "_mA"), GND.ToString(), mA.ToString(), R));
		CircuitCalculator.SpiceEntities.Add(new Resistor(string.Concat(EntityID, "_A"), GND.ToString(), A.ToString(), R));
		CircuitCalculator.SpicePorts.Add(ChildPorts[0]);
		CircuitCalculator.SpicePorts.Add(ChildPorts[1]);
		CircuitCalculator.SpicePorts.Add(ChildPorts[2]);
	}

	// 数字电流表实现IAmmeter接口，可计算自身电流
	public void CalculateCurrent()
	{
		ChildPorts[1].I = (ChildPorts[1].U - ChildPorts[0].U) / R;
		ChildPorts[2].I = (ChildPorts[2].U - ChildPorts[0].U) / R;
	}

	public override EntityData Save()
	{
		// 数字电流表属于简单元件
		return new SimpleEntityData<DigtalAmmeter>(transform.position, transform.rotation, ChildPortID);
	}
}