using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//端口
public class CircuitPort : MonoBehaviour
{
	public int Connected = 0;//是否连接
	public double U = 0;//电压探针
	public double I = 0;//流出接线柱的电流
	public int PortID;//本接线柱ID
	public int PortID_Global;//本接线柱ID_全局
	public EntityBase father;
	private void OnMouseDown()
	{
		if (!Global.boolMove) return;
		Global.Other.ClickPort(this);
	}
	private void OnMouseEnter()
	{
		if (!Global.boolMove) return;
		this.gameObject.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
	}
	private void OnMouseOver()//持续期间
	{
		if (!Global.boolMove) return;
		Global.Other.OverPort(this);
	}
	private void OnMouseExit()
	{
		if (!Global.boolMove) return;
		this.gameObject.transform.localScale = new Vector3(1, 1, 1);
	}
}
