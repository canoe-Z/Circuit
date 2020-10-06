using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowOperate : MonoBehaviour
{
	public GameObject prefab;
	GameObject gm;
	public bool getTexture;
	public float size = 1;
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{
		if (prefab)
		{
			if (gm) Destroy(gm);

			gm = Instantiate(prefab, Vector3.zero, Quaternion.identity, transform);
			gm.transform.localPosition = Vector3.zero;
			gm.name = gm.name.Replace("(Clone)", "");

			EntityBase[] es = GetComponentsInChildren<EntityBase>();
			foreach (var e in es)
			{
				Destroy(e);
			}
			CircuitPort[] cs = GetComponentsInChildren<CircuitPort>();
			foreach (var c in cs)
			{
				Destroy(c);
			}
			Rigidbody rigidbody = gm.GetComponent<Rigidbody>();
			rigidbody.isKinematic = true;

			prefab = null;
		}
		if (getTexture)
		{
			getTexture = false;
			ScreenCapture.CaptureScreenshot("C:/Users/vc/Desktop/Screen/" + gm.name + ".png");
			deleteCounter = 10;

		}
		if (gm) gm.transform.localScale = new Vector3(size, size, size);
		if (deleteCounter > 0)
		{
			deleteCounter--;
			if (deleteCounter == 0)
			{
				Destroy(gm);
				gm = null;
			}

		}
	}
	int deleteCounter = 0;
}
