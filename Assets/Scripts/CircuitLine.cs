using UnityEngine;

//
// 3D界面连个导线：先创建空物体，然后挂这个脚本，最后调用连接函数就行了
// 电路层面：只要读取两个ID就行了
//
public class CircuitLine : MonoBehaviour
{
	public int StartID_Global { get; set; }    // 端口的全局ID
	public int EndID_Global { get; set; }
	public bool IsActived { get; set; }

	// 对外暴露端口以注入电压
	public CircuitPort StartPort { get; set; }
	public CircuitPort EndPort { get; set; }

	// 这函数只需要调用1次
	public void CreateLine(GameObject Ini, GameObject Lst)
	{
		StartPort = Ini.GetComponent<CircuitPort>();
		EndPort = Lst.GetComponent<CircuitPort>();
		//StartPort.Connected = 1;
		//EndPort.Connected = 1;
		StartID_Global = Ini.GetComponent<CircuitPort>().PortID_Global;
		EndID_Global = Lst.GetComponent<CircuitPort>().PortID_Global;
		IsActived = true;
		CircuitCalculator.AllLines.AddLast(this);
	}

	public void DestroyLine()
	{
		CircuitCalculator.AllLines.Remove(this);
	}

	public void DestroyRope()
	{
		DestroyLine();
		Destroy(gameObject);
	}

	private void OnMouseOver()
	{
		ShowTip.OverChain();
		ShowTip.IsTipShowed = false;
		if (Input.GetMouseButtonDown(1))
		{
			DestroyRope();
		}
	}

	// Update is called once per frame
	void FixedUpdate()
	{
		gameObject.GetComponent<MeshCollider>().sharedMesh = gameObject.GetComponent<MeshFilter>().sharedMesh;
	}
}
