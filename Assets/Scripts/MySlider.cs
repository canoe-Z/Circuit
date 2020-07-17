using UnityEngine;

public class MySlider : MonoBehaviour
{
	public int Devide { get; set; } = -1;
	public int SliderID { get; set; }
	public float SliderPos { get; set; } = 0;
	public int SliderPos_int { get; set; } = 0;

	public static event EnterEventHandler MouseEnter;
	public static event ExitEventHandler MouseExit;

	void Awake()//防止爹比儿子先出来
	{
		if (int.TryParse(this.gameObject.name, out int id))
			SliderID = id;
		else
			Debug.LogError("ErrorSliderID");
	}

	void OnMouseEnter()
	{
		if (!MoveController.CanOperate) return;
		MouseEnter?.Invoke(this);
	}

	void OnMouseOver()
	{
		if (!MoveController.CanOperate) return;
		MouseEnter?.Invoke(this);
	}

	void OnMouseExit()
	{
		if (!MoveController.CanOperate) return;
		MouseExit?.Invoke(this);
	}

	void OnMouseDrag()
	{
		if (!MoveController.CanOperate) return;
		if (HitOnlyOne(out Vector3 hitPos))//打到就算
		{
			CircuitCalculator.CalculateByConnection();
			Vector3 localPos = transform.parent.InverseTransformPoint(hitPos);//转换成本地坐标
			localPos.x = 0;
			localPos.y = 0;
			ChangeSliderPos(localPos.z);
		}
	}
	public void ChangeSliderPos(float newPos)//包含检查
	{
		if (newPos > 1) newPos = 1;
		else if (newPos < 0) newPos = 0;

		if (Devide > 0)
		{
			float pre = 1f / Devide;//每份的长度
			SliderPos_int = (int)(newPos * Devide);//不连续的整数
			if (SliderPos_int == Devide) SliderPos_int = Devide - 1;//保证不能满
			newPos = SliderPos_int * pre + pre / 2;
		}

		Vector3 localPos = transform.localPosition;
		localPos.z = newPos;
		this.SliderPos = localPos.z;
		transform.localPosition = localPos;
	}

	public static bool HitOnlyOne(out Vector3 hitpos)
	{
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		if (Physics.Raycast(ray, out RaycastHit hitObj))
		{
			hitpos = hitObj.point;
			return true;
		}
		else
		{
			hitpos = new Vector3(0, 0, 0);
		}
		return false;
	}
}
