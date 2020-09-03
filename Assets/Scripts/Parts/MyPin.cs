using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 刻度盘
/// 需要手动初始化
/// </summary>
public class MyPin : MonoBehaviour
{
	public float pos = 0;                               //当前的位置，0-1
	private string unitSymbol = "uA";                   //显示在表盘的字符串
	private int maxScale = 100;                         //最大刻度
	private Transform thePin;                           //指针（物理意义）

	private List<Text> ScaleTexts = new List<Text>();   //刻度文本
	private Text unitText;                              //单位文本

	public void PinAwake()
	{
		// 获取刻度文本和单位文本
		ScaleTexts = GetComponentsInChildren<Text>()
			.Where(x =>
			{
				if (int.TryParse(x.name, out int num))
				{
					return true;
				}
				else
				{
					if (x.name == "Danwei") unitText = x;
					return false;
				}
			})
			.OrderBy(x => x.name)
			.ToList();

		thePin = transform.FindComponent_DFS<Transform>("ThePin");
	}

	/// <summary>
	/// 不显示刻度
	/// </summary>
	public void CloseText() => ScaleTexts.ForEach(x => x.enabled = false);

	/// <summary>
	/// 接收0-1的值，最左端到最右端
	/// </summary>
	public void SetPos(float newPos)
	{
		if (newPos > 1.1f) newPos = 1.1f;
		if (newPos < -0.1f) newPos = -0.1f;
		pos = newPos;

		// 指针转动
		thePin.transform.localEulerAngles = new Vector3(0, 0, 50 - 100 * pos);
	}

	public void SetPos(double newPos) => SetPos((float)newPos);

	/// <summary>
	/// 设置字符串
	/// </summary>
	/// <param name="unit">单位</param>
	/// <param name="scaleMax">最大刻度</param>
	public void SetString(string unit, int scaleMax)
	{
		unitSymbol = unit;
		maxScale = scaleMax;

		unitText.text = unitSymbol;

		for (int i = 0; i < ScaleTexts.Count; i++)
		{
			ScaleTexts[i].text = (i * maxScale / (ScaleTexts.Count - 1)).ToString();
		}
	}
}
