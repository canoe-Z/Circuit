using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WdwMenu_Save : MonoBehaviour
{
	public Button btnSave;
	public Button btnLoad;
	public Text txtIndex;

	public Button[] btnSaves;
	Text[] txtSaves;

	bool isLoading = true;
	void Start()
	{
		btnSave.onClick.AddListener(ToSave);
		btnLoad.onClick.AddListener(ToLoad);

		txtSaves = new Text[btnSaves.Length];
		for (int i = 0; i < btnSaves.Length; i++)
		{
			btnSaves[i].onClick.AddListener(delegate () { OnButtonSave(i); });//添加带参数的按钮
			txtSaves[i] = btnSaves[i].GetComponentInChildren<Text>();
			txtSaves[i].text = "hello\nworld";
		}


		ToLoad();
	}

	void ToSave()
	{
		isLoading = false;
		txtIndex.text = "请选择保存的存档";
	}
	void ToLoad()
	{
		isLoading = true;
		txtIndex.text = "请选择加载的存档";
	}
	void OnButtonSave(int id)
	{
		if (isLoading)
		{
			SaveManager.Instance.Load(id);
		}
		else
		{
			SaveManager.Instance.Save(id, "nmsl");
		}
	}
}
