using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wdw_SpeedLimit : MonoBehaviour
{
	Rigidbody rigidBody;
	public float speedLimit = 1f;
    // Start is called before the first frame update
    void Start()
    {
		rigidBody = GetComponent<Rigidbody>();
	}
	private void FixedUpdate()//与物理引擎保持帧同步
	{
		if (rigidBody)
		{
			if (rigidBody.velocity.magnitude > speedLimit)
			{
				rigidBody.velocity = rigidBody.velocity.normalized * speedLimit;
			}
		}
	}
}
