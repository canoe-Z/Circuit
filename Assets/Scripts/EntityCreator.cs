using System.Collections.Generic;
using UnityEngine;
using static ThreeSource;

/// <summary>
/// 实例化元件，只负责位置和ID的设定
/// </summary>
public class EntityCreator : MonoBehaviour
{
	/// <summary>
	/// 创建元件
	/// </summary>
	/// <typeparam name="T">类型</typeparam>
	/// <param name="pos">位置</param>
	/// <param name="angle">角度</param>
	/// <param name="IDlist">ID</param>
	/// <returns></returns>
	public static T CreateEntity<T>(Float3 pos = null, Float4 angle = null, List<int> IDlist = null, SourceMode sourceMode = SourceMode.three) where T : Component
	{
		pos = pos ?? Float3.zero;
		angle = angle ?? Float4.identity;

		// 加载预制体
		GameObject TGameObject;

		// 对于可调电源需要预先设定模式
		if (typeof(T).ToString() == "ThreeSource")
		{
			switch (sourceMode)
			{
				case SourceMode.one:
					TGameObject = (GameObject)Resources.Load("ThreeSource1");
					break;
				case SourceMode.three:
					TGameObject = (GameObject)Resources.Load("ThreeSource");
					break;
				case SourceMode.twoOfThree:
					TGameObject = (GameObject)Resources.Load("ThreeSource2");
					break;
				default:
					TGameObject = null;
					break;
			}
		}
		else
		{
			TGameObject = (GameObject)Resources.Load(typeof(T).ToString());
		}
		T t = Instantiate(TGameObject, pos.ToVector3(), angle.ToQuaternion()).GetComponent<T>();

		// 注入ID
		if (IDlist != null) SetEntityID(t, IDlist);
		return t;
	}

	private static void SetEntityID<T>(T t, List<int> IDlist) where T : Component
	{
		if (t is EntityBase entity)
		{
			for (var i = 0; i < entity.PortNum; i++)
			{
				entity.ChildPortID = IDlist;
				entity.ChildPorts[i].ID = IDlist[i];
			}
		}
	}
}
