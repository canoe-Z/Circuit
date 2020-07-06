using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuScript : MonoBehaviour
{
    public void OnYesButton()
    {
        Application.Quit();
    }

    public void OnNoButton()
    {
        Global.CloseMenu();
    }
}
