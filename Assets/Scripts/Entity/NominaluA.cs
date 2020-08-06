using SpiceSharp.Components;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 微安表，有各种量程和内阻的规格，内阻为标称
/// </summary>
public class NominaluA : EntityBase, IAmmeter
{
	private int maxuI;             //量程，单位微安
	private double nominalR;       //内阻为标称值
	private double realR;
	private MyPin myPin;           //指针（显示数字的那种）
	private int PortID_GND, PortID_V0;

	public override void EntityAwake()
	{
		// 得到引用并且初始化
		myPin = GetComponentInChildren<MyPin>();
		myPin.PinAwake();
		myPin.SetPos(0);
		myPin.SetString("uA", maxuI);
	}

	void Start()
	{
		PortID_GND = ChildPorts[0].ID;
		PortID_V0 = ChildPorts[1].ID;
	}

	void Update()
	{
		double maxI = maxuI / 1e6;
		myPin.SetPos((float)(ChildPorts[1].I / maxI));
	}

	public override void LoadElement()
	{
		CircuitCalculator.UF.Union(PortID_GND, PortID_V0);
	}

	public override void SetElement()
	{
		int EntityID = CircuitCalculator.EntityNum;
		CircuitCalculator.SpiceEntities.Add(new Resistor(EntityID.ToString(), PortID_GND.ToString(), PortID_V0.ToString(), nominalR));
		CircuitCalculator.SpicePorts.AddRange(ChildPorts);
	}

	// 计算自身电流
	public void CalculateCurrent()
	{
		ChildPorts[1].I = (ChildPorts[1].U - ChildPorts[0].U) / nominalR;
	}

	public static GameObject Create(int maxuI, double nominalR,
	double? realR = null, Float3 pos = null, Float4 angle = null, List<int> IDlist = null)
	{
		NominaluA nominaluA = BaseCreate<NominaluA>(pos, angle, IDlist);
		nominaluA.nominalR = nominalR;
		nominaluA.maxuI = maxuI;
		if (realR != null) nominaluA.realR = Nominal.GetRealValue(realR.Value);
		return nominaluA.gameObject;
	}

	public override EntityData Save()
	{
		///TODO：微安表并非简单元件
		return new NominaluAData(maxuI, nominalR, realR, transform.position, transform.rotation, ChildPortID);
	}
}

[System.Serializable]
public class NominaluAData : EntityData
{
	private readonly double nominalR;
	private readonly double realR;
	private readonly int maxuI;

	public NominaluAData(int maxuI, double nominalR, double realR, Vector3 pos, Quaternion angle, List<int> IDList) : base(pos, angle, IDList)
	{
		this.nominalR = nominalR;
		this.realR = realR;
		this.maxuI = maxuI;
	}

	override public void Load() => NominaluA.Create(maxuI, nominalR, realR, pos, angle, IDList);
}