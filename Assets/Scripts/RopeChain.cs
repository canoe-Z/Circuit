using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RopeChain : MonoBehaviour
{
	private void OnMouseExit()
	{
		this.GetComponent<Rigidbody>().mass = 0.001f;
	}
	private void OnMouseOver()
	{
		ShowTip.OverChain();
	}
	private void OnMouseDrag()
	{
		this.GetComponent<Rigidbody>().mass = 1000;

		Vector3 campos = Camera.main.transform.position;
		Vector3 thispos = this.gameObject.transform.position;
		float dis = (thispos - campos).magnitude;

		/*{//对dis进行处理
			Vector3 hitvec;
			if(Fun.HitOnlyOne(out hitvec))
			{
				float newdis = (hitvec - campos).magnitude;
				newdis -= 0.1f;
				if (newdis < dis) dis = newdis;//修正距离
			}
		}*/
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		Vector3 vec = ray.direction;
		vec.Normalize();
		vec *= dis;

		thispos = campos + vec;
		this.gameObject.transform.position = thispos;
	}
	public void Break()
    {
		GameObject father = this.gameObject.transform.parent.gameObject;
		Destroy(father);
    }
}
