<<<<<<< HEAD
﻿using UnityEngine;
=======
﻿using System.Collections;
using UnityEngine;
>>>>>>> d2f577c21810436a6bc014131fee5161befcc1ea
using UnityEngine.UI;

/// <summary>
/// 用于显示光标，控制颜色和显示提示
/// </summary>
public class DisplayController : MonoBehaviour
{
	/// <summary>
	/// 保留奇怪的东西，方便存档。随便艹的变量
	/// /// </summary>
	public static int MyColorID { get; set; } = 0;
	/// <summary>
	/// 返回当前真实的颜色值
	/// </summary>
	public static Color MyColorReal { get; private set; } = Color.black;
	/// <summary>
	/// 改这个就能把FPS关闭
	/// </summary>
	public static bool MyShowFps
	{
		set
		{
			thisInstance.txtFps.enabled = value;
		}
	}
	/// <summary>
	/// 改这个就能把光标关闭
	/// </summary>
	public static bool MyShowCross
	{
		set
		{
			thisInstance.imgCross.enabled = value;
		}
	}


	Image imgCross;
	Text txtFps;

	private static readonly int colorMax = 5;
	private static readonly Color[] corssColor = new Color[5];
	private static Rect rectTex = new Rect(0, 0, 100, 100);
	public static bool IsCursorHidden = false;
	public Canvas canvas;
	public Text fpsText;

	void Awake()
	{
		fpsText = canvas.transform.FindComponent_BFS<Text>("FPSDisplay");
		// 加载光标颜色
<<<<<<< HEAD
		corssColor[0] = Color.black;
		corssColor[1] = Color.white;
		corssColor[2] = Color.red;
		corssColor[3] = Color.yellow;
		corssColor[4] = Color.green;
	}

	static DisplayController thisInstance;
	void Start()
	{
		thisInstance = this;
		imgCross = GetComponentInChildren<Image>();
		txtFps = GetComponentInChildren<Text>();
=======
		mouseTex[0] = (Texture2D)Resources.Load("Cross");
		mouseTex[1] = (Texture2D)Resources.Load("CrossWhite");
		mouseTex[2] = (Texture2D)Resources.Load("CrossRed");
		mouseTex[3] = (Texture2D)Resources.Load("CrossYellow");
		mouseTex[4] = (Texture2D)Resources.Load("CrossGreen");

		StartCoroutine(CursorUpdate());
>>>>>>> d2f577c21810436a6bc014131fee5161befcc1ea
	}

	void Update()
	{
		// 颜色控制
		// 按Q切换颜色
		if (Input.GetKeyDown(KeyCode.Q))
		{
			MyColorID--;
		}

		// 按E切换颜色
		if (Input.GetKeyDown(KeyCode.E))
		{
			MyColorID++;
		}

		// 循环颜色
		if (MyColorID < 0)
		{
			MyColorID += colorMax;
		}
		if (MyColorID >= colorMax)
		{
			MyColorID -= colorMax;
		}
<<<<<<< HEAD
=======

		//Debug.LogError(fpsText.text);

		IsCursorHidden = true;
	}

	void OnGUI()
	{
		float mousex = Input.mousePosition.x;
		float mousey = Screen.height - Input.mousePosition.y;
>>>>>>> d2f577c21810436a6bc014131fee5161befcc1ea

		imgCross.transform.position = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
		imgCross.color = corssColor[MyColorID];
		MyColorReal =	corssColor[MyColorID];
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
