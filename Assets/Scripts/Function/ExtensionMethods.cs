using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 代替Vector3以序列化
/// </summary>
[System.Serializable]
public class Float3
{
	public float x, y, z;
	public static Float3 zero = new Float3(0, 0, 0);

	public Vector3 ToVector3()
	{
		return new Vector3(x, y, z);
	}

	public Float3(float x, float y, float z)
	{
		this.x = x;
		this.y = y;
		this.z = z;
	}
}

/// <summary>
/// 代替Quaternion以序列化
/// </summary>
[System.Serializable]
public class Float4
{
	public float x, y, z, w;
	public static Float4 identity = new Float4(0, 0, 0, 1);

	public Quaternion ToQuaternion()
	{
		return new Quaternion(x, y, z, w);
	}

	public Float4(float x, float y, float z, float w)
	{
		this.x = x;
		this.y = y;
		this.z = z;
		this.w = w;
	}
}

/// <summary>
/// Vector3拓展方法
/// </summary>
public static class Vector3Extensions
{
	public static Float3 ToFloat3(this Vector3 vector)
	{
		return new Float3(vector.x, vector.y, vector.z);
	}
}

/// <summary>
/// Quaternion拓展方法
/// </summary>
public static class QuaternionExtensions
{
	public static Float4 ToFloat4(this Quaternion quaternion)
	{
		return new Float4(quaternion.x, quaternion.y, quaternion.z, quaternion.w);
	}
}

/// <summary>
/// Transform拓展方法
/// </summary>
public static partial class TransformExtensions
{
	/// <summary>
	/// 广度优先搜索遍历
	/// </summary>
	/// <param name="root"></param>
	/// <typeparam name="TP">遍历时调用的函数的参数的类型</typeparam>
	/// <typeparam name="TR">遍历时调用的函数的返回值的类型</typeparam>
	/// <param name="visitFunc">遍历时调用的函数
	/// <para>TR Function(Transform t, T para)</para>
	/// </param>
	/// <param name="para">遍历时调用的函数的第二个参数</param>
	/// <param name="failReturnValue">遍历时查找失败的返回值</param>
	/// <returns>遍历时调用的函数的返回值</returns>
	public static TR BFSVisit<TP, TR>(this Transform root, System.Func<Transform, TP, TR> visitFunc, TP para, TR failReturnValue = default)
	{
		TR ret = visitFunc(root, para);
		if (ret != null && !ret.Equals(failReturnValue))
			return ret;
		Queue<Transform> parents = new Queue<Transform>();
		parents.Enqueue(root);
		while (parents.Count > 0)
		{
			Transform parent = parents.Dequeue();
			foreach (Transform child in parent)
			{
				ret = visitFunc(child, para);
				if (ret != null && !ret.Equals(failReturnValue))
					return ret;
				parents.Enqueue(child);
			}
		}
		return failReturnValue;
	}

	/// <summary>
	/// 深度优先搜索遍历
	/// </summary>
	/// <param name="root"></param>
	/// <typeparam name="TP">遍历时调用的函数的参数的类型</typeparam>
	/// <typeparam name="TR">遍历时调用的函数的返回值的类型</typeparam>
	/// <param name="visitFunc">遍历时调用的函数
	/// <para>TR Function(Transform t, T para)</para>
	/// </param>
	/// <param name="para">遍历时调用的函数的第二个参数</param>
	/// <param name="failReturnValue">遍历时查找失败的返回值</param>
	/// <returns>遍历时调用的函数的返回值</returns>
	public static TR DFSVisit<TP, TR>(this Transform root, System.Func<Transform, TP, TR> visitFunc, TP para, TR failReturnValue = default)
	{
		Stack<Transform> parents = new Stack<Transform>();
		parents.Push(root);
		while (parents.Count > 0)
		{
			Transform parent = parents.Pop();
			TR ret = visitFunc(parent, para);
			if (ret != null && !ret.Equals(failReturnValue))
				return ret;
			for (int i = parent.childCount - 1; i >= 0; i--)
			{
				parents.Push(parent.GetChild(i));
			}
		}
		return failReturnValue;
	}

