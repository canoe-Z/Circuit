using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public delegate void EntityDestroyEventHandler();

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

	public virtual bool IsConnected()
	{
		foreach (CircuitPort port in ChildPorts)
		{
			if (port.Connected == 1)
			{
				return true;
			}
		}
		return false;
	}

	public abstract void EntityAwake();
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