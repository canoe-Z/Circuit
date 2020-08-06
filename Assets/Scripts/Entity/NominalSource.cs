using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 待测电源
/// </summary>
public class NominalSource : Source
{
	private double nominalE, nominalR;
	private string strShow;
	void Start()
	{
		sourceText.text = strShow;

		PortID_G = ChildPorts[0].ID;
		PortID_V = ChildPorts[1].ID;
	}

	public static GameObject Create(double nominalE, double nominalR, string strShow,
		double? realE = null, double? realR = null, Float3 pos = null, Float4 angle = null, List<int> IDList = null)
	{
		NominalSource nominalSource = BaseCreate<NominalSource>(pos, angle, IDList);

		nominalSource.nominalE = nominalE;
		nominalSource.nominalR = nominalR;
		nominalSource.strShow = strShow;

		// 创建时生成新随机值，读档时写入旧值
		if (realE != null)
		{
			nominalSource.E = realE.Value;
		}
		else
		{
			nominalSource.E = Nominal.GetRealValue(nominalE);
		}

		// 创建时生成新随机值，读档时写入旧值
		if (realR != null)
		{
			nominalSource.R = realR.Value;
		}
		else
		{
			nominalSource.R = Nominal.GetRealValue(nominalR);
		}

		return nominalSource.gameObject;
	}

	public override EntityData Save() => new NominalSourceData(nominalE, nominalR, strShow, E, R, transform.position, transform.rotation, ChildPortID);
}

[System.Serializable]
public class NominalSourceData : EntityData
{
	private readonly double realE, realR, nominalE, nominalR;
	private readonly string strShow;
	public NominalSourceData(double nominalE, double nominalR, string strShow,
		double realE, double realR, Vector3 pos, Quaternion angle, List<int> IDList) : base(pos, angle, IDList)
	{
		this.realE = realE;
		this.realR = realR;
		this.nominalE = nominalE;
		this.nominalR = nominalR;
		this.strShow = strShow;
	}

	public override void Load() => NominalSource.Create(nominalE, nominalR, strShow, realE, realR, pos, angle, IDList);
}