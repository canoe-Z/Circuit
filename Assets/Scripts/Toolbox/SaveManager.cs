using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

/// <summary>
/// 存档数据
/// </summary>
[System.Serializable]
public class SaveData
{
	// 所有数据存储在List中
	private readonly List<EntityData> entityDataList = new List<EntityData>();
	private readonly List<LineData> lineDataList = new List<LineData>();
	private int colorID;
	private CameraData cameraData;

	private SaveData() { }

	public static SaveData Create()
	{
		SaveData savedata = new SaveData();
		foreach (EntityBase entity in CircuitCalculator.Entities)
		{
			savedata.entityDataList.Add(entity.Save());
		}

		foreach (CircuitLine line in CircuitCalculator.Lines)
		{
			savedata.lineDataList.Add(line.Save());
		}
		savedata.colorID = DisplayController.ColorID;
		savedata.cameraData = SmallCamManager.Save();
		return savedata;
	}

	public void Load()
	{
		foreach (EntityData entitydata in entityDataList)
		{
			entitydata.Load();
		}

		foreach (LineData linedata in lineDataList)
		{
			linedata.Load();
		}
		DisplayController.ColorID = colorID;
		cameraData.Load();
	}
}

/// <summary>
/// 存档管理器，包含存档读档等与文件的交互，和一些存档时需要用到的方法
/// </summary>
public class SaveManager : Singleton<SaveManager>
{
	public delegate void CallBack(int saveID, string saveName, byte[] bytes);//利用委托回调可以先关闭UI，截取到没有UI的画面
																			 //XmlDocument xml = new XmlDocument();
	SaveInfo[] saveInfoList = new SaveInfo[100];

