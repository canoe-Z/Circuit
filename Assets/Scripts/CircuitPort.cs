using UnityEngine;
//端口
public class CircuitPort : MonoBehaviour
{
	public int Connected = 0;//是否连接
	public double U = 0;//电压探针
	public double I = 0;//流出接线柱的电流
	public int PortID;//本接线柱ID
	public int PortID_Global;//本接线柱ID_全局
	public EntityBase father;
	private void OnMouseDown()
	{
		if (!MoveController.boolMove) return;
		ConnectionManager.ClickPort(this);
	}
	private void OnMouseEnter()
	{
		if (!MoveController.boolMove) return;
		this.gameObject.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
	}
	private void OnMouseOver()//持续期间
	{
		if (!MoveController.boolMove) return;
		ShowTip.OverPort(this);
	}
	private void OnMouseExit()
	{
		if (!MoveController.boolMove) return;
		this.gameObject.transform.localScale = new Vector3(1, 1, 1);
	}
}
