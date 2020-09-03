using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    private bool isEmission = false;
    // Start is called before the first frame update
    void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if(!isEmission)
			{
                GetComponent<Renderer>().material.SetColor("Color_592D9D79", Color.yellow);
                isEmission = true;
            }
            else
			{
                GetComponent<Renderer>().material.SetColor("Color_592D9D79", Color.black);
                isEmission = false;
            }
        }
    }
}
