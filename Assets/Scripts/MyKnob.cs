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
	public float angleRange = 330;
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

		if (Input.GetMouseButtonDown(0))//左键
		{
			UpOne();
			calculator = true;
		}
		else if (Input.GetMouseButtonDown(1))//右键
		{
			DownOne();
			calculator = true;
		}
	}

	//加一
	void UpOne()
	{
		if (devide > 0)//离散才可以执行
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
	}

	//减一
	void DownOne()
	{
		if (devide > 0)//离散才可以执行
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
