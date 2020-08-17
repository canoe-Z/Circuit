using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;

public class Wdw_Menu_Settings : MonoBehaviour
{
	public Slider sldMove;
	public Slider sldTurn;

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
			}
		}
		catch(Exception e)
		{
#if UNITY_EDITOR
			Debug.LogError(e);
#endif
		}

		MoveController.MoveRatio = sldMove.value;
		MoveController.TurnRatio = sldTurn.value;

		sldMove.onValueChanged.AddListener(ChangeMoveRatio);
		sldTurn.onValueChanged.AddListener(ChangeTurnRatio);
	}

	private void ChangeMoveRatio(float value)=> MoveController.MoveRatio = value;

	private void ChangeTurnRatio(float value) => MoveController.TurnRatio = value;

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

		public SettingsData(Wdw_Menu_Settings menu_Settings)
		{
			moveRatio = menu_Settings.sldMove.value;
			turnRatio = menu_Settings.sldTurn.value;
		}

		public void Load(Wdw_Menu_Settings menu_Settings)
		{
			menu_Settings.sldMove.value = moveRatio;
			menu_Settings.sldTurn.value = turnRatio;
		}
	}
}

