using Obi;
using UnityEngine;

/// <summary>
/// 3D界面连个导线：先创建空物体，然后挂这个脚本，最后调用连接函数就行了，电路层面：读取两个ID
/// </summary>
public class CircuitLine : MonoBehaviour
{
	public int StartID { get; set; }    // 端口的全局ID
	public int EndID { get; set; }
	public bool IsActived { get; set; }

	// 对外暴露端口以注入电压
	public CircuitPort StartPort { get; set; }
	public CircuitPort EndPort { get; set; }

	public static event EnterEventHandler MouseEnter;
	public static event ExitEventHandler MouseExit;

	void OnMouseEnter()
	{
		if (!MoveController.CanOperate) return;
		MouseEnter?.Invoke(this);
	}

	void OnMouseExit()
	{
		if (!MoveController.CanOperate) return;
		MouseExit?.Invoke(this);
	}

	void Update()
	{
		if (!MoveController.CanOperate) return;

		if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
		{
			Debug.LogError("起飞");

			// 鼠标操作时更新碰撞体
			gameObject.GetComponent<MeshCollider>().sharedMesh = gameObject.GetComponent<MeshFilter>().sharedMesh;
			gameObject.GetComponent<MeshCollider>().convex = true;

			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray, out RaycastHit hitInfo))
			{
				GameObject gameObj = hitInfo.collider.gameObject;
				if (gameObj.name == "Rope")
				{
					if (Input.GetMouseButtonDown(1))
					{
						DestroyRope();
					}
				}
			}
		}
	}

	public ObiSolver solver;
	public float radiusScale = 1;
	public int particleIndex;
	public Vector3 worldPosition;
	private Vector3 lastMousePos = Vector3.zero;
	private int pickedParticleIndex = -1;
	private float pickedParticleDepth = 0;


	void LateUpdate()
	{
		solver = ConnectionManager.GetSolver();
		// Click:
		if (Input.GetMouseButtonDown(0))
		{

			pickedParticleIndex = -1;

			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

			float closestMu = float.MaxValue;
			float closestDistance = float.MaxValue;

			// Find the closest particle hit by the ray:
			for (int i = 0; i < solver.renderablePositions.count; ++i)
			{

				Vector3 projected = ObiUtils.ProjectPointLine(solver.renderablePositions[i], ray.origin, ray.origin + ray.direction, out float mu, false);
				float distanceToRay = Vector3.SqrMagnitude((Vector3)solver.renderablePositions[i] - projected);

				// Disregard particles behind the camera:
				mu = Mathf.Max(0, mu);

				float radius = solver.principalRadii[i][0] * radiusScale;

				if (distanceToRay <= radius * radius && distanceToRay < closestDistance && mu < closestMu)
				{
					closestMu = mu;
					closestDistance = distanceToRay;
					pickedParticleIndex = i;
				}
			}

			if (pickedParticleIndex >= 0)
			{

				pickedParticleDepth = Camera.main.transform.InverseTransformVector((Vector3)solver.renderablePositions[pickedParticleIndex] - Camera.main.transform.position).z;


				Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, pickedParticleDepth));
				//Debug.LogError("111111");
			}

		}
		lastMousePos = Input.mousePosition;
	}

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

		CircuitCalculator.Lines.AddLast(this);
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