using UnityEngine;
using UnityEngine.UI;

public class AmmeterText : MonoBehaviour
{
    private DigtalAmmeter digitalAmmter;

    // Start is called before the first frame update
    void Start()
    {
        digitalAmmter = transform.parent.gameObject.transform.parent.gameObject.GetComponent<DigtalAmmeter>();
    }

    // Update is called once per frame
    void Update()
    {
		double Atext;
		double mA = digitalAmmter.ChildPorts[1].I;
        double A = digitalAmmter.ChildPorts[2].I;
        if (digitalAmmter.ChildPorts[1].Connected == 1)
        {
            Atext = mA * 1000;
        }
        else if (digitalAmmter.ChildPorts[2].Connected == 1)
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
