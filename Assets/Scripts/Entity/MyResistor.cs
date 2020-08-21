using SpiceSharp.Components;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 电阻
/// </summary>
public class MyResistor : EntityBase
{
	protected double RValue = 120;
	protected Text resistanceText;
	protected int PortID_Left, PortID_Right;
	private const int maxSize = 7;  //填满一行的字符数量

	public override void EntityAwake()
	{
		resistanceText = transform.FindComponent_DFS<Text>("Text");
	}

	void Start()
	{
		// 不能在Awake(）中执行，Awake()之后还可能修改阻值
		string str;

		// 根据阻值确定显示方式
		if (RValue >= 1e6)
		{
			str = (RValue / 1e6).ToString() + "MΩ";
		}
		else if (RValue >= 1e3)
		{
			str = (RValue / 1e3).ToString() + "kΩ";
		}
		else
		{
			str = RValue.ToString() + "Ω";
		}

		// 控制字符串长度
		if (str.Length >= maxSize)
		{
			resistanceText.fontSize = resistanceText.fontSize * 7 / str.Length;
		}

		resistanceText.text = str;

		PortID_Left = ChildPorts[0].ID;
		PortID_Right = ChildPorts[1].ID;
	}

	public override void LoadElement() => CircuitCalculator.UF.Union(PortID_Left, PortID_Right);

	public override void SetElement(int entityID)
	{
		CircuitCalculator.SpiceEntities.Add(new Resistor(entityID.ToString(), PortID_Left.ToString(), PortID_Right.ToString(), RValue));
	}

	public static GameObject Create(double RValue)
	{
		return BaseCreate<MyResistor>().Set(RValue).gameObject;
	}

	public MyResistor Set(double RValue)
	{
		this.RValue = RValue;
		return this;
	}

	public override EntityData Save() => new ResistorData(this);

	[System.Serializable]
	protected class ResistorData : EntityData
	{
		protected double RValue;

		public ResistorData(MyResistor resistance)
		{
			baseData = new EntityBaseData(resistance);
			RValue = resistance.RValue;
		}

		public override void Load() => BaseCreate<MyResistor>(baseData).Set(RValue);
	}
}

