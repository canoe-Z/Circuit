using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// 鼠标操控
/// </summary>
public class MoveController : MonoBehaviour
{
	public static float myMoveSpeed = 1f;//移动速度的倍率
	public static float myTurnSpeed = 1f;//转头速度的倍率
	public static bool CanOperate { get; set; } = true;
	public static bool CanTurn { get; set; } = true;
	// 用于获取CapsLock的状态
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
		RopeUpdate();
	}

	/// <summary>
	/// 视角旋转，应每帧调用
	/// </summary>
	public void Rotate()
	{
		if (!MoveController.CanTurn) return;//在菜单的时候禁止转头
		Vector3 camRot = Camera.main.transform.eulerAngles;
		//鼠标移动距离
		float rh = Input.GetAxis("Mouse X");
		float rv = Input.GetAxis("Mouse Y");
		rh *= myTurnSpeed;
		rv *= myTurnSpeed;
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
		if (!CanTurn) return;//在菜单的时候禁止移动

		float dFront = 0;
		float dRight = 0;
		float dUp = 0;

		if (W) dFront += moveSpeed;
		if (D) dRight += moveSpeed;

		if (S) dFront -= moveSpeed;
		if (A) dRight -= moveSpeed;

		if (Up) dUp -= moveSpeed;
		if (Down) dUp += moveSpeed;

		Vector3 control = new Vector3(dRight, dUp, dFront) * Time.deltaTime * 100 * myMoveSpeed;
		Vector3 world = Camera.main.gameObject.transform.TransformDirection(control);
		characterController.Move(world);
	}

	void RopeUpdate()
	{
		if (!CanOperate) return;

		if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
		{
			// 鼠标操作时更新所有导线的碰撞体
			foreach (CircuitLine line in CircuitCalculator.Lines)
			{
				line.GetComponent<MeshCollider>().sharedMesh = line.GetComponent<MeshFilter>().sharedMesh;
				line.GetComponent<MeshCollider>().convex = true;
			}

			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray, out RaycastHit hitInfo))
			{
				GameObject hitObject = hitInfo.collider.gameObject;
				if (hitObject.name == "Rope")
				{
					if (Input.GetMouseButtonDown(1))
					{
						hitObject.GetComponent<CircuitLine>().DestroyRope();
					}
				}
			}
		}
	}
}