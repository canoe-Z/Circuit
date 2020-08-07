using UnityEngine;

/// <summary>
/// 含有标称值的元件或将用到的方法
/// </summary>
public static class Nominal
{
	public static double GetRealValue(double nominalValue)
	{
		return nominalValue * (1 + Random.Range(-0.05f, 0.05f));
	}

	public static double GetRealValueLargeError(double nominalValue)
	{
		return nominalValue * (1 + Random.Range(-0.1f, 0.1f));
	}
}
