using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 待测电阻
/// </summary>
public class NominalR : Resistance
{
	public bool RealValueSet = false;
	public double NominalValue;
	void Start()
	{
		if(!RealValueSet)
		{
			Value = Nominal.GetRealValue(NominalValue);
			RealValueSet = true;
		}
		if (resistanceText) resistanceText.text = "待测\n" + NominalValue.ToString() + "Ω";
	}

	public override EntityData Save()
	{
		return new NominalRData(NominalValue, Value, transform.position, transform.rotation, ChildPortID);
	}
}

[System.Serializable]
public class NominalRData : ResistanceData
{
	private readonly double nominalValue;
	private readonly double realValue;
	public NominalRData(double nominalValue, double realValue, Vector3 pos, Quaternion angle, List<int> id) : base(realValue, pos, angle, id)
	{
		this.nominalValue = nominalValue;
		this.realValue = realValue;
	}

	override public void Load()
	{
		NominalR resistance = EntityCreator.CreateEntity<NominalR>(posfloat, anglefloat, IDList);
		// 读档要沿用原本生成的真实值
		resistance.RealValueSet = true;
		resistance.NominalValue = nominalValue;
		resistance.Value = realValue;
	}
}
