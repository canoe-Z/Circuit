using UnityEngine;
using UnityEngine.UI;

public class VoltmeterText : MonoBehaviour
{
    DigtalVoltmeter digtalVoltmeter;

    // Start is called before the first frame update
    void Start()
    {
        digtalVoltmeter = transform.parent.gameObject.transform.parent.gameObject.GetComponent<DigtalVoltmeter>();
    }

    // Update is called once per frame
    void Update()
    {
        double Vtext;
        double GND = digtalVoltmeter.ChildPorts[0].U;
        double mV = digtalVoltmeter.ChildPorts[1].U;
        double V = digtalVoltmeter.ChildPorts[2].U;
        if (digtalVoltmeter.ChildPorts[1].Connected == 1)
        {
            Vtext = (mV - GND) * 1000;
        }
        else if (digtalVoltmeter.ChildPorts[2].Connected == 1)
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
