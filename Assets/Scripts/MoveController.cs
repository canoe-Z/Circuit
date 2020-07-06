using System.Runtime.InteropServices;
using UnityEngine;
public class MoveController : MonoBehaviour
{
	[DllImport("user32.dll")]
	private static extern short GetKeyState(int keyCode);
	private CharacterController _characterController;
	private readonly float RotateSpeed = 1;
	private readonly float DefaultMoveSpeed = 0.1f;
	private float MoveSpeed;
	private bool W, A, S, D, Up, Down;
	void Start()
	{
		_characterController = Camera.main.GetComponent<CharacterController>();
	}
	void Update()
	{
		GetKeyState();
		SetMoveSpeed();
		Rotate();
		Move();
	}

	public void Rotate()
	{
		Vector3 camRot = Camera.main.transform.eulerAngles;
		//鼠标移动距离
		float rh = Input.GetAxis("Mouse X");
		float rv = Input.GetAxis("Mouse Y");
		camRot.x -= rv * RotateSpeed;
		camRot.y += rh * RotateSpeed;
		if (camRot.x > 89 && camRot.x < 180) camRot.x = 89;
		if (camRot.x < 271 && camRot.x > 180) camRot.x = 271;
		if (camRot.x < -89) camRot.x = -89;
		Camera.main.transform.eulerAngles = camRot;
	}

	public void SetMoveSpeed(float moveSpeed)
	{
		MoveSpeed = moveSpeed;
		SlowMove();

	}
	public void SetMoveSpeed()
	{
		MoveSpeed = DefaultMoveSpeed;
		SlowMove();
	}

	public void SlowMove()
	{
		//(((ushort)GetKeyState(0x14)) & 0xffff) != 0 -->大写锁定已打开
		if ((((ushort)GetKeyState(0x14)) & 0xffff) != 0)
		{
			MoveSpeed /= 10;
		}
	}

	public void GetKeyState()
	{
		W = Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W);
		D = Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D);
		S = Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S);
		A = Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A);
		Up = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
		Down = Input.GetKey(KeyCode.Space);
	}

	public void Move()
	{
		float dFront = 0;
		float dRight = 0;
		float dUp = 0;

		if (W) dFront += MoveSpeed;
		if (D) dRight += MoveSpeed;

		if (S) dFront -= MoveSpeed;
		if (A) dRight -= MoveSpeed;

		if (Up) dUp -= MoveSpeed;
		if (Down) dUp += MoveSpeed;

		Vector3 world = Camera.main.gameObject.transform.TransformDirection(new Vector3(dRight, dUp, dFront));
		_characterController.Move(world);
	}
}