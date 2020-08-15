using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 待测电阻
/// </summary>
public class NominalR : Resistance
{
	private double nominalValue;    //标称值
	private string prefix;          //前缀

	void Start()
	{
		string str;

		// 根据标称值确定阻值的显示方式
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

		// 最终显示结果为前缀+阻值
		resistanceText.text = prefix + "\n" + str;

		PortID_Left = ChildPorts[0].ID;
		PortID_Right = ChildPorts[1].ID;
	}

	public static GameObject Create(double nominalValue, string prefix)
	{
		NominalR nominalR = Set(BaseCreate<NominalR>(), nominalValue, prefix);

		// 创建时生成新随机值
		nominalR.RValue = Nominal.GetRealValue(nominalValue);
		return nominalR.gameObject;
	}

	private static NominalR Set(NominalR nominalR, double nominalValue, string prefix)
	{
		nominalR.nominalValue = nominalValue;
		nominalR.prefix = prefix;
		return nominalR;
	}

	public override EntityData Save() => new NominalRData(this);

	[System.Serializable]
	private class NominalRData : ResistanceData
	{
		private readonly double nominalValue;
		private readonly string prefix;

		public NominalRData(NominalR nominalR) : base(nominalR)
		{
			nominalValue = nominalR.nominalValue;
			prefix = nominalR.prefix;
		}

		public override void Load()
		{
			NominalR nominalR = Set(BaseCreate<NominalR>(baseData), nominalValue, prefix);

			// 读档时读取旧随机值
			nominalR.RValue = RValue;
		}
	}
}

