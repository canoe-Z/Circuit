using UnityEngine;

/// <summary>
/// 3D界面连个导线：先创建空物体，然后挂这个脚本，最后调用连接函数就行了，电路层面：读取两个ID
/// </summary>
public class CircuitLine : MonoBehaviour, ISave
{
	public int StartID_Global { get; set; }    // 端口的全局ID
	public int EndID_Global { get; set; }
	public bool IsActived { get; set; }

	// 对外暴露端口以注入电压
	public CircuitPort StartPort;
	public CircuitPort EndPort;

	public static event EnterEventHandler MouseEnter;
	public static event ExitEventHandler MouseExit;

	void OnMouseEnter()
	{
		if (!MoveController.CanOperate) return;
		MouseEnter?.Invoke(this);
	}

	void OnMouseOver()
	{
		if (!MoveController.CanOperate) return;
		if (Input.GetMouseButtonDown(1))
		{
			DestroyRope();
		}
	}
	void OnMouseExit()
	{
		if (!MoveController.CanOperate) return;
		MouseExit?.Invoke(this);
	}

	void FixedUpdate()
	{
		gameObject.GetComponent<MeshCollider>().sharedMesh = gameObject.GetComponent<MeshFilter>().sharedMesh;
	}

	/// <summary>
	/// 连接导线（记录ID）
	/// </summary>
	/// <param name="Ini">接线柱1</param>
	/// <param name="Lst">接线柱2</param>
	public void CreateLine(GameObject Ini, GameObject Lst)
	{
		StartPort = Ini.GetComponent<CircuitPort>();
		EndPort = Lst.GetComponent<CircuitPort>();
		StartID_Global = Ini.GetComponent<CircuitPort>().ID;
		EndID_Global = Lst.GetComponent<CircuitPort>().ID;
		IsActived = true;
		StartPort.Father.EntityDestroy += DestroyRope;
		EndPort.Father.EntityDestroy += DestroyRope;
		CircuitCalculator.Lines.AddLast(this);
	}

	/// <summary>
	/// 删除连接关系
	/// </summary>
	public void DestroyLine()
	{
		// 从链表中移除
		Debug.LogError("111");
		CircuitCalculator.Lines.Remove(this);
	}

	/// <summary>
	/// 删除导线，包括删除绳子和连接关系
	/// </summary>
	public void DestroyRope()
	{
		StartPort.Father.EntityDestroy -= DestroyRope;
		EndPort.Father.EntityDestroy -= DestroyRope;
		DestroyLine();
		Destroy(gameObject);
	}

	public ILoad Save()
	{
		return new LineData(StartID_Global, EndID_Global);
	}
}

[System.Serializable]
public class LineData: ILoad
{
	public int startID;
	public int endID;
	public LineData(int startID_Global, int endID_Global)
	{
		startID = startID_Global;
		endID = endID_Global;
	}
	public void Load()
	{
		ConnectionManager.ConnectRope(SaveManager.GetItemById(startID, CircuitCalculator.Ports), SaveManager.GetItemById(endID, CircuitCalculator.Ports));
	}
}