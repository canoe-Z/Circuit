using SpiceSharp.Components;
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

	//电路相关
	override public bool IsConnected()//判断是否有一端连接，避免浮动节点
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
		//获取端口ID并完成并查集连接
		int GND = ChildPorts[0].ID;
		int mA = ChildPorts[1].ID;
		int A = ChildPorts[2].ID;
		CircuitCalculator.UF.Union(GND, mA);
		CircuitCalculator.UF.Union(GND, A);
	}

	override public void SetElement()//得到约束方程
	{
		//获取元件ID作为元件名称
		int EntityID = CircuitCalculator.EntityNum;
		int GND = ChildPorts[0].ID;
		int mA = ChildPorts[1].ID;
		int A = ChildPorts[2].ID;
		//获取端口ID并完成内部连接
		CircuitCalculator.SpiceEntities.Add(new Resistor(string.Concat(EntityID, "_mA"), GND.ToString(), mA.ToString(), R));
		CircuitCalculator.SpiceEntities.Add(new Resistor(string.Concat(EntityID, "_A"), GND.ToString(), A.ToString(), R));
		CircuitCalculator.SpicePorts.Add(ChildPorts[0]);
		CircuitCalculator.SpicePorts.Add(ChildPorts[1]);
		CircuitCalculator.SpicePorts.Add(ChildPorts[2]);
	}
	public void CalculateCurrent()//计算自身电流
	{
		ChildPorts[1].I = (ChildPorts[1].U - ChildPorts[0].U) / R;
		ChildPorts[2].I = (ChildPorts[2].U - ChildPorts[0].U) / R;
	}

	public override EntityData Save()
	{
		return new SimpleEntityData<DigtalAmmeter>(gameObject.transform.position, ChildPortID);
	}
}