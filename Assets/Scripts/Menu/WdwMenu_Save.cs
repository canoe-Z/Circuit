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

	public Button[] btnSaves;
	Text[] txtSaves;

	bool isLoading = true;
	void Start()
	{
		btnSave.onClick.AddListener(Save);
		btnLoad.onClick.AddListener(Load);

		txtSaves = new Text[btnSaves.Length];
		for (int i = 0; i < btnSaves.Length; i++)
		{
			btnSaves[i].onClick.AddListener(delegate () { OnButtonSelect(i); });//添加带参数的按钮
			txtSaves[i] = btnSaves[i].GetComponentInChildren<Text>();
			txtSaves[i].text = "hello\nworld";
		}

		saveOrLoad.enabled = false;
	}

	void Save()
	{
		isLoading = false;
	}
	void Load()
	{
		isLoading = true;
	}
	void OnButtonSelect(int id)
	{
		saveOrLoad.enabled = true;
		if (isLoading)
		{

		}
		else
		{

		}
	}
}
