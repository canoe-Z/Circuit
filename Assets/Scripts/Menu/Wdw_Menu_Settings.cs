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
		if (File.Exists("Saves/settings.binary"))
		{
			BinaryFormatter formatter = new BinaryFormatter();
			FileStream saveFile = File.Open("Saves/settings.binary", FileMode.Open);
			SettingsData datafromfile = (SettingsData)formatter.Deserialize(saveFile);
			saveFile.Close();

			sldMove.value = datafromfile.MyMoveSpeed;
			sldTurn.value = datafromfile.MyTurnSpeed;
		}
	}

	void Start()
	{
		MoveController.myMoveSpeed = sldMove.value;
		MoveController.myTurnSpeed = sldTurn.value;
		sldMove.onValueChanged.AddListener(ChangeMove);
		sldTurn.onValueChanged.AddListener(ChangeTurn);
	}

	void OnApplicationQuit()
	{
		Save(new SettingsData(this));
	}

	void ChangeMove(float value)
	{
		MoveController.myMoveSpeed = value;
	}

	void ChangeTurn(float value)
	{
		MoveController.myTurnSpeed = value;
	}

	private void Save(SettingsData settingsData)
	{
		if (!Directory.Exists("Saves"))
			Directory.CreateDirectory("Saves");

		BinaryFormatter formatter = new BinaryFormatter();
		FileStream saveFile = File.Create("Saves/settings.binary");
		formatter.Serialize(saveFile, settingsData);
		saveFile.Close();
	}
}

[System.Serializable]
public class SettingsData
{
	public float MyMoveSpeed, MyTurnSpeed;

	public SettingsData(Wdw_Menu_Settings menu_Settings)
	{
		MyMoveSpeed = menu_Settings.sldMove.value;
		MyTurnSpeed = menu_Settings.sldTurn.value;
	}
}
