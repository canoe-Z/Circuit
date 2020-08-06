using SpiceSharp.Components;
using UnityEngine.UI;

public class DigtalAmmeter : EntityBase, ICalculatorUpdate
{
	private readonly double R = 0.001;
	private Text digtalAmmeterText;
	private int PortID_GND, PortID_mA, PortID_A;
	
	public override void EntityAwake()
	{
		digtalAmmeterText = transform.FindComponent_DFS<Text>("Text");
	}

	void Start()
	{
		// CalculatorUpdate()统一在Start()中执行，保证在实例化并写入元件自身属性完毕后执行
		CircuitCalculator.CalculateEvent += CalculatorUpdate;
		CalculatorUpdate();

		PortID_GND = ChildPorts[0].ID;
		PortID_mA = ChildPorts[1].ID;
		PortID_A = ChildPorts[2].ID;
	}

	public void CalculatorUpdate()
	{
		// 计算自身电流
		ChildPorts[1].I = (ChildPorts[1].U - ChildPorts[0].U) / R;
		ChildPorts[2].I = (ChildPorts[2].U - ChildPorts[0].U) / R;

		if (ChildPorts[1].IsConnected)
		{
			double mA = ChildPorts[1].I * 1000;
			digtalAmmeterText.text = EntityText.GetText(mA, 999.99, 2);
		}
		else if (ChildPorts[2].IsConnected)
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
		CircuitCalculator.UF.Union(PortID_GND, PortID_mA);
		CircuitCalculator.UF.Union(PortID_GND, PortID_A);
	}

	public override void SetElement()
	{
		int EntityID = CircuitCalculator.EntityNum;

		CircuitCalculator.SpiceEntities.Add(new Resistor(string.Concat(EntityID, "_mA"), PortID_GND.ToString(), PortID_mA.ToString(), R));
		CircuitCalculator.SpiceEntities.Add(new Resistor(string.Concat(EntityID, "_A"), PortID_GND.ToString(), PortID_A.ToString(), R));

		CircuitCalculator.SpicePorts.AddRange(ChildPorts);
	}

	// 数字电流表属于简单元件
	public override EntityData Save() => new SimpleEntityData<DigtalAmmeter>(transform.position, transform.rotation, ChildPortID);
}