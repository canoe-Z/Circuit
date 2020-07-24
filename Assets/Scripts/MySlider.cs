using UnityEngine;

/// <summary>
/// 挂在包含有碰撞体和刚体的物体上，令localPosition.z变动范围为0-1
/// </summary>
public class MySlider : MonoBehaviour
{
	// 鼠标响应事件，用于控制Tip显示
	public static event EnterEventHandler MouseEnter;
	public static event ExitEventHandler MouseExit;

	// 滑块位置变化事件
	public delegate void SliderEventHandler();
	public event SliderEventHandler SliderEvent;

	/// <summary>
	/// 是否为离散的，-1为连续的
	/// </summary>
	public int Devide { get; set; } = -1;

	/// <summary>
	/// 0-1的数值
	/// </summary>
	public float SliderPos { get; set; } = 0;

	/// <summary>
	/// 保证小于Devide的整数
	/// </summary>
	public int SliderPos_int { get; private set; } = 0;

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
			ChangeSliderPos(transform.parent.InverseTransformPoint(hitPos).z);
			CircuitCalculator.CalculateByConnection();
		}
	}

	/// <summary>
	/// 更改Slider的位置，已经包含了“检查数据是否满足0-1”，特别耐c
	/// </summary>
	public void ChangeSliderPos(float newPos)
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

		transform.SetLocalPositionZ(newPos);
		SliderPos = newPos;

		//更改位置后发送消息
		SliderEvent?.Invoke();
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
