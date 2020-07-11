using UnityEngine;

public class CircuitRope : MonoBehaviour
{
	public void DestroyRope()
	{
		gameObject.GetComponent<CircuitLine>().DestroyLine();
		Destroy(gameObject);
	}
	private void OnMouseOver()
	{
		ShowTip.OverChain();
		ShowTip.IsTipShowed = false;
		if (Input.GetMouseButtonDown(1))
		{
			DestroyRope();
		}
	}

	// Update is called once per frame
	void FixedUpdate()
	{
		gameObject.GetComponent<MeshCollider>().sharedMesh = gameObject.GetComponent<MeshFilter>().sharedMesh;
	}
}
