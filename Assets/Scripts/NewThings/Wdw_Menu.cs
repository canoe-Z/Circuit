using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Wdw_Menu : MonoBehaviour
{
	public GameObject mainThings;
	public Button exitGame;
	public Button continueGame;
	public Button copy_0;
	public GameObject beCopied_0;
	public Button copy_1;
	public GameObject beCopied_1;
	public Button copy_2;
	public GameObject beCopied_2;
	void Start()
	{
		if (mainThings == null) Debug.LogError("这里没挂");
		if (exitGame == null) Debug.LogError("这里没挂");
		if (continueGame == null) Debug.LogError("这里没挂");
		if (copy_0 == null) Debug.LogError("这里没挂");
		if (copy_1 == null) Debug.LogError("这里没挂");
		if (copy_2 == null) Debug.LogError("这里没挂");
		CloseMenu();
		exitGame.onClick.AddListener(OnYesButton);
		continueGame.onClick.AddListener(OnNoButton);
		copy_0.onClick.AddListener(OnButton_0);
		copy_1.onClick.AddListener(OnButton_1);
		copy_2.onClick.AddListener(OnButton_2);
	}

	// Update is called once per frame
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.P))
		{
			MoveController.CanMove = false;
		}
		if (Input.GetKeyDown(KeyCode.Escape))//开启或者关闭菜单，取决于是否进行过无法移动锁定
		{
			if (MoveController.CanMove) OpenMenu();
			else CloseMenu();
		}
	}

	//打开菜单
	void OpenMenu()
	{
		Cursor.lockState = CursorLockMode.None;//解除鼠标锁定
		Cursor.visible = true;
		MoveController.CanMove = false;//不允许移动视角
		mainThings.SetActive(true);
	}
	//关闭菜单
	void CloseMenu()
	{
		Cursor.lockState = CursorLockMode.Locked;//锁定鼠标于中央
		Cursor.visible = false;
		MoveController.CanMove = true;//允许移动视角
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

	void OnButton_0()
	{
		if (beCopied_0 == null) Debug.LogError("这里没挂");
	}
	void OnButton_1()
	{
		if (beCopied_1 == null) Debug.LogError("这里没挂");
	}
	void OnButton_2()
	{
		if (beCopied_2 == null) Debug.LogError("这里没挂");
	}
}
