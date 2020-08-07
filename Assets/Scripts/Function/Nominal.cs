using UnityEngine;

/// <summary>
/// 通过标称值获取真实值
/// </summary>
public static class Nominal
{
	public static double GetRealValue(double nominalValue, float range = 0.05f)
	{
		return nominalValue * (1 + Random.Range(-range, range));
	}
}