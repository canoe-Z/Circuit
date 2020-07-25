using SpiceSharp.Components;

/// <summary>
/// 微安表，有各种量程和内阻的规格，内阻为标称
/// </summary>
public class SampleuA : EntityBase, IAmmeter
{
    public double MaxI0 = 0.05;
	public double R0 = 2;

	public override void EntityAwake()
	{
		//
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
		CircuitCalculator.SpiceEntities.Add(new Resistor(EntityID.ToString(), GND.ToString(), V0.ToString(), R0));
	}

	public override EntityData Save()
	{
		// 微安表属于简单元件
		return new SimpleEntityData<SampleuA>(transform.position, transform.rotation, ChildPortID);
	}

	public void CalculateCurrent()
	{
		ChildPorts[1].I = (ChildPorts[1].U - ChildPorts[0].U) / R0;
	}

	// Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