	/// <summary>
	/// 根据名字查找并返回子孙，广度优先搜索
	/// </summary>
	/// <param name="trans"></param>
	/// <param name="childName">要查找的子孙的名字</param>
	/// <returns>要查找的子孙的Transform</returns>
	public static T FindComponent_BFS<T>(this Transform trans, string childName) where T : Component
	{
		var target = BFSVisit<string, Transform>(trans,
			(t, str) => { if (t.name.Equals(str)) return t; return null; },
			childName
		);

		if (target == null)
		{
			Debug.LogError(string.Format("Cann't Find Child Transform {0} in {1}", childName, trans.gameObject.name));
			return null;
		}

		T component = target.GetComponent<T>();
		if (component == null)
		{
			Debug.LogError("Component is null, Type = " + typeof(T).Name);
			return null;
		}
		return component;
	}

	/// <summary>
	/// 根据Tag查找并返回子孙，广度优先搜索
	/// </summary>
	/// <param name="trans"></param>
	/// <param name="tagName">要查找的子孙的名字</param>
	/// <returns>要查找的子孙的Transform</returns>
	public static Transform FindChild_ByTag(this Transform trans, string tagName)
	{
		var target = BFSVisit<string, Transform>(trans,
			(t, str) => { if (t.CompareTag(str)) return t; return null; },
			tagName
		);

		if (target == null)
		{
			Debug.LogError(string.Format("Cann't Find Child Transform {0} in {1}", tagName, trans.gameObject.name));
			return null;
		}

		return target;
	}

	/// <summary>
	/// 根据名字查找并返回子孙，深度优先搜索
	/// </summary>
	/// /// <param name="trans"></param>
	/// <param name="childName">要查找的子孙的名字</param>
	/// <returns>要查找的子孙的Transform</returns>
	public static T FindComponent_DFS<T>(this Transform trans, string childName) where T : Component
	{
		var target = DFSVisit<string, Transform>(trans,
			(t, str) => { if (t.name.Equals(str)) return t; return null; },
			childName
		);

		if (target == null)
		{
			Debug.LogWarning(string.Format("Cann't Find Child Transform {0} in {1}", childName, trans.gameObject.name));
			return null;
		}

		T component = target.GetComponent<T>();
		if (component == null)
		{
			Debug.LogError("Component is null, Type = " + typeof(T).Name);
			return null;
		}
		return component;
	}

	/// <summary>
	/// 查找指定名称子对象的指定脚本，不能查找孙节点
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="trans"></param>
	/// <param name="name"></param>
	/// /// <param name="reportError"></param>
	/// <returns></returns>
	public static T FindComponent<T>(this Transform trans, string name, bool reportError = true) where T : Component
	{
		Transform target = trans.Find(name);
		if (target == null)
		{
			if (reportError)
			{
				Debug.LogError("Transform is null, name = " + name);
			}

			return null;
		}

		T component = target.GetComponent<T>();
		if (component == null)
		{
			if (reportError)
			{
				Debug.LogError("Component is null, type = " + typeof(T).Name);
			}

			return null;
		}

		return component;
	}

	/// <summary>
	/// 初始化物体的相对位置、旋转、缩放
	/// </summary>
	/// <param name="trans"></param>
	public static void InitTransformLocal(this Transform trans)
	{
		trans.localPosition = Vector3.zero;
		trans.localScale = Vector3.one;
		trans.localRotation = Quaternion.identity;
	}

	/// <summary>
	/// 直接设置物体x轴的世界坐标
	/// </summary>
	/// <param name="trans"></param>
	/// <param name="x"></param>
	public static void SetPositionX(this Transform trans, float x)
	{
		var position = trans.position;
		position = new Vector3(x, position.y, position.z);
		trans.position = position;
	}

