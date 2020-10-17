using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;

public static class MySettings
{
	public static bool openMyPinDamping = true;
	public static bool isEmission = true;
	public static float moveRatio = 1f;//移动速度
	public static float turnRatio = 1f;//转动速度

	//未保存程序
	public static float roomTemperature = 20;//室温
	public static float hotRInterval = 0.1f;//热敏电阻计算间隔
	public static float xLimitCursorRange = 100;//鼠标fixed的位置离屏幕距离
	public static float yLimitCursorRange = 100;

}
public class Wdw_Menu_Settings : MonoBehaviour
{
	public Slider sldMove;
	public Slider sldTurn;
	public Toggle tglLine;
	public Toggle tglMyPinDamping;
	void Awake()
	{
		LoadSettings();//加载存档

		sldMove.onValueChanged.AddListener(ChangeMoveRatio);
		sldTurn.onValueChanged.AddListener(ChangeTurnRatio);
		tglLine.onValueChanged.AddListener(ChangeLine);
		tglMyPinDamping.onValueChanged.AddListener(ChangeMyPin);
		ChangeMoveRatio(sldMove.value);
		ChangeTurnRatio(sldTurn.value);
		ChangeLine(tglLine.isOn);
		ChangeMyPin(tglMyPinDamping.isOn);
	}

	private void ChangeMoveRatio(float value)
	{
		MySettings.moveRatio = value;
	}

	private void ChangeTurnRatio(float value)
	{
		MySettings.turnRatio = value;
	}

	void ChangeLine(bool value)
	{
		MySettings.isEmission = tglLine.isOn;
	}
	void ChangeMyPin(bool value)
	{
		MySettings.openMyPinDamping = tglMyPinDamping.isOn;
	}

	void OnApplicationQuit()
	{
		SaveSettings();//保存存档
	}

	private void SaveSettings()
	{
		if (!Directory.Exists("Saves"))
			Directory.CreateDirectory("Saves");

		BinaryFormatter formatter = new BinaryFormatter();
		FileStream saveFile = File.Create("Saves/settings.binary");
		formatter.Serialize(saveFile, new SettingsData(this));
		saveFile.Close();
	}
	void LoadSettings()
	{
		string saveDirectory = "Saves/";
		string saveFile = "settings.binary";
		if (!Directory.Exists(saveDirectory))
			Directory.CreateDirectory(saveDirectory);
		if (File.Exists(saveDirectory + saveFile))
		{
			try//抓取可能出现的错误
			{
				FileStream fs = File.Open(saveDirectory + saveFile, FileMode.Open);
				BinaryFormatter formatter = new BinaryFormatter();
				try//尝试反序列化
				{
					SettingsData datafromfile = (SettingsData)formatter.Deserialize(fs);
					datafromfile.Load(this);
					fs.Close();
				}
				catch (System.Runtime.Serialization.SerializationException)//反序列化失败，处理掉这个文件
				{
					fs.Close();
					File.Delete(saveDirectory + saveFile);
				}
			}
			catch (Exception e)
			{
#if UNITY_EDITOR
				Debug.LogError(e);
#endif
			}
		}
	}


	[System.Serializable]
	private class SettingsData
	{
		private readonly float moveRatio;
		private readonly float turnRatio;
		private readonly bool lineguang = false;
		private readonly bool mypinDamping = true;


		public SettingsData(Wdw_Menu_Settings menu_Settings)
		{
			moveRatio = menu_Settings.sldMove.value;
			turnRatio = menu_Settings.sldTurn.value;
			lineguang = menu_Settings.tglLine.isOn;
			mypinDamping = menu_Settings.tglMyPinDamping.isOn;
		}

		public void Load(Wdw_Menu_Settings menu_Settings)
		{
			menu_Settings.sldMove.value = moveRatio;
			menu_Settings.sldTurn.value = turnRatio;
			menu_Settings.tglLine.isOn = lineguang;
			menu_Settings.tglMyPinDamping.isOn = mypinDamping;
		}
	}
}

