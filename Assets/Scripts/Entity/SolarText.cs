using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SolarText : MonoBehaviour
{
    Solar Solar;

    // Start is called before the first frame update
    void Start()
    {
        Solar = transform.parent.gameObject.transform.parent.gameObject.GetComponent<Solar>();
    }

    // Update is called once per frame
    void Update()
    {
        double Stext = Solar.sliders[0].SliderPos * 1000;
        Text Text = GetComponent<Text>();
        Text.text = Stext.ToString("0.00");
    }
}
