using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// 鼠标操控
/// </summary>
public class MoveController : MonoBehaviour
{
	public static float MoveRatio { get; set; } = 1f;		// 移动速度倍率
	public static float TurnRatio { get; set; } = 1f;       // 转头速度倍率

	/// <summary>
	/// 可以操纵场景内已有对象
	/// </summary>
	public static bool CanOperate { get; set; } = true;

	/// <summary>
	/// 可以操纵角色
	/// </summary>
	public static bool CanControll { get; set; } = true;

	// 用于获取CapsLock的状态
	[DllImport("user32.dll")]
	private static extern short GetKeyState(int keyCode);
	private CharacterController characterController;

	private readonly float rotateSpeed = 1;
	private readonly float moveSpeed = 0.1f;

	private bool W, A, S, D, Up, Down;

	void Start()
	{
		// 锁定光标并隐藏
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;

		characterController = Camera.main.GetComponent<CharacterController>();
	}

	void Update()
	{
		GetKeyState();
		Rotate(rotateSpeed);
		Move(moveSpeed);
		RopeUpdate();
	}

	/// <summary>
	/// 判断按键状态
	/// </summary>
	private void GetKeyState()
	{
		W = Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W);
		D = Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D);
		S = Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S);
		A = Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A);
		Up = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
		Down = Input.GetKey(KeyCode.Space);
	}

	/// <summary>
	/// 视角旋转
	/// </summary>
	private void Rotate(float speed)
	{
		// 在菜单的时候禁止转头
		if (!CanControll) return;
		Vector3 camRot = Camera.main.transform.eulerAngles;

		// 鼠标移动距离
		float rh = Input.GetAxis("Mouse X");
		float rv = Input.GetAxis("Mouse Y");

		// 大写锁定打开时，转头速度乘以0.8
		// (((ushort)GetKeyState(0x14)) & 0xffff) != 0 -->大写锁定已打开
		if ((((ushort)GetKeyState(0x14)) & 0xffff) != 0)
		{
			speed *= 0.8f;
		}

		camRot.x -= rv * speed * TurnRatio;
		camRot.y += rh * speed * TurnRatio;

		if (camRot.x > 89 && camRot.x < 180) camRot.x = 89;
		if (camRot.x < 271 && camRot.x > 180) camRot.x = 271;
		if (camRot.x < -89) camRot.x = -89;

		Camera.main.transform.eulerAngles = camRot;
	}

	/// <summary>
	/// 视角移动
	/// </summary>
	private void Move(float speed)
	{
		// 在菜单的时候禁止移动
		if (!CanControll) return;

		float dFront = 0;
		float dRight = 0;
		float dUp = 0;

		// 大写锁定打开时，移动速度变为十分之一
		// (((ushort)GetKeyState(0x14)) & 0xffff) != 0 -->大写锁定已打开
		if ((((ushort)GetKeyState(0x14)) & 0xffff) != 0)
		{
			speed /= 10;
		}

		if (W) dFront += speed;
		if (D) dRight += speed;

		if (S) dFront -= speed;
		if (A) dRight -= speed;

		if (Up) dUp -= speed;
		if (Down) dUp += speed;

		Vector3 control = new Vector3(dRight, dUp, dFront) * Time.deltaTime * 100 * MoveRatio;
		Vector3 world = Camera.main.gameObject.transform.TransformDirection(control);
		characterController.Move(world);
	}

	/// <summary>
	/// 绳子操作
	/// </summary>
	private void RopeUpdate()
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
					// 右键删除
					if (Input.GetMouseButtonDown(1))
					{
						hitObject.GetComponent<CircuitLine>().DestroyRope();
					}
				}
			}
		}
	}
}