using UnityEngine;

/// <summary>
/// 元件按动开关
/// 挂在包含有碰撞体和刚体的物体上，令localEular.z变动
/// </summary>
public class MyButton : MonoBehaviour
{
	// 开关状态变化事件
	public delegate void ButtonEventHandler();
	public event ButtonEventHandler ButtonEvent;

	/// <summary>
	/// 开关状态
	/// </summary>
	private bool isOn = true;
	public bool IsOn
	{
		get
		{
			return isOn;
		}
		set
		{
			isOn = value;
			ChangeState();
		}
	}

	/// <summary>
	/// 是否会改变电路连接关系
	/// </summary>
	public bool IsChangeConnection { get; set; } = false;

	private readonly float posYRange = 0.1f;

	private Transform Sw;
	private Vector3 basicPos;

	void Awake()
	{
		Sw = transform.FindComponent_DFS<Transform>("Switch");
		basicPos = Sw.transform.localPosition;
	}

	void OnMouseOver()
	{
		if (!MoveController.CanOperate) return;
		if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
		{
			IsOn = !IsOn;
			ChangeState();
			ButtonEvent?.Invoke();
			if (IsChangeConnection)
			{

				CircuitCalculator.NeedCalculate = true;
			}
			else
			{
				CircuitCalculator.NeedCalculateByConnection = true;
			}
		}
	}

	// 根据开关状态修改颜色和位置
	private void ChangeState()
	{
		if (IsOn)
		{
			ChangeMat(Sw, Color.green);
			Sw.transform.localPosition = basicPos + new Vector3(0, 0, 0);
		}
		else
		{
			ChangeMat(Sw, Color.red);
			Sw.transform.localPosition = basicPos + new Vector3(0, posYRange, 0);
		}
	}

	// 修改颜色
	private void ChangeMat(Transform trans, Color color)
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
