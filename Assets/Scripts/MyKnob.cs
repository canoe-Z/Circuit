using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 挂在包含有碰撞体和刚体的物体上，令localEular.z变动范围为0-(-360)
/// </summary>
public class MyKnob : MonoBehaviour
{
	/// <summary>
	/// 是否为离散的，-1为连续的
	/// </summary>
	public int devide = -1;
	/// <summary>
	/// 是否可以进行循环
	/// </summary>
	public bool canLoop = false;
	/// <summary>
	/// 旋转的限制角度
	/// </summary>
	public float angleRange = 360;
	/// <summary>
	/// 模式为连续的话，每秒的速度增量，指从0-1
	/// </summary>
	public float speedUpPerSecond = 0.001f;
	/// <summary>
	/// 0-1的数值
	/// </summary>
	public float knobPos { get;private set; } = 0;
	/// <summary>
	/// 保证小于Devide的整数，离散的
	/// </summary>
	public int knobPos_int { get; private set; } = 0;


	
	bool calculator = false;//为true时将会在下一帧进行电路运算
	void OnMouseOver()
	{
		if (!MoveController.CanOperate) return;
		if (calculator)
		{
			calculator = false;
			CircuitCalculator.CalculateByConnection();
		}

		if (devide > 0)//离散才可以执行
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
			else if(Input.GetMouseButton(1))
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
		nowSpeedPerSec += speedUpPerSecond * Time.deltaTime;
		//写入、检查数据
		knobPos += nowSpeedPerSec;
		if (knobPos > 1) knobPos = 1;
		//旋转模型
		transform.localEulerAngles = new Vector3(0, 0, knobPos * angleRange);
	}
	//下降一点点
	void DownYiDiandian()
	{
		nowSpeedPerSec += speedUpPerSecond * Time.deltaTime;
		//写入、检查数据
		knobPos -= nowSpeedPerSec;
		if (knobPos < 0) knobPos = 0;
		//旋转模型、写入数据
		transform.localEulerAngles = new Vector3(0, 0, knobPos * angleRange);
	}
	//加一
	void UpOne()
	{
		knobPos_int++;
		if (knobPos_int >= devide)//加冒了
		{
			if (canLoop) knobPos_int -= devide;
			else knobPos_int = devide - 1;
		}

		float pre = 1f / devide;//每份的长度
		float newRot = knobPos_int * pre;//连续的角度

		//旋转模型、写入数据
		transform.localEulerAngles = new Vector3(0, 0, newRot * angleRange);
		knobPos = newRot;
	}

	//减一
	void DownOne()
	{
		knobPos_int--;
		if (knobPos_int < 0)//减冒了
		{
			if (canLoop) knobPos_int += devide;
			else knobPos_int = 0;
		}

		float pre = 1f / devide;//每份的长度
		float newRot = knobPos_int * pre;//连续的角度

		//旋转模型、写入数据
		transform.localEulerAngles = new Vector3(0, 0, newRot * angleRange);
		knobPos = newRot;
	}

	/// <summary>
	/// 更改Knob的转角，已经包含了“检查数据是否满足0-1”，特别耐c
	/// </summary>
	public void ChangeKnobRot(float newRot)
	{
		//检查数据
		if (newRot > 1) newRot = 1;
		else if (newRot < 0) newRot = 0;

		//离散的
		if (devide > 0)
		{
			float pre = 1f / devide;//每份的长度
			knobPos_int = (int)(newRot * devide);//不连续的整数
			if (knobPos_int == devide) knobPos_int = devide - 1;//保证不能满
			newRot = knobPos_int * pre;
		}

		//旋转模型、写入数据
		transform.localEulerAngles = new Vector3(0, 0, newRot * angleRange);
		knobPos = newRot;
	}
	
}
