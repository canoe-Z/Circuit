using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
/// <summary>
/// 编译前的操作命令，用于批量处理奇怪的东西
/// </summary>
[ExecuteInEditMode]
public class QuickOperate : MonoBehaviour
{
	void Work()
	{
		InputChecker[] inputCheckers = FindObjectsOfType<InputChecker>();
		foreach(var i in inputCheckers)
		{
			Transform now = i.transform;
			string outS = "";
			while (now)
			{
				outS += now.gameObject.name;
				now = now.parent;
			}
			Debug.Log(outS);
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
