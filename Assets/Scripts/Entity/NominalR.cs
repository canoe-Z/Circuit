using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 待测电阻
/// </summary>
public class NominalR : Resistance
{
	public bool RealValueSet = false;//置为1时，在生成时不随机变化
	public double NominalValue;//假的值
	public string Prefix;//显示在数字前面的字符串

	void Start()
	{
		if (!RealValueSet)
		{
			Value = Nominal.GetRealValue(NominalValue);
			RealValueSet = true;
		}

		string str;

		// 根据阻值确定显示方式
		if (NominalValue >= 1e6 || Math.Abs(NominalValue - 1e6) < 0.001)
		{
			str = (NominalValue / 1e6).ToString() + "MΩ";
		}
		else if (NominalValue >= 1e3 || Math.Abs(NominalValue - 1e3) < 0.001)
		{
			str = (NominalValue / 1e3).ToString() + "kΩ";
		}
		else
		{
			str = NominalValue.ToString() + "Ω";
		}

		if (resistanceText)
		{
			resistanceText.text = Prefix + "\n" + str;
		}
	}

	public static NominalR Create(double nominalValue, string prefix = "")
	{
		NominalR nominalR = EntityCreator.CreateEntity<NominalR>();
		nominalR.NominalValue = nominalValue;
		nominalR.Prefix = prefix;
		return nominalR;
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
