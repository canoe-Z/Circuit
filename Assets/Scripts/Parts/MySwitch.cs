using UnityEngine;

/// <summary>
/// 元件开关
/// 挂在包含有碰撞体和刚体的物体上，令localEular.z变动
/// </summary>
public class MySwitch : MonoBehaviour
{
	// 开关状态变化事件
	public delegate void SwitchEventHandler();
	public event SwitchEventHandler SwitchEvent;

	/// <summary>
	/// 开关状态
	/// </summary>
	public bool IsOn { get; set; } = true;

	private readonly float angleRange = 15;

	private Transform Sw;
	private Renderer[] renderers;
	private Vector3 basicPos;

	void Start()
	{
		Sw = transform.FindComponent_DFS<Transform>("Sw");
		basicPos = Sw.transform.localEulerAngles;
		renderers = Sw.GetComponentsInChildren<Renderer>();
		ChangeState();
	}

	void OnMouseOver()
	{
		if (!MoveController.CanOperate) return;
		if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
		{
			IsOn = !IsOn;
			ChangeState();
			SwitchEvent?.Invoke();
			CircuitCalculator.NeedCalculateByConnection = true;
		}
	}

	/// <summary>
	/// 根据开关状态修改颜色和位置
	/// </summary>
	void ChangeState()
	{
		if (IsOn)
		{
			ChangeMat(Sw, Color.green);
			Sw.transform.localEulerAngles = basicPos + new Vector3(angleRange, 0, 0);
		}
		else
		{
			ChangeMat(Sw, Color.red);
			Sw.transform.localEulerAngles = basicPos + new Vector3(-angleRange, 0, 0);
		}
	}

	/// <summary>
	/// 修改颜色
	/// </summary>
	/// <param name="color">颜色</param>
	void ChangeMat(Transform trans, Color color)
	{
		Renderer[] renderers = trans.GetComponentsInChildren<Renderer>();
		foreach (var renderer in renderers)
		{
			foreach (var material in renderer.materials)
			{
				material.SetColor("Color_51411BA8", color);
			}
		}
	}
}
