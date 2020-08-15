using System.Collections.Generic;
using UnityEngine;
using static Source;

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

	public static GameObject Create(double nominalE, double nominalR, string strShow)
	{
		NominalSource nominalSource = BaseCreate<NominalSource>().Set(nominalE, nominalR, strShow);

		// 创建时生成新随机值
		nominalSource.E = Nominal.GetRealValue(nominalE);
		nominalSource.R = Nominal.GetRealValue(nominalR);

		return nominalSource.gameObject;
	}

	private NominalSource Set(double nominalE, double nominalR, string strShow)
	{
		this.nominalE = nominalE;
		this.nominalR = nominalR;
		this.strShow = strShow;
		return this;
	}

	public override EntityData Save() => new NominalSourceData(this);

	[System.Serializable]
	private class NominalSourceData : SourceStandData
	{
		private readonly double nominalE, nominalR;
		private readonly string strShow;
		public NominalSourceData(NominalSource nominalSource) : base(nominalSource)
		{
			nominalE = nominalSource.nominalE;
			nominalR = nominalSource.nominalR;
			strShow = nominalSource.strShow;
		}

		public override void Load()
		{
			NominalSource nominalSource = BaseCreate<NominalSource>(baseData).Set(nominalE, nominalR, strShow);
			nominalSource.E = E;
			nominalSource.R = R;
		}
	}
}