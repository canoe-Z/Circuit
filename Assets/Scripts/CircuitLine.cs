using UnityEngine;

/// <summary>
/// 导线
/// </summary>
public class CircuitLine : MonoBehaviour
{
	// 端口的全局ID
	public int StartID { get; set; }
	public int EndID { get; set; }
	public bool IsActived { get; set; }
	public static bool IsEmission { get; set; } = false;

	// 对外暴露端口以注入电压
	public CircuitPort StartPort { get; set; }
	public CircuitPort EndPort { get; set; }

	/// <summary>
	/// 连接导线（记录ID）
	/// </summary>
	/// <param name="port1">接线柱1</param>
	/// <param name="port2">接线柱2</param>
	public void CreateLine(GameObject port1, GameObject port2)
	{
		StartPort = port1.GetComponent<CircuitPort>();
		EndPort = port2.GetComponent<CircuitPort>();

		StartID = StartPort.ID;
		EndID = EndPort.ID;

		IsActived = true;

		// 删除元件时，删除端口上关联的导线
		StartPort.Father.EntityDestroy += DestroyRope;
		EndPort.Father.EntityDestroy += DestroyRope;

		// 鼠标悬停于接线柱时，该接线柱上的导线边缘发光

		CircuitCalculator.Lines.AddLast(this);
	}

	void Update()
	{
		if(IsEmission)
		{
			GetComponent<MeshCollider>().sharedMesh = GetComponent<MeshFilter>().sharedMesh;
		}
	}

	void OnMouseEnter()
	{
		if (IsEmission)
		{
			transform.EnableFresnel(Color.blue);
		}
	}

	void OnMouseExit()
	{
		if (IsEmission)
		{
			transform.DisablFresnel();
		}
	}

	/// <summary>
	/// 删除导线，包括删除绳子和连接关系
	/// </summary>
	public void DestroyRope()
	{
		StartPort.Father.EntityDestroy -= DestroyRope;
		EndPort.Father.EntityDestroy -= DestroyRope;

		CircuitCalculator.Lines.Remove(this);

		CircuitCalculator.NeedCalculate = true;

		Destroy(gameObject);
	}

	public LineData Save() => new LineData(StartID, EndID);
}

/// <summary>
/// 导线存档数据
/// </summary>
[System.Serializable]
public class LineData
{
	private readonly int startID;
	private readonly int endID;

	public LineData(int startID, int endID)
	{
		this.startID = startID;
		this.endID = endID;
	}

	public void Load()
	{
		ConnectionManager.ConnectRope(SaveManager.GetItemById(startID, CircuitCalculator.Ports), SaveManager.GetItemById(endID, CircuitCalculator.Ports));
	}
}