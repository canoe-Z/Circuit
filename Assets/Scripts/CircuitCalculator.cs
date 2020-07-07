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
		SourceCheck();
		SolarCheck();
		ExSourceCheck();
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
		Resistance[] AllResistance = GameObject.FindObjectsOfType<Resistance>();
		for (int i = 0; i < AllResistance.Length; i++)
		{
			if (AllResistance[i].IsConnected())
			{
				AllResistance[i].LoadElement();
			}
		}
		Source[] AllSource = GameObject.FindObjectsOfType<Source>();
		for (int i = 0; i < AllSource.Length; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				if (AllSource[i].IsConnected(j))
				{
					Debug.LogWarning("电源E" + j + "有连接");
					AllSource[i].LoadElement(j);
				}
				else
				{
					Debug.LogWarning("电源E" + j + "无连接");
				}
			}
		}
		Voltmeter[] AllVoltmeter = GameObject.FindObjectsOfType<Voltmeter>();
		for (int i = 0; i < AllVoltmeter.Length; i++)
		{
			if (AllVoltmeter[i].IsConnected())
			{
				AllVoltmeter[i].LoadElement();
			}
		}
		TVoltmeter[] AllTVoltmeter = GameObject.FindObjectsOfType<TVoltmeter>();
		for (int i = 0; i < AllVoltmeter.Length; i++)
		{
			if (AllTVoltmeter[i].IsConnected())
			{
				AllTVoltmeter[i].LoadElement();
			}
		}
		Ammeter[] AllAmmeter = GameObject.FindObjectsOfType<Ammeter>();
		for (int i = 0; i < AllAmmeter.Length; i++)
		{
			if (AllAmmeter[i].IsConnected())
			{
				AllAmmeter[i].LoadElement();
			}
		}
		TAmmeter[] AllTAmmeter = GameObject.FindObjectsOfType<TAmmeter>();
		for (int i = 0; i < AllAmmeter.Length; i++)
		{
			if (AllTAmmeter[i].IsConnected())
			{
				AllTAmmeter[i].LoadElement();
			}
		}
		RBox[] AllRBox = GameObject.FindObjectsOfType<RBox>();
		for (int i = 0; i < AllRBox.Length; i++)
		{
			if (AllRBox[i].IsConnected())
			{
				AllRBox[i].LoadElement();
			}
		}
		SourceStand[] AllSourceStand = GameObject.FindObjectsOfType<SourceStand>();
		for (int i = 0; i < AllSourceStand.Length; i++)
		{
			if (AllSourceStand[i].IsConnected())
			{
				AllSourceStand[i].LoadElement();
			}
		}
		Gmeter[] AllGmeter = GameObject.FindObjectsOfType<Gmeter>();
		for (int i = 0; i < AllAmmeter.Length; i++)
		{
			if (AllGmeter[i].IsConnected())
			{
				AllGmeter[i].LoadElement();
			}
		}
		Switch[] AllSwitch = GameObject.FindObjectsOfType<Switch>();
		for (int i = 0; i < AllSwitch.Length; i++)
		{
			if (AllSwitch[i].IsConnected())
			{
				AllSwitch[i].LoadElement();
			}
		}
		SliderR[] AllSliderR = GameObject.FindObjectsOfType<SliderR>();
		for (int i = 0; i < AllSliderR.Length; i++)
		{
			if (AllSliderR[i].IsConnected())
			{
				AllSliderR[i].LoadElement();
			}
		}
		Solar[] AllSolar = GameObject.FindObjectsOfType<Solar>();
		for (int i = 0; i < AllSolar.Length; i++)
		{
			if (AllSolar[i].IsConnected())
			{
				AllSolar[i].LoadElement();
			}
		}
	}
	private static void SetElement() //已经将有问题的节点的可连接标志去除，正式连接
	{
		EntityNum = 0;
		Resistance[] AllResistance = GameObject.FindObjectsOfType<Resistance>();
		for (int i = 0; i < AllResistance.Length; i++)
		{
			if (AllResistance[i].IsConnected())
			{
				AllResistance[i].SetElement();
				EntityNum++;
			}
		}
		Source[] AllSource = GameObject.FindObjectsOfType<Source>();
		for (int i = 0; i < AllSource.Length; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				if (AllSource[i].IsConnected(j))
				{
					AllSource[i].SetElement(j);
				}
			}
			EntityNum++;
		}
		Voltmeter[] AllVoltmeter = GameObject.FindObjectsOfType<Voltmeter>();
		for (int i = 0; i < AllVoltmeter.Length; i++)
		{
			if (AllVoltmeter[i].IsConnected())
			{
				AllVoltmeter[i].SetElement();
				EntityNum++;
			}
		}
		TVoltmeter[] AllTVoltmeter = GameObject.FindObjectsOfType<TVoltmeter>();
		for (int i = 0; i < AllVoltmeter.Length; i++)
		{
			if (AllTVoltmeter[i].IsConnected())
			{
				AllTVoltmeter[i].SetElement();
				EntityNum++;
			}
		}
		Ammeter[] AllAmmeter = GameObject.FindObjectsOfType<Ammeter>();
		for (int i = 0; i < AllAmmeter.Length; i++)
		{
			if (AllAmmeter[i].IsConnected())
			{
				AllAmmeter[i].SetElement();
				EntityNum++;
			}
		}
		TAmmeter[] AllTAmmeter = GameObject.FindObjectsOfType<TAmmeter>();
		for (int i = 0; i < AllAmmeter.Length; i++)
		{
			if (AllTAmmeter[i].IsConnected())
			{
				AllTAmmeter[i].SetElement();
				EntityNum++;
			}
		}
		RBox[] AllRBox = GameObject.FindObjectsOfType<RBox>();
		for (int i = 0; i < AllRBox.Length; i++)
		{
			if (AllRBox[i].IsConnected())
			{
				AllRBox[i].SetElement();
				EntityNum++;
			}
		}
		SourceStand[] AllSourceStand = GameObject.FindObjectsOfType<SourceStand>();
		for (int i = 0; i < AllSourceStand.Length; i++)
		{
			if (AllSourceStand[i].IsConnected())
			{
				AllSourceStand[i].SetElement();
				EntityNum++;
			}
		}
		Gmeter[] AllGmeter = GameObject.FindObjectsOfType<Gmeter>();
		for (int i = 0; i < AllAmmeter.Length; i++)
		{
			if (AllGmeter[i].IsConnected())
			{
				AllGmeter[i].SetElement();
				EntityNum++;
			}
		}
		Switch[] AllSwitch = GameObject.FindObjectsOfType<Switch>();
		for (int i = 0; i < AllSwitch.Length; i++)
		{
			if (AllSwitch[i].IsConnected())
			{
				AllSwitch[i].SetElement();
				EntityNum++;
			}
		}
		SliderR[] AllSliderR = GameObject.FindObjectsOfType<SliderR>();
		for (int i = 0; i < AllSliderR.Length; i++)
		{
			if (AllSliderR[i].IsConnected())
			{
				AllSliderR[i].SetElement();
				EntityNum++;
			}
		}
		Solar[] AllSolar = GameObject.FindObjectsOfType<Solar>();
		for (int i = 0; i < AllSolar.Length; i++)
		{
			if (AllSolar[i].IsConnected())
			{
				AllSolar[i].SetElement();
				EntityNum++;
			}
		}
	}

	//电源检测
	private static void SourceCheck()
	{
		Source[] AllSource = GameObject.FindObjectsOfType<Source>();
		for (int i = 0; i < AllSource.Length; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				if (AllSource[i].IsConnected(j))
				{
					if (!UF.Connected(AllSource[i].G[j], 0))
					{
						UF.Union(AllSource[i].G[j], 0);
						entities.Add(new VoltageSource(string.Concat(AllSource[i].EntityID.ToString(), "_GND", j), AllSource[i].G[j].ToString(), "0", 0));
						Debug.LogWarning("电源E" + j + "有连接但悬空，将其接地");
					}
					else
					{
						Debug.LogWarning("电源E" + j + "已经接地");
					}
				}
			}
		}
	}
	//额外电源检测
	private static void ExSourceCheck()
	{
		SourceStand[] AllSourceStand = GameObject.FindObjectsOfType<SourceStand>();
		for (int i = 0; i < AllSourceStand.Length; i++)
		{
			if (AllSourceStand[i].IsConnected())
			{
				if (!UF.Connected(AllSourceStand[i].G, 0))
				{
					UF.Union(AllSourceStand[i].G, 0);
					entities.Add(new VoltageSource(string.Concat(AllSourceStand[i].EntityID.ToString(), "_GND"), AllSourceStand[i].G.ToString(), "0", 0));
					Debug.LogError("额外电源悬空，将额外电源接地");
				}
				else
				{
					Debug.LogError("额外电源已经接地");
				}
			}
		}
	}
	//太阳能电池检测
	private static void SolarCheck()
	{
		Solar[] AllSolar = GameObject.FindObjectsOfType<Solar>();
		for (int i = 0; i < AllSolar.Length; i++)
		{
			if (AllSolar[i].IsConnected())
			{
				if (!UF.Connected(AllSolar[i].GND, 0))
				{
					UF.Union(AllSolar[i].GND, 0);
					entities.Add(new VoltageSource(string.Concat(AllSolar[i].EntityID.ToString(), "_GND"), AllSolar[i].GND.ToString(), "0", 0));
					Debug.LogError("太阳能电池悬空，将太阳能电池接地");
				}
				else
				{
					Debug.LogError("太阳能电池已经接地");
				}
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