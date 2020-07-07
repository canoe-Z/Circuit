using SpiceSharp.Components;

public class Resistance : EntityBase , INormal
{
	public double Rnum = 120;
	public void Start()
    {
		FindCircuitPort();
		if (double.TryParse(this.gameObject.name, out double Rnum)) //阻值
		{
			this.Rnum = Rnum;
		}
	}

	//电路相关
	public bool IsConnected()//判断是否有一端连接，避免浮动节点
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
	public void LoadElement()
	{
		//获取端口ID并完成并查集连接
		int LeftPortID, RightPortID;
		LeftPortID = childsPorts[0].PortID_Global;
		RightPortID = childsPorts[1].PortID_Global;
		CircuitCalculator.UF.Union(LeftPortID, RightPortID);
	}
	public void SetElement()
	{
		//获取元件ID作为元件名称
		int EntityID = CircuitCalculator.EntityNum;
		//获取端口ID并完成内部连接
		int LeftPortID, RightPortID;
		LeftPortID = childsPorts[0].PortID_Global;
		RightPortID = childsPorts[1].PortID_Global;
		CircuitCalculator.entities.Add(new Resistor(EntityID.ToString(), LeftPortID.ToString(), RightPortID.ToString(), Rnum));
		CircuitCalculator.ports.Add(childsPorts[0]); 
		CircuitCalculator.ports.Add(childsPorts[1]);
	}
}