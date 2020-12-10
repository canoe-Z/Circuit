using UnityEngine;

/// <summary>
/// 滑块，挂在包含有碰撞体和刚体的物体上，令localPosition在两个向量之间变化
/// </summary>
public class MySlider : MonoBehaviour
{
	[Header("填入两个localPosition")]
	public Vector3 localStartPos = new Vector3(0, 0, 0);
	public Vector3 localEndPos = new Vector3(0, 0, 1);
	// 滑块位置变化事件
	public delegate void SliderEventHandler();
	public event SliderEventHandler SliderEvent;

	/// <summary>
	/// 是否为离散的，-1为连续的，其它正整数表示离散取值数
	/// </summary>
	public int Devide { get; set; } = -1;

	/// <summary>
	/// 滑块位置，0-1的数值
	/// </summary>
	public float SliderPos { get; private set; } = 0;

	/// <summary>
	/// 滑块离散位置，为小于Devide的整数
	/// </summary>
	public int SliderPos_int { get; private set; } = 0;

	void OnMouseDrag()
	{
		if (!MoveController.CanOperate) return;

		if (HitOnlyOne(out Vector3 hitPos))//打到就算
		{
			//转换成本地坐标
			Vector3 hitPosLocal = transform.parent.InverseTransformPoint(hitPos);
			//两个向量
			Vector3 start_hitPos = hitPosLocal - localStartPos;
			Vector3 start_end = localEndPos - localStartPos;
			//点积求投影长度
			float shadowLen = Vector3.Dot(start_hitPos, start_end.normalized);
			//总长度
			float totalLen = start_end.magnitude;
			//按比例扔进去
			SetSliderPos(shadowLen / totalLen);
			CircuitCalculator.NeedCalculateByConnection = true;
		}
	}

	/// <summary>
	/// 更改Slider的位置，已经包含了“检查数据是否满足0-1”，特别耐c
	/// </summary>
	public void SetSliderPos(float newPos)
	{
		if (newPos > 1) newPos = 1;
		else if (newPos < 0) newPos = 0;

		if (Devide > 0)
		{
			// 计算滑块的连续位置
			float pre = 1f / Devide;
			SliderPos_int = (int)(newPos * Devide);
			if (SliderPos_int >= Devide) SliderPos_int = Devide - 1;
			newPos = SliderPos_int * pre + pre / 2;
		}
		//按比例设置
		transform.localPosition = newPos * localEndPos + (1 - newPos) * localStartPos;
		SliderPos = newPos;

		// 更改位置后发送消息
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
