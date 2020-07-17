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

	private void OnMouseDown()
	{
		if (!MoveController.CanMove) return;
		ConnectionManager.ClickPort(this);
	}

	private void OnMouseEnter()
	{
		if (!MoveController.CanMove) return;
		MouseEnter?.Invoke(this);
		gameObject.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
	}

	private void OnMouseExit()
	{
		if (!MoveController.CanMove) return;
		MouseExit?.Invoke(this);
		gameObject.transform.localScale = new Vector3(1, 1, 1);
	}
}
