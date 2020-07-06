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
					CircuitcalCulator.CalculateAll();//连接完导线，计算
				}
			}
		}
		//滑块部分
		public static void DragSlider(MySlider which) //滑动滑块时
		{
			CircuitcalCulator.CalculateAll();
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
				CircuitcalCulator.CalculateAll();//删除导线，计算
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
			if(CircuitcalCulator.error == 1)
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
			if (CircuitcalCulator.error == 1)
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
			if (CircuitcalCulator.error == 1)
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
			if (CircuitcalCulator.error == 1)
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
			if (CircuitcalCulator.error == 1)
			{
				CamMain.ShowTips("电路中存在悬空状态，请检查连接\n", 0);
			}
		}
	}
	//电路程序

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
}