using UnityEngine;

/// <summary>
/// 用于显示光标，控制颜色和显示提示
/// </summary>
public class DisplayController : MonoBehaviour
{
	public static int ColorID { get; set; } = 0;
	private static readonly int colorMax = 5;
	private static readonly Texture2D[] mouseTex = new Texture2D[5];
	private static Rect rectTex = new Rect(0, 0, 100, 100);

	void Awake()
	{
		// 加载光标颜色
		mouseTex[0] = (Texture2D)Resources.Load("Cross");
		mouseTex[1] = (Texture2D)Resources.Load("CrossWhite");
		mouseTex[2] = (Texture2D)Resources.Load("CrossRed");
		mouseTex[3] = (Texture2D)Resources.Load("CrossYellow");
		mouseTex[4] = (Texture2D)Resources.Load("CrossGreen");
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
		float mousex = Input.mousePosition.x;
		float mousey = Screen.height - Input.mousePosition.y;

		// 绘制光标
		rectTex.x = mousex - rectTex.width / 2;
		rectTex.y = mousey - rectTex.height / 2;
		GUI.DrawTexture(rectTex, mouseTex[ColorID]);
	}
}
