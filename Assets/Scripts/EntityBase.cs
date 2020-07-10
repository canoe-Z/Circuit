using UnityEngine;
abstract public class EntityBase : MonoBehaviour
{
	public int PortNum;//本元件的端口数量
	public CircuitPort[] childsPorts = null;//端口们的引用
	public void FindCircuitPort()
	{
		CircuitPort[] disorderPorts = this.gameObject.GetComponentsInChildren<CircuitPort>();//寻找所有挂脚本的子物体
		PortNum = disorderPorts.Length;//元件的端口数量
		childsPorts = new CircuitPort[disorderPorts.Length];//开个数组存引用
		for (int i = 0; i < PortNum; i++)//排个序
		{
			int.TryParse(disorderPorts[i].name, out int ID); //名字转换成ID
			childsPorts[ID] = disorderPorts[i];
			childsPorts[ID].PortID = ID;
			childsPorts[ID].PortID_Global = ID + CircuitCalculator.PortNum;
			childsPorts[ID].father = this;
		}
		CircuitCalculator.PortNum += disorderPorts.Length; //全局ID++
	}

	//物体控制
	public void OnMouseDrag()
	{
		if (!MoveController.boolMove) return;
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
		if (!MoveController.boolMove) return;
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
}

interface ICurrent
{
	bool IsConnected(int n);
	void LoadElement(int n);
	void SetElement(int n);
}