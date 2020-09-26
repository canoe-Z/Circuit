using SpiceSharp.Components;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 数字电流表
/// </summary>
public class DigtalAmmeter : EntityBase, ICalculatorUpdate
{
	private readonly double R = 0.001;
	private int PortID_GND, PortID_mA, PortID_A;

	private const int randNum = 2;					// 需要的随机数数量，一般和元件的挡位数量相同
	private float[] rands = null;                   // 随机数

	private Text digtalAmmeterText;
	private MySwitch mySwitch;

	public override void EntityAwake()
	{
		digtalAmmeterText = transform.FindComponent_DFS<Text>("Text");
		mySwitch = transform.FindComponent_DFS<MySwitch>("MySwitch");
	}

	void Start()
	{
		// 先处理随机数
		if (rands == null)
		{
			rands = new float[randNum];
			for (var i = 0; i < randNum; i++)
			{
				rands[i] = Random.Range(-1f, 1f);
			}
		}

		// CalculatorUpdate()统一在Start()中执行，保证在实例化并写入元件自身属性完毕后执行
		CircuitCalculator.CalculateEvent += CalculatorUpdate;
		CalculatorUpdate();

		PortID_GND = ChildPorts[0].ID;
		PortID_mA = ChildPorts[1].ID;
		PortID_A = ChildPorts[2].ID;

		// 默认启动时开机，读档可覆盖该设置
		mySwitch.IsOn = true;
	}

	public void CalculatorUpdate()
	{
		// 计算自身电流
		ChildPorts[1].I = (ChildPorts[1].U - ChildPorts[0].U) / R;
		ChildPorts[2].I = (ChildPorts[2].U - ChildPorts[0].U) / R;

		if (ChildPorts[0].IsConnected && ChildPorts[1].IsConnected)
		{
			double mA = ChildPorts[1].I * 1000;
			double tolerance_mA = 0.02 * 0.01 * mA + 0.001 * 2;
			double nominal_mA = mA + tolerance_mA * rands[0];
			digtalAmmeterText.text = EntityText.GetText(nominal_mA, 999.99, 2);
		}
		else if (ChildPorts[0].IsConnected && ChildPorts[2].IsConnected)
		{
			double A = ChildPorts[2].I;
			double tolerance_A = 0.02 * 0.01 * A + 0.001 * 2;
			double nominal_A = A + tolerance_A * rands[1];
			digtalAmmeterText.text = EntityText.GetText(nominal_A, 999.99, 2);
		}
		else
		{
			digtalAmmeterText.text = "0.00";
		}

		// 开关变化引起电路重新计算，之后即可调用该部分
		if (!mySwitch.IsOn)
		{
			digtalAmmeterText.text = "0.00";
		}
	}

	public override void LoadElement()
	{
		CircuitCalculator.UF.Union(PortID_GND, PortID_mA);
		CircuitCalculator.UF.Union(PortID_GND, PortID_A);
	}

	public override void SetElement(int entityID)
	{
		if (mySwitch.IsOn)
		{
			CircuitCalculator.SpiceEntities.Add(new Resistor(string.Concat(entityID, "_mA"), PortID_GND.ToString(), PortID_mA.ToString(), R));
			CircuitCalculator.SpiceEntities.Add(new Resistor(string.Concat(entityID, "_A"), PortID_GND.ToString(), PortID_A.ToString(), R));

			CircuitCalculator.SpicePorts.AddRange(ChildPorts);
		}
	}

	public override EntityData Save() => new DigtalAmmeterData(this);

	[System.Serializable]
	public class DigtalAmmeterData : EntityData
	{
		private readonly bool isOn;
		private readonly float[] rands;

		public DigtalAmmeterData(DigtalAmmeter digtalAmmeter)
		{
			baseData = new EntityBaseData(digtalAmmeter);
			isOn = digtalAmmeter.mySwitch.IsOn;
			rands = digtalAmmeter.rands;
		}

		public override void Load()
		{
			DigtalAmmeter digtalAmmeter = BaseCreate<DigtalAmmeter>(baseData);
			// 此时执行Awake()
			digtalAmmeter.mySwitch.IsOn = isOn;
			if (rands != null) digtalAmmeter.rands = rands;
			// 此时执行Start()
		}
	}
}