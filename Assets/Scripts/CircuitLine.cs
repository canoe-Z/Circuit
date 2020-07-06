using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//
//3D界面连个导线：先创建空物体，然后挂这个脚本，最后调用连接函数就行了
//电路层面：只要读取两个ID就行了
//
public class CircuitLine : MonoBehaviour
{
	public int startID_Global;//端口的全局ID
	public int endID_Global;

	private GameObject pStart;//端口
	private GameObject pEnd;//端口
	//private GameObject[] chains = null;

	//对外暴露端口以注入电压
	public CircuitPort StartPort;
	public CircuitPort EndPort;

	//这函数只需要调用1次
	public void CreateLine(GameObject Ini, GameObject Lst)
	{
		pStart = Ini;
		pEnd = Lst;
		StartPort = pStart.GetComponent<CircuitPort>();
		EndPort = pEnd.GetComponent<CircuitPort>();
		StartPort.Connected = 1;
		EndPort.Connected = 1;
		startID_Global = pStart.GetComponent<CircuitPort>().PortID_Global;
		endID_Global = pEnd.GetComponent<CircuitPort>().PortID_Global;
	}

	public void DestroyLine()
	{
		StartPort.Connected = 0;
		EndPort.Connected = 0;
	}
	public void ReLine()
	{
		StartPort.Connected = 1;
		EndPort.Connected = 1;
	}
}
