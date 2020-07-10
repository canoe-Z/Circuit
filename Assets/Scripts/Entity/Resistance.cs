using SpiceSharp.Components;

public class Resistance : EntityBase
{
	public double Rnum = 120;
	public int LeftPortID, RightPortID;
	public void Start()
    {
		FindCircuitPort();
		LeftPortID = childsPorts[0].PortID_Global;
		RightPortID = childsPorts[1].PortID_Global;
		if (double.TryParse(this.gameObject.name, out double Rnum)) //阻值
		{
			this.Rnum = Rnum;
		}
	}

	//电路相关
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
		CircuitCalculator.UF.Union(LeftPortID, RightPortID);
	}
	override public void SetElement()
	{
		//获取元件ID作为元件名称
		int EntityID = CircuitCalculator.EntityNum;
		CircuitCalculator.entities.Add(new Resistor(EntityID.ToString(), LeftPortID.ToString(), RightPortID.ToString(), Rnum));
		/*
		CircuitCalculator.ports.Add(childsPorts[0]); 
		CircuitCalculator.ports.Add(childsPorts[1]);
		*/
	}
}