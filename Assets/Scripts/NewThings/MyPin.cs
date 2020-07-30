using UnityEngine;
using UnityEngine.UI;

public class MyPin : MonoBehaviour
{
	/// <summary>
	/// 期望接收的参数从0-1
	/// </summary>
	public void MyChangePos(float position)
	{
		if (position > 1.1f) position = 1.1f;
		if (position < -0.9f) position = -0.1f;
		nowPos = position;
		changeFlag = true;
	}

	/// <summary>
	/// 设置显示在表盘的字符串、最大刻度值（整数），这个函数可以在任意时候调用
	/// </summary>
	public void MySetString(string danWei, int maxKedu)
	{
		strDanwei = danWei;
		intMaxKedu = maxKedu;
		changeFlag = true;
	}

	bool changeFlag = true;//为了避免奇怪的Bug（主要是Awake和Start的顺序），使用这套系统
	float nowPos = 0;//当前的位置，0-1
	string strDanwei = "uA";//显示在表盘的字符串
	int intMaxKedu = 100;//最大单位

	Transform thePin;//指针
	Text[] txtKedu = new Text[6];
	Text txtDanwei;
	void Start()
	{
		Text[] texts_notOrder = GetComponentsInChildren<Text>();
		foreach (Text textPer in texts_notOrder)
		{
			if (int.TryParse(textPer.name, out int num))
			{
				if (num < txtKedu.Length && num >= 0)
				{
					txtKedu[num] = textPer;
				}
			}
			else if (textPer.name == "Danwei")
			{
				txtDanwei = textPer;
			}
		}
		Transform[] transforms = GetComponentsInChildren<Transform>();
		foreach (Transform tr in transforms)
		{
			if (tr.name == "ThePin")
			{
				thePin = tr;
			}
		}
	}

	void Update()
	{
		if (changeFlag)
		{
			changeFlag = false;
			txtDanwei.text = strDanwei;
			for (int i = 0; i < txtKedu.Length; i++)
			{
				txtKedu[i].text = (i * intMaxKedu / (txtKedu.Length - 1)).ToString();
			}
			thePin.transform.localEulerAngles = new Vector3(0, 0, 50 - 100 * nowPos);//转动
		}
	}
}
