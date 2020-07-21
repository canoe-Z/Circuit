public static class EntityText
{
	public static string GetText(double value)
	{
		return value.ToString();
	}

	public static string GetText(double value, double maxValue)
	{
		if (value > maxValue)
		{
			value = maxValue;
		}
		if (value < -maxValue)
		{
			value = -maxValue;
		}
		return GetText(value);
	}

	public static string GetText(double value, int decimalNum)
	{
		return value.ToString("N" + decimalNum);
	}

	public static string GetText(double value, double maxValue, int decimalNum)
	{
		if (value > maxValue)
		{
			value = maxValue;
		}
		if (value < -maxValue)
		{
			value = -maxValue;
		}
		return value.ToString("N" + decimalNum);
	}
}