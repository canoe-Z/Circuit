using UnityEngine;

/// <summary>
/// 元件开关
/// 挂在包含有碰撞体和刚体的物体上，令localEular.z变动
/// </summary>
public class MySwitch : MonoBehaviour
{
	public bool MyIsOn { get; private set; } = false;
	const float angleRange = 15;


	Renderer[] renderers;
	private void Start()
	{
		renderers = GetComponentsInChildren<Renderer>();
		Renew();
	}

	void OnMouseOver()
	{
		if (!MoveController.CanOperate) return;
		if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
		{
			MyIsOn = !MyIsOn;
			Renew();
			CircuitCalculator.NeedCalculateByConnection = true;
		}
	}

	void Renew()
	{
		if (MyIsOn)
		{
			ChangeMat(Color.green);
			transform.localEulerAngles = new Vector3(0, 0, angleRange);
		}
		else
		{
			ChangeMat(Color.red);
			transform.localEulerAngles = new Vector3(0, 0, -angleRange);
		}
	}
	void ChangeMat(Color color)
	{
		foreach(var m in renderers)
		{
			m.material.color = color;
		}
	}
}
