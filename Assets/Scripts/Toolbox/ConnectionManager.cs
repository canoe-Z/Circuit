using UnityEngine;
using Obi;

/// <summary>
/// 绳子与导线连接
/// </summary>
public class ConnectionManager : MonoBehaviour
{
	static CircuitPort clickedPort = null;//可能存在的上一个导线

	// 导线颜色配置

	// Obi
	private static ObiRopeBlueprint blueprint;
	private static ObiSolver solver = null;

	// 用于确保线程安全
	private static readonly object padlock = new object();

	void Awake()
	{
		Physics.IgnoreLayerCollision(0, 8);
	}

	void Update()
	{
		if (clickedPort)
		{
			DisplayController.myOperateTipsToShow += "点击下一个接线柱连接导线，单击右键取消本次连接\n";
		}
		// 右键清除连接状态
		if (Input.GetMouseButtonDown(1))
		{
			clickedPort = null;
		}
	}

	/// <summary>
	/// solver采用单例模式
	/// </summary>
	/// <returns>单例solver</returns>
	public static ObiSolver GetSolver()
	{
		lock (padlock)
		{
			if (solver == null)
			{
				solver = CreateSolver();
			}
		}
		return solver;
	}

	/// <summary>
	/// 鼠标点击端口
	/// </summary>
	/// <param name="port">被点击的端口</param>
	public static void ClickPort(CircuitPort port)
	{
		if (clickedPort == null)
		{
			clickedPort = port;
		}
		else
		{
			if (clickedPort != port)
			{
				ConnectRope(clickedPort, port);
				clickedPort = null;
				CircuitCalculator.NeedCalculate = true;
			}
		}
	}

	/// <summary>
	/// 连接导线
	/// </summary>
	/// <param name="port1">接线柱1</param>
	/// <param name="port2">接线柱2</param>
	public static void ConnectRope(CircuitPort port1, CircuitPort port2)
	{
		GameObject rope = CreateRope(port1.gameObject, port2.gameObject, GetSolver());
		ObiParticlePicker picker = rope.AddComponent<ObiParticlePicker>();
		picker.solver = GetSolver();

		// 关闭碰撞检测
		rope.layer = 8;
		rope.AddComponent<MeshCollider>();
		Material RopeMat = Resources.Load<Material>("Rope");

		// 使用MyShader以实现边缘发光
		rope.GetComponent<MeshRenderer>().material = RopeMat;
		rope.GetComponent<MeshRenderer>().material.SetColor("Color_51411BA8", DisplayController.MyColorReal);

		rope.AddComponent<CircuitLine>().CreateLine(port1.gameObject, port2.gameObject);
	}

	/// <summary>
	/// 创建Obi解析器
	/// </summary>
	/// <returns>解析器</returns>
	public static ObiSolver CreateSolver()
	{
		GameObject solverObject = new GameObject("RopeSolver", typeof(ObiSolver), typeof(ObiFixedUpdater));
		ObiSolver solver = solverObject.GetComponent<ObiSolver>();
		ObiFixedUpdater updater = solverObject.GetComponent<ObiFixedUpdater>();
		updater.solvers.Add(solver);
		return solver;
	}

	/// <summary>
	/// 创建绳子
	/// </summary>
	/// <param name="obj1">需要连接的物体1</param>
	/// <param name="obj2">需要连接的物体2</param>
	/// <param name="solver">使用的解析器</param>
	/// <returns>返回绳子实体</returns>
	public static GameObject CreateRope(GameObject obj1, GameObject obj2, ObiSolver solver)
	{
		GameObject ropeObject = new GameObject("Rope", typeof(ObiRope), typeof(ObiRopeExtrudedRenderer));
		ObiRope rope = ropeObject.GetComponent<ObiRope>();
		ObiRopeExtrudedRenderer ropeRenderer = ropeObject.GetComponent<ObiRopeExtrudedRenderer>();
		ropeRenderer.section = Resources.Load<ObiRopeSection>("DefaultRopeSection");
		blueprint = ScriptableObject.CreateInstance<ObiRopeBlueprint>();

		blueprint.thickness = 0.025f;
		blueprint.resolution = 0.2f;

		// 实现导线高出接线柱一段距离
		Vector3 pos1 = obj1.transform.TransformPoint(new Vector3(0, 0.1f, 0));
		Vector3 pos2 = obj2.transform.TransformPoint(new Vector3(0, 0.1f, 0));

		blueprint.path.Clear();
		blueprint.path.AddControlPoint(pos1, -Vector3.right, Vector3.right, Vector3.up, 0.1f, 0.1f, 1, 1, Color.white, "start");
		blueprint.path.AddControlPoint(pos2, -Vector3.right, Vector3.right, Vector3.up, 0.1f, 0.1f, 1, 1, Color.white, "end");
		blueprint.path.FlushEvents();
		blueprint.Generate();

		rope.ropeBlueprint = blueprint;
		rope.GetComponent<MeshRenderer>().enabled = true;

		ObiParticleAttachment ParticleAttachment1 = ropeObject.AddComponent<ObiParticleAttachment>();
		ObiParticleAttachment ParticleAttachment2 = ropeObject.AddComponent<ObiParticleAttachment>();
		ParticleAttachment1.target = obj1.transform;
		ParticleAttachment2.target = obj2.transform;
		ParticleAttachment1.particleGroup = blueprint.groups[0];
		ParticleAttachment2.particleGroup = blueprint.groups[1];

		rope.transform.parent = solver.transform;
		return ropeObject;
	}
}