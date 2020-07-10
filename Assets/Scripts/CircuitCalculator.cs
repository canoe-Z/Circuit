using System.Collections.Generic;
using UnityEngine;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using SpiceSharp.Circuits;

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
	//连接关系改变需要重算，连接关系改变，但
	public static void CalculateAll()
	{
		EntityNum = 0;
		gndLines.Clear();
		ports.Clear();
		entities.Clear();
		GoodLine.Clear();
		ProblemLine.Clear();
		sources.Clear();
		ammeters.Clear();
		SpiceON();
	}

	public static void CalculateSome()
	{
		EntityNum = 0;
		ports.Clear();
		entities.Clear();
		SomeSpiceON();
	}
	//连接关系改变需要彻底重算
	private static void SpiceON()
	{
		//在并查集中加载有连接元件的内部连接
		LoadElement();

		//以下三步顺序不能调换
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

		//接地
		foreach (GNDLine i in gndLines)
		{
			entities.Add(new VoltageSource(string.Concat(i.GNDLineID.ToString(), "_GND"), i.PortToGND.ToString(), "0", 0));
		}

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

		//创建电路
		var ckt = new Circuit(entities);

		//直流工作点分析并写入端口电压值
		var op = new OP("op");
		op.ExportSimulationData += (sender, exportDataEventArgs) =>
		{
			foreach (CircuitPort i in ports)
			{
				i.U = exportDataEventArgs.GetVoltage(i.PortID_Global.ToString());
				Debug.Log(exportDataEventArgs.GetVoltage(i.PortID_Global.ToString()));
			}
		};

		// 启动仿真
		try
		{
			op.Run(ckt);
			Debug.Log(GoodLine.Count);
			Debug.Log("仿真成功");
		}
		catch
		{
			Debug.Log(ProblemLine.Count);
			Debug.Log("电路中存在悬空状态");
		}

		//计算电流
		foreach (IAmmeter i in ammeters)
		{
			i.CalculateCurrent();
		}

		//仿真通过后恢复通过并查集删除的导线
		foreach (CircuitLine i in ProblemLine)
		{
			i.ReLine();
		}
	}

	private static void SomeSpiceON()
	{
		//接地
		foreach (GNDLine i in gndLines)
		{
			entities.Add(new VoltageSource(string.Concat(i.GNDLineID.ToString(), "_GND"), i.PortToGND.ToString(), "0", 0));
		}

		for (int i = 0; i < GoodLine.Count; i++)
		{
			entities.Add(new VoltageSource(string.Concat("Line", "_", i), GoodLine[i].startID_Global.ToString(), GoodLine[i].endID_Global.ToString(), 0));
			EntityNum++;
		}

		//在SpiceSharp中加载元件
		SetElement();

		//创建电路
		var ckt = new Circuit(entities);

		//直流工作点分析并写入端口电压值
		var op = new OP("op");
		op.ExportSimulationData += (sender, exportDataEventArgs) =>
		{
			foreach (CircuitPort i in ports)
			{
				i.U = exportDataEventArgs.GetVoltage(i.PortID_Global.ToString());
				Debug.Log(exportDataEventArgs.GetVoltage(i.PortID_Global.ToString()));
			}
		};

		// 启动仿真
		try
		{
			op.Run(ckt);
			Debug.Log(GoodLine.Count);
			Debug.Log("仿真成功");
		}
		catch
		{
			Debug.Log(ProblemLine.Count);
			Debug.Log("电路中存在悬空状态");
		}

		//计算电流
		foreach (IAmmeter i in ammeters)
		{
			i.CalculateCurrent();
		}
	}

	private static void LoadElement() //通过并查集预连接，同时按照加载不同的接口
	{
		UF.Clear(10000);
		for (int i = 0; i < allEntity.Count; i++)
		{
			if(allEntity[i].IsConnected())
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

	private static void SetElement() //已经将有问题的节点的可连接标志去除，正式连接
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
}

public class GNDLine
{
	public int GlobalGNDLineID = 0;
	public int GNDLineID;
	public int PortToGND;
	public GNDLine(int portToGND)
	{
		GNDLineID = GlobalGNDLineID;
		GlobalGNDLineID++;
		PortToGND = portToGND;
	}
}

//并查集
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