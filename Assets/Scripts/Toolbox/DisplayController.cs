using UnityEngine;

// TODO简化事件调用

/// <summary>
/// 鼠标进入委托
/// </summary>
/// <param name="component">脚本</param>
public delegate void EnterEventHandler(Component component);

/// <summary>
/// 鼠标离开委托
/// </summary>
/// /// <param name="component">脚本</param>
public delegate void ExitEventHandler(Component component);

/// <summary>
/// 用于显示光标，控制颜色和显示提示
/// </summary>
public class DisplayController : MonoBehaviour
{
	public static int ColorID { get; set; } = 0;
	private static readonly int colorMax = 5;
	private static readonly Texture2D[] mouseTex = new Texture2D[5];
	private static Rect rectTex = new Rect(0, 0, 100, 100);
	private static Rect rectTxt = new Rect(0, 0, 1000, 800);
	private static readonly string[] tip = new string[10];

	/// <summary>
	/// 显示提示
	/// </summary>
	/// <param name="toShow">提示内容</param>
	/// <param name="stage">显示优先级</param>
	public static void ShowTips(string toShow, int stage)
	{
		if (stage >= tip.Length)
		{
			Debug.LogError("tips的stage超出范围");
		}
		tip[stage] = toShow;
	}

	void Awake()
	{
		// 订阅各部件的鼠标事件
		EntityBase.MouseEnter += MouseEnter;
		EntityBase.MouseExit += MouseExit;
		CircuitLine.MouseEnter += MouseEnter;
		CircuitLine.MouseExit += MouseExit;
		MySlider.MouseEnter += MouseEnter;
		MySlider.MouseExit += MouseExit;
		CircuitPort.MouseEnter += MouseEnter;
		CircuitPort.MouseExit += MouseExit;

		// 加载光标颜色
		mouseTex[0] = (Texture2D)Resources.Load("Cross");
		mouseTex[1] = (Texture2D)Resources.Load("CrossWhite");
		mouseTex[2] = (Texture2D)Resources.Load("CrossRed");
		mouseTex[3] = (Texture2D)Resources.Load("CrossYellow");
		mouseTex[4] = (Texture2D)Resources.Load("CrossGreen");
	}

	void Start()
	{
		// 写入初始显示和固定显示的内容
		ShowTips("捕捉到接线柱并单击，开始连接导线。\n", 0);
		ShowTips("Shift + 数字键1234 打开/关闭1234号小摄像机，并将其视角设置为当前视角。\n数字键1234 和1234号小摄像机调换位置。\n", 2);
		ShowTips("Q 或 E 切换下一个连接的导线的颜色", 3);
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
	}

	void OnGUI()
	{
		// 绘制提示
		GUI.skin.label.normal.textColor = Color.black;
		float mousex = Input.mousePosition.x;
		float mousey = Screen.height - Input.mousePosition.y;
		{
			rectTxt.x = mousex + 50;
			rectTxt.y = mousey + 50;
			string allTips = null;
			for (int i = 0; i < tip.Length; i++)
			{
				allTips += tip[i];
			}
			GUI.Label(rectTxt, allTips);
		}

		// 绘制光标
		rectTex.x = mousex - rectTex.width / 2;
		rectTex.y = mousey - rectTex.height / 2;
		GUI.DrawTexture(rectTex, mouseTex[ColorID]);
	}

	/// <summary>
	/// 鼠标进入
	/// </summary>
	/// <param name="component">传入脚本（用于判断物体类型）</param>
	public static void MouseEnter(Component component)
	{
		// 端口（写入阶段0）
		if (component is CircuitPort)
		{
			string portTip;
			if (ConnectionManager.clickedPort == null)
			{
				portTip = "单击以连接导线。\n";
			}
			else
			{
				if (ConnectionManager.clickedPort == component as CircuitPort)
				{
					portTip = "在其它接线柱单击，完成连接导线。\n按 鼠标右键 清除当前连接。\n";
				}
				else
				{
					portTip = "在这里单击，完成连接导线。\n按 鼠标右键 清除当前连接。\n";
				}
			}
			ShowTips(portTip, 0);
		}

		// 元件
		else if (component is EntityBase)
		{
			ShowTips("鼠标拖动，移动这个元件。\n按 鼠标中键 摆正元件。\n", 1);
		}

		// 导线
		else if (component is CircuitLine)
		{
			ShowTips("按 鼠标右键 删除这个导线。\n", 1);
		}

		// 滑块
		else if (component is MySlider)
		{
			ShowTips("滑动以调节参数。\n", 1);
		}
	}

	/// <summary>
	/// 鼠标离开
	/// </summary>
	/// <param name="component">传入脚本（用于判断物体类型）</param>
	public static void MouseExit(Component component)
	{
		// 端口（写入阶段0）
		if (component is CircuitPort)
		{
			string portTip;
			if (ConnectionManager.clickedPort == null)
			{
				portTip = "捕捉到接线柱并单击，开始连接导线。\n";
			}
			else
			{
				portTip = "在接线柱处单击，继续连接导线。\n按 鼠标右键 清除当前连接。\n";
			}
			ShowTips(portTip, 0);
		}

		// 其他（元件，导线，滑块等写入阶段1的）
		else
		{
			ShowTips(null, 1);
		}
	}
}
