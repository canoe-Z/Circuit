using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 编译前的操作命令，用于批量处理奇怪的东西
/// </summary>
[ExecuteInEditMode]
public class QuickOperate : MonoBehaviour
{
	void Work()
	{
		MySlider[] mySliders = FindObjectsOfType<MySlider>();
		foreach (var sl in mySliders)
		{
			Debug.Log(sl.transform.root.gameObject);
		}
	}




	public bool boolStartWork = false;
	void Update()
	{
		if (boolStartWork)
		{
			boolStartWork = false;
			Debug.Log("开始");
			Work();
			Debug.Log("结束");
		}
	}
}
