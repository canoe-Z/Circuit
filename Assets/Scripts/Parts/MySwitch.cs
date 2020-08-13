using UnityEngine;

/// <summary>
/// 元件开关
/// 挂在包含有碰撞体和刚体的物体上，令localEular.z变动
/// </summary>
public class MySwitch : MonoBehaviour
{
	public bool MyIsOn { get; private set; } = false;
	const float angleRange = 15;

	GameObject Sw;
	Renderer[] renderers;
	Vector3 basicPos;
	private void Start()
	{
		Transform[] transforms = GetComponentsInChildren<Transform>();
		foreach (var tr in transforms)
		{
			if (tr.name == "Sw")
			{
				Sw = tr.gameObject;
			}
		}
		basicPos = Sw.transform.localEulerAngles;
		renderers = Sw.GetComponentsInChildren<Renderer>();
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
			Sw.transform.localEulerAngles = basicPos + new Vector3(angleRange, 0, 0);
		}
		else
		{
			ChangeMat(Color.red);
			Sw.transform.localEulerAngles = basicPos + new Vector3(-angleRange, 0, 0);
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
