using UnityEngine;

/// <summary>
/// 图形界面
/// </summary>
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
		MyUI.Start();
	}
	private void Update()
	{
	}
	private void OnGUI()
	{
		GUI.skin.label.normal.textColor = Color.black;//字体颜色为黑色
		MyUI.OnGUI();
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
			ConnectionManager.colors[0] = Color.black;
			ConnectionManager.colors[1] = Color.white;
			ConnectionManager.colors[2] = Color.red;
			ConnectionManager.colors[3] = Color.yellow;
			ConnectionManager.colors[4] = Color.green;
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