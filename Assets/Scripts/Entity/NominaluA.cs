using SpiceSharp.Components;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 微安表，有各种量程和内阻的规格，内阻为标称
/// </summary>
public class NominaluA : EntityBase, IAmmeter
{
	public double MaxI = 0.05;              //量程，单位安培
	public bool RealValueSet = false;
	public double NominalR = 2;             //内阻为标称值
	public double RealR;
	private MyPin myPin;                    //指针（显示数字的那种）
	private int GND, V0;

	/// TODO:实际书上给出的量程和内阻并无明显关系
	/// <summary>
	/// 变成某一种微安表，单位微安
	/// </summary>
	public void MyChangeToWhichType(int uA)
	{
		MaxI = (double)uA / 1000000;
		NominalR = (double)100 / uA;//50微安时为2欧姆，成反比
		myPin.MyChangePos(0);
		myPin.MySetString("uA", uA);
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
		if (!RealValueSet)
		{
			RealR = Nominal.GetRealValue(NominalR);
			RealValueSet = true;
		}

		GND = ChildPorts[0].ID;
		V0 = ChildPorts[1].ID;
	}

	void Update()
	{
		myPin.MyChangePos((float)(ChildPorts[1].I / MaxI));
	}

	public override void LoadElement()
	{
		CircuitCalculator.UF.Union(GND, V0);
	}

	public override void SetElement()
	{
		int EntityID = CircuitCalculator.EntityNum;

		CircuitCalculator.SpiceEntities.Add(new Resistor(EntityID.ToString(), GND.ToString(), V0.ToString(), NominalR));

		CircuitCalculator.SpicePorts.Add(ChildPorts[0]);
		CircuitCalculator.SpicePorts.Add(ChildPorts[1]);
	}

	// 计算自身电流
	public void CalculateCurrent()
	{
		ChildPorts[1].I = (ChildPorts[1].U - ChildPorts[0].U) / NominalR;
	}

	public override EntityData Save()
	{
		///TODO：微安表并非简单元件
		return new NominaluAData(NominalR, RealR, MaxI, transform.position, transform.rotation, ChildPortID);
	}
}

[System.Serializable]
public class NominaluAData : EntityData
{
	private readonly double nominalR;
	private readonly double realR;
	private readonly double maxI;

	public NominaluAData(double nominalR, double realR, double maxI, Vector3 pos, Quaternion angle, List<int> id) : base(pos, angle, id)
	{
		this.nominalR = nominalR;
		this.realR = realR;
		this.maxI = maxI;
	}

	override public void Load()
	{
		NominaluA nominaluA = EntityCreator.CreateEntity<NominaluA>(posfloat, anglefloat, IDList);
		// 读档要沿用原本生成的真实值
		nominaluA.RealValueSet = true;
		nominaluA.MaxI = maxI;
		nominaluA.NominalR = nominalR;
		nominaluA.RealR = realR;
	}
}