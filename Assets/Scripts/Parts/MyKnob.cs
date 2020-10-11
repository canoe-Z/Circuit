using UnityEngine;

/// <summary>
/// 旋钮
/// 挂在包含有碰撞体和刚体的物体上，令localEular.z变动范围为0-(-360)
/// </summary>
public class MyKnob : MonoBehaviour
{
	// 旋钮位置变化事件
	public delegate void KnobEventHandler();
	public event KnobEventHandler KnobEvent;

	/// <summary>
	/// 是否为离散的，-1为连续的，其它正整数表示离散取值数
	/// </summary>
	public int Devide { get; set; } = -1;

	/// <summary>
	/// 是否可以进行循环，仅离散有效
	/// </summary>
	public bool CanLoop { get; set; } = false;

	/// <summary>
	/// 旋转的限制角度
	/// </summary>
	public float AngleRange { get; set; } = 360;

	// 连续旋转时，每秒从0-1的速度增量
	const float speedUpPerSecond = 0.001f;

	/// <summary>
	/// 旋钮位置，0-1的数值
	/// </summary>
	public float KnobPos { get; private set; } = 0;

	/// <summary>
	/// 旋钮离散位置，为小于Devide的整数
	/// </summary>
	public int KnobPos_int { get; private set; } = 0;

	/// <summary>
	/// 是否会改变电路连接关系
	/// </summary>
	public bool IsChangeConnection { get; set; } = false;

	// 当前旋转速度
	private float nowSpeedPerSec = 0;

	void OnMouseEnter()
	{
		if (!MoveController.CanOperate) return;
		transform.EnableFresnel((Color.red + Color.yellow) / 2);
	}

	void OnMouseOver()
	{
		if (!MoveController.CanOperate) return;

		// 离散情况
		if (Devide > 0)
		{
			// 左键增加
			if (Input.GetMouseButtonDown(0))
			{
				SetKnobRot(KnobPos_int + 1);
				if (IsChangeConnection)
				{

					CircuitCalculator.NeedCalculate = true;
				}
				else
				{
					CircuitCalculator.NeedCalculateByConnection = true;
				}
			}
			// 右键减小
			else if (Input.GetMouseButtonDown(1))
			{
				SetKnobRot(KnobPos_int - 1);
				if (IsChangeConnection)
				{

					CircuitCalculator.NeedCalculate = true;
				}
				else
				{
					CircuitCalculator.NeedCalculateByConnection = true;
				}
			}
		}
		// 连续情况
		else
		{
			// 左键增加
			if (Input.GetMouseButton(0))
			{
				// 转速逐步加快
				nowSpeedPerSec += speedUpPerSecond * Time.deltaTime;
				SetKnobRot(KnobPos + nowSpeedPerSec * Time.deltaTime * 60);
				if (IsChangeConnection)
				{

					CircuitCalculator.NeedCalculate = true;
				}
				else
				{
					CircuitCalculator.NeedCalculateByConnection = true;
				}
			}
			// 右键减小
			else if (Input.GetMouseButton(1))
			{
				// 转速逐步加快
				nowSpeedPerSec += speedUpPerSecond * Time.deltaTime;
				SetKnobRot(KnobPos - nowSpeedPerSec * Time.deltaTime * 60);
				if (IsChangeConnection)
				{

					CircuitCalculator.NeedCalculate = true;
				}
				else
				{
					CircuitCalculator.NeedCalculateByConnection = true;
				}
			}
			else
			{
				// 不旋转时将转速重置
				nowSpeedPerSec = 0;
			}
		}
	}

	void OnMouseExit()
	{
		transform.DisablFresnel();
	}

	/// <summary>
	/// 参数0-1
	/// </summary>
	/// <param name="newRot"></param>
	public void SetKnobRot(float newRot)
	{
		// 如果旋钮是离散的，报错
		if (Devide > 0)
		{
			Debug.LogError("调用错误");
			return;
		}

		// 处理溢出
		if (newRot > 1) newRot = 1;
		else if (newRot < 0) newRot = 0;

		WriteKnobRot(newRot);
	}

	/// <summary>
	/// 离散的参数
	/// </summary>
	/// <param name="newRot_int"></param>
	public void SetKnobRot(int newRot_int)
	{
		// 如果旋钮是连续的，报错
		if (Devide == -1)
		{
			Debug.LogError("调用错误");
			return;
		}

		// 处理溢出，包括循环判断
		if (newRot_int >= Devide)
		{
			if (CanLoop) newRot_int -= Devide;
			else newRot_int = Devide - 1;
		}

		if (newRot_int < 0)
		{
			if (CanLoop) KnobPos_int += Devide;
			else newRot_int = 0;
		}
		KnobPos_int = newRot_int;

		// 用每份的长度计算出旋钮的连续位置
		float pre = 1f / Devide;
		float newRot = newRot_int * pre;

		WriteKnobRot(newRot);
	}
	private void WriteKnobRot(float newRot)
	{
		KnobPos = newRot;

		// 旋转模型
		transform.localEulerAngles = new Vector3(0, 0, newRot * AngleRange);

		// 更改位置后发送消息通知元件
		KnobEvent?.Invoke();
	}
}

/// <summary>
/// 旋钮存档数据（元件中存在混合旋钮时用到）
/// </summary>
[System.Serializable]
public struct KnobData
{
	public float KnobPos;
	public int KnobPos_int;

	public KnobData(float knobPos, int knobPos_int)
	{
		KnobPos = knobPos;
		KnobPos_int = knobPos_int;
	}
}