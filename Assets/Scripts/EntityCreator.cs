using System.Collections.Generic;
using UnityEngine;

public class EntityCreator : MonoBehaviour
{
	public static T CreateEntity<T>() where T : Component
	{
		GameObject TGameObject = (GameObject)Resources.Load(typeof(T).ToString());
		return Instantiate(TGameObject, Vector3.zero, Quaternion.identity).GetComponent<T>();
	}

	public static T CreateEntity<T>(Vector3 pos) where T : Component
	{
		GameObject TGameObject = (GameObject)Resources.Load(typeof(T).ToString());
		return Instantiate(TGameObject, pos, Quaternion.identity).GetComponent<T>();
	}

	public static T CreateEntity<T>(Float3 pos) where T : Component
	{
		GameObject TGameObject = (GameObject)Resources.Load(typeof(T).ToString());
		return Instantiate(TGameObject, new Vector3(pos.x, pos.y, pos.z), Quaternion.identity).GetComponent<T>();
	}

	public static T CreateEntity<T>(Float3 pos, List<int> IDlist) where T : Component
	{
		GameObject TGameObject = (GameObject)Resources.Load(typeof(T).ToString());
		T t = Instantiate(TGameObject, new Vector3(pos.x, pos.y, pos.z), Quaternion.identity).GetComponent<T>();
		for (var i = 0; i < (t as EntityBase).PortNum; i++)
		{
			(t as EntityBase).ChildPortID = IDlist;
			(t as EntityBase).ChildPorts[i].ID = IDlist[i];
		}
		(t as EntityBase).IsIDSet = true;
		return t;
	}

	public static T CreateEntity<T>(Float3 pos, Float4 angle, List<int> IDlist) where T : Component
	{
		GameObject TGameObject = (GameObject)Resources.Load(typeof(T).ToString());
		T t = Instantiate(TGameObject, new Vector3(pos.x, pos.y, pos.z), new Quaternion(angle.x, angle.y, angle.z, angle.w)).GetComponent<T>();
		for (var i = 0; i < (t as EntityBase).PortNum; i++)
		{
			(t as EntityBase).ChildPortID = IDlist;
			(t as EntityBase).ChildPorts[i].ID = IDlist[i];
		}
		(t as EntityBase).IsIDSet = true;
		return t;
	}
}
