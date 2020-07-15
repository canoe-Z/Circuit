﻿using System.Runtime.InteropServices;
using UnityEngine;

public class MoveController : MonoBehaviour
{
	public static bool CanMove { get; set; } = true;
	[DllImport("user32.dll")]
	private static extern short GetKeyState(int keyCode);
	private CharacterController characterController;
	private readonly float rotateSpeed = 1;
	private readonly float defaultMoveSpeed = 0.1f;
	private float moveSpeed;
	private bool W, A, S, D, Up, Down;

	void Start()
	{
		Cursor.lockState = CursorLockMode.Locked;//锁定中央
		Cursor.visible = false;
		characterController = Camera.main.GetComponent<CharacterController>();
	}

	void Update()
	{
		GetKeyState();
		SetMoveSpeed();
		Rotate();
		Move();
	}

	/// <summary>
	/// 视角旋转，应每帧调用
	/// </summary>
	public void Rotate()
	{
		if (!MoveController.CanMove) return;//在菜单的时候禁止转头
		Vector3 camRot = Camera.main.transform.eulerAngles;
		//鼠标移动距离
		float rh = Input.GetAxis("Mouse X");
		float rv = Input.GetAxis("Mouse Y");
		camRot.x -= rv * rotateSpeed;
		camRot.y += rh * rotateSpeed;
		if (camRot.x > 89 && camRot.x < 180) camRot.x = 89;
		if (camRot.x < 271 && camRot.x > 180) camRot.x = 271;
		if (camRot.x < -89) camRot.x = -89;
		Camera.main.transform.eulerAngles = camRot;
	}

	/// <summary>
	/// 设置移动速度
	/// </summary>
	/// <param name="givenMoveSpeed">给定的移动速度</param>
	public void SetMoveSpeed(float givenMoveSpeed)
	{
		moveSpeed = givenMoveSpeed;
		SlowMove();

	}

	/// <summary>
	/// 使用默认的移动速度
	/// </summary>
	public void SetMoveSpeed()
	{
		moveSpeed = defaultMoveSpeed;
		SlowMove();
	}

	/// <summary>
	/// 大写锁定打开时，移动速度变为十分之一
	/// </summary>
	public void SlowMove()
	{
		// (((ushort)GetKeyState(0x14)) & 0xffff) != 0 -->大写锁定已打开
		if ((((ushort)GetKeyState(0x14)) & 0xffff) != 0)
		{
			moveSpeed /= 10;
		}
	}

	/// <summary>
	/// 判断按键状态
	/// </summary>
	public void GetKeyState()
	{
		W = Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W);
		D = Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D);
		S = Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S);
		A = Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A);
		Up = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
		Down = Input.GetKey(KeyCode.Space);
	}

	/// <summary>
	/// 视角移动，应每帧调用
	/// </summary>
	public void Move()
	{
		float dFront = 0;
		float dRight = 0;
		float dUp = 0;

		if (W) dFront += moveSpeed;
		if (D) dRight += moveSpeed;

		if (S) dFront -= moveSpeed;
		if (A) dRight -= moveSpeed;

		if (Up) dUp -= moveSpeed;
		if (Down) dUp += moveSpeed;

		Vector3 world = Camera.main.gameObject.transform.TransformDirection(new Vector3(dRight, dUp, dFront));
		characterController.Move(world);
	}
}