using System.Collections.Generic;
using SpiceSharp.Components;
using UnityEngine;
using UnityEngine.UI;

public class Resistance : EntityBase
{
	[ReadOnly] public double Value = 120;
	protected Text resistanceText;

	public override void EntityAwake()
	{
		resistanceText = transform.FindComponent_DFS<Text>("Text");
	}

	const int maxSize = 7;//填满一行的字符数量
	void Start()
	{
		// 不能在Awake(）中执行，Awake()之后还可能修改阻值
		string str;

		// 根据阻值确定显示方式
		if (Value > 1000000)
		{
			str = (Value / 1000000).ToString() + "MΩ";
		}
		else if (Value > 1000)
		{
			str = (Value / 1000).ToString() + "kΩ";
		}
		else
		{
			str = Value.ToString() + "Ω";
		}

		if (str.Length > maxSize)
		{
			resistanceText.fontSize = resistanceText.fontSize * 7 / str.Length;
		}
		resistanceText.text = str;
	}

	public override void LoadElement()
	{
		int LeftPortID = ChildPorts[0].ID;
		int RightPortID = ChildPorts[1].ID;

		CircuitCalculator.UF.Union(LeftPortID, RightPortID);
	}

	public override void SetElement()
	{
		int EntityID = CircuitCalculator.EntityNum;
		int LeftPortID = ChildPorts[0].ID;
		int RightPortID = ChildPorts[1].ID;

		CircuitCalculator.SpiceEntities.Add(new Resistor(EntityID.ToString(), LeftPortID.ToString(), RightPortID.ToString(), Value));
	}

	public override EntityData Save()
	{
		return new ResistanceData(Value, transform.position, transform.rotation, ChildPortID);
	}
}

[System.Serializable]
public class ResistanceData : EntityData
{
	private readonly double value;
	public ResistanceData(double value, Vector3 pos, Quaternion angle, List<int> IDList) : base(pos, angle, IDList)
	{
		this.value = value;
	}

	override public void Load()
	{
		Resistance resistance = EntityCreator.CreateEntity<Resistance>(posfloat, anglefloat, IDList);
		resistance.Value = value;
	}
}