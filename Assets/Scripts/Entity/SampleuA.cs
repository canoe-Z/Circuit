using SpiceSharp.Components;
using UnityEngine;

/// <summary>
/// 微安表，有各种量程和内阻的规格，内阻为标称
/// </summary>
public class SampleuA : EntityBase, IAmmeter
{
    public double maxI0 = 0.05;//量程，单位安培
	public double r0 = 2;//内阻

	/// <summary>
	/// 变成某一种微安表
	/// </summary>
	public void MyChangeToWhichType(int uA)
	{
		calculateFlag = true;
		maxI0 = (double)uA / 1000000;
		r0 = (double)100 / uA;//50微安时为2欧姆，成反比
		myPin.MyChangePos(0);
		myPin.MySetString("uA", uA);//
	}
	bool calculateFlag = false;//这东西被置为true时，在下一帧Update电路进行重新计算

	MyPin myPin;//指针（显示数字的那种）
	public override void EntityAwake()
	{
		//得到引用并且初始化
		myPin = GetComponentInChildren<MyPin>();
		MyChangeToWhichType(50);
	}

	public override void LoadElement()
	{
		int GND = ChildPorts[0].ID;
		int V0 = ChildPorts[1].ID;
		CircuitCalculator.UF.Union(GND, V0);
	}

	public override void SetElement()
	{
		int EntityID = CircuitCalculator.EntityNum;
		int GND = ChildPorts[0].ID;
		int V0 = ChildPorts[1].ID;
		CircuitCalculator.SpiceEntities.Add(new Resistor(EntityID.ToString(), GND.ToString(), V0.ToString(), r0));

		CircuitCalculator.SpicePorts.Add(ChildPorts[0]);
		CircuitCalculator.SpicePorts.Add(ChildPorts[1]);
	}

	public override EntityData Save()
	{
		// 微安表属于简单元件
		return new SimpleEntityData<SampleuA>(transform.position, transform.rotation, ChildPortID);
	}

	public void CalculateCurrent()
	{
		ChildPorts[1].I = (ChildPorts[1].U - ChildPorts[0].U) / r0;
		
		myPin.MyChangePos((float)(ChildPorts[1].I / maxI0));

		//myPin.MyChangePos(0.55f);//改变指针位置，这里有bug
	}
	
	void Start()
    {
        
    }
	
    void Update()
    {
		if (calculateFlag)
		{
			calculateFlag = false;
			CircuitCalculator.CalculateAll();
		}
	}
}
