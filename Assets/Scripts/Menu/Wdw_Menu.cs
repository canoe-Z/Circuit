using JetBrains.Annotations;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Wdw_Menu : Singleton<Wdw_Menu>
{
	public Canvas mainThings;
	public Canvas createThings;
	public WdwMenu_Save saveThings;
	public Canvas settingsThings;
	public Button exitGame;
	public Button continueGame;
	public Button btnToCreate;
	public Button btnToSave;
	public Button btnToSettings;

	void Awake()
	{
	}

	EventSystem es;
	void Start()
	{
		es = EventSystem.current;
		MyCloseMenu();
		exitGame.onClick.AddListener(OnQuitButton);
		continueGame.onClick.AddListener(OnContinueButton);
		btnToCreate.onClick.AddListener(ToCreateMode);
		btnToSave.onClick.AddListener(ToSaveMode);
		btnToSettings.onClick.AddListener(ToSettingsMode);
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			// 能操作场景对象，能操作角色，意味着菜单未开启
			if (MoveController.CanOperate && MoveController.CanControll)
			{
				MyOpenMenu();
				return;
			}

			// 不能操作场景对象，不能操作角色，意味着菜单已经开启
			if (!MoveController.CanOperate && !MoveController.CanControll)
			{
				MyCloseMenu();
				return;
			}

			// 不能操作场景对象，但是能操作角色，意味着正在添加元件，放下物体
			if (!MoveController.CanOperate && MoveController.CanControll)
			{
				MyOpenMenu();
				return;
			}

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
		MyCloseMenu();
	}

	//变为创建元件的界面
	void ToCreateMode()
	{
		createThings.enabled = true;
		saveThings.SetCanvas(false);
		settingsThings.enabled = false;
	}
	//变为存档的界面
	public void ToSaveMode()
	{
		createThings.enabled = false;
		saveThings.SetCanvas(true);
		settingsThings.enabled = false;
	}
	void ToSettingsMode()
	{
		createThings.enabled = false;
		saveThings.SetCanvas(false);
		settingsThings.enabled = true;
	}


	//奇怪的函数

	//打开菜单
	public void MyOpenMenu()
	{
		//默认界面是创建元件的界面
		ToCreateMode();
		Cursor.visible = true;
		MoveController.CanOperate = false;
		MoveController.CanControll = false;
		mainThings.enabled = true;
		es.enabled = true;

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
	public void MyCloseMenu()
	{
		Cursor.visible = false;
		MoveController.CanOperate = true;
		MoveController.CanControll = true;
		mainThings.enabled = false;
		es.enabled = false;
	}
}
