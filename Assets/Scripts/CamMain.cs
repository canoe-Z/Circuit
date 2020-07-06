using UnityEngine;
using System.Runtime.InteropServices;
//
//图形界面
//
public class CamMain : MonoBehaviour
{
	//显示特定文本信息
	public static void ShowTips(string toShow, int stage)
	{
		//0Port连接，1物体拖动，2链子，3滑块，456保留
		//7摄像机相关提示符
		if (stage >= MyUI.tip.Length)
		{
			Debug.LogError("tips的stage超出范围");
		}
		MyUI.tip[stage] = toShow;
	}
	public static void ChangeColor(int id)
	{
		MyUI.tex = id;
	}
	private void Start()
	{
		Global.Other.SetUp();
		Cursor.lockState = CursorLockMode.Locked;//锁定中央
		Cursor.visible = false;
		CAMERA.Start();
		//MoveController.Start();//移动的初始化
		MyUI.Start();
	}
	private void Update()
	{
		//if(Global.boolMove) MoveController.Update();//移动
		CAMERA.Update();//更新摄像机
		Global.Other.Loop();
	}
	private void OnGUI()
	{
		GUI.skin.label.normal.textColor = Color.black;//字体颜色为黑色
		//FPS.OnGUI();//显示FPS
		MyUI.OnGUI();
	}

	public static class CAMERA
	{
		static Camera mainCam = null;
		static Camera[] smallCam = new Camera[4];
		static void SetAs(Camera which, Camera source)
		{
			Quaternion rot = source.transform.rotation;
			Vector3 pos = source.transform.position;
			which.transform.SetPositionAndRotation(pos, rot);
		}
		static void Exchange(Camera one, Camera two)
		{
			Quaternion rot1 = one.transform.rotation;
			Vector3 pos1 = one.transform.position;
			Quaternion rot2 = two.transform.rotation;
			Vector3 pos2 = two.transform.position;
			one.transform.SetPositionAndRotation(pos2, rot2);
			two.transform.SetPositionAndRotation(pos1, rot1);
		}
		public static void Start()
		{
			mainCam = Camera.main;
			for (int i = 0; i < 4; i++)
			{
				smallCam[i] = GameObject.Instantiate(Camera.main);
				smallCam[i].name = "smallCam" + i;
				smallCam[i].depth = 1;
				smallCam[i].enabled = false;//关闭次要摄像机
				Destroy(smallCam[i].gameObject.GetComponent<CharacterController>());
			}
			smallCam[0].rect = new Rect(0, 0.6f, 0.4f, 0.4f);
			smallCam[1].rect = new Rect(0.6f, 0.6f, 0.4f, 0.4f);
			smallCam[2].rect = new Rect(0f, 0, 0.4f, 0.4f);
			smallCam[3].rect = new Rect(0.6f, 0, 0.4f, 0.4f);
			mainCam.rect = new Rect(0, 0, 1, 1);//全屏
												
			//Tips，7摄像机相关提示符
			ShowTips("Shift + 数字键1234 打开/关闭1234号小摄像机，并将其视角设置为当前视角。\n数字键1234 和1234号小摄像机调换位置。\n", 7);
		}
		public static void Update()
		{
			bool SHIFT = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
			if (Input.GetKeyDown(KeyCode.X))
			{
				Camera.main.tag = "Untagged";
				smallCam[3].tag = "MainCamera";
			}

			for(int i = 0; i < 4; i++)
			{
				if (smallCam[i].enabled)
				{
					if (Input.GetKeyDown(KeyCode.Alpha1 + i))
					{
						if (SHIFT)
						{
							smallCam[i].enabled = false;
						}
						else
						{
							Exchange(smallCam[i], mainCam);
						}
					}
				}
				else
				{
					if (Input.GetKeyDown(KeyCode.Alpha1 + i))
					{
						if (SHIFT)
						{
							smallCam[i].enabled = true;
							SetAs(smallCam[i], mainCam);
						}
					}
				}
			}
		}
	}

	private static class MyUI
	{
		//下面是UI界面，包括tips和鼠标光标
		public static string[] tip = new string[10];
		public static Rect rectTex = new Rect(0, 0, 100, 100);
		public static Rect rectTxt = new Rect(0, 0, 1000, 800);
		private static Texture2D cross = null;
		private static Texture2D white = null;
		private static Texture2D yellow = null;
		private static Texture2D green = null;
		private static Texture2D red = null;
		public static int tex = 0;
		public static void Start()
		{
			Global.Other.colors[0] = Color.black;
			Global.Other.colors[1] = Color.white;
			Global.Other.colors[2] = Color.red;
			Global.Other.colors[3] = Color.yellow;
			Global.Other.colors[4] = Color.green;
			cross = (Texture2D)Resources.Load("Cross");
			white = (Texture2D)Resources.Load("CrossWhite");
			red = (Texture2D)Resources.Load("CrossRed");
			yellow = (Texture2D)Resources.Load("CrossYellow");
			green = (Texture2D)Resources.Load("CrossGreen");
			//Tips，8导线颜色
			ShowTips("Q 或 E 切换下一个连接的导线的颜色", 8);
		}
		//下面是鼠标UI界面
		public static void OnGUI()
		{
			float mousex = Input.mousePosition.x;
			float mousey = Screen.height - Input.mousePosition.y;
			{
				rectTxt.x = mousex + 50;
				rectTxt.y = mousey + 50;
				string will = null;
				for(int i = 0; i < tip.Length; i++)
				{
					will += tip[i];
				}
				GUI.Label(rectTxt, will);
			}
			rectTex.x = mousex - rectTex.width / 2;
			rectTex.y = mousey - rectTex.height / 2;
			switch (tex)
			{
				case 0:GUI.DrawTexture(rectTex, cross);break;
				case 1:GUI.DrawTexture(rectTex, white);break;
				case 2:GUI.DrawTexture(rectTex, red);break;
				case 3:GUI.DrawTexture(rectTex, yellow);break;
				case 4:GUI.DrawTexture(rectTex, green);break;
			}
		}
	}
}