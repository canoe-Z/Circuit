using UnityEngine;

/// <summary>
/// 端口
/// </summary>
public class CircuitPort : MonoBehaviour
{
	public int Connected = 0;			//是否连接
	public double U = 0;				//电压探针
	public double I = 0;				//流出接线柱的电流
	public int PortID;					//本接线柱ID
	public int PortID_Global;			//本接线柱ID_全局
	public EntityBase father;
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
