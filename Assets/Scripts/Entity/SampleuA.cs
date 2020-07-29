using SpiceSharp.Components;

/// <summary>
/// 微安表，有各种量程和内阻的规格，内阻为标称
/// </summary>
public class SampleuA : EntityBase, IAmmeter
{
	public double MaxI = 0.05;//量程，单位安培
	public double Rvalue = 2;//内阻
	private MyPin myPin;//指针（显示数字的那种）
	private int GND, V0;

	/// TODO:实际书上给出的量程和内阻并无明显关系
	/// <summary>
	/// 变成某一种微安表，单位微安
	/// </summary>
	public void MyChangeToWhichType(int uA)
	{
		MaxI = (double)uA / 1000000;
		Rvalue = (double)100 / uA;//50微安时为2欧姆，成反比
		myPin.ChangePos(0);
		myPin.SetString("uA", uA);
		CircuitCalculator.NeedCalculate = true;
	}

	public override void EntityAwake()
	{
		// 得到引用并且初始化
		myPin = GetComponentInChildren<MyPin>();
		MyChangeToWhichType(50);
	}

	void Start()
	{
		GND = ChildPorts[0].ID;
		V0 = ChildPorts[1].ID;
	}

	void Update()
    {
		myPin.ChangePos((float)(ChildPorts[1].I / MaxI));
	}

	public override void LoadElement()
	{
		CircuitCalculator.UF.Union(GND, V0);
	}

	public override void SetElement()
	{
		int EntityID = CircuitCalculator.EntityNum;

		CircuitCalculator.SpiceEntities.Add(new Resistor(EntityID.ToString(), GND.ToString(), V0.ToString(), Rvalue));

		CircuitCalculator.SpicePorts.Add(ChildPorts[0]);
		CircuitCalculator.SpicePorts.Add(ChildPorts[1]);
	}

	// 计算自身电流
	public void CalculateCurrent()
	{
		ChildPorts[1].I = (ChildPorts[1].U - ChildPorts[0].U) / Rvalue;
	}

	public override EntityData Save()
	{
		///TODO：微安表并非简单元件
		return new SimpleEntityData<SampleuA>(transform.position, transform.rotation, ChildPortID);
	}
}
