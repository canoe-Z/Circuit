using System.Collections;
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

	public static int MenuState = -1;

	void Awake()
	{
		StartCoroutine(MenuUpdate());
	}

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
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			// 能操作场景对象，能操作角色，意味着菜单未开启
			if (MoveController.CanOperate && MoveController.CanControll)
			{
				MenuState = 1;
				return;
			}

			// 不能操作场景对象，不能操作角色，意味着菜单已经开启
			if (!MoveController.CanOperate && !MoveController.CanControll)
			{
				MenuState = 0;
				return;
			}

			// 不能操作场景对象，但是能操作角色，意味着正在添加元件，放下物体
			if (!MoveController.CanOperate && MoveController.CanControll)
			{
				MenuState = 1;
				return;
			}

			// 能操作场景对象，但是不能操作角色，不存在这种情况，报错
			if (MoveController.CanOperate && !MoveController.CanControll)
			{
				return;
			}
		}
	}

	private IEnumerator MenuUpdate()
	{
		while (true)
		{
			if (MenuState == 1)
			{
				OpenMenu();
				MenuState = -1;
			}
			else if (MenuState == 0)
			{
				CloseMenu();
				MenuState = -1;
			}
			yield return null; //下一帧再次调用，yield return null的执行时机在Update()之后
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
	private void OpenMenu()
	{
		//默认界面是创建元件的界面
		ToCreateMode();

		Cursor.lockState = CursorLockMode.None;//解除鼠标锁定
		Cursor.visible = true;
		MoveController.CanOperate = false;
		MoveController.CanControll = false;
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
	private void CloseMenu()
	{
		Cursor.lockState = CursorLockMode.Locked;//锁定鼠标于中央
		Cursor.visible = false;
		MoveController.CanOperate = true;
		MoveController.CanControll = true;
		mainThings.enabled = false;
	}
}
