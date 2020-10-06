using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;

public static class MySettings
{
	public static bool openMyPinDamping = true;
}
public class Wdw_Menu_Settings : MonoBehaviour
{
	public Slider sldMove;
	public Slider sldTurn;
	public Toggle tglLine;
	public Toggle tglMyPinDamping;
	void Awake()
	{
		try//抓取可能出现的错误
		{
			// 启动时自动读取上一次的设置
			if (File.Exists("Saves/settings.binary"))
			{
				BinaryFormatter formatter = new BinaryFormatter();
				FileStream saveFile = File.Open("Saves/settings.binary", FileMode.Open);
				SettingsData datafromfile = (SettingsData)formatter.Deserialize(saveFile);
				saveFile.Close();

				datafromfile.Load(this);
			}
			else
			{
				sldMove.value = 1f;
				sldTurn.value = 1f;
				tglLine.isOn = true;
				tglMyPinDamping.isOn = true;
			}
		}
		catch(Exception e)
		{
#if UNITY_EDITOR
			Debug.LogError(e);
#endif
		}

		sldMove.onValueChanged.AddListener(ChangeMoveRatio);
		sldTurn.onValueChanged.AddListener(ChangeTurnRatio);
		tglLine.onValueChanged.AddListener(ChangeLine);
		tglMyPinDamping.onValueChanged.AddListener(ChangeMyPin);
		ChangeMoveRatio(sldMove.value);
		ChangeTurnRatio(sldTurn.value);
		ChangeLine(tglLine.isOn);
		ChangeMyPin(tglMyPinDamping.isOn);
	}

	private void ChangeMoveRatio(float value)=> MoveController.MoveRatio = value;

	private void ChangeTurnRatio(float value) => MoveController.TurnRatio = value;

	void ChangeLine(bool value)
    {
		CircuitLine.IsEmission = tglLine.isOn;
	}
	void ChangeMyPin(bool value)
	{
		MySettings.openMyPinDamping = tglMyPinDamping.isOn;
	}

	void OnApplicationQuit() => Save(new SettingsData(this));

	private void Save(SettingsData settingsData)
	{
		if (!Directory.Exists("Saves"))
			Directory.CreateDirectory("Saves");

		BinaryFormatter formatter = new BinaryFormatter();
		FileStream saveFile = File.Create("Saves/settings.binary");
		formatter.Serialize(saveFile, settingsData);
		saveFile.Close();
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

