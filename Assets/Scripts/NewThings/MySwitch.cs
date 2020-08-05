using UnityEngine;

/// <summary>
/// 挂在包含有碰撞体和刚体的物体上，令localEular.z变动范围为0-(-360)
/// </summary>
public class MySwitch : MonoBehaviour
{
	int devide = 2;
	/// <summary>
	/// 按钮状态
	/// </summary>
	public int MyDevide
	{
		set
		{
			devide = value;
			if (MySwitchPos >= devide)
				MySwitchPos = devide - 1;
			Renew();
		}
	}


	/// <summary>
	/// 是否可以进行循环
	/// </summary>
	public bool MyCanLoop { get; set; } = true;


	float angleRange = 30;
	/// <summary>
	/// 旋转的限制角度
	/// </summary>
	public float MyAngleRange
	{
		get
		{
			return angleRange;
		}
		set
		{
			angleRange = value;
			Renew();
		}
	}

	/// <summary>
	/// 按钮的位置
	/// </summary>
	public int MySwitchPos { get; private set; } = 0;

	private void Start()
	{
		Renew();
	}

	void OnMouseOver()
	{
		if (!MoveController.CanOperate) return;

		// 左键增加
		if (Input.GetMouseButtonDown(0))
		{
			MySwitchPos++;
			if (MySwitchPos >= devide)
			{
				if (MyCanLoop)
					MySwitchPos = 0;
				else
					MySwitchPos = devide - 1;
			}
			Renew();
			CircuitCalculator.NeedCalculateByConnection = true;
		}
		// 右键减小
		else if (Input.GetMouseButtonDown(1))
		{
			MySwitchPos--;
			if (MySwitchPos < 0)
			{
				if (MyCanLoop)
					MySwitchPos = devide - 1;
				else
					MySwitchPos = 0;
			}
			Renew();
			CircuitCalculator.NeedCalculateByConnection = true;
		}
	}

	void Renew()
	{
		// 旋转模型
		transform.localEulerAngles = new Vector3(0, 0, MyAngleRange * MySwitchPos / (devide - 1) - (angleRange / 2));
	}
}
