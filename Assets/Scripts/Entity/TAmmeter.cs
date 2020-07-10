using SpiceSharp.Components;

public class TAmmeter : EntityBase, IAmmeter
{
	public double R = 0.001;
	void Start()
	{
		FindCircuitPort();
	}

	//电路相关
	override public bool IsConnected()//判断是否有一端连接，避免浮动节点
	{
		if (childsPorts[0].Connected == 1 || childsPorts[1].Connected == 1 || childsPorts[2].Connected == 1)
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
		int GND = childsPorts[0].PortID_Global;
		int mA = childsPorts[1].PortID_Global;
		int A = childsPorts[2].PortID_Global;
		CircuitCalculator.UF.Union(GND, mA);
		CircuitCalculator.UF.Union(GND, A);
	}
	override public void SetElement()//得到约束方程
	{
		//获取元件ID作为元件名称
		int EntityID = CircuitCalculator.EntityNum;
		int GND = childsPorts[0].PortID_Global;
		int mA = childsPorts[1].PortID_Global;
		int A = childsPorts[2].PortID_Global;
		//获取端口ID并完成内部连接
		CircuitCalculator.entities.Add(new Resistor(string.Concat(EntityID, "_mA"), GND.ToString(), mA.ToString(), R));
		CircuitCalculator.entities.Add(new Resistor(string.Concat(EntityID, "_A"), GND.ToString(), A.ToString(), R));
		CircuitCalculator.ports.Add(childsPorts[0]);
		CircuitCalculator.ports.Add(childsPorts[1]);
		CircuitCalculator.ports.Add(childsPorts[2]);
	}
	public void CalculateCurrent()//计算自身电流
	{
		childsPorts[1].I = (childsPorts[1].U - childsPorts[0].U) / R;
		childsPorts[2].I = (childsPorts[2].U - childsPorts[0].U) / R;
	}
}
