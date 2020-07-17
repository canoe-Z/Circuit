using UnityEngine;

/// <summary>
/// 端口
/// </summary>
public class CircuitPort : MonoBehaviour
{
	public int Connected { get; set; } = 0;				//是否连接
	public double U { get; set; } = 0;					//电压探针（需要时更新）
	public double I { get; set; } = 0;                  //流出接线柱的电流（需要时更新）
	public int PortID { get; set; }						//接线柱全局ID

	public static event EnterEventHandler MouseEnter;
	public static event ExitEventHandler MouseExit;

	void Start()
	{
		CircuitCalculator.Ports.AddLast(this);
	}

	void OnMouseDown()
	{
		if (!MoveController.CanOperate) return;
		ConnectionManager.ClickPort(this);
	}

	void OnMouseEnter()
	{
		if (!MoveController.CanOperate) return;
		MouseEnter?.Invoke(this);
		gameObject.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
	}

	void OnMouseExit()
	{
		if (!MoveController.CanOperate) return;
		MouseExit?.Invoke(this);
		gameObject.transform.localScale = new Vector3(1, 1, 1);
	}
}
