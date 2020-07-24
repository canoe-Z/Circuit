using UnityEngine;

/// <summary>
/// 挂在包含有碰撞体和刚体的物体上，令localEular.z变动范围为0-(-360)
/// </summary>
public class MyKnob : MonoBehaviour
{
	// 旋钮位置变化事件
	public delegate void KnobEventHandler();
	public event KnobEventHandler KnobEvent;

	/// <summary>
	/// 是否为离散的，-1为连续的
	/// </summary>
	public int Devide = -1;
	/// <summary>
	/// 是否可以进行循环
	/// </summary>
	public bool CanLoop = false;
	/// <summary>
	/// 旋转的限制角度
	/// </summary>
	public float AngleRange = 360;
	/// <summary>
	/// 模式为连续的话，每秒的速度增量，指从0-1
	/// </summary>
	public float SpeedUpPerSecond = 0.001f;
	/// <summary>
	/// 0-1的数值
	/// </summary>
	public float KnobPos { get; private set; } = 0;

	/// <summary>
	/// 保证小于Devide的整数，离散的
	/// </summary>
	public int KnobPos_int { get; private set; } = 0;

	bool calculator = false;//为true时将会在下一帧进行电路运算
	void OnMouseOver()
	{
		if (!MoveController.CanOperate) return;
		if (calculator)
		{
			calculator = false;
			CircuitCalculator.CalculateByConnection();
		}

		if (Devide > 0)//离散才可以执行
		{
			if (Input.GetMouseButtonDown(0))//左键按下
			{
				UpOne();
				calculator = true;
			}
			else if (Input.GetMouseButtonDown(1))//右键按下
			{

				DownOne();
				calculator = true;
			}
		}
		else//连续的
		{
			if (Input.GetMouseButton(0))
			{
				UpYiDiandian();
				calculator = true;
			}
			else if (Input.GetMouseButton(1))
			{
				DownYiDiandian();
				calculator = true;
			}
			else
			{
				NotXuanZhuan();
			}
		}
	}
	float nowSpeedPerSec = 0;
	//本帧没有进行旋转
	void NotXuanZhuan()
	{
		nowSpeedPerSec = 0;
	}

	//上升一点点
	void UpYiDiandian()
	{
		nowSpeedPerSec += SpeedUpPerSecond * Time.deltaTime;
		//写入、检查数据
		float newRot = KnobPos + nowSpeedPerSec;
		if (newRot > 1) newRot = 1;
		ChangeKnobRot(newRot);
	}

	//下降一点点
	void DownYiDiandian()
	{
		nowSpeedPerSec += SpeedUpPerSecond * Time.deltaTime;
		//写入、检查数据
		float newRot = KnobPos - nowSpeedPerSec;
		if (newRot < 0) newRot = 0;
		ChangeKnobRot(newRot);
	}

	//加一
	void UpOne()
	{
		KnobPos_int++;
		if (KnobPos_int >= Devide)//加冒了
		{
			if (CanLoop) KnobPos_int -= Devide;
			else KnobPos_int = Devide - 1;
		}

		float pre = 1f / Devide;//每份的长度
		float newRot = KnobPos_int * pre;//连续的角度

		ChangeKnobRot(newRot);
	}

	//减一
	void DownOne()
	{
		KnobPos_int--;
		if (KnobPos_int < 0)//减冒了
		{
			if (CanLoop) KnobPos_int += Devide;
			else KnobPos_int = 0;
		}

		float pre = 1f / Devide;//每份的长度
		float newRot = KnobPos_int * pre;//连续的角度

		ChangeKnobRot(newRot);
	}

	/// <summary>
	/// 更改Knob的转角，已经包含了“检查数据是否满足0-1”，特别耐c
	/// </summary>
	public void SafeChangeKnobRot(float newRot)
	{
		//检查数据
		if (newRot > 1) newRot = 1;
		else if (newRot < 0) newRot = 0;

		//离散的
		if (Devide > 0)
		{
			float pre = 1f / Devide;//每份的长度
			KnobPos_int = (int)(newRot * Devide);//不连续的整数
			if (KnobPos_int == Devide) KnobPos_int = Devide - 1;//保证不能满
			newRot = KnobPos_int * pre;
		}

		ChangeKnobRot(newRot);
	}

	public void ChangeKnobRot(float newRot)
	{
		KnobPos = newRot;
		// 旋转模型
		transform.localEulerAngles = new Vector3(0, 0, newRot * AngleRange);
		// 更改位置后发送消息
		KnobEvent?.Invoke();
	}
}