	void Start()
	{
		if (File.Exists("Saves/saveinfo.banary"))
		{
			BinaryFormatter formatter = new BinaryFormatter();
			FileStream saveFile = File.Open("Saves/saveinfo.banary", FileMode.Open);
			saveInfoList = (SaveInfo[])formatter.Deserialize(saveFile);
			saveFile.Close();
		}
		else
		{
			saveInfoList = CreateSaveInfo();
			BinaryFormatter formatter = new BinaryFormatter();
			FileStream saveFile = File.Create("Saves/saveinfo.banary");
			formatter.Serialize(saveFile, saveInfoList);
			saveFile.Close();
		}
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.F5))
		{
			Save(1, "test");
		}

		if (Input.GetKeyDown(KeyCode.F9))
		{
			Load(1);
		}
	}

	public void Save(int saveID, string saveName)
	{
		SaveData savedata = SaveData.Create();

		if (!Directory.Exists("Saves")) Directory.CreateDirectory("Saves");

		BinaryFormatter formatter = new BinaryFormatter();
		FileStream saveFile = File.Create("Saves/saveData" + saveID.ToString() + ".binary");
		formatter.Serialize(saveFile, savedata);
		saveFile.Close();

		StartCoroutine(ScreenShotTex(SaveCallBack, saveID, saveName));
	}

	private void SaveCallBack(int saveID, string saveName, byte[] bytes)
	{
		saveInfoList[saveID] = new SaveInfo(true, saveName, GetSaveTime(), bytes);
		BinaryFormatter formatter = new BinaryFormatter();
		FileStream saveFile = File.Create("Saves/saveinfo.banary");
		formatter.Serialize(saveFile, saveInfoList);
		saveFile.Close();
	}

	public SaveInfo[] CreateSaveInfo()
	{
		SaveInfo[] datainfos = new SaveInfo[100];
		for (var i = 0; i < 100; i++)
		{
			datainfos[i] = new SaveInfo(false);
		}
		return datainfos;
	}

	public SaveInfo[] LoadSaveInfo()
	{
		BinaryFormatter formatter = new BinaryFormatter();
		FileStream saveFile = File.Open("Saves/saveinfo.banary", FileMode.Open);
		SaveInfo[] saveInfoList = (SaveInfo[])formatter.Deserialize(saveFile);
		saveFile.Close();
		return saveInfoList;
	}

	private string GetSaveTime()
	{
		return string.Format("{0:D4}/{1:D2}/{2:D2}" + " " + "{3:D2}:{4:D2}",
			DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,
			DateTime.Now.Hour, DateTime.Now.Minute);
	}

	private void Load(int saveID)
	{
		// 删除场景内所有元件，通过委托调用也将删除所有端口和导线
		var node = CircuitCalculator.Entities.First;
		while (node != null)
		{
			var next = node.Next;
			node.Value.DestroyEntity();
			node = next;
		}

		if (CircuitCalculator.Lines.Count != 0) Debug.LogError(CircuitCalculator.Lines.Count);


		BinaryFormatter formatter = new BinaryFormatter();
		FileStream saveFile = File.Open("Saves/saveData" + saveID.ToString() + ".binary", FileMode.Open);
		SaveData datafromfile = (SaveData)formatter.Deserialize(saveFile);
		saveFile.Close();

		datafromfile.Load();

		// 下一帧计算，直接调用计算会在Start()之前执行计算，丢失引用
		CircuitCalculator.NeedCalculate = true;
	}


	/// <summary>
	/// UnityEngine自带截屏Api，只能截全屏
	/// </summary>
	/// <param name="fileName">文件名</param>
	/// <param name="callBack">截图完成回调</param>
	/// <returns>协程</returns>
	public IEnumerator ScreenShotTex(CallBack callBack, int saveID, string saveName)
	{
		// 等待菜单关闭
		yield return null;
		yield return new WaitForEndOfFrame();//等到帧结束，不然会报错
		Texture2D tex = ScreenCapture.CaptureScreenshotAsTexture();//截图返回Texture2D对象
		byte[] bytes = tex.EncodeToPNG();//将纹理数据，转化成一个png图片
		File.WriteAllBytes("Saves/save2.png", bytes);//写入数据
		callBack?.Invoke(saveID, saveName, bytes);
	}

	/// <summary>
	/// 通过ID寻找脚本,需要实现IUniqueIdentity接口
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="ID"></param>
	/// <param name="list"></param>
	/// <returns></returns>
	public static T GetItemById<T>(int ID, IEnumerable<T> list) where T : IUniqueIdentity
	{
		IEnumerable<T> mainList = list;
		// SingleOrDefault:返回序列中的唯一记录；如果该序列为空，则返回默认值；如果该序列包含多个元素，则引发异常
		return mainList.SingleOrDefault(item => item.ID == ID);

	}

	/// <summary>
	/// 寻找实现接口T的所有脚本
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <returns></returns>
	public static List<T> FindAllTypes<T>()
	{
		List<T> interfaces = new List<T>();
		var types = FindObjectsOfType<MonoBehaviour>().OfType<T>();
		foreach (T t in types)
		{
			interfaces.Add(t);
		}
		return interfaces;
	}
}

public struct XmlInfo
{
	public bool isUsed;
	public string saveName;
	public string saveDate;

	public XmlInfo(string isUsed, string saveName, string saveDate)
	{
		if (isUsed == "1")
		{
			this.isUsed = true;
			this.saveName = saveName;
			this.saveDate = saveDate;

		}
		else
		{
			this.isUsed = false;
			this.saveName = "";
			this.saveDate = "";
		}
	}
}

[System.Serializable]
public struct SaveInfo
{
	public bool isUsed;
	public byte[] bytes;
	public string saveName;
	public string saveDate;

	public SaveInfo(bool isUsed, string saveName, string saveDate, byte[] bytes)
	{
		this.isUsed = isUsed;
		this.bytes = bytes;
		this.saveName = saveName;
		this.saveDate = saveDate;
	}

	public SaveInfo(bool isUsed)
	{
		this.isUsed = isUsed;
		bytes = null;
		saveName = "";
		saveDate = "";
	}
}

/// <summary>
/// 实现该接口，则对象由唯一ID标识
/// </summary>
public interface IUniqueIdentity
{
	int ID { get; }
}