using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WdwMenu_Save : MonoBehaviour
{
	public Button btnLast;
	public Button btnNext;
	public Button btnSave;
	public Button btnLoad;
	public Image imgPic;
	public Text txtIndex;

	int nowIndex = 0;
	int totalNum = 10;//总的存档数量
    void Start()
    {
		btnLast.onClick.AddListener(ToLast);
		btnNext.onClick.AddListener(ToNext);
		btnSave.onClick.AddListener(Save);
		btnLoad.onClick.AddListener(Load);
		ChangeText();
	}
	//更新当前文本
	void ChangeText()
	{
		txtIndex.text = "第" + (nowIndex + 1).ToString() + "/" + totalNum.ToString() + "档";
	}

	//下面是按钮
	void ToLast()
	{
		nowIndex--;
		if (nowIndex < 0)
		{
			nowIndex = 0;
		}
		ChangeText();
	}
	void ToNext()
	{
		nowIndex++;
		if (nowIndex >= totalNum)
		{
			nowIndex = totalNum - 1;
		}
		ChangeText();
	}
	void Save()
	{

	}
	void Load()
	{

	}
}
