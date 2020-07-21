using UnityEngine;

[System.Serializable]
public class Float3
{
	public float x, y, z;

	public Float3(float x, float y, float z)
	{
		this.x = x;
		this.y = y;
		this.z = z;
	}
}

[System.Serializable]
public class Float4
{
	public float x, y, z, w;

	public Float4(float x, float y, float z, float w)
	{
		this.x = x;
		this.y = y;
		this.z = z;
		this.w = w;
	}
}

public static class Vector3Extensions
{
	public static Float3 ToFloat3(this Vector3 vector)
	{
		return new Float3(vector.x, vector.y, vector.z);
		
	}
}

public static class QuaternionExtensions
{
	public static Float4 ToFloat3(this Quaternion quaternion)
	{
		return new Float4(quaternion.x, quaternion.y, quaternion.z, quaternion.w);
	}
}