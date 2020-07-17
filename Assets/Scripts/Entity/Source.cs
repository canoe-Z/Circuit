using SpiceSharp.Components;

public class Source : EntityBase, ISource
{
	public const int SourceNum = 3;
	public double R0 = 0.1;
	public double E0Max = 30;
	public double R1 = 0.1;
	public double E1Max = 30;
	public double R2 = 0.1;
	MySlider[] sliders = new MySlider[2];
	public int[] G = new int[3];
	public int[] V = new int[3];
	public double[] E = new double[3] { 30, 30, 5 };
	public double[] R = new double[3] { 0.1, 0.1, 0.1 };
	public int EntityID;
	override public void EntityStart()
	{
		FindCircuitPort();
		MySlider[] slidersDisorder = this.gameObject.GetComponentsInChildren<MySlider>();
		sliders[slidersDisorder[0].SliderID] = slidersDisorder[0];
		sliders[slidersDisorder[1].SliderID] = slidersDisorder[1];
	}

	void Update()
	{
		E[0] = sliders[0].SliderPos * E0Max;
		E[1] = sliders[1].SliderPos * E1Max;
	}
	//电路相关
	public bool IsConnected(int n)
	{
		if (ChildPorts[2 * n + 1].Connected == 1 || ChildPorts[2 * n].Connected == 1)
		{
			return true;
		}
		else
		{
			return false;
		}
	}
	override public bool IsConnected()
	{
		bool _isConnected = false;
		for (int j = 0; j < 3; j++)
		{
			if (IsConnected(j))
			{
				_isConnected = true;
			}
		}
		return _isConnected;
	}
	public void LoadElement(int n)
	{
		G[n] = ChildPorts[2 * n + 1].PortID;
		V[n] = ChildPorts[2 * n].PortID;
		CircuitCalculator.UF.Union(G[n], V[n]);
	}
	override public void LoadElement()
	{
		for (int j = 0; j < SourceNum; j++)
		{
			if (IsConnected(j))
			{
				LoadElement(j);
			}
		}
	}
	public void SetElement(int n)
	{
		EntityID = CircuitCalculator.EntityNum;
		G[n] = ChildPorts[2 * n + 1].PortID;
		V[n] = ChildPorts[2 * n].PortID;
		CircuitCalculator.SpiceEntities.Add(new VoltageSource(string.Concat(EntityID, "_", n), V[n].ToString(), string.Concat(EntityID, "_rPort", n), E[n]));
		CircuitCalculator.SpiceEntities.Add(new Resistor(string.Concat(EntityID.ToString(), "_r", n), string.Concat(EntityID, "_rPort", n), G[n].ToString(), R[n]));
	}
	override public void SetElement()
	{
		for (int j = 0; j < SourceNum; j++)
		{
			if (IsConnected(j))
			{
				SetElement(j);
			}
		}
	}

	public void GroundCheck()
	{
		for (int j = 0; j < SourceNum; j++)
		{
			if (IsConnected(j))
			{
				if (!CircuitCalculator.UF.Connected(G[j], 0))
				{
					CircuitCalculator.UF.Union(G[j], 0);
					CircuitCalculator.GNDLines.Add(new GNDLine(G[j]));
				}
			}
		}
	}
}
