using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EachRope : MonoBehaviour
{
	public void DestroyRope()
	{
		foreach (Transform child in transform)
		{
			Debug.Log("检测到待删除导线:" + child.gameObject.name);
			child.GetComponent<CircuitLine>().DestroyLine();
		}
		Debug.Log("删除导线成功");
		Destroy(this.gameObject.transform.root.gameObject);
	}
	private void OnMouseOver()
	{
		ShowTip.OverChain();
		if (Input.GetMouseButtonDown(1))
		{
			DestroyRope();
		}
	}
	// Start is called before the first frame update
	void Start()
    {
		Physics.IgnoreLayerCollision(0, 8);
	}

	// Update is called once per frame
	void Update()
	{
		this.gameObject.GetComponent<MeshCollider>().sharedMesh = null;
		this.gameObject.GetComponent<MeshCollider>().sharedMesh = this.gameObject.GetComponent<MeshFilter>().sharedMesh;
		//this.gameObject.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load<Material>("Button");
	}
}
