using System.Collections.Generic;
using UnityEngine;

public class SampleR : Resistance
{
	public double SampleValue;
	void Start()
	{
		if (resistanceText) resistanceText.text = SampleValue.ToString() + "Ω";
	}

	public override EntityData Save()
	{
		return new SampleRData(SampleValue, Value, transform.position, transform.rotation, ChildPortID);
	}
}

[System.Serializable]
public class SampleRData : ResistanceData
{
	private readonly double sampleValue;
	private readonly double value;
	public SampleRData(double sampleValue, double value, Vector3 pos, Quaternion angle, List<int> id) : base(value, pos, angle, id)
	{
		this.sampleValue = sampleValue;
		this.value = value;
	}

	override public void Load()
	{
		SampleR resistance = EntityCreator.CreateEntity<SampleR>(posfloat, anglefloat, IDList);
		resistance.SampleValue = sampleValue;
		resistance.Value = value;
	}
}
