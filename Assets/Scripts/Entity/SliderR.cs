using SpiceSharp.Components;

public class SliderR : EntityBase
{
	public double Rmax = 300;
	public double RL = 300;
	public double RR = 0;
	MySlider myslider;
	void Start()
    {
		FindCircuitPort();
		myslider = this.gameObject.GetComponentInChildren<MySlider>();
		myslider.SliderPos = 1;
	}

    void Update()
    {
		this.RL = Rmax * myslider.SliderPos;
		this.RR = Rmax - RL;
    }

	//电路相关
	override public bool IsConnected()//判断是否有一端连接，避免浮动节点
	{
		if (childsPorts[0].Connected == 1 || childsPorts[1].Connected == 1 || childsPorts[2].Connected == 1 || childsPorts[3].Connected == 1)
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
		//获取端口ID并完成内部连接
		int TL, TR, L, R;
		TL = childsPorts[0].PortID_Global;
		TR = childsPorts[1].PortID_Global;
		L = childsPorts[2].PortID_Global;
		R = childsPorts[3].PortID_Global;
		CircuitCalculator.UF.Union(TL, L);
		CircuitCalculator.UF.Union(TL, R);
		CircuitCalculator.UF.Union(TL, TR);
	}
	override public void SetElement()
	{
		//获取元件ID作为元件名称
		int EntityID = CircuitCalculator.EntityNum;
		//获取端口ID并完成内部连接
		int TL, TR, L, R;
		TL = childsPorts[0].PortID_Global;
		TR = childsPorts[1].PortID_Global;
		L = childsPorts[2].PortID_Global;
		R = childsPorts[3].PortID_Global;
		CircuitCalculator.entities.Add(new Resistor(string.Concat(EntityID, "_L"), TL.ToString(), L.ToString(), RL));
		CircuitCalculator.entities.Add(new Resistor(string.Concat(EntityID, "_R"), TL.ToString(), R.ToString(), RR));
		CircuitCalculator.entities.Add(new VoltageSource(string.Concat(EntityID, "_T"), TL.ToString(), TR.ToString(), 0));
		CircuitCalculator.ports.Add(childsPorts[0]);
		CircuitCalculator.ports.Add(childsPorts[1]);
		CircuitCalculator.ports.Add(childsPorts[2]);
		CircuitCalculator.ports.Add(childsPorts[3]);

	}
}
