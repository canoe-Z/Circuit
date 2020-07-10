using SpiceSharp.Components;

public class TVoltmeter : EntityBase
{
	public double R = 15000;
	void Start()
	{
		FindCircuitPort();
	}

	//电路相关
	override public bool IsConnected()//判断是否有一端连接，避免浮动节点
	{
		if (childsPorts[0].Connected == 1 || childsPorts[1].Connected == 1 || childsPorts[2].Connected == 1 )
		{
			return true;
		}
		else
		{
			return false;
		}
	}
	override public void LoadElement()//添加元件
	{
		int GND = childsPorts[0].PortID_Global;
		int mV = childsPorts[1].PortID_Global;
		int V = childsPorts[2].PortID_Global;
		CircuitCalculator.UF.Union(GND, mV);
		CircuitCalculator.UF.Union(GND, V);
	}
	override public void SetElement()//添加元件
	{
		int EntityID = CircuitCalculator.EntityNum;
		int GND = childsPorts[0].PortID_Global;
		int mV = childsPorts[1].PortID_Global;
		int V = childsPorts[2].PortID_Global;
		//获取端口ID并完成内部连接
		CircuitCalculator.entities.Add(new Resistor(string.Concat(EntityID, "_mV"), GND.ToString(), mV.ToString(), R));
		CircuitCalculator.entities.Add(new Resistor(string.Concat(EntityID, "_V"), GND.ToString(), V.ToString(), R));
		CircuitCalculator.ports.Add(childsPorts[0]);
		CircuitCalculator.ports.Add(childsPorts[1]);
		CircuitCalculator.ports.Add(childsPorts[2]);
	}
}
