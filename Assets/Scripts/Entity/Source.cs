using System.Collections.Generic;
using SpiceSharp.Components;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 单个独立电源，准确使用时可用于标准电源等场景，派生出标称类，用于待测电源和干电池
/// </summary>
public class Source : EntityBase, ISource
{
	protected double E, R;
	protected int PortID_G, PortID_V;
	protected Text sourceText;
	public override void EntityAwake()
	{
		sourceText = GetComponentInChildren<Text>();
	}

	void Start()
	{
		sourceText.text = E.ToString() + "V";

		PortID_G = ChildPorts[0].ID;
		PortID_V = ChildPorts[1].ID;
	}

	public override void LoadElement() => CircuitCalculator.UF.Union(PortID_G, PortID_V);

	public override void SetElement(int entityID)
	{
		CircuitCalculator.SpiceEntities.Add(new VoltageSource(entityID.ToString(), PortID_V.ToString(), string.Concat(entityID.ToString(), "_rPort"), E));
		CircuitCalculator.SpiceEntities.Add(new Resistor(string.Concat(entityID.ToString(), "_r"), string.Concat(entityID.ToString(), "_rPort"), PortID_G.ToString(), R));
		//默认不接地，连接到电路中使用，如果电路中没有形成对0的通路，将其接地
	}

	public void GroundCheck()
	{
		if (IsConnected())
		{
			if (!CircuitCalculator.UF.Connected(PortID_G, 0))
			{
				CircuitCalculator.UF.Union(PortID_G, 0);
				CircuitCalculator.GNDLines.Add(new GNDLine(PortID_G));
			}
		}
	}

	public static GameObject Create(double E, double R)
	{
		return Set(BaseCreate<Source>(), E, R).gameObject;
	}

	public static Source Set(Source source, double E, double R)
	{
		source.E = E;
		source.R = R;
		return source;
	}

	public override EntityData Save() => new SourceStandData(this);

	[System.Serializable]
	protected class SourceStandData : EntityData
	{
		protected double E, R;
		public SourceStandData(Source source)
		{
			baseData = new EntityBaseData(source);
			E = source.E;
			R = source.R;
		}

		public override void Load() => Set(BaseCreate<Source>(baseData), E, R);
	}
}
