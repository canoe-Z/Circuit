using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//通用物体
//端口ID
public class NormItem : MonoBehaviour
{
	public int PortNum;//本元件的端口数量
	public CircuitPort[] childsPorts = null;//端口们的引用

    void Start()
    {
		CircuitPort[] disorderPorts = this.gameObject.GetComponentsInChildren<CircuitPort>();//寻找所有挂脚本的子物体
		PortNum = disorderPorts.Length;//元件的端口数量
		childsPorts = new CircuitPort[disorderPorts.Length];//开个数组存引用
		for(int i = 0; i < PortNum; i++)//排个序
		{
			int.TryParse(disorderPorts[i].name, out int ID); //名字转换成ID
			childsPorts[ID] = disorderPorts[i];
			childsPorts[ID].PortID = ID;
			childsPorts[ID].PortID_Global = ID + CircuitCalculator.PortNum;
			childsPorts[ID].father = this;
		}
		CircuitCalculator.PortNum += disorderPorts.Length; //全局ID++

    }

	//物体控制
	private void OnMouseDrag()
	{
		if (!Global.boolMove) return;
		if (Global.Fun.HitCheck("Table", out Vector3 hitPos))
		{
			this.transform.position = hitPos;
		}
		else
		{
			Vector3 campos = Camera.main.transform.position;
			Vector3 thispos = this.gameObject.transform.position;
			float dis = (thispos - campos).magnitude;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			Vector3 vec = ray.direction;
			vec.Normalize();
			vec *= dis;
			thispos = campos + vec;
			this.gameObject.transform.position = thispos;
		}
	}
	private void OnMouseOver()//持续期间
	{
		if (!Global.boolMove) return;
		Global.Other.OverItem(this);
	}
	public void Straighten()//摆正元件
	{
		this.gameObject.transform.eulerAngles = new Vector3(0, 0, 0);
	}
}
