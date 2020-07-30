using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 待测电源
/// </summary>
public class NominalSource : Source
{
	public bool RealValueSet = false;
	public double NominalE;
	/*
	void Start()
	{
		if (!RealValueSet)
		{
			Value = Nominal.GetRealValue(NominalValue);
			RealValueSet = true;
		}
		if (resistanceText) resistanceText.text = "待测" + NominalValue.ToString() + "Ω";
	}


	private void Update()
	{
		Debug.LogError(Value.ToString());
	}
	public override EntityData Save()
	{
		return new NominalRData(NominalValue, Value, transform.position, transform.rotation, ChildPortID);
	}
	*/
}
