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
		SetEntityID(t, IDlist);
		return t;
	}

	public static T CreateEntity<T>(Float3 pos, Float4 angle, List<int> IDlist) where T : Component
	{
		GameObject TGameObject = (GameObject)Resources.Load(typeof(T).ToString());
		T t = Instantiate(TGameObject, new Vector3(pos.x, pos.y, pos.z), new Quaternion(angle.x, angle.y, angle.z, angle.w)).GetComponent<T>();
		// 元件执行Awake()后
		SetEntityID(t, IDlist);
		return t;
	}

	public static void SetEntityID<T>(T t,List<int> IDlist) where T : Component
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

	public static ThreeSource CreateThreeSource(ThreeSource.SourceMode sourceMode,Float3 pos, Float4 angle, List<int> IDlist)
	{
		GameObject SourceObject;
		switch (sourceMode)
		{
			case ThreeSource.SourceMode.one:
				SourceObject = (GameObject)Resources.Load("ThreeSource1");
				break;
			case ThreeSource.SourceMode.three:
				SourceObject = (GameObject)Resources.Load("ThreeSource");
				break;
			case ThreeSource.SourceMode.twoOfThree:
				SourceObject = (GameObject)Resources.Load("ThreeSource2");
				break;
			default:
				SourceObject = null;
				break;
		}
		ThreeSource threeSource = Instantiate(SourceObject, new Vector3(pos.x, pos.y, pos.z), new Quaternion(angle.x, angle.y, angle.z, angle.w)).GetComponent<ThreeSource>();
		SetEntityID(threeSource, IDlist);
		return threeSource;
	}
}
