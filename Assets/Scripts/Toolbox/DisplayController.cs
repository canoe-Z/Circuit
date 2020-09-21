using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 用于显示光标，控制颜色和显示提示
/// </summary>
public class DisplayController : MonoBehaviour
{
	public static int ColorID { get; set; } = 0;
	private static readonly int colorMax = 5;
	private static readonly Texture2D[] mouseTex = new Texture2D[5];
	private static Rect rectTex = new Rect(0, 0, 100, 100);
	public static bool IsCursorHidden = false;
	public Canvas canvas;
	public Text fpsText;

	void Awake()
	{
		fpsText = canvas.transform.FindComponent_BFS<Text>("FPSDisplay");
		// 加载光标颜色
		mouseTex[0] = (Texture2D)Resources.Load("Cross");
		mouseTex[1] = (Texture2D)Resources.Load("CrossWhite");
		mouseTex[2] = (Texture2D)Resources.Load("CrossRed");
		mouseTex[3] = (Texture2D)Resources.Load("CrossYellow");
		mouseTex[4] = (Texture2D)Resources.Load("CrossGreen");

		StartCoroutine(CursorUpdate());
	}

	void Update()
	{
		// 颜色控制
		// 按Q切换颜色
		if (Input.GetKeyDown(KeyCode.Q))
		{
			ColorID--;
		}

		// 按E切换颜色
		if (Input.GetKeyDown(KeyCode.E))
		{
			ColorID++;
		}

		// 循环颜色
		if (ColorID < 0)
		{
			ColorID += colorMax;
		}
		if (ColorID >= colorMax)
		{
			ColorID -= colorMax;
		}

		Debug.LogError(fpsText.text);

		IsCursorHidden = true;
	}

	void OnGUI()
	{
		float mousex = Input.mousePosition.x;
		float mousey = Screen.height - Input.mousePosition.y;

		// 绘制光标
		rectTex.x = mousex - rectTex.width / 2;
		rectTex.y = mousey - rectTex.height / 2;
		GUI.DrawTexture(rectTex, mouseTex[ColorID]);
	}

	private IEnumerator CursorUpdate()
	{
		while (true)
		{
			// 鼠标隐藏一帧
			if (IsCursorHidden)
			{
				IsCursorHidden = false;
				// 关闭光标
				fpsText.enabled = false;
				// 多等待一帧
				yield return null;
				
				//TODO
			}
			else
			{
				fpsText.enabled = true;
				//TODO
			}
			yield return null;
			//下一帧再次调用，yield return null的执行时机在Update()之后
		}
	}
}
