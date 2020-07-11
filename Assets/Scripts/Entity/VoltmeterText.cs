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
        double GND = TVoltmeter.ChildPorts[0].U;
        double mV = TVoltmeter.ChildPorts[1].U;
        double V = TVoltmeter.ChildPorts[2].U;
        if (TVoltmeter.ChildPorts[1].Connected == 1)
        {
            Vtext = (mV - GND) * 1000;
        }
        else if (TVoltmeter.ChildPorts[2].Connected == 1)
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
