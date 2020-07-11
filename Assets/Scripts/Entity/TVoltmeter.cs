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
		if (ChildPorts[0].Connected == 1 || ChildPorts[1].Connected == 1 || ChildPorts[2].Connected == 1 )
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
		int GND = ChildPorts[0].PortID_Global;
		int mV = ChildPorts[1].PortID_Global;
		int V = ChildPorts[2].PortID_Global;
		CircuitCalculator.UF.Union(GND, mV);
		CircuitCalculator.UF.Union(GND, V);
	}
	override public void SetElement()//添加元件
	{
		int EntityID = CircuitCalculator.EntityNum;
		int GND = ChildPorts[0].PortID_Global;
		int mV = ChildPorts[1].PortID_Global;
		int V = ChildPorts[2].PortID_Global;
		//获取端口ID并完成内部连接
		CircuitCalculator.SpiceEntities.Add(new Resistor(string.Concat(EntityID, "_mV"), GND.ToString(), mV.ToString(), R));
		CircuitCalculator.SpiceEntities.Add(new Resistor(string.Concat(EntityID, "_V"), GND.ToString(), V.ToString(), R));
		CircuitCalculator.SpicePorts.Add(ChildPorts[0]);
		CircuitCalculator.SpicePorts.Add(ChildPorts[1]);
		CircuitCalculator.SpicePorts.Add(ChildPorts[2]);
	}
}
