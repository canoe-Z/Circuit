using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Wdw_EscapeMenu : MonoBehaviour
{
	public GameObject mainThings;
	public Button yes;
	public Button no;
	public Button addSource;
    void Start()
    {
		if (mainThings == null) Debug.LogError("这里没挂");
		if (yes == null) Debug.LogError("这里没挂");
		if (no == null) Debug.LogError("这里没挂");
		if (addSource == null) Debug.LogError("这里没挂");
		CloseMenu();
		yes.onClick.AddListener(OnYesButton);
		no.onClick.AddListener(OnNoButton);
		addSource.onClick.AddListener(OnAddSourceButton);
    }

	// Update is called once per frame
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.P))
		{
			Global.boolMove = false;
		}
		if (Input.GetKeyDown(KeyCode.Escape))//开启或者关闭菜单，取决于是否进行过无法移动锁定
		{
			if (Global.boolMove) OpenMenu();
			else CloseMenu();
		}
	}
	
	//打开菜单
	void OpenMenu()
	{
		Cursor.lockState = CursorLockMode.None;//解除鼠标锁定
		Cursor.visible = true;
		Global.boolMove = false;//不允许移动视角
		mainThings.SetActive(true);
	}
	//关闭菜单
	void CloseMenu()
	{
		Cursor.lockState = CursorLockMode.Locked;//锁定鼠标于中央
		Cursor.visible = false;
		Global.boolMove = true;//允许移动视角
		mainThings.SetActive(false);
	}

	void OnYesButton()
	{
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
	}

	void OnNoButton()
	{
		CloseMenu();
	}

	void OnAddSourceButton()
	{
		Debug.LogError("这里还没写好");
	}
}
