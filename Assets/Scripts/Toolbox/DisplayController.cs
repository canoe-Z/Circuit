using UnityEngine;
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

	void Awake()
	{
		// 加载光标颜色
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

		imgCross.transform.position = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
		imgCross.color = corssColor[MyColorID];
		MyColorReal =	corssColor[MyColorID];
	}
}
