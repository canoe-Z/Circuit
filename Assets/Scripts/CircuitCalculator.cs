using System.Collections.Generic;
using UnityEngine;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using SpiceSharp.Circuits;
using System.Collections;

/// <summary>
/// 电路计算
/// </summary>
public class CircuitCalculator : MonoBehaviour
{
	public static int PortNum { get; set; } = 1;                                                // 端口总数，创建端口时++
	public static int EntityNum { get; set; } = 0;                                              // 元件总数，创建元件时++
	public static List<Entity> SpiceEntities { get; set; } = new List<Entity>();                // SpiceSharp计算的元件
	public static List<CircuitPort> SpicePorts { get; set; } = new List<CircuitPort>();         // SpiceSharp计算的端口
	public static WeightedQuickUnionUF UF { get; set; } = new WeightedQuickUnionUF(10000);      // 并查集，用于接地判断
	public static WeightedQuickUnionUF LineUF { get; set; } = new WeightedQuickUnionUF(10000);  // 并查集，用于接地判断

	public static List<CircuitLine> DisabledLines { get; set; } = new List<CircuitLine>();      // 问题导线
	public static List<CircuitLine> EnabledLines { get; set; } = new List<CircuitLine>();       // 被禁用的导线
	public static List<GNDLine> GNDLines { get; set; } = new List<GNDLine>();                   // 接地导线

	public static List<ISource> Sources { get; set; } = new List<ISource>();                    // 所有元件

	public static LinkedList<EntityBase> Entities { get; set; } = new LinkedList<EntityBase>();
	public static LinkedList<CircuitPort> Ports { get; set; } = new LinkedList<CircuitPort>();
	public static LinkedList<CircuitLine> Lines { get; set; } = new LinkedList<CircuitLine>();

	public static bool NeedCalculate { get; set; } = false;
	public static bool NeedCalculateByConnection { get; set; } = false;

	public delegate void CalculateEventHandler();
	public static event CalculateEventHandler CalculateEvent;

	void Awake()
	{
		// 开启计算协程
		StartCoroutine(CalculateUpdate());
	}

	private IEnumerator CalculateUpdate()
	{
		while (true)
		{
			// 需要全算则不再部分算
			if (NeedCalculate)
			{
				CalculateAll();
				CalculateEvent?.Invoke();
				NeedCalculate = false;
				NeedCalculateByConnection = false;
			}
			else if (NeedCalculateByConnection)
			{
				CalculateByConnection();
				CalculateEvent?.Invoke();
				NeedCalculate = false;
				NeedCalculateByConnection = false;
			}
			yield return null; //下一帧再次调用，yield return null的执行时机在Update()之后
		}
	}

	/// <summary>
	/// 清除所有计算记录和连接关系
	/// </summary>
	public static void ClearAll()
	{
		GNDLines.Clear();
		GNDLine.GlobalGNDLineID = 0;

		SpicePorts.Clear();
		SpiceEntities.Clear();
		EnabledLines.Clear();
		DisabledLines.Clear();
		Sources.Clear();

		UF.Clear(10000);
		LineUF.Clear(10000);
	}

	/// <summary>
	/// 电路重新计算
	/// </summary>
	private static void CalculateAll()
	{
		ClearAll();

		// 在并查集中连接导线
		// 首先需要清空所有端口的连接状态
		foreach (CircuitPort port in Ports)
		{
			port.IsConnected = false;
		}

		// 冗余检测：通过并查集判断，对于有效的导线，更新端口连接状态，对于冗余导线则要禁用
		foreach (CircuitLine line in Lines)
		{
			if (LineUF.Connected(line.StartID, line.EndID))
			{
				Debug.Log("导线因冗余被禁用");
				line.IsActived = false;
				DisabledLines.Add(line);
			}
			else
			{
				LineUF.Union(line.StartID, line.EndID);
				UF.Union(line.StartID, line.EndID);

				// 对于第一次检测有效的导线，激活有连接的元件
				line.StartPort.IsConnected = true;
				line.EndPort.IsConnected = true;
			}
		}

		// 在并查集中加载激活元件的内部连接
		// 注意：这里被激活的元件此后还可能被禁用，区分的目的仅是为了缩小并查集规模，这里也可以选择不判断激活状态，直接加载所有元件的内部连接
		LoadElement(Entities);

		// 对有连接的电源实行接地检测，只涉及并查集操作
		Sources.ForEach(x => x.GroundCheck());

		// 连接接地线，只涉及并查集操作
		ConnectGND(GNDLines);

		// 再次清空所有端口的连接状态
		// 注意：此前的有效导线未经接地检测，不经此步骤，会导致孤立元件内部连接出不接地的导线，导致仿真错误
		foreach (CircuitPort port in Ports)
		{
			port.IsConnected = false;
			port.U = 0;
			port.I = 0;
		}

		// 接地检测：检查对地连通性，对于正确连通的导线则连接，更新连接状态，对于不通的导线需要禁用
		foreach (var (line, index) in Lines.WithIndex())
		{
			if (line.IsActived)
			{
				if (UF.Connected(line.StartID, 0))
				{
					// 对于两次检测均有效的导线，激活有连接的元件
					line.StartPort.IsConnected = true;
					line.EndPort.IsConnected = true;

					EnabledLines.Add(line);
					SpiceEntities.Add(new VoltageSource(string.Concat("Line", "_", index), line.StartID.ToString(), line.EndID.ToString(), 0));
					EntityNum++;
				}
				else
				{
					Debug.Log("导线因不接地被禁用");
					line.IsActived = false;
					DisabledLines.Add(line);
				}
			}
		}

		// 在SpiceSharp中加载元件
		SetElement(Entities);
		SpiceSharpCalculate(SpiceEntities);

		// 仿真通过后恢复通过并查集禁用的导线
		DisabledLines.ForEach(x => x.IsActived = true);
	} // public static void CalculateAll()

