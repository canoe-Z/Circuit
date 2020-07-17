using UnityEngine;

abstract public class EntityBase : MonoBehaviour
{
	private int portNum;									//本元件的端口数量
	public CircuitPort[] ChildPorts { get; set; } = null;	//端口们的引用

	public static event EnterEventHandler MouseEnter;
	public static event ExitEventHandler MouseExit;

	public void FindCircuitPort()
	{
		CircuitPort[] disorderPorts = this.gameObject.GetComponentsInChildren<CircuitPort>();
		portNum = disorderPorts.Length;
		ChildPorts = new CircuitPort[disorderPorts.Length];

		// 对获取到的子端口排序
		for (var i = 0; i < portNum; i++)
		{
			// 名字转换成ID
			int.TryParse(disorderPorts[i].name, out int id);
			ChildPorts[id] = disorderPorts[i];
			ChildPorts[id].PortID = id + CircuitCalculator.PortNum;
		}
		CircuitCalculator.PortNum += disorderPorts.Length;
	}

	//物体控制
	public void OnMouseDrag()
	{
		if (!MoveController.CanMove) return;
		if (HitCheck("Table", out Vector3 hitPos))
		{
			transform.position = hitPos;
		}
		else
		{
			Vector3 campos = Camera.main.transform.position;
			Vector3 thispos = this.gameObject.transform.position;
			float dis = (thispos - campos).magnitude;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			Vector3 vec = ray.direction;
			vec.Normalize();
			vec *= dis;
			thispos = campos + vec;
			this.gameObject.transform.position = thispos;
		}
	}

	void OnMouseEnter()
	{
		if (!MoveController.CanMove) return;
		MouseEnter?.Invoke(this);
	}

	void OnMouseOver()
	{
		if (!MoveController.CanMove) return;

		// 按鼠标中键摆正元件
		if (Input.GetMouseButtonDown(2))
		{
			gameObject.transform.eulerAngles = new Vector3(0, 0, 0);
		}
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
			if (tag == null || hitedItem.tag == tag)
			{
				hitPos = hitObj[i].point;
				return true;
			}
		}
		return false;
	}

	//速度限制
	Rigidbody rigidBody;
	float speedLimit = 1f;
	private void FixedUpdate()//与物理引擎保持帧同步
	{
		if (rigidBody)
		{
			if (rigidBody.velocity.magnitude > speedLimit)
			{
				rigidBody.velocity = rigidBody.velocity.normalized * speedLimit;
				
			}
		}
		else
		{
			rigidBody = GetComponent<Rigidbody>();
			if (rigidBody == null)
			{
				Debug.LogError("没找到刚体");
			}
		}
	}

	abstract public bool IsConnected();
	abstract public void LoadElement();
	abstract public void SetElement();

}

public interface ISource
{
	void GroundCheck();
}

public interface IAmmeter
{
	void CalculateCurrent();
}