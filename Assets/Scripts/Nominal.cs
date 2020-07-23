using System;

/// <summary>
/// 含有标称值的元件或将用到的方法
/// </summary>
public static class Nominal
{
	public static double GetRealValue(double nominalValue)
	{
		Random rd = new Random();
		return nominalValue * (1 + rd.Next(0, 500) * 0.0001);
	}

	public static double GetRealValueLargeError(double nominalValue)
	{
		Random rd = new Random();
		return nominalValue * (1 + rd.Next(0, 1000) * 0.0001);
	}
}
