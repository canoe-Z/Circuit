using SpiceSharp.Components;
using UnityEngine.UI;

public class DigtalAmmeter : EntityBase, ICalculatorUpdate
{
	private const double R = 0.001;
	private Text digtalAmmeterText;
	private int GND, mA, A;

	public override void EntityAwake()
	{
		digtalAmmeterText = transform.FindComponent_DFS<Text>("Text");
	}

	void Start()
	{
		CircuitCalculator.CalculateEvent += CalculatorUpdate;
		CalculatorUpdate();

		GND = ChildPorts[0].ID;
		mA = ChildPorts[1].ID;
		A = ChildPorts[2].ID;
	}

	public void CalculatorUpdate()
	{
		// 计算自身电流
		ChildPorts[1].I = (ChildPorts[1].U - ChildPorts[0].U) / R;
		ChildPorts[2].I = (ChildPorts[2].U - ChildPorts[0].U) / R;

		if (ChildPorts[1].Connected == 1)
		{
			double mA = ChildPorts[1].I * 1000;
			digtalAmmeterText.text = EntityText.GetText(mA, 999.99, 2);
		}
		else if (ChildPorts[2].Connected == 1)
		{
			double A = ChildPorts[2].I;
			digtalAmmeterText.text = EntityText.GetText(A, 999.99, 2);
		}
		else
		{
			digtalAmmeterText.text = EntityText.GetText(0, 2);
		}
	}

	public override void LoadElement()
	{
		CircuitCalculator.UF.Union(GND, mA);
		CircuitCalculator.UF.Union(GND, A);
	}

	public override void SetElement()
	{
		int EntityID = CircuitCalculator.EntityNum;

		CircuitCalculator.SpiceEntities.Add(new Resistor(string.Concat(EntityID, "_mA"), GND.ToString(), mA.ToString(), R));
		CircuitCalculator.SpiceEntities.Add(new Resistor(string.Concat(EntityID, "_A"), GND.ToString(), A.ToString(), R));
		CircuitCalculator.SpicePorts.Add(ChildPorts[0]);
		CircuitCalculator.SpicePorts.Add(ChildPorts[1]);
		CircuitCalculator.SpicePorts.Add(ChildPorts[2]);
	}

	public override EntityData Save()
	{
		// 数字电流表属于简单元件
		return new SimpleEntityData<DigtalAmmeter>(transform.position, transform.rotation, ChildPortID);
	}
}