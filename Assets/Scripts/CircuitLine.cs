using UnityEngine;
//
//3D界面连个导线：先创建空物体，然后挂这个脚本，最后调用连接函数就行了
//电路层面：只要读取两个ID就行了
//
public class CircuitLine : MonoBehaviour
{
	public int startID_Global;//端口的全局ID
	public int endID_Global;

	//对外暴露端口以注入电压
	public CircuitPort startPort;
	public CircuitPort endPort;

	//这函数只需要调用1次
	public void CreateLine(GameObject Ini, GameObject Lst)
	{
		startPort = Ini.GetComponent<CircuitPort>();
		endPort = Lst.GetComponent<CircuitPort>();
		startPort.Connected = 1;
		endPort.Connected = 1;
		startID_Global = Ini.GetComponent<CircuitPort>().PortID_Global;
		endID_Global = Lst.GetComponent<CircuitPort>().PortID_Global;
		CircuitCalculator.allLines.Add(this);
	}
	public void DestroyLine()
	{
		startPort.Connected = 0;
		endPort.Connected = 0;
		CircuitCalculator.allLines.Remove(this);
	}
	public void DisableLine()
	{
		startPort.Connected = 0;
		endPort.Connected = 0;
	}
	public void ReenableLine()
	{
		startPort.Connected = 1;
		endPort.Connected = 1;
	}
}
