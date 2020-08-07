﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Wdw_Menu : MonoBehaviour
{
	public Canvas mainThings;
	public Canvas createThings;
	public Canvas saveThings;
	public Canvas settingsThings;
	public Button exitGame;
	public Button continueGame;
	public Button btnToCreate;
	public Button btnToSave;
	public Button btnToSettings;
	/// <summary>
	/// 置为1时，强制关闭菜单
	/// </summary>
	[HideInInspector]
	public static bool shouldCloseMenu = false;
	void Start()
	{
		if (mainThings == null) Debug.LogError("这里没挂");
		if (exitGame == null) Debug.LogError("这里没挂");
		if (continueGame == null) Debug.LogError("这里没挂");
		CloseMenu();
		exitGame.onClick.AddListener(OnQuitButton);
		continueGame.onClick.AddListener(OnContinueButton);
		btnToCreate.onClick.AddListener(ToCreateMode);
		btnToSave.onClick.AddListener(ToSaveMode);
		btnToSettings.onClick.AddListener(ToSettingsMode);
	}

	void Update()
	{
		if (shouldCloseMenu)//由其它类强制关闭
		{
			shouldCloseMenu = false;
			CloseMenu();
		}
		if (Input.GetKeyDown(KeyCode.Escape))//开启或者关闭菜单，取决于是否进行过无法移动锁定
		{
			if (MoveController.CanOperate) OpenMenu();
			else CloseMenu();
		}
	}


	//下面是按钮

	//退出
	void OnQuitButton()
	{
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
	}
	//继续
	void OnContinueButton()
	{
		CloseMenu();
	}

	//变为创建元件的界面
	void ToCreateMode()
	{
		createThings.enabled = true;
		saveThings.enabled = false;
		settingsThings.enabled = false;
	}
	//变为存档的界面
	void ToSaveMode()
	{
		createThings.enabled = false;
		saveThings.enabled = true;
		settingsThings.enabled = false;
	}
	void ToSettingsMode()
	{
		createThings.enabled = false;
		saveThings.enabled = false;
		settingsThings.enabled = true;
	}


	//奇怪的函数

	//打开菜单
	void OpenMenu()
	{
		//默认界面是创建元件的界面
		ToCreateMode();

		Cursor.lockState = CursorLockMode.None;//解除鼠标锁定
		Cursor.visible = true;
		MoveController.CanOperate = false;//不允许移动视角
		MoveController.CanTurn = false;
		mainThings.enabled = true;

		//清空输入框和下拉菜单
		InputField[] inputFields = createThings.gameObject.GetComponentsInChildren<InputField>();
		foreach (var ifds in inputFields)
		{
			ifds.text = "";
		}
		Dropdown[] dropdowns = createThings.gameObject.GetComponentsInChildren<Dropdown>();
		foreach (var dds in dropdowns)
		{
			dds.value = 0;
		}

	}
	//关闭菜单
	void CloseMenu()
	{
		Cursor.lockState = CursorLockMode.Locked;//锁定鼠标于中央
		Cursor.visible = false;
		MoveController.CanOperate = true;//允许移动视角
		MoveController.CanTurn = true;
		mainThings.enabled = false;
	}
}