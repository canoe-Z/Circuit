using System;
using System.Text.RegularExpressions;
using SpiceSharp.Components;
using SpiceSharp.Entities;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 太阳能电池
/// </summary>
public class Solar : EntityBase, ISource
{
	private const double IscMax = 0.06; // 最大短路电流
	private double Isc;
	private int PortID_G, PortID_V;

	private MySwitch mySwitch;
	private MySlider mySlider;
	private Text sloarText;
	private Light sloarLight;

	public override void EntityAwake()
	{
		mySwitch = transform.FindComponent_DFS<MySwitch>("MySwitch");
		mySlider = GetComponentInChildren<MySlider>();
		sloarText = transform.FindComponent_DFS<Text>("Text");
		sloarLight = transform.FindComponent_DFS<Light>("Spot Light");

		// 默认启动时关机，读档可覆盖该设置
		mySwitch.IsOn = false;
	}

	void Start()
	{
		// 第一次执行初始化，此后受事件控制，UpdateSlider由ChangePower初始化
		mySwitch.SwitchEvent += ChangePower;
		mySlider.SliderEvent += UpdateSlider;
		ChangePower();

		PortID_G = ChildPorts[0].ID;
		PortID_V = ChildPorts[1].ID;
	}

	void UpdateSlider()
	{
		// 关机时滑块可以调整，但是不更新太阳能电池
		if (!mySwitch.IsOn) return;
		const float minDis = 0.2f;//0.2米
		const float maxDis = 1.2f;//0.2米
		float distance = minDis + (maxDis - minDis) * mySlider.SliderPos;

		float fm = distance / 0.19f;  // 会被平方的分母
		float lightStrength = 1 / (fm * fm);    // 这东西最小值1/36，最大值1
		Isc = lightStrength * IscMax;

		// 更新光照强度的数值
		sloarText.text = EntityText.GetText(lightStrength * 1000, 1000.00, 2);
	}

	/// <summary>
	/// 根据MySwtich状态开启/关闭
	/// </summary>
	private void ChangePower()
	{
		// 开机
		if (mySwitch.IsOn)
		{
			// 根据滑块位置更新太阳能电池状态
			UpdateSlider();
			sloarLight.enabled = true;
		}
		// 关机
		else
		{
			Isc = 0;
			sloarText.text = "";
			sloarLight.enabled = false;
		}
	}

	public override void LoadElement() => CircuitCalculator.UF.Union(PortID_G, PortID_V);

	// 构建二极管模型
	protected void ApplyParameters(Entity entity, string definition)
	{
		// Get all assignments
		definition = Regex.Replace(definition, @"\s*\=\s*", "=");
		var assignments = definition.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
		foreach (var assignment in assignments)
		{
			// Get the name and value
			var parts = assignment.Split('=');
			if (parts.Length != 2)
				throw new System.Exception("Invalid assignment");
			var name = parts[0].ToLower();
			var value = double.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);

			// Set the entity parameter
			entity.SetParameter(name, value);
		}
	}

	private DiodeModel CreateDiodeModel(string name, string parameters)
	{
		var dm = new DiodeModel(name);
		ApplyParameters(dm, parameters);
		return dm;
	}

	public override void SetElement(int entityID)
	{
		PortID_G = ChildPorts[0].ID;
		PortID_V = ChildPorts[1].ID;

		CircuitCalculator.SpiceEntities.Add(new CurrentSource(string.Concat(entityID, "_S"), "S+", PortID_G.ToString(), Isc));
		CircuitCalculator.SpiceEntities.Add(new Diode(string.Concat(entityID, "_D"), PortID_G.ToString(), "S+", "Solar_1N4007"));
		CircuitCalculator.SpiceEntities.Add(CreateDiodeModel("Solar_1N4007", "Is=1.09774e-8 Rs=0.0414388 N=1.78309 Cjo=2.8173e-11 M=0.318974 tt=9.85376e-6 Kf=0 Af=1"));
		CircuitCalculator.SpiceEntities.Add(new Resistor(string.Concat(entityID, "_R1"), "S+", PortID_G.ToString(), 10000));
		CircuitCalculator.SpiceEntities.Add(new Resistor(string.Concat(entityID, "_R2"), PortID_V.ToString(), "S+", 0.5));
	}

	public void GroundCheck()
	{
		if (IsConnected())
		{
			if (!CircuitCalculator.UF.Connected(PortID_G, 0))
			{
				CircuitCalculator.UF.Union(PortID_G, 0);
				CircuitCalculator.GNDLines.Add(new GNDLine(PortID_G));
			}
		}
	}

	public override EntityData Save() => new SolarData(this);

	[System.Serializable]
	public class SolarData : EntityData
	{
		private readonly bool isOn;
		private readonly float sliderPos;

		public SolarData(Solar solar)
		{
			baseData = new EntityBaseData(solar);
			isOn = solar.mySwitch.IsOn;
			sliderPos = solar.mySlider.SliderPos;
		}

		public override void Load()
		{
			Solar solar = BaseCreate<Solar>(baseData);
			solar.mySwitch.IsOn = isOn;
			solar.mySlider.SetSliderPos(sliderPos);
		}
	}
}