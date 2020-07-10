using UnityEngine;
using UnityEngine.UI;

public class VoltmeterText : MonoBehaviour
{
    TVoltmeter TVoltmeter;

    // Start is called before the first frame update
    void Start()
    {
        TVoltmeter = transform.parent.gameObject.transform.parent.gameObject.GetComponent<TVoltmeter>();
    }

    // Update is called once per frame
    void Update()
    {
        double Vtext;
        double GND = TVoltmeter.childsPorts[0].U;
        double mV = TVoltmeter.childsPorts[1].U;
        double V = TVoltmeter.childsPorts[2].U;
        if (TVoltmeter.childsPorts[1].Connected == 1)
        {
            Vtext = (mV - GND) * 1000;
        }
        else if (TVoltmeter.childsPorts[2].Connected == 1)
        {
            Vtext = V - GND;
        }
        else
		{
            Vtext = 0;
        }
        if (Vtext > 999.99)
		{
            Vtext = 999.99;
        }
        if (Vtext < -999.99)
        {
            Vtext = -999.99;
        }
        Text Text = GetComponent<Text>();
        Text.text = Vtext.ToString("0.00");
    }
}
