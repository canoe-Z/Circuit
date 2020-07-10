using UnityEngine;
using SpiceSharp.Components;

public class SourceStand : EntityBase, ISource
{
	public double E = 1.5f;
	public double R = 100;
	public int G, V;
	public int EntityID;
	void Start()
	{
		FindCircuitPort();
	}

	//下面是电路相关的
	override public bool IsConnected()//判断是否有一端连接，避免浮动节点
	{
		if (childsPorts[0].Connected == 1 || childsPorts[1].Connected == 1)
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
		G = childsPorts[0].PortID_Global;
		V = childsPorts[1].PortID_Global;
		CircuitCalculator.UF.Union(G, V);
	}
	override public void SetElement()
	{
		//获取元件ID作为元件名称
		EntityID = CircuitCalculator.EntityNum;
		//获取端口ID并完成内部连接
		G = childsPorts[0].PortID_Global;
		V = childsPorts[1].PortID_Global;
		CircuitCalculator.entities.Add(new VoltageSource(EntityID.ToString(), V.ToString(), string.Concat(EntityID.ToString(), "_rPort"), E));
		CircuitCalculator.entities.Add(new Resistor(string.Concat(EntityID.ToString(), "_r"), string.Concat(EntityID.ToString(), "_rPort"), G.ToString(), R));
		//默认不接地，连接到电路中使用，如果电路中没有形成对0的通路，将其接地
	}

	public void GroundCheck()
	{
		if (IsConnected())
		{
			if (!CircuitCalculator.UF.Connected(G, 0))
			{
				CircuitCalculator.UF.Union(G, 0);
				CircuitCalculator.entities.Add(new VoltageSource(string.Concat(EntityID.ToString(), "_GND"), G.ToString(), "0", 0));
				Debug.LogError("额外电源悬空，将额外电源接地");
			}
			else
			{
				Debug.LogError("额外电源已经接地");
			}
		}
	}
}
