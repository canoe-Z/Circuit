using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class MyHelper : MonoBehaviour
{
	Canvas canvas;
	void Start()
	{
		canvas = GetComponent<Canvas>();
		canvas.enabled = false;
	}

	// Update is called once per frame
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.F1))
		{
			canvas.enabled = !canvas.enabled;
		}
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			canvas.enabled = false;
		}

	}
}
