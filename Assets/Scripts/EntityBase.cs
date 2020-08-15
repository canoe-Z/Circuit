using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 找端口，需要带有CircuitPort的端口，把自己重命名成序号
/// </summary>
abstract public class EntityBase : MonoBehaviour
{
	public int PortNum { get; set; }                                        //本元件的端口数量
	public List<CircuitPort> ChildPorts { get; set; }                       //端口们的引用
	public List<int> ChildPortID { get; set; }

	public static event EnterEventHandler MouseEnter;
	public static event ExitEventHandler MouseExit;

	public delegate void EntityDestroyEventHandler();
	public event EntityDestroyEventHandler EntityDestroy;

	void Awake()
	{
		// 找刚体
		rigidBody = GetComponent<Rigidbody>();
		if (rigidBody == null)
		{
			Debug.LogError("没找到刚体");
		}

		// 找端口的引用
		ChildPorts = FindCircuitPort();
		PortNum = ChildPorts.Count;
		ChildPortID = ChildPorts.Select(x => x.ID).ToList();

		// 纳入Calculator管理
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
		if (HitCheck("Table", out Vector3 hitPos))
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
		if (!MoveController.CanOperate) return;
		MouseEnter?.Invoke(this);
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

	void OnMouseExit()
	{
		MouseExit?.Invoke(this);
	}

	private static bool HitCheck(string tag, out Vector3 hitPos)
	{
		hitPos = new Vector3(0, 0, 0);
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit[] hitObj;
		hitObj = Physics.RaycastAll(ray);

		for (int i = 0; i < hitObj.Length; i++)
		{
			GameObject hitedItem = hitObj[i].collider.gameObject;
			if (tag == null || hitedItem.CompareTag(tag))
			{
				hitPos = hitObj[i].point;
				return true;
			}
		}
		return false;
	}

	//速度限制
	Rigidbody rigidBody;
	readonly float speedLimit = 1f;
	private void FixedUpdate()//与物理引擎保持帧同步
	{
		if (rigidBody.velocity.magnitude > speedLimit)
		{
			rigidBody.velocity = rigidBody.velocity.normalized * speedLimit;

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