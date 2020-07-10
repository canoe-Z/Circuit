using System.Collections.Generic;
using UnityEngine;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using SpiceSharp.Circuits;

public static class CircuitCalculator
{
	public static int PortNum = 0;//端口总数，创建端口时++
	public static int EntityNum = 0;//元件总数，创建元件时++
	public static int error = 0;//浮空错误提示
	public static int SourceFlag = 0;//为1时代表并查集与地(直流电源)不通
	public static int SourceStandardFlag = 0;//将标准电源接地后，并查集仍不通
	public static List<Entity> entities = new List<Entity>();//元件
	public static List<CircuitPort> ports = new List<CircuitPort>();//已连接元件的端口ID(用于端口电压检测序列)
	public static List<TAmmeter> tammeter = new List<TAmmeter>();//电流表端口电流检测序列
	public static List<Ammeter> ammeter = new List<Ammeter>();//电流表端口电流检测序列
	public static List<Gmeter> gmeter = new List<Gmeter>();//电位计端口电流检测序列
	public static WeightedQuickUnionUF UF = new WeightedQuickUnionUF(10000);//并查集
	public static List<CircuitLine> ProblemLine = new List<CircuitLine>();//问题导线
	public static List<CircuitLine> GoodLine = new List<CircuitLine>();//正常导线
	public static List<ISource> source = new List<ISource>();

	public static void CalculateAll()
	{
		EntityNum = 0;
		tammeter.Clear();
		ammeter.Clear();
		gmeter.Clear();
		ports.Clear();
		entities.Clear();
		error = 0;
		GoodLine.Clear();
		ProblemLine.Clear();
		source.Clear();
		SpiceON();
	}
	private static void SpiceON()
	{
		LoadElement();//预加载
		//预连接导线
		CircuitLine[] AllLine = GameObject.FindObjectsOfType<CircuitLine>();
		for (int i = 0; i < AllLine.Length; i++)
		{
			UF.Union(AllLine[i].startID_Global, AllLine[i].endID_Global);
		}
		EntityBase[] allEntity = GameObject.FindObjectsOfType<EntityBase>();
		for (int i = 0; i < allEntity.Length; i++)
		{
			if(allEntity[i] is ISource)
			{
				source.Add(allEntity[i] as ISource);
			}
		}
		foreach (ISource i in source)
		{
			i.GroundCheck();
		}
		//检查对地连通性，区分出问题导线
		for (int i = 0; i < AllLine.Length; i++)
		{
			if (UF.Connected(AllLine[i].startID_Global, 0))
			{
				GoodLine.Add(AllLine[i]);
			}
			else
			{
				ProblemLine.Add(AllLine[i]);
			}
		}
		//处理问题导线，实际加载元件
		for (int i = 0; i < ProblemLine.Count; i++)
		{
			ProblemLine[i].DestroyLine();
		}
		SetElement();
		//实际连接正确导线
		for (int i = 0; i < GoodLine.Count; i++)
		{
			entities.Add(new VoltageSource(string.Concat("Line", "_", i), GoodLine[i].startID_Global.ToString(), GoodLine[i].endID_Global.ToString(), 0));
			EntityNum++;
		}
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
		foreach (TAmmeter i in tammeter)
		{
			i.Calculate();
		}
		foreach (Ammeter i in ammeter)
		{
			i.Calculate();
		}
		foreach (Gmeter i in gmeter)
		{
			i.Calculate();
		}
		//仿真通过后恢复通过并查集删除的导线
		foreach (CircuitLine i in ProblemLine)
		{
			i.ReLine();
		}

	}
	private static void LoadElement() //通过并查集预连接
	{
		UF.Clear(10000);
		EntityBase[] allEntity = GameObject.FindObjectsOfType<EntityBase>();
		for (int i = 0; i < allEntity.Length; i++)
		{
			if(allEntity[i].IsConnected())
			{
				allEntity[i].LoadElement();
			}
		}
	}

	private static void SetElement() //已经将有问题的节点的可连接标志去除，正式连接
	{
		EntityNum = 0;
		EntityBase[] allEntity = GameObject.FindObjectsOfType<EntityBase>();
		for (int i = 0; i < allEntity.Length; i++)
		{
			if (allEntity[i].IsConnected())
			{
				allEntity[i].SetElement();
				EntityNum++;
			}
		}
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