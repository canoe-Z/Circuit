using UnityEngine;
using UnityEngine.UI;

public class AmmeterText : MonoBehaviour
{
    TAmmeter TAmmeter;

    // Start is called before the first frame update
    void Start()
    {
        TAmmeter = transform.parent.gameObject.transform.parent.gameObject.GetComponent<TAmmeter>();
    }

    // Update is called once per frame
    void Update()
    {
		double Atext;
		double mA = TAmmeter.ChildPorts[1].I;
        double A = TAmmeter.ChildPorts[2].I;
        if (TAmmeter.ChildPorts[1].Connected == 1)
        {
            Atext = mA * 1000;
        }
        else if (TAmmeter.ChildPorts[2].Connected == 1)
        {
            Atext = A;
        }
        else
        {
            Atext = 0;
        }
        if (Atext > 999.99)
        {
            Atext = 999.99;
        }
        if (Atext < -999.99)
        {
            Atext = -999.99;
        }
        Text Text = GetComponent<Text>();
        Text.text = Atext.ToString("0.00");
    }
}
