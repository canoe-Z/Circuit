using System.Collections.Generic;
using UnityEngine;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using SpiceSharp.Circuits;

/// <summary>
/// 电路计算
/// </summary>
public class CircuitCalculator : MonoBehaviour
{
	public static int PortNum = 0;//端口总数，创建端口时++
	public static int EntityNum = 0;//元件总数，创建元件时++
	public static List<Entity> entities = new List<Entity>();//元件
	public static List<CircuitPort> ports = new List<CircuitPort>();//已连接元件的端口ID(用于端口电压检测序列)
	public static WeightedQuickUnionUF UF = new WeightedQuickUnionUF(10000);//并查集

	public static List<CircuitLine> ProblemLine = new List<CircuitLine>();//问题导线
	public static List<CircuitLine> GoodLine = new List<CircuitLine>();//正常导线
	public static List<GNDLine> gndLines = new List<GNDLine>();//接地导线

	public static List<ISource> sources = new List<ISource>();
	public static List<IAmmeter> ammeters = new List<IAmmeter>();

	public static List<EntityBase> allEntity = new List<EntityBase>();
	public static List<CircuitLine> allLine = new List<CircuitLine>();

	private void Start()
	{
		//寻找场景中的初始元件，加入到allEntity中去
		var allEntityArray = FindObjectsOfType<EntityBase>();
		for (int i = 0; i < allEntityArray.Length; i++)
		{
			allEntity.Add(allEntityArray[i]);
		}
	}

	/// <summary>
	/// 清除所有计算记录和连接关系
	/// </summary>
	public static void ClearAll()
	{
		EntityNum = 0;
		gndLines.Clear();
		GNDLine.GlobalGNDLineID = 0;
		ports.Clear();
		entities.Clear();
		GoodLine.Clear();
		ProblemLine.Clear();
		sources.Clear();
		ammeters.Clear();
	}

	/// <summary>
	/// 电路重新计算
	/// </summary>
	public static void CalculateAll()
	{
		ClearAll();
		//在并查集中加载有连接元件的内部连接
		LoadElement();

		//在并查集中连接导线
		for (int i = 0; i < allLine.Count; i++)
		{
			UF.Union(allLine[i].startID_Global, allLine[i].endID_Global);
		}

		//对电源实行接地检测
		foreach (ISource i in sources)
		{
			i.GroundCheck();
		}

		ConnectGND();

		//检查对地连通性，区分出问题导线，并分别处理
		for (int i = 0; i < allLine.Count; i++)
		{
			if (UF.Connected(allLine[i].startID_Global, 0))
			{
				GoodLine.Add(allLine[i]);
				entities.Add(new VoltageSource(string.Concat("Line", "_", i), allLine[i].startID_Global.ToString(), allLine[i].endID_Global.ToString(), 0));
				EntityNum++;
			}
			else
			{
				ProblemLine.Add(allLine[i]);
				allLine[i].DestroyLine();
			}
		}

		//在SpiceSharp中加载元件
		SetElement();
		SpiceSharpCalculate();

		//仿真通过后恢复通过并查集删除的导线
		foreach (CircuitLine i in ProblemLine)
		{
			i.ReLine();
		}
	}

	/// <summary>
	/// 在现有的连接关系上重新计算
	/// </summary>
	public static void CalculateByConnection()
	{
		EntityNum = 0;
		GNDLine.GlobalGNDLineID = 0;
		ports.Clear();
		entities.Clear();
		ConnectGND();
		//直接连接正常导线
		for (int i = 0; i < GoodLine.Count; i++)
		{
			entities.Add(new VoltageSource(string.Concat("Line", "_", i), GoodLine[i].startID_Global.ToString(), GoodLine[i].endID_Global.ToString(), 0));
			EntityNum++;
		}
		SetElement();
		SpiceSharpCalculate();
	}

	/// <summary>
	/// 通过并查集预连接，同时按照加载不同的接口
	/// </summary>
	private static void LoadElement()
	{
		UF.Clear(10000);
		for (int i = 0; i < allEntity.Count; i++)
		{
			if (allEntity[i].IsConnected())
			{
				allEntity[i].LoadElement();
			}
			if (allEntity[i] is ISource)
			{
				sources.Add(allEntity[i] as ISource);
			}
			if (allEntity[i] is IAmmeter)
			{
				ammeters.Add(allEntity[i] as IAmmeter);
			}
		}
	}

	/// <summary>
	/// 正式连接（需要事先处理不接地连接）
	/// </summary>
	private static void SetElement()
	{
		EntityNum = 0;
		for (int i = 0; i < allEntity.Count; i++)
		{
			if (allEntity[i].IsConnected())
			{
				allEntity[i].SetElement();
				EntityNum++;
			}
		}
	}

	/// <summary>
	/// 将需要接地的电源接地
	/// </summary>
	private static void ConnectGND()
	{
		foreach (GNDLine i in gndLines)
		{
			entities.Add(new VoltageSource(string.Concat(i.GNDLineID.ToString(), "_GND"), i.PortToGND.ToString(), "0", 0));
		}
	}

	
	/// <summary>
	/// 调用SpiceSharp进行计算
	/// </summary>
	private static void SpiceSharpCalculate()
	{
		//创建电路
		var ckt = new Circuit(entities);

		//直流工作点分析并写入端口电压值
		var op = new OP("op");
		op.ExportSimulationData += (sender, exportDataEventArgs) =>
		{
			foreach (CircuitPort i in ports)
			{
				i.U = exportDataEventArgs.GetVoltage(i.PortID_Global.ToString());
			}
		};

		// 启动仿真
		try
		{
			op.Run(ckt);
			Debug.Log("仿真成功");
		}
		catch
		{
			Debug.Log("悬空导线的数目为：" + ProblemLine.Count);
			Debug.LogWarning("仿真失败，电路无通路");
		}

		//计算电流
		foreach (IAmmeter i in ammeters)
		{
			i.CalculateCurrent();
		}
	}
}

/// <summary>
/// 接地连接
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
/// 并查集
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

	public bool Connected(int p, int q)
	{
		return Find(p) == Find(q);
	}

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
}