using UnityEngine;

/// <summary>
/// 端口
/// </summary>
public class CircuitPort : MonoBehaviour, IUniqueIdentity
{
	public bool IsConnected { get; set; } = false;              // 是否连接（非字面含义）
	public double U { get; set; } = 0;                          // 电压探针（需要时更新）
	public double I { get; set; } = 0;                          // 流出接线柱的电流（需要时更新）
	public int ID { get; set; }                                 // 接线柱全局ID
	public int LocalID { get; set; }                            // 接线柱本地ID
	public EntityBase Father { get; set; }

	void Awake()
	{
		CircuitCalculator.Ports.AddLast(this);
	}

	void Start()
	{
		// 删除元件时，删除其端口
		Father.EntityDestroy += DestroyPort;
	}

	public void DestroyPort()
	{
		Father.EntityDestroy -= DestroyPort;
		CircuitCalculator.Ports.Remove(this);
	}

	void OnMouseDown()
	{
		if (!MoveController.CanOperate) return;
		ConnectionManager.ClickPort(this);
	}

	void OnMouseEnter()
	{
		if (!MoveController.CanOperate) return;
		/*
		Renderer[] renderers = GetComponentsInChildren<Renderer>();
		foreach (var renderer in renderers)
		{
			renderer.material.SetColor("Color_592D9D79", Color.blue);
			renderer.material.SetInt("_SurfaceType", 0);
			renderer.material.SetInt("_RenderQueueType", 5);

		}
		*/
		transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
	}

	void OnMouseExit()
	{
		if (!MoveController.CanOperate) return;
		/*
		Renderer[] renderers = GetComponentsInChildren<Renderer>();
		foreach (var renderer in renderers)
		{
			renderer.material.SetColor("Color_592D9D79", Color.black);
		}
		*/
		transform.localScale = new Vector3(1, 1, 1);
	}
}
