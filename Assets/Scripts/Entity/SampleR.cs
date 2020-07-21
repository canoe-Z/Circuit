using System.Collections.Generic;
using UnityEngine;

public class SampleR : Resistance
{
	public double SampleValue;
	void Start()
	{
		if (resistanceText) resistanceText.text = SampleValue.ToString();
	}

	public override EntityData Save()
	{
		return new SampleRData(SampleValue, Value, gameObject.transform.position, ChildPortID);
	}
}

[System.Serializable]
public class SampleRData : ResistanceData
{
	private readonly double sampleValue;
	private readonly double rnum;
	public SampleRData(double sampleValue, double rnum, Vector3 pos, List<int> id) : base(rnum, pos, id)
	{
		this.sampleValue = sampleValue;
		this.rnum = rnum;
	}

	override public void Load()
	{
		SampleR resistance = EntityCreator.CreateEntity<SampleR>(posfloat, IDList);
		resistance.SampleValue = sampleValue;
		resistance.Value = rnum;
	}
}
