using System.Collections.Generic;
using UnityEngine;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using SpiceSharp.Circuits;
using System;

public static class Global
{
	static GameObject menu = null;
	public static bool boolMove = true;//可以通过鼠标移动
	public static void OpenMenu()
	{
		Cursor.lockState = CursorLockMode.None;//解除鼠标锁定
		Cursor.visible = true;
		boolMove = false;
		menu.SetActive(true);
	}
	public static void CloseMenu()
	{
		Cursor.lockState = CursorLockMode.Locked;//锁定鼠标于中央
		Cursor.visible = false;
		boolMove = true;
		menu.SetActive(false);
	}
	public static class Other
	{
		//单击端口以连接导线
		static CircuitPort prePort = null;
		public static Color[] colors = new Color[5]; //导线颜色配置
		static int colorID = 0;
		static readonly int colorMax = 5;
		public static void ClickPort(CircuitPort which)
		{
			if (prePort == null)
			{
				prePort = which;
			}
			else
			{
				if (prePort != which)
				{//连接导线
					GameObject gameObject = new GameObject("Line");
					gameObject.AddComponent<Rope>();
					GameObject rope = gameObject.GetComponent<Rope>().CreateRope(prePort.gameObject, which.gameObject, Rope.CreateSolver());
					rope.layer = 8; //关闭碰撞检测
					rope.AddComponent<EachRope>();
					rope.AddComponent<MeshCollider>();
					var RopeMat = Resources.Load<Material>("Button");
					rope.GetComponent<MeshRenderer>().material = RopeMat;
					rope.GetComponent<MeshRenderer>().material.color = colors[colorID];
					gameObject.transform.parent = rope.transform;
					gameObject.AddComponent<CircuitLine>().CreateLine(prePort.gameObject, which.gameObject);
					prePort = null;
					MyCircuit.CalculateAll();//连接完导线，计算
				}
			}
		}
		//滑块部分
		public static void DragSlider(MySlider which) //滑动滑块时
		{
			MyCircuit.CalculateAll();
		}
		public static void SetUp()//摄像机的Start
		{
			menu = GameObject.Find("Menu");
			if (menu == null) Debug.LogError("未找到菜单");
			CloseMenu();
		}
		//Tips，0Port连接，1物体拖动，2链子，3滑块，456保留
		public static void Loop()//每帧由摄像机调用
		{
			if (Input.GetKeyDown(KeyCode.Escape)) //开启菜单
			{
				if (boolMove) OpenMenu();
				else CloseMenu();
			}
			if (Input.GetMouseButtonDown(1))//右键清除连接状态
			{
				prePort = null;
			}
			if (Input.GetMouseButtonUp(1))//右键抬起时，删除完毕导线，开始计算
			{
				MyCircuit.CalculateAll();//删除导线，计算
			}
			if (Input.GetKeyDown(KeyCode.Q)) colorID--; //颜色控制
			if (Input.GetKeyDown(KeyCode.E)) colorID++;
			if (colorID < 0) colorID += colorMax;
			if (colorID >= colorMax) colorID -= colorMax;
			CamMain.ChangeColor(colorID);
		}
		public static void OverPort(CircuitPort which)//鼠标置于某端口上
		{
			if (prePort == null) CamMain.ShowTips("单击以连接导线。\n", 0);
			else
			{
				if (prePort == which) CamMain.ShowTips("在其它接线柱单击，完成连接导线。\n按 鼠标右键 清除当前连接。\n", 0);
				else CamMain.ShowTips("在这里单击，完成连接导线。\n按 鼠标右键 清除当前连接。\n", 0);
			}
			CamMain.ShowTips(null, 1);
			CamMain.ShowTips(null, 2);
			CamMain.ShowTips(null, 3);
			if(Global.MyCircuit.error==1)
			{
				CamMain.ShowTips("电路中存在悬空状态，请检查连接\n", 0);
			}
		}
		public static void OverItem(NormItem which)//鼠标置于某物体上
		{
			if (prePort == null) CamMain.ShowTips("捕捉到接线柱并单击，开始连接导线。\n", 0);
			else CamMain.ShowTips("在接线柱处单击，继续连接导线。\n按 鼠标右键 清除当前连接。\n", 0);
			CamMain.ShowTips("鼠标拖动，移动这个元件。不要扔到桌子外面哦！\n按 鼠标中键 将这个元件的方向正回来。（如果被你玩飞了的话AwA）\n", 1);
			if (Input.GetMouseButtonDown(2)) which.Straighten();
			CamMain.ShowTips(null, 2);
			CamMain.ShowTips(null, 3);
			if (Global.MyCircuit.error == 1)
			{
				CamMain.ShowTips("电路中存在悬空状态，请检查连接\n", 0);
			}
		}
		public static void OverElse()//鼠标置于其他位置
		{
			if (prePort == null) CamMain.ShowTips("捕捉到接线柱并单击，开始连接导线。\n", 0);
			else CamMain.ShowTips("在接线柱处单击，继续连接导线。\n按 鼠标右键 清除当前连接。\n", 0);
			CamMain.ShowTips(null, 1);
			CamMain.ShowTips(null, 2);
			CamMain.ShowTips(null, 3);
			if (Global.MyCircuit.error == 1)
			{
				CamMain.ShowTips("电路中存在悬空状态，请检查连接\n", 0);
			}
		}
		public static void OverChain()//鼠标置于导线上
		{
			if (prePort == null) CamMain.ShowTips("捕捉到接线柱并单击，开始连接导线。\n", 0);
			else CamMain.ShowTips("在接线柱处单击，继续连接导线。\n按 鼠标右键 清除当前连接。\n", 0);
			CamMain.ShowTips(null, 1);
			CamMain.ShowTips("按 鼠标右键 删除这个导线。\n", 2);
			CamMain.ShowTips(null, 3);
			if (Global.MyCircuit.error == 1)
			{
				CamMain.ShowTips("电路中存在悬空状态，请检查连接\n", 0);
			}
		}
		public static void OverSlider()//鼠标置于滑块上
		{
			if (prePort == null) CamMain.ShowTips("捕捉到接线柱并单击，开始连接导线。\n", 0);
			else CamMain.ShowTips("在接线柱处单击，继续连接导线。\n按 鼠标右键 清除当前连接。\n", 0);
			CamMain.ShowTips(null, 1);
			CamMain.ShowTips(null, 2);
			CamMain.ShowTips("滑动以调节参数。\n", 3);
			if (Global.MyCircuit.error == 1)
			{
				CamMain.ShowTips("电路中存在悬空状态，请检查连接\n", 0);
			}
		}
	}
	//电路程序
	public static class MyCircuit
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
						Debug.LogError("电源E" + j + "有连接");
						AllSource[i].LoadElement(j);
					}
					else
					{
						Debug.LogError("电源E" + j + "无连接");
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
							entities.Add(new VoltageSource(string.Concat(AllSource[i].EntityID.ToString(), "_GND",j), AllSource[i].G[j].ToString(), "0", 0));
							Debug.LogError("电源E" + j + "有连接但悬空，将其接地");
						}
						else
						{
							Debug.LogError("电源E" + j + "已经接地");
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


	//特殊功能函数
	public static class Fun
	{
		public static bool HitCheck(string tag, out Vector3 hitPos)
		{
			hitPos = new Vector3(0, 0, 0);
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit[] hitObj;
			hitObj = Physics.RaycastAll(ray);

			for (int i = 0; i < hitObj.Length; i++)
			{
				GameObject hitedItem = hitObj[i].collider.gameObject;
				if (tag == null || hitedItem.tag == tag)
				{
					hitPos = hitObj[i].point;
					return true;
				}
			}
			return false;
		}
		public static bool HitOnlyOne(out Vector3 hitpos)
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray, out RaycastHit hitObj))
			{
				hitpos = hitObj.point;
				return true;
			}
			else
			{
				hitpos = new Vector3(0, 0, 0);
			}
			return false;
		}
		public static void OutDouble(double[] will)
		{
			Debug.Log("一组数据" + will.Length);
			for (int i = 0; i < will.Length; i++)
			{
				Debug.Log(will[i] + "序号" + i);
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
}