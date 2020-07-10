using UnityEngine;

public static class Global
{
	public static class Other
	{
		//单击端口以连接导线
		public static CircuitPort prePort = null;
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
					CircuitCalculator.CalculateAll();//连接完导线，计算
				}
			}
		}
		//滑块部分
		public static void DragSlider(MySlider which) //滑动滑块时
		{
			CircuitCalculator.CalculateByConnection();
		}
		//Tips，0Port连接，1物体拖动，2链子，3滑块，456保留
		public static void Loop()//每帧由摄像机调用
		{
			if (Input.GetMouseButtonDown(1))//右键清除连接状态
			{
				prePort = null;
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
	}
}