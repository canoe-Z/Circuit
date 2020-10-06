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
	const float zeroRange = 0.1f;//浮动最大范围占整个表盘的比例
	public MyKnob knobZero;

	/// <summary>
	/// 设置调零随机值（打乱调零）
	/// </summary>
	public bool myRandomFlag { set; private get; } = false;

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
			.OrderBy(x => int.Parse(x.name))
			.ToList();

		thePin = transform.FindComponent_DFS<Transform>("ThePin");

		if (knobZero)
		{
			knobZero.CanLoop = false;
			knobZero.AngleRange = 360;
			knobZero.Devide = -1;
			knobZero.SetKnobRot(0.5f);
		}
	}

	const float f = 0.2f;//阻尼系数
	const float m = 0.1f;//质量
	const float k = 1;//劲度系数
	float nowSpeed = 0;//当前速度
	float willPos = 0;//目标位置
	float nowPos = 0;//当前的位置

	private void Update()
	{
		if (myRandomFlag)
		{
			myRandomFlag = false;
			int a = Random.Range(0, 2);//0-1的随机数
			float f = 0.7f * a + Random.Range(0, 0.3f);//0-0.3，0.7-1
			if (knobZero) knobZero.SetKnobRot(f);
		}

		if (MySettings.openMyPinDamping)
		{
			float deltaPos = willPos - nowPos;//距离差值
			float force = k * deltaPos;//力
			float a = (force - nowSpeed * f) / m;//加速度
			nowSpeed += a * Time.deltaTime;//对时间积分
			nowPos += nowSpeed * Time.deltaTime;//再次积分

			if (nowPos > 1.5f)//限制在-0.5到1.5范围内，并且不让速度过大
			{
				nowPos = 1.5f;
				if (nowSpeed > 0) nowSpeed = 0;
			}
			else if (nowPos < -0.5f)
			{
				nowPos = -0.5f;
				if (nowSpeed < 0) nowSpeed = 0;
			}
		}
		else
		{
			nowSpeed = 0;
			nowPos = willPos;
		}

		float posToSet = nowPos;//即将设置的位置
		if (knobZero) posToSet += (knobZero.KnobPos - 0.5f) * zeroRange;//如果有调零的话

		if (posToSet > 1.1f) posToSet = 1.1f;
		if (posToSet < -0.1f) posToSet = -0.1f;
		// 指针转动
		thePin.transform.localEulerAngles = new Vector3(0, 0, 50 - 100 * posToSet);
	}


	private string unitSymbol = "uA";                   //显示在表盘的字符串
	private int maxScale = 100;                         //最大刻度
	private Transform thePin;                           //指针（物理意义）

	private List<Text> ScaleTexts = new List<Text>();   //刻度文本
	private Text unitText;                              //单位文本

	/// <summary>
	/// 不显示刻度
	/// </summary>
	public void CloseText() => ScaleTexts.ForEach(x => x.enabled = false);

	/// <summary>
	/// 接收0-1的值，实际上你可以随意设置，最左端到最右端
	/// </summary>
	public void SetPos(float newPos)
	{
		willPos = newPos;
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
