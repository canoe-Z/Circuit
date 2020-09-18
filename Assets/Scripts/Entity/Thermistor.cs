using SpiceSharp.Components;
using UnityEngine.UI;

/// <summary>
/// 热敏电阻
/// </summary>
public class Thermistor : EntityBase
{
	private double RValue;
	private double TWill = 90;
	private double TNow = 30;
	private MyKnob knob;
	private Text TWillText;
	private Text TNowText;
	protected int PortID_Left, PortID_Right;

	public override void EntityAwake()
	{
		TWillText = transform.GetChildByName("Will").GetComponent<Text>();
		TNowText = transform.GetChildByName("Now").GetComponent<Text>();
		knob = GetComponentInChildren<MyKnob>();
	}

	void Start()
	{
		PortID_Left = ChildPorts[0].ID;
		PortID_Right = ChildPorts[1].ID;
	}

	void FixedUpdate()
	{
		if(TWill>TNow)
		{
			TWill -= 0.1;
		}
		else
		{
			TWill += 0.1;
		}
		RValue = 100 * TNow;
		CircuitCalculator.NeedCalculate = true;
	}

	public override void LoadElement() => CircuitCalculator.UF.Union(PortID_Left, PortID_Right);


	public override void SetElement(int entityID)
	{
		CircuitCalculator.SpiceEntities.Add(new Resistor(entityID.ToString(), PortID_Left.ToString(), PortID_Right.ToString(), RValue));
	}

	public override EntityData Save() => new SimpleEntityData<Thermistor>(this);
}