	/// <summary>
	/// 在现有的连接关系上重新计算
	/// </summary>
	private static void CalculateByConnection()
	{
		GNDLine.GlobalGNDLineID = 0;
		SpicePorts.Clear();
		SpiceEntities.Clear();

		ConnectGND(GNDLines);

		// 直接连接正常导线
		for (var i = 0; i < EnabledLines.Count; i++)
		{
			SpiceEntities.Add(new VoltageSource(string.Concat("Line", "_", i), EnabledLines[i].StartID.ToString(), EnabledLines[i].EndID.ToString(), 0));
			EntityNum++;
		}
		SetElement(Entities);
		SpiceSharpCalculate(SpiceEntities);
	}

	/// <summary>
	/// 通过并查集预连接，同时按照加载不同的接口
	/// </summary>
	private static void LoadElement(LinkedList<EntityBase> entities)
	{
		foreach (EntityBase entity in entities)
		{
			if (entity.IsConnected())
			{
				entity.LoadElement();
			}
			if (entity is ISource source)
			{
				Sources.Add(source);
			}
		}
	}

	/// <summary>
	/// 正式连接（需要事先处理不接地连接）
	/// </summary>
	private static void SetElement(LinkedList<EntityBase> entities)
	{
		EntityNum = 0;
		foreach (EntityBase entity in entities)
		{
			if (entity.IsConnected())
			{
				entity.SetElement(EntityNum);
				EntityNum++;
			}
		}
	}

	/// <summary>
	/// 将需要接地的电源接地
	/// </summary>
	private static void ConnectGND(List<GNDLine> GNDLines)
	{
		foreach (GNDLine i in GNDLines)
		{
			SpiceEntities.Add(new VoltageSource(string.Concat(i.GNDLineID.ToString(), "_GND"), i.PortToGND.ToString(), "0", 0));
		}
	}

	/// <summary>
	/// 调用SpiceSharp进行计算
	/// </summary>
	private static void SpiceSharpCalculate(List<Entity> spiceEntities)
	{
		// 创建电路
		var ckt = new Circuit(spiceEntities);

		// 直流工作点分析并写入端口电压值
		var op = new OP("op");
		op.ExportSimulationData += (sender, exportDataEventArgs) =>
		{
			foreach (CircuitPort i in SpicePorts)
			{
				i.U = exportDataEventArgs.GetVoltage(i.ID.ToString());
			}
		};

		// 启动仿真
		try
		{
			op.Run(ckt);
			Debug.Log("被禁用的导线数目为：" + DisabledLines.Count);
			Debug.Log("仿真成功");
		}
		catch
		{
			Debug.Log("被禁用的导线数目为：" + DisabledLines.Count);
			if (spiceEntities.Count == 0)
			{
				Debug.Log("尚未连接电路");
			}
			else
			{
				Debug.LogError(spiceEntities.Count.ToString());
				Debug.LogError("仿真错误！");
			}
		}
	} // private static void SpiceSharpCalculate()
}

/// <summary>
/// 接地导线类，作用是记录需要接地的端口ID
/// </summary>
public class GNDLine
{
	public static int GlobalGNDLineID = 0;
	public int GNDLineID;
	public int PortToGND;
	public GNDLine(int portToGND)
	{
		GNDLineID = GlobalGNDLineID;
		GlobalGNDLineID++;
		PortToGND = portToGND;
	}
}

/// <summary>
/// 并查集数据结构的实现
/// </summary>
public class WeightedQuickUnionUF
{
	private int[] id;
	private int[] sz; // 各根结点对应的分量大小

	public WeightedQuickUnionUF(int N)
	{
		id = new int[N];
		for (int i = 0; i < N; i++)
			id[i] = i;

		sz = new int[N];
		for (int i = 0; i < N; i++)
			sz[i] = 1;
	}

	public void Clear(int N)
	{
		id = new int[N];
		for (int i = 0; i < N; i++)
			id[i] = i;

		sz = new int[N];
		for (int i = 0; i < N; i++)
			sz[i] = 1;
	}

	public int Find(int i)
	{
		while (i != id[i])
		{
			id[i] = id[id[i]];
			i = id[i];
		}
		return i;
	}

	public bool Connected(int p, int q) => Find(p) == Find(q);

	public void Union(int p, int q)
	{
		int pRoot = Find(p);
		int qRoot = Find(q);

		if (pRoot == qRoot)
			return;

		if (sz[pRoot] < sz[qRoot])
		{
			id[pRoot] = qRoot;
			sz[qRoot] += sz[pRoot];
		}
		else
		{
			id[qRoot] = pRoot;
			sz[pRoot] = sz[qRoot];
		}
	}

	public void ListUnion(List<(int, int)> list) => list.ForEach(x => Union(x.Item1, x.Item2));
} // public class WeightedQuickUnionUF