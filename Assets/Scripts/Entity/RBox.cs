using System.Collections.Generic;
using System.Linq;
using SpiceSharp.Components;
using UnityEngine;

public class RBox : EntityBase
{
	private const int knobNum = 6;              // 含有的旋钮个数
	private double R_99999, R_99, R_09;         // 不同挡位下的内阻
	private int G, R999, R99, R9;

	public List<MyKnob> Knobs { get; set; }

	public override void EntityAwake()
	{
		Knobs = transform.FindComponentsInChildren<MyKnob>().OrderBy(x => x.name).ToList();
		if (Knobs.Count != knobNum) Debug.LogError("旋钮个数不合法");
		Knobs.ForEach(x => x.Devide = 10);
	}

	void Start()
	{
		// 第一次执行初始化，此后受事件控制
		Knobs.ForEach(x => x.KnobEvent += UpdateKnob);
		UpdateKnob();

		G = ChildPorts[0].ID;
		R9 = ChildPorts[1].ID;
		R99 = ChildPorts[2].ID;
		R999 = ChildPorts[3].ID;
	}

	void UpdateKnob()
	{
		int total = 0;
		for (int i = 0; i < knobNum; i++)
		{
			total *= 10;
			total += Knobs[i].KnobPos_int;
		}
		R_99999 = total / (float)10;
		R_99 = total % 100 / (float)10;
		R_09 = total % 10 / (float)10;
	}

	public override void LoadElement()
	{
		CircuitCalculator.UF.Union(G, R9);
		CircuitCalculator.UF.Union(G, R99);
		CircuitCalculator.UF.Union(G, R999);
	}

	public override void SetElement()
	{
		int EntityID = CircuitCalculator.EntityNum;

		// 指定三个电阻的ID
		string[] ResistorID = new string[3];
		for (int i = 0; i < 3; i++)
		{
			ResistorID[i] = string.Concat(EntityID, "_", i);
		}

		CircuitCalculator.SpiceEntities.Add(new Resistor(ResistorID[0], G.ToString(), R999.ToString(), R_99999));
		CircuitCalculator.SpiceEntities.Add(new Resistor(ResistorID[1], G.ToString(), R99.ToString(), R_99));
		CircuitCalculator.SpiceEntities.Add(new Resistor(ResistorID[2], G.ToString(), R9.ToString(), R_09));
	}

	// 电阻箱添加时按简单元件处理
	public static GameObject Create(List<int> knobRotIntList = null, Float3 pos = null, Float4 angle = null, List<int> IDList = null)
	{
		RBox rBox = BaseCreate<RBox>(pos, angle, IDList);
		if (knobRotIntList != null)
		{
			for (var i = 0; i < knobRotIntList.Count; i++)
			{
				rBox.Knobs[i].SetKnobRot(knobRotIntList[i]);
			}
		}
		return rBox.gameObject;
	}

	public override EntityData Save() => new RboxData(Knobs, transform.position, transform.rotation, ChildPortID);
}

[System.Serializable]
public class RboxData : EntityData
{
	private readonly List<int> KnobRotIntList = new List<int>();
	public RboxData(List<MyKnob> knobs, Vector3 pos, Quaternion angle, List<int> IDList) : base(pos, angle, IDList)
	{
		knobs.ForEach(x => KnobRotIntList.Add(x.KnobPos_int));
	}

	override public void Load() => RBox.Create(KnobRotIntList, pos, angle, IDList);
}