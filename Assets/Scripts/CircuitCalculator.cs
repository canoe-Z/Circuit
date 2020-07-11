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
	public static int PortNum { get; set; } = 0;												//端口总数，创建端口时++
	public static int EntityNum { get; set; } = 0;												//元件总数，创建元件时++
	public static List<Entity> SpiceEntities { get; set; } = new List<Entity>();				//SpiceSharp计算的元件
	public static List<CircuitPort> SpicePorts { get; set; } = new List<CircuitPort>();         //SpiceSharp计算的端口
	public static WeightedQuickUnionUF UF { get; set; } = new WeightedQuickUnionUF(10000);		//并查集，用于接地判断

	public static List<CircuitLine> DisabledLines { get; set; } = new List<CircuitLine>();		//问题导线
	public static List<CircuitLine> EnabledLines { get; set; } = new List<CircuitLine>();		//正常导线
	public static List<GNDLine> GNDLines { get; set; } = new List<GNDLine>();					//接地导线

	public static List<ISource> Sources { get; set; } = new List<ISource>();					//所有元件
	public static List<IAmmeter> Ammeters { get; set; } = new List<IAmmeter>();                 //所有端口

	public static List<EntityBase> AllEntities { get; set; } = new List<EntityBase>();
	public static List<CircuitLine> AllLines { get; set; } = new List<CircuitLine>();

	private void Awake()
	{
		// 寻找场景中的初始元件，加入到allEntity中去
		EntityBase[] allEntityArray = FindObjectsOfType<EntityBase>();
		for (int i = 0; i < allEntityArray.Length; i++)
		{
			AllEntities.Add(allEntityArray[i]);
		}
	}

	/// <summary>
	/// 清除所有计算记录和连接关系
	/// </summary>
	public static void ClearAll()
	{
		EntityNum = 0;
		GNDLines.Clear();
		GNDLine.GlobalGNDLineID = 0;
		SpicePorts.Clear();
		SpiceEntities.Clear();
		EnabledLines.Clear();
		DisabledLines.Clear();
		Sources.Clear();
		Ammeters.Clear();
	}

	/// <summary>
	/// 电路重新计算
	/// </summary>
	public static void CalculateAll()
	{
		ClearAll();

		// 在并查集中加载有连接元件的内部连接
		LoadElement();

		// 在并查集中连接导线
		for (var i = 0; i < AllLines.Count; i++)
		{
			UF.Union(AllLines[i].StartID_Global, AllLines[i].EndID_Global);
		}

		// 对电源实行接地检测
		foreach (ISource i in Sources)
		{
			i.GroundCheck();
		}

		ConnectGND();

		//检查对地连通性，区分出问题导线，并分别处理
		for (var i = 0; i < AllLines.Count; i++)
		{
			if (UF.Connected(AllLines[i].StartID_Global, 0))
			{
				EnabledLines.Add(AllLines[i]);
				SpiceEntities.Add(new VoltageSource(string.Concat("Line", "_", i), AllLines[i].StartID_Global.ToString(), AllLines[i].EndID_Global.ToString(), 0));
				EntityNum++;
			}
			else
			{
				AllLines[i].DisableLine();
				DisabledLines.Add(AllLines[i]);
			}
		}

		// 在SpiceSharp中加载元件
		SetElement();
		SpiceSharpCalculate();

		// 仿真通过后恢复通过并查集删除的导线
		foreach (CircuitLine disabledLine in DisabledLines)
		{
			disabledLine.ReenableLine();
		}
	} // public static void CalculateAll()

	/// <summary>
	/// 在现有的连接关系上重新计算
	/// </summary>
	public static void CalculateByConnection()
	{
		EntityNum = 0;
		GNDLine.GlobalGNDLineID = 0;
		SpicePorts.Clear();
		SpiceEntities.Clear();
		ConnectGND();

		// 直接连接正常导线
		for (int i = 0; i < EnabledLines.Count; i++)
		{
			SpiceEntities.Add(new VoltageSource(string.Concat("Line", "_", i), EnabledLines[i].StartID_Global.ToString(), EnabledLines[i].EndID_Global.ToString(), 0));
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
		for (var i = 0; i < AllEntities.Count; i++)
		{
			if (AllEntities[i].IsConnected())
			{
				AllEntities[i].LoadElement();
			}
			if (AllEntities[i] is ISource)
			{
				Sources.Add(AllEntities[i] as ISource);
			}
			if (AllEntities[i] is IAmmeter)
			{
				Ammeters.Add(AllEntities[i] as IAmmeter);
			}
		}
	}

	/// <summary>
	/// 正式连接（需要事先处理不接地连接）
	/// </summary>
	private static void SetElement()
	{
		EntityNum = 0;
		for (int i = 0; i < AllEntities.Count; i++)
		{
			if (AllEntities[i].IsConnected())
			{
				AllEntities[i].SetElement();
				EntityNum++;
			}
		}
	}

	/// <summary>
	/// 将需要接地的电源接地
	/// </summary>
	private static void ConnectGND()
	{
		foreach (GNDLine i in GNDLines)
		{
			SpiceEntities.Add(new VoltageSource(string.Concat(i.GNDLineID.ToString(), "_GND"), i.PortToGND.ToString(), "0", 0));
		}
	}

	
	/// <summary>
	/// 调用SpiceSharp进行计算
	/// </summary>
	private static void SpiceSharpCalculate()
	{
		// 创建电路
		var ckt = new Circuit(SpiceEntities);

		// 直流工作点分析并写入端口电压值
		var op = new OP("op");
		op.ExportSimulationData += (sender, exportDataEventArgs) =>
		{
			foreach (CircuitPort i in SpicePorts)
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
			Debug.Log("悬空导线的数目为：" + DisabledLines.Count);
			Debug.LogWarning("仿真失败，电路无通路");
		}

		// 计算电流
		foreach (IAmmeter i in Ammeters)
		{
			i.CalculateCurrent();
		}
	} //private static void SpiceSharpCalculate()
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
} //public class WeightedQuickUnionUF