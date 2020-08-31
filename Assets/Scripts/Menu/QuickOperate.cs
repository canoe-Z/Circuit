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
		EventSystem[] eventSystems = FindObjectsOfType<EventSystem>();
		foreach(var e in eventSystems)
		{
			Debug.Log(e.gameObject);
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
