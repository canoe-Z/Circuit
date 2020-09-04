﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.BaseShaderGUI;

/// <summary>
/// 找端口，需要带有CircuitPort的端口，把自己重命名成序号
/// </summary>
abstract public class EntityBase : MonoBehaviour
{
	public int PortNum { get; set; }                                        //本元件的端口数量
	public List<CircuitPort> ChildPorts { get; set; }                       //端口们的引用
	public List<int> ChildPortID { get; set; }

	//private bool isEmission = false;

	// 元件删除事件
	public delegate void EntityDestroyEventHandler();
	public event EntityDestroyEventHandler EntityDestroy;

	void Awake()
	{
		// 获取刚体引用
		rigidBody = transform.SafeGetComponent<Rigidbody>();

		// 获取端口引用
		ChildPorts = FindCircuitPort();
		PortNum = ChildPorts.Count;
		ChildPortID = ChildPorts.Select(x => x.ID).ToList();

		// 元件纳入Calculator管理
		CircuitCalculator.Entities.AddLast(this);

		// 执行子类的Awake()
		EntityAwake();
	}

	private List<CircuitPort> FindCircuitPort()
	{
		List<CircuitPort> circuitPorts = new List<CircuitPort>();
		CircuitPort[] disorderPorts = GetComponentsInChildren<CircuitPort>();
		foreach (CircuitPort port in disorderPorts)
		{
			port.Father = this;
			EntityDestroy += port.DestroyPort;
			port.LocalID = int.Parse(port.name);
			port.ID = port.LocalID + CircuitCalculator.PortNum;
			circuitPorts.Add(port);
		}
		CircuitCalculator.PortNum += circuitPorts.Count;
		circuitPorts.Sort((x, y) => { return x.name.CompareTo(y.name); });

		foreach (CircuitPort port in circuitPorts)
		{
			if (circuitPorts.IndexOf(port) != port.LocalID) Debug.LogError("端口ID有误");
		}

		return circuitPorts;
	}

	// 元件移动
	void OnMouseDrag()
	{
		if (!MoveController.CanOperate) return;

		if (HitCheckTable(out Vector3 hitPos))
		{
			transform.position = hitPos;
		}
		else
		{
			Vector3 campos = Camera.main.transform.position;
			Vector3 thispos = gameObject.transform.position;
			float dis = (thispos - campos).magnitude;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			Vector3 vec = ray.direction;
			vec.Normalize();
			vec *= dis;
			thispos = campos + vec;
			gameObject.transform.position = thispos;
		}
	}

	void OnMouseEnter()
	{
		Renderer[] renderers = GetComponentsInChildren<Renderer>();
		foreach(var renderer in renderers)
		{
			if(renderer.material.shader.name== "Shader Graphs/MyShader")
            {
				Debug.Log("hello");
            }
			renderer.material.SetColor("Color_592D9D79", Color.yellow);
		}
	}
	void OnMouseExit()
	{
		Renderer[] renderers = GetComponentsInChildren<Renderer>();
		foreach (var renderer in renderers)
		{
			renderer.material.SetColor("Color_592D9D79", Color.black);

		}
	}

	void OnMouseOver()
	{
		if (!MoveController.CanOperate) return;

		// 按鼠标中键摆正元件
		if (Input.GetMouseButtonDown(2))
		{
			gameObject.transform.eulerAngles = new Vector3(0, 0, 0);
		}

		if (Input.GetMouseButtonDown(1))
		{
			DestroyEntity();
		}
	}

	public void DestroyEntity()
	{
		if (this is ICalculatorUpdate updateEntity)
		{
			CircuitCalculator.CalculateEvent -= updateEntity.CalculatorUpdate;
		}
		EntityDestroy?.Invoke();
		CircuitCalculator.Entities.Remove(this);
		CircuitCalculator.NeedCalculate = true;
		Destroy(gameObject);
	}

	/// <summary>
	/// 得到打击到桌子的点
	/// </summary>
	private static bool HitCheckTable(out Vector3 hitPos)
	{
		RaycastHit info;
		Transform tr = SmallCamManager.MainCam.transform;
		if (Physics.Raycast(tr.position, tr.forward, out info, 2000, 1 << 11))
		{
			hitPos = info.point;
			return true;
		}
		else
		{
			hitPos = Vector3.zero;
			return false;
		}
	}

	//速度限制
	Rigidbody rigidBody;
	bool isOnTable = true;
	const float speedLimit = 1f;
	const float speedLimit_Y = 1f;

	void FixedUpdate()//与物理引擎保持帧同步
	{
		//奇怪的东西
		//deltaPos *= 0.9f;

		Vector3 speed = rigidBody.velocity;
		//备份Y
		float spdY = speed.y;
		speed.y = 0;
		//两个方向的速度限制
		if (isOnTable)
		{
			//竖直方向速度限制
			if (spdY > speedLimit_Y) spdY = speedLimit_Y;
			if (speed.magnitude > speedLimit)
			{
				speed = speed.normalized * speedLimit;
			}
		}
		//还原Y
		speed.y = spdY;
		rigidBody.velocity = speed;

		//bool值
		isOnTable = false;
	}

	void OnTriggerStay(Collider other)
	{
		if (other.attachedRigidbody.gameObject.layer == 12)
		{
			isOnTable = true;
		}
	}

	public virtual bool IsConnected() => ChildPorts.Select(x => x.IsConnected).Contains(true);

	public abstract void EntityAwake();

	public abstract void LoadElement();

	public abstract void SetElement(int entityID);

	public abstract EntityData Save();

	public static T BaseCreate<T>(EntityBaseData? baseData = null, string prefabName = null) where T : Component
	{
		// 加载预制体
		GameObject TGameObject;
		prefabName = prefabName ?? typeof(T).ToString();
		TGameObject = (GameObject)Resources.Load(prefabName);

		T t;
		if (baseData == null)
		{
			t = Instantiate(TGameObject, Vector3.zero, Quaternion.identity).GetComponent<T>();
		}
		else
		{
			t = Instantiate(TGameObject, baseData.Value.pos.ToVector3(), baseData.Value.angle.ToQuaternion()).GetComponent<T>();
			// 注入ID
			SetEntityID(t, baseData.Value.IDList);
		}

		return t;
	}

	protected static void SetEntityID<T>(T t, List<int> IDlist) where T : Component
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

[System.Serializable]
public struct EntityBaseData
{
	public readonly Float3 pos;
	public readonly Float4 angle;
	public readonly List<int> IDList;

	public EntityBaseData(EntityBase entity)
	{
		IDList = entity.ChildPortID;
		pos = entity.transform.position.ToFloat3();
		angle = entity.transform.rotation.ToFloat4();
	}
}

[System.Serializable]
abstract public class EntityData
{
	protected EntityBaseData baseData;

	public abstract void Load();
}


[System.Serializable]
public class SimpleEntityData<T> : EntityData where T : Component
{
	public SimpleEntityData(EntityBase simpleEntity)
	{
		baseData = new EntityBaseData(simpleEntity);
	}

	public override void Load() => EntityBase.BaseCreate<T>(baseData);
}

public interface ISource
{
	void GroundCheck();
}

public interface ICalculatorUpdate
{
	void CalculatorUpdate();
}