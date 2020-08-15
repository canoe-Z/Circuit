using System.Collections.Generic;
using System.Linq;
using SpiceSharp.Components;
using UnityEngine;

/// <summary>
/// 电阻箱
/// </summary>
public class RBox : EntityBase
{
	private readonly int knobNum = 6;                           // 含有的旋钮个数
	private double R_99999, R_99, R_9;                          // 不同挡位下的内阻
	private int PortID_G, PortID_R999, PortID_R99, PortID_R9;
	private List<MyKnob> knobs;

	public override void EntityAwake()
	{
		knobs = transform.FindComponentsInChildren<MyKnob>().OrderBy(x => x.name).ToList();
		if (knobs.Count != knobNum) Debug.LogError("旋钮个数不合法");
		knobs.ForEach(x => x.Devide = 10);
	}

	void Start()
	{
		// 第一次执行初始化，此后受事件控制
		knobs.ForEach(x => x.KnobEvent += UpdateKnob);
		UpdateKnob();

		PortID_G = ChildPorts[0].ID;
		PortID_R9 = ChildPorts[1].ID;
		PortID_R99 = ChildPorts[2].ID;
		PortID_R999 = ChildPorts[3].ID;
	}

	private void UpdateKnob()
	{
		int total = 0;
		for (int i = 0; i < knobNum; i++)
		{
			total *= 10;
			total += knobs[i].KnobPos_int;
		}
		R_99999 = total / (float)10;
		R_99 = total % 100 / (float)10;
		R_9 = total % 10 / (float)10;
	}

	public override void LoadElement()
	{
		CircuitCalculator.UF.Union(PortID_G, PortID_R9);
		CircuitCalculator.UF.Union(PortID_G, PortID_R99);
		CircuitCalculator.UF.Union(PortID_G, PortID_R999);
	}

	public override void SetElement(int entityID)
	{
		CircuitCalculator.SpiceEntities.Add(new Resistor(string.Concat(entityID, "_", 0), PortID_G.ToString(), PortID_R999.ToString(), R_99999));
		CircuitCalculator.SpiceEntities.Add(new Resistor(string.Concat(entityID, "_", 1), PortID_G.ToString(), PortID_R99.ToString(), R_99));
		CircuitCalculator.SpiceEntities.Add(new Resistor(string.Concat(entityID, "_", 2), PortID_G.ToString(), PortID_R9.ToString(), R_9));
	}

	public override EntityData Save() => new RboxData(this);

	[System.Serializable]
	public class RboxData : EntityData
	{
		private readonly List<int> knobRotIntList = new List<int>();

		public RboxData(RBox RBox)
		{
			baseData = new EntityBaseData(RBox);
			RBox.knobs.ForEach(x => knobRotIntList.Add(x.KnobPos_int));
		}

		public override void Load()
		{
			RBox RBox = BaseCreate<RBox>(baseData);
			for (var i = 0; i < knobRotIntList.Count; i++)
			{
				RBox.knobs[i].SetKnobRot(knobRotIntList[i]);
			}
		}
	}
}

