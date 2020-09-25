using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WdwMenu_Save : MonoBehaviour
{
	public Button btnLastPage;
	public Button btnNextPage;
	public Button btnSave;
	public Button btnLoad;
	public Button btnDelete;
	public Button btnImport;
	public Button btnExport;
	public Button btnOK;
	public InputField iptName;
	public Canvas saveOrLoad;
	public Canvas ok;

	public GameObject btnFathers;
	private Button[] btnSavesAndPics;

	/// <summary>
	/// 当前页显示的文本
	/// </summary>
	private Text[] txtSaves;

	/// <summary>
	/// 当前页显示的图片
	/// </summary>
	private Image[] imgSaves;

	/// <summary>
	/// 100个saveInfos
	/// </summary>
	private List<SaveInfo> saveInfos = new List<SaveInfo>();

	/// <summary>
	/// 100个精灵
	/// </summary>
	private readonly List<Sprite> saveImgSprites = new List<Sprite>();

	private Canvas canvas;

	private int selectedID = 0;         // 当前选中的ID：0-8
	private int idNowPage = 0;          // 存档页数：0-无穷
	private int idInOnePage = 9;        // 每页存档数

	void Start()
	{
		// 自己的画布
		canvas = GetComponent<Canvas>();

		// 按钮
		btnLastPage.onClick.AddListener(OnButtonLastPage);
		btnNextPage.onClick.AddListener(OnButtonNextPage);
		btnSave.onClick.AddListener(OnButtonSave);
		btnLoad.onClick.AddListener(OnButtonLoad);
		btnDelete.onClick.AddListener(OnButtonDelete);
		btnExport.onClick.AddListener(OnButtonExport);
		btnImport.onClick.AddListener(OnButtonImport);
		btnOK.onClick.AddListener(OnButtonOK);

		// 存档们
		btnSavesAndPics = btnFathers.GetComponentsInChildren<Button>();

		idInOnePage = btnSavesAndPics.Length;
		txtSaves = new Text[btnSavesAndPics.Length];
		imgSaves = new Image[btnSavesAndPics.Length];


		for (int i = 0; i < btnSavesAndPics.Length; i++)
		{
			int id = i;
			btnSavesAndPics[i].onClick.AddListener(delegate ()
			{
				OnButtonSelect(id);
			});//添加带参数的按钮

			//处理按钮下面的文本
			txtSaves[i] = btnSavesAndPics[i].GetComponentInChildren<Text>();
			imgSaves[i] = btnSavesAndPics[i].GetComponentInChildren<Image>();
		}

		saveOrLoad.enabled = false;

		// 启动时读取所有存档
		LoadAll();
	}

	
	public void SetCanvas(bool value)
	{
		canvas.enabled = value;//打开/关闭画布
		saveOrLoad.enabled = false;//每次切换都会关闭这个弹窗
		ok.enabled = false;//每次切换都会关闭弹窗上的悬浮窗
		if (value)//打开画布时
		{
			RenewOnePage();//更新存档的显示
		}
	}

	void OnButtonNextPage()
	{
		idNowPage++;
		if (idNowPage > 9) idNowPage = 9;
		RenewOnePage();
	}

	void OnButtonLastPage()
	{
		idNowPage--;
		if (idNowPage < 0) idNowPage = 0;
		RenewOnePage();
	}

	void OnButtonSave()
	{
		// 关闭选择框
		ok.enabled = true;
	}

	void OnButtonOK()
	{
		// 默认存档名
		if (iptName.text == "")
		{
			iptName.text = "未命名存档";
			return;
		}

		// 关闭选择框
		ok.enabled = false;

		// 按照ID存档
		int saveID = selectedID + idInOnePage * idNowPage;
		SaveInfo saveinfo = new SaveInfo(true, iptName.text, GetSaveTime(), null);
		StartCoroutine(ScreenShotTex(saveID, saveinfo));

		// 关闭弹窗
		saveOrLoad.enabled = false;
	}

	void OnButtonLoad()
	{
		// 按照ID读档
		int saveID = selectedID + idInOnePage * idNowPage;
		if (saveInfos[saveID].isUsed)
		{
			SaveManager.Instance.MyLoad(saveID);
			saveOrLoad.enabled = false;         // 关闭弹窗
			Wdw_Menu.Instance.MyCloseMenu();    // 关闭菜单
		}
		else
		{
			saveOrLoad.enabled = false;         // 关闭弹窗
		}

	}

	void OnButtonDelete()
	{
		// 按照ID读档
		int saveID = selectedID + idInOnePage * idNowPage;
		SaveManager.Instance.MyClear(saveID);
		saveInfos[saveID] = new SaveInfo(false, null, null, null);
		saveImgSprites[saveID] = null;
		RenewOnePage();

		// 关闭弹窗
		saveOrLoad.enabled = false;         
	}

	void OnButtonImport()
	{
		// 按照ID读档
		int saveID = selectedID + idInOnePage * idNowPage;
		if (SaveManager.Instance.MyImport(saveID, out SaveManager.ExportData exportData))
		{
			// 刷新存档页面
			txtSaves[selectedID].text = GetDisplayText(saveID, exportData.saveName, exportData.saveTime);
			saveImgSprites[saveID] = GetSprite(exportData.saveData.Bytes);
			imgSaves[selectedID].sprite = saveImgSprites[saveID];
		}

		// 关闭弹窗
		saveOrLoad.enabled = false;         
	}

	void OnButtonExport()
	{
		// 按照ID读档
		int saveID = selectedID + idInOnePage * idNowPage;

		if (saveInfos[saveID].isUsed)
		{
			SaveManager.Instance.MyExport(saveID, saveInfos[saveID]);
		}
		saveOrLoad.enabled = false;         // 关闭弹窗
	}

	void OnButtonSelect(int id)
	{
		// 打开弹窗并清空内容
		selectedID = id;
		saveOrLoad.enabled = true;
	}

	/// <summary>
	/// 刷新一页的存档，包括名称时间和图片
	/// </summary>
	private void RenewOnePage()
	{
		for (int i = 0; i < idInOnePage; i++)
		{
			int nowID = idNowPage * idInOnePage + i;
			if (saveInfos[i].isUsed)
			{
				txtSaves[i].text = GetDisplayText(
					nowID, saveInfos[nowID].saveName, saveInfos[nowID].saveTime);
			}
			else
			{
				txtSaves[i].text = nowID.ToString("00") + "空存档";
			}

			imgSaves[i].sprite = saveImgSprites[nowID];
		}
	}

	/// <summary>
	/// 从文件中读取所有SaveInfo
	/// </summary>
	private void LoadAll()
	{
		saveInfos = SaveManager.Instance.MyLoadSaveInfo();

		for (int i = 0; i < saveInfos.Count; i++)
		{
			saveImgSprites.Add(GetSprite(saveInfos[i].bytes));
		}
	}

	private IEnumerator ScreenShotTex(int saveID, SaveInfo saveInfo)
	{
		// 关闭光标和菜单
		DisplayController.MyShowCross = false;
		Wdw_Menu.Instance.MyCloseMenu();

		// 等待帧结束截图
		yield return new WaitForEndOfFrame();
		byte[] image = ScreenCapture.CaptureScreenshotAsTexture().EncodeToPNG();
		saveInfo.bytes = image;

		// 开启光标和菜单
		DisplayController.MyShowCross = true;
		Wdw_Menu.Instance.MyOpenMenu();
		Wdw_Menu.Instance.ToSaveMode();

		// 写入List
		var saveImgSprite = GetSprite(saveInfo.bytes);
		saveInfos[saveID] = saveInfo;
		saveImgSprites[saveID] = saveImgSprite;

		// 刷新存档页面
		txtSaves[saveID % idInOnePage].text = 
			GetDisplayText(saveID, saveInfo.saveName, saveInfo.saveTime);
		imgSaves[saveID % idInOnePage].sprite = saveImgSprite;

		// 给出saveInfo后异步写入文件
		yield return null;
		SaveManager.Instance.MySave(saveID, saveInfo.saveName, saveInfo.saveTime, image);
	}

	/// <summary>
	/// 获取系统时间
	/// </summary>
	/// <returns>格式化的日期字符串</returns>
	private string GetSaveTime() =>
	string.Format("{0:D4}/{1:D2}/{2:D2}" + " " + "{3:D2}:{4:D2}",
		DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,
		DateTime.Now.Hour, DateTime.Now.Minute);

	/// <summary>
	/// 获取存档界面的显示文本
	/// </summary>
	/// <param name="saveID">存档ID</param>
	/// <param name="saveName">存档名称</param>
	/// <param name="SaveTime">存档时间</param>
	/// <returns></returns>
	private string GetDisplayText(int saveID, string saveName, string SaveTime) =>
		saveID.ToString("00") + "：" + saveName + "\n" + SaveTime;

	/// <summary>
	/// 通过字节数组获取精灵
	/// </summary>
	/// <param name="bytes">字节数组</param>
	/// <returns>精灵</returns>
	private Sprite GetSprite(byte[] bytes)
	{
		Texture2D tex = new Texture2D(0, 0);
		tex.LoadImage(bytes);
		return Sprite.Create(
			tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
	}
}