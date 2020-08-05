using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 待测电阻
/// </summary>
public class NominalR : Resistance
{
	private double nominalValue;	//标称值
	private string prefix;			//前缀

	void Start()
	{
		string str;

		// 根据标称阻值确定显示方式
		if (nominalValue >= 1e6 || Math.Abs(nominalValue - 1e6) < 0.001)
		{
			str = (nominalValue / 1e6).ToString() + "MΩ";
		}
		else if (nominalValue >= 1e3 || Math.Abs(nominalValue - 1e3) < 0.001)
		{
			str = (nominalValue / 1e3).ToString() + "kΩ";
		}
		else
		{
			str = nominalValue.ToString() + "Ω";
		}

		resistanceText.text = prefix + "\n" + str;
	}

	public static GameObject Create(double nominalValue, string prefix, 
		double? realValue = null, Float3 pos = null, Float4 angle = null, List<int> IDlist = null)
	{
		NominalR nominalR = BaseCreate<NominalR>(pos, angle, IDlist);

		nominalR.nominalValue = nominalValue;
		nominalR.prefix = prefix;

		// 创建时生成新随机值，读档时写入旧值
		if (realValue != null)
		{
			nominalR.rValue = realValue.Value;
		}
		else
		{
			nominalR.rValue = Nominal.GetRealValue(nominalValue);
		}

		return nominalR.gameObject;
	}

	public override EntityData Save() => new NominalRData(nominalValue, rValue, prefix, transform.position, transform.rotation, ChildPortID);
}

[System.Serializable]
public class NominalRData : ResistanceData
{
	private readonly double nominalValue;
	private readonly double? realValue;
	private readonly string prefix;

	public NominalRData(double nominalValue, double? realValue, string prefix, Vector3 pos, Quaternion angle, List<int> id) : base(realValue, pos, angle, id)
	{
		this.nominalValue = nominalValue;
		this.realValue = realValue;
		this.prefix = prefix;
	}

	public override void Load() => NominalR.Create(nominalValue, prefix, realValue, pos, angle, IDList);
}