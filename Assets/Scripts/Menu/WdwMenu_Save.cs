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
	public Button btnOK;
	public InputField iptName;
	public Canvas saveOrLoad;
	public Canvas ok;

	public GameObject btnFathers;
	Button[] btnSavesAndPics;
	Text[] txtSaves;
	Image[] imgSaves;

	void Start()
	{
		//自己的画布
		canvas = GetComponent<Canvas>();

		//按钮
		btnLastPage.onClick.AddListener(OnButtonLastPage);
		btnNextPage.onClick.AddListener(OnButtonNextPage);
		btnSave.onClick.AddListener(OnButtonSave);
		btnLoad.onClick.AddListener(OnButtonLoad);
		btnOK.onClick.AddListener(OnButtonOK);

		//存档们
		btnSavesAndPics = btnFathers.GetComponentsInChildren<Button>();

		idInOnePage = btnSavesAndPics.Length;
		txtSaves = new Text[btnSavesAndPics.Length];
		imgSaves = new Image[btnSavesAndPics.Length];


		for (int i = 0; i < btnSavesAndPics.Length; i++)
		{
			int id = i;
			btnSavesAndPics[i].onClick.AddListener(delegate () {
				OnButtonSelect(id); });//添加带参数的按钮

			//处理按钮下面的文本
			txtSaves[i] = btnSavesAndPics[i].GetComponentInChildren<Text>();
			imgSaves[i] = btnSavesAndPics[i].GetComponentInChildren<Image>();
		}

		saveOrLoad.enabled = false;
	}
	Canvas canvas;
	public void SetCanvas(bool value)
	{
		canvas.enabled = value;//打开/关闭画布
		saveOrLoad.enabled = false;//每次切换都会关闭这个弹窗
		ok.enabled = false;//每次切换都会关闭弹窗上的悬浮窗
		if (value)//打开画布时
		{
			MyRenewNameAndImages();//更新存档的显示
		}
	}

	int selectedID = 0;//0-8
	int idNowPage = 0;//0-无穷
	int idInOnePage = 9;//一页有多少个存档

	/// <summary>
	/// 刷新
	/// </summary>
	public void MyRenewNameAndImages()
	{
		int startId = idNowPage * idInOnePage;
		int endId = startId + idInOnePage;
		List<SaveInfo> saveInfos = SaveManager.Instance.MyLoadSaveInfo(startId, endId);
		if (saveInfos.Count != idInOnePage)
		{
			Debug.LogError("数量不匹配");
			Debug.Log(saveInfos.Count);
		}
		
		for(int i = 0; i < saveInfos.Count; i++)
		{
			int nowID = idNowPage * idInOnePage + i;
			if (saveInfos[i].isUsed)
				txtSaves[i].text = nowID.ToString("00") + "：" + saveInfos[i].saveName + "\n" + saveInfos[i].saveTime;
			else
				txtSaves[i].text = nowID.ToString("00") + "空存档";

			Texture2D tex = new Texture2D(0, 0);
			tex.LoadImage(saveInfos[i].bytes);
			imgSaves[i].sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
		}
	}

	void OnButtonNextPage()
	{
		idNowPage++;
		if (idNowPage > 9) idNowPage = 9;
		MyRenewNameAndImages();
	}
	void OnButtonLastPage()
	{
		idNowPage--;
		if (idNowPage < 0) idNowPage = 0;
		MyRenewNameAndImages();
	}
	void OnButtonSave()
	{
		ok.enabled = true;//关闭选择框
	}
	void OnButtonOK()
	{
		ok.enabled = false;//关闭选择框
		if (iptName.text == "") iptName.text = "未命名";

		//按照ID存档
		int saveID = selectedID + idInOnePage * idNowPage;
		SaveManager.Instance.MySave(saveID, iptName.text);

		saveOrLoad.enabled = false;//关闭弹窗
		//Wdw_Menu.Instance.MyCloseMenu();//关闭菜单
	}
	void OnButtonLoad()
	{
		//按照ID存档
		int saveID = selectedID + idInOnePage * idNowPage;
		SaveManager.Instance.MyLoad(saveID);

		saveOrLoad.enabled = false;//关闭弹窗
		Wdw_Menu.Instance.MyCloseMenu();//关闭菜单
	}
	void OnButtonSelect(int id)
	{
		selectedID = id;
		saveOrLoad.enabled = true;//打开弹窗
		iptName.text = "";//清空弹窗的内容
	}
}
