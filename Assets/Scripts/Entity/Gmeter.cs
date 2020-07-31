using SpiceSharp.Components;
using UnityEngine;

public class Gmeter : EntityBase, IAmmeter
{
	private double MaxI = 0.001;
	private double R = 10;
	private GameObject pin = null;
	private float pinPos = 0;//1单位1分米1600像素，750像素=0.46875，1500像素=0.9375，800爆表0.5
	public MySlider mySlider = null;
	private int LeftPortID, RightPortID;

	public override void EntityAwake()
	{
		FindPin();
		//滑块
		mySlider = this.gameObject.GetComponentInChildren<MySlider>();
		mySlider.Devide = 5;
	}

	void Start()
	{
		LeftPortID = ChildPorts[0].ID;
		RightPortID = ChildPorts[1].ID;
	}

	void Update()
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
		CircuitCalculator.UF.Union(LeftPortID, RightPortID);
	}

	public override void SetElement()
	{
		int EntityID = CircuitCalculator.EntityNum;

		CircuitCalculator.SpiceEntities.Add(new Resistor(EntityID.ToString(), LeftPortID.ToString(), RightPortID.ToString(), R));

		CircuitCalculator.SpicePorts.Add(ChildPorts[0]);
		CircuitCalculator.SpicePorts.Add(ChildPorts[1]);
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
