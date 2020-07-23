using System.Collections.Generic;
using UnityEngine;

public delegate void EntityDestroyEventHandler();

abstract public class EntityBase : MonoBehaviour
{
	public int PortNum { get; set; }                                    //本元件的端口数量
	public CircuitPort[] ChildPorts { get; set; } = null;               //端口们的引用
	public List<int> ChildPortID { get; set; } = new List<int>();
	public bool IsIDSet { get; set; } = false;

	public static event EnterEventHandler MouseEnter;
	public static event ExitEventHandler MouseExit;
	public event EntityDestroyEventHandler EntityDestroy;

	void Awake()
	{
		rigidBody = GetComponent<Rigidbody>();
		if (rigidBody == null)
		{
			Debug.LogError("没找到刚体");
		}
		CircuitCalculator.Entities.AddLast(this);
		FindCircuitPort();
		EntityAwake();
	}

	void Start()
	{
		if (!IsIDSet)
		{
			CircuitCalculator.PortNum += ChildPorts.Length;
			IsIDSet = false;
		}
		else
		{
			Debug.Log("PortNum不增长");
		}
	}

	public void FindCircuitPort()
	{
		CircuitPort[] disorderPorts = gameObject.GetComponentsInChildren<CircuitPort>();
		PortNum = disorderPorts.Length;
		ChildPorts = new CircuitPort[disorderPorts.Length];

		// 对获取到的子端口排序
		for (var i = 0; i < PortNum; i++)
		{
			// 名字转换成ID
			int.TryParse(disorderPorts[i].name, out int id);
			ChildPorts[id] = disorderPorts[i];
			ChildPorts[id].ID = id + CircuitCalculator.PortNum;
			ChildPorts[id].Father = this;
			ChildPortID.Add(ChildPorts[id].ID);
		}
		CircuitCalculator.PortNum += disorderPorts.Length;
	}

	//物体控制
	public void OnMouseDrag()
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
		EntityDestroy?.Invoke();
		CircuitCalculator.Entities.Remove(this);
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

	public abstract void EntityAwake();
	public abstract bool IsConnected();
	public abstract void LoadElement();
	public abstract void SetElement();
	public abstract EntityData Save();
}

[System.Serializable]
abstract public class EntityData
{
	public readonly List<int> IDList;
	public readonly Float3 posfloat;
	public readonly Float4 anglefloat;

	public EntityData(Vector3 pos, Quaternion angle, List<int> IDList)
	{
		this.IDList = IDList;
		posfloat = pos.ToFloat3();
		anglefloat = angle.ToFloat4();
	}

	public abstract void Load();
}

[System.Serializable]
public class SimpleEntityData<T> : EntityData where T : Component
{
	public SimpleEntityData(Vector3 pos, Quaternion angle, List<int> IDList) : base(pos, angle, IDList) { }

	public override void Load()
	{
		EntityCreator.CreateEntity<T>(posfloat, anglefloat, IDList);
	}
}

public interface ISource
{
	void GroundCheck();
}

public interface IAmmeter
{
	void CalculateCurrent();
}