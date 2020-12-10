using SpiceSharp.Components;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 电阻
/// </summary>
public class MyResistor : EntityBase, IShow
{
	protected double RValue = 120;
	protected Text resistanceText;
	protected int PortID_Left, PortID_Right;

	public override void EntityAwake()
	{
		resistanceText = transform.FindComponent_DFS<Text>("Text");
	}

	void Start()
	{
		PortID_Left = ChildPorts[0].ID;
		PortID_Right = ChildPorts[1].ID;
	}

	public override void LoadElement() => CircuitCalculator.UF.Union(PortID_Left, PortID_Right);

	public override void SetElement(int entityID)
	{
		CircuitCalculator.SpiceEntities.Add(new Resistor(entityID.ToString(), PortID_Left.ToString(), PortID_Right.ToString(), RValue));
	}

	public static GameObject Create(double RValue, string str)
	{
		return BaseCreate<MyResistor>().Set(RValue, str).gameObject;
	}

	public MyResistor Set(double RValue, string str)
	{
		this.RValue = RValue;
		resistanceText.text = str;
		return this;
	}

	public override EntityData Save() => new ResistorData(this);

	[System.Serializable]
	protected class ResistorData : EntityData
	{
		protected double RValue;
		private string str;

		public ResistorData(MyResistor resistor)
		{
			baseData = new EntityBaseData(resistor);
			RValue = resistor.RValue;
			str = resistor.resistanceText.text;
		}

		public override void Load() => BaseCreate<MyResistor>(baseData).Set(RValue, str);
	}
	public void MyShowString()
	{
		DisplayController.myTipsToShow = "电阻\n当前真实阻值：" + RValue.ToString("0.000000");
	}
}

