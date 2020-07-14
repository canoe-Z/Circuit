using UnityEngine;

abstract public class EntityBase : MonoBehaviour
{
	private int portNum;//本元件的端口数量
	public CircuitPort[] ChildPorts { get; set; } = null;//端口们的引用
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
			ChildPorts[id].PortID = id;
			ChildPorts[id].PortID_Global = id + CircuitCalculator.PortNum;
			ChildPorts[id].father = this;
		}
		CircuitCalculator.PortNum += disorderPorts.Length;
	}

	//物体控制
	public void OnMouseDrag()
	{
		if (!MoveController.CanMove) return;
		if (HitCheck("Table", out Vector3 hitPos))
		{
			this.transform.position = hitPos;
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
	public void OnMouseOver()//持续期间
	{
		if (!MoveController.CanMove) return;
		ShowTip.OverItem(this);
		ShowTip.IsTipShowed = false;
	}
	public void Straighten()//摆正元件
	{
		this.gameObject.transform.eulerAngles = new Vector3(0, 0, 0);
	}
	public static bool HitCheck(string tag, out Vector3 hitPos)
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

	abstract public bool IsConnected();
	abstract public void LoadElement();
	abstract public void SetElement();

	//
	//速度限制
	//

	Rigidbody rigidBody;
	float speedLimit = 0.5f;
	private void Start()
	{
		rigidBody = GetComponent<Rigidbody>();
		if (rigidBody == null)
		{
			Debug.LogError("没找到刚体");
		}
	}
	private void FixedUpdate()//与物理引擎保持帧同步
	{
		if (rigidBody)
		{
			if (rigidBody.velocity.magnitude > speedLimit)
			{
				rigidBody.velocity = rigidBody.velocity.normalized * speedLimit;
			}
		}
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