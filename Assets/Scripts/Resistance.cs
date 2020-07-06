using System.Collections.Generic;
using UnityEngine;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;

public class Resistance : MonoBehaviour
{
	public NormItem bodyItem;
	public double Rnum = 120;
    // Start is called before the first frame update
    void Start()
    {
		if (double.TryParse(this.gameObject.name, out double Rnum)) //阻值
		{
			this.Rnum = Rnum;
		}
		bodyItem = this.gameObject.GetComponent<NormItem>();
	}

	//电路相关
	public bool IsConnected()//判断是否有一端连接，避免浮动节点
	{
		if (bodyItem.childsPorts[0].Connected == 1 || bodyItem.childsPorts[1].Connected == 1)
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
		LeftPortID = bodyItem.childsPorts[0].PortID_Global;
		RightPortID = bodyItem.childsPorts[1].PortID_Global;
		Global.MyCircuit.UF.Union(LeftPortID, RightPortID);
	}
	public void SetElement()
	{
		//获取元件ID作为元件名称
		int EntityID = Global.MyCircuit.EntityNum;
		//获取端口ID并完成内部连接
		int LeftPortID, RightPortID;
		LeftPortID = bodyItem.childsPorts[0].PortID_Global;
		RightPortID = bodyItem.childsPorts[1].PortID_Global;
		Global.MyCircuit.entities.Add(new Resistor(EntityID.ToString(), LeftPortID.ToString(), RightPortID.ToString(), Rnum));
		Global.MyCircuit.ports.Add(bodyItem.childsPorts[0]); 
		Global.MyCircuit.ports.Add(bodyItem.childsPorts[1]);
	}
}