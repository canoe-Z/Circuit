using SpiceSharp.Components;
using UnityEngine;

/// <summary>
/// 电流计
/// </summary>
public class Gmeter : EntityBase, ICalculatorUpdate
{
	private double MaxI = 0.001;
	private double R = 10;
	private GameObject pin = null;
	private float pinPos = 0;//1单位1分米1600像素，750像素=0.46875，1500像素=0.9375，800爆表0.5
	private MySlider mySlider = null;
	private int PortID_Left, PortID_Right;

	public override void EntityAwake()
	{
		FindPin();
		//滑块
		mySlider = this.gameObject.GetComponentInChildren<MySlider>();
		mySlider.Devide = 5;
	}

	void Start()
	{
		CircuitCalculator.CalculateEvent += CalculatorUpdate;
		CalculatorUpdate();

		PortID_Left = ChildPorts[0].ID;
		PortID_Right = ChildPorts[1].ID;
	}

	public void CalculatorUpdate()
	{
		//量程
		MaxI = 0.1;
		R = 10;
		for (int i = 0; i < mySlider.SliderPos_int; i++)
		{
			MaxI *= 0.01;
			R *= 2;
		}

		//示数
		double doublePin = (ChildPorts[0].I) / MaxI;
		//doublePin -= 0.5;
		pinPos = (float)(doublePin * 0.9375);
		if (pinPos > 0.5) pinPos = 0.5f;
		else if (pinPos < -0.5) pinPos = -0.5f;

		Vector3 pos = pin.transform.localPosition;
		pos.z = pinPos;
		pin.transform.localPosition = pos;

	}

	//电路相关
	public void FindPin()
	{
		int childNum = transform.childCount;
		for (int i = 0; i < childNum; i++)
		{
			if (transform.GetChild(i).name == "Pin")
			{
				pin = transform.GetChild(i).gameObject;
				return;
			}
		}
	}

	public override void LoadElement()
	{
		CircuitCalculator.UF.Union(PortID_Left, PortID_Right);
	}

	public override void SetElement(int entityID)
	{
		CircuitCalculator.SpiceEntities.Add(new Resistor(entityID.ToString(), PortID_Left.ToString(), PortID_Right.ToString(), R));

		CircuitCalculator.SpicePorts.AddRange(ChildPorts);
	}

	public void CalculateCurrent()
	{
		ChildPorts[0].I = (ChildPorts[1].U - ChildPorts[0].U) / R;
	}

	public override EntityData Save()
	{
		return new SimpleEntityData<Gmeter>(transform.position, transform.rotation, ChildPortID);
	}
}
