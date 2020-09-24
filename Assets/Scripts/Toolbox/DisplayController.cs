using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 用于显示光标，控制颜色和显示提示
/// </summary>
public class DisplayController : MonoBehaviour
{
	/// <summary>
	/// 随便艹的变量，可用于保存存档
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
		get
		{
			return thisInstance.imgCross.enabled;
		}
		set
		{
			thisInstance.imgCross.enabled = value;
		}
	}
	/// <summary>
	/// 令这些东西隐藏一帧
	/// </summary>
	public static void MyHideOneFrame()
	{
		MyShowCross = false;
		MyShowFps = false;
		thisInstance.frameHide_counter = 2;
	}



	Color[] crossColor = new Color[colorMax];
	const int colorMax = 5;
	Image imgCross;
	Text txtFps;

	void Awake()
	{
		// 加载光标颜色
		crossColor[0] = Color.black;
		crossColor[1] = Color.white;
		crossColor[2] = Color.red;
		crossColor[3] = Color.yellow;
		crossColor[4] = Color.green;
	}

	static DisplayController thisInstance;
	void Start()
	{
		thisInstance = this;
		imgCross = GetComponentInChildren<Image>();
		txtFps = GetComponentInChildren<Text>();
	}

	int frameHide_counter = 0;
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
		//光标位置
		imgCross.transform.position = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
		//光标颜色
		imgCross.color = crossColor[MyColorID];
		//更新用于外部读取的颜色
		MyColorReal = crossColor[MyColorID];


		//实现延时一帧的效果
		if (frameHide_counter > 0)
		{
			frameHide_counter--;
			if (frameHide_counter == 0)
			{
				MyShowCross = true;
				MyShowFps = true;
			}
		}
	}
}