	/// <summary>
	/// 直接设置物体y轴的世界坐标
	/// </summary>
	/// <param name="trans"></param>
	/// <param name="y"></param>
	public static void SetPositionY(this Transform trans, float y)
	{
		var position = trans.position;
		position = new Vector3(position.x, y, position.z);
		trans.position = position;
	}

	/// <summary>
	/// 直接设置物体z轴的世界坐标
	/// </summary>
	/// <param name="trans"></param>
	/// <param name="z"></param>
	public static void SetPositionZ(this Transform trans, float z)
	{
		var position = trans.position;
		position = new Vector3(position.x, position.y, z);
		trans.position = position;
	}

	/// <summary>
	/// 直接设置物体x轴的本地坐标
	/// </summary>
	/// <param name="trans"></param>
	/// <param name="x"></param>
	public static void SetLocalPositionX(this Transform trans, float x)
	{
		var localPosition = trans.localPosition;
		localPosition = new Vector3(x, localPosition.y, localPosition.z);
		trans.localPosition = localPosition;
	}

	/// <summary>
	/// 直接设置物体y轴的本地坐标
	/// </summary>
	/// <param name="trans"></param>
	/// <param name="y"></param>
	public static void SetLocalPositionY(this Transform trans, float y)
	{
		var localPosition = trans.localPosition;
		localPosition = new Vector3(localPosition.x, y, localPosition.z);
		trans.localPosition = localPosition;
	}

	/// <summary>
	/// 直接设置物体z轴的本地坐标
	/// </summary>
	/// <param name="trans"></param>
	/// <param name="z"></param>
	public static void SetLocalPositionZ(this Transform trans, float z)
	{
		var localPosition = trans.localPosition;
		localPosition = new Vector3(localPosition.x, localPosition.y, z);
		trans.localPosition = localPosition;
	}

	/// <summary>
	/// 递归查找所有子节点的某个T类型的组件
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="transform"></param>
	/// <param name="recursive"></param>
	/// <param name="includeInactive"></param>
	/// <returns></returns>
	public static List<T> FindComponentsInChildren<T>(this Transform transform, bool recursive = true, bool includeInactive = true) where T : Component
	{
		List<T> list = new List<T>();
		if (recursive)
		{
			GetChildren(transform, includeInactive, ref list);
		}
		else
		{
			T[] Components = transform.GetComponentsInChildren<T>(includeInactive);
			foreach (T t in Components)
			{
				list.Add(t);
			}
		}
		return list;
	}

	public static Transform GetChildByName(this Transform transform, string name, bool includeInactive = true)
	{
		Transform target;
		for (int i = 0; i < transform.childCount; i++)
		{
			Transform child = transform.GetChild(i);
			if (child != null && (includeInactive || transform.gameObject.activeSelf))
			{
				if (child.name == name)
				{
					return child;
				}
				else
				{
					target = child.GetChildByName(name);
					if (target != null) return target;
				}
			}
		}
		return null;
	}

	private static void GetChildren<T>(Transform t, bool includeInactive, ref List<T> list)
	{
		if (includeInactive || t.gameObject.activeSelf)
		{
			for (int i = 0; i < t.childCount; i++)
			{
				if (t.GetChild(i) != null)
				{
					GetChildren(t.GetChild(i), includeInactive, ref list);
				}
			}

			var comp = t.GetComponent<T>();
			if (comp != null)
			{
				list.Add(comp);
			}
		}
	}

	public static T SafeGetComponent<T>(this Transform transform) where T : Component
	{
		T t = transform.GetComponent<T>();
		if (t == null)
		{
			Debug.LogError("找不到指定脚本" + typeof(T).ToString());
			return null;
		}
		else
		{
			return t;
		}
	}

	public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> self) => self.Select((item, index) => (item, index));
}