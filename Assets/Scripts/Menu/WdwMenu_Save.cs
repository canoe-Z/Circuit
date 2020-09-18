using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WdwMenu_Save : MonoBehaviour
{
	public Button btnSave;
	public Button btnLoad;
	public InputField iptName;
	public Canvas saveOrLoad;

	public Button[] btnSavesAndPics;
	Text[] txtSaves;

	void Start()
	{
		//自己的画布
		canvas = GetComponent<Canvas>();

		btnSave.onClick.AddListener(Save);
		btnLoad.onClick.AddListener(Load);

		txtSaves = new Text[btnSavesAndPics.Length];
		for (int i = 0; i < btnSavesAndPics.Length; i++)
		{
			btnSavesAndPics[i].onClick.AddListener(delegate () { OnButtonSelect(i); });//添加带参数的按钮

			//处理按钮下面的文本
			txtSaves[i] = btnSavesAndPics[i].GetComponentInChildren<Text>();
			txtSaves[i].text = "hello\nworld";
		}

		saveOrLoad.enabled = false;
	}
	Canvas canvas;
	public void SetCanvas(bool value)
	{
		canvas.enabled = value;
		saveOrLoad.enabled = false;//每次切换都会关闭这个弹窗
	}

	int selectedID = 0;
	void Save()
	{
		if (iptName.text == "") iptName.text = "default";
		SaveManager.Instance.Save(selectedID, iptName.text);
		saveOrLoad.enabled = true;//关闭弹窗
	}
	void Load()
	{
		SaveManager.Instance.Load(selectedID);
		saveOrLoad.enabled = true;//关闭弹窗
	}
	void OnButtonSelect(int id)
	{
		Debug.Log(id);
		selectedID = id;
		saveOrLoad.enabled = true;//打开弹窗
		iptName.text = "";//清空弹窗的内容
	}
}
