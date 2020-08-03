﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MyPin : MonoBehaviour
{
	private float pos = 0;								//当前的位置，0-1
	private string unitSymbol = "uA";					//显示在表盘的字符串
	private int maxScale = 100;							//最大刻度
	private Transform thePin;							//指针（物理意义）
	private List<Text> ScaleTexts = new List<Text>();	//刻度文本
	private Text unitText;								//单位文本

	public void PinAwake()
	{
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

		// 关闭电压表以及电流表的显示
		if (GetComponentInParent<Ammeter>() || GetComponentInParent<Voltmeter>())
		{
			foreach (var scaleText in ScaleTexts)
			{
				scaleText.enabled = false;
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

	public void SetPos(float newPos)
	{
		if (newPos > 1.1f) newPos = 1.1f;
		if (newPos < -0.9f) newPos = -0.1f;
		pos = newPos;

		// 指针转动
		thePin.transform.localEulerAngles = new Vector3(0, 0, 50 - 100 * pos);
	}

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
