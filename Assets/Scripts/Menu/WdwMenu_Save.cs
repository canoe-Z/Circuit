using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WdwMenu_Save : MonoBehaviour
{
	public Button btnSave;
	public Button btnLoad;

	public Button save_0;
	public Button save_1;
	public Button save_2;
	public Text txtIndex;

	bool isLoading = true;
	void Start()
	{
		btnSave.onClick.AddListener(Save);
		btnLoad.onClick.AddListener(Load);

		Load();
	}
	//更新当前文本
	void ChangeText()
	{
	}

	void Save()
	{
		isLoading = false;
		txtIndex.text = "请选择保存的存档";
	}
	void Load()
	{
		isLoading = true;
		txtIndex.text = "请选择加载的存档";
	}
}
