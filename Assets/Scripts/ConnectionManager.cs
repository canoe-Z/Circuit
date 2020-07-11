using UnityEngine;
using Obi;
public class ConnectionManager : MonoBehaviour
{
	public static CircuitPort clickedPort = null;
	public static Color[] colors = new Color[5]; //导线颜色配置
	static int colorID = 0;
	static readonly int colorMax = 5;
	private static ObiRopeBlueprint blueprint;
	private static ObiSolver solver = null;
	private static readonly object padlock = new object();//用于确保线程安全
	void Start()
	{
		Physics.IgnoreLayerCollision(0, 8);
	}
	void Update()
	{
		//单击端口以连接导线
		//Tips，0Port连接，1物体拖动，2链子，3滑块，456保留
		if (Input.GetMouseButtonDown(1))//右键清除连接状态
		{
			clickedPort = null;
		}
		if (Input.GetMouseButtonUp(1))//右键抬起时，删除完毕导线，开始计算
		{
			CircuitCalculator.CalculateAll();//删除导线，计算
		}
		if (Input.GetKeyDown(KeyCode.Q)) colorID--; //颜色控制
		if (Input.GetKeyDown(KeyCode.E)) colorID++;
		if (colorID < 0) colorID += colorMax;
		if (colorID >= colorMax) colorID -= colorMax;
		CamMain.ChangeColor(colorID);
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

	public static void ClickPort(CircuitPort port)
	{
		if (clickedPort == null)
		{
			clickedPort = port;
		}
		else
		{
			//连接绳子
			if (clickedPort != port)
			{
				ConnectRope(clickedPort,port);
				CircuitCalculator.CalculateAll();//连接完导线，计算
			}
			//端口不能和自身连接
		}
	}
	public static void ConnectRope(CircuitPort port1, CircuitPort port2)
	{
		GameObject rope = CreateRope(port1.gameObject, port2.gameObject, GetSolver());
		rope.layer = 8; //关闭碰撞检测
		rope.AddComponent<MeshCollider>();
		if(rope.GetComponent<MeshCollider>().sharedMesh != rope.GetComponent<MeshFilter>().sharedMesh)
		{
			Debug.LogError("绳子碰撞体连接时有问题");
		}
		var RopeMat = Resources.Load<Material>("Button");
		rope.GetComponent<MeshRenderer>().material = RopeMat;
		rope.GetComponent<MeshRenderer>().material.color = colors[colorID];
		rope.AddComponent<CircuitLine>().CreateLine(port1.gameObject, port2.gameObject);
		clickedPort = null;
	}
	public static ObiSolver CreateSolver()
	{
		GameObject solverObject = new GameObject("RopeSolver", typeof(ObiSolver), typeof(ObiFixedUpdater));
		ObiSolver solver = solverObject.GetComponent<ObiSolver>();
		ObiFixedUpdater updater = solverObject.GetComponent<ObiFixedUpdater>();
		updater.solvers.Add(solver);
		return solver;
	}
	public static GameObject CreateRope(GameObject obj1, GameObject obj2, ObiSolver solver)
	{
		GameObject ropeObject = new GameObject("Rope", typeof(ObiRope), typeof(ObiRopeExtrudedRenderer));
		ObiRope rope = ropeObject.GetComponent<ObiRope>();
		ObiRopeExtrudedRenderer ropeRenderer = ropeObject.GetComponent<ObiRopeExtrudedRenderer>();
		ropeRenderer.section = Resources.Load<ObiRopeSection>("DefaultRopeSection");
		blueprint = ScriptableObject.CreateInstance<ObiRopeBlueprint>();

		blueprint.thickness = 0.025f;
		blueprint.resolution = 0.2f;

		//实现导线高出接线柱一段距离
		var pos1 = obj1.transform.TransformPoint(new Vector3(0, 0.1f, 0));
		var pos2 = obj2.transform.TransformPoint(new Vector3(0, 0.1f, 0));

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