using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
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
		savedata.colorID = DisplayController.MyColorID;
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
		DisplayController.MyColorID = colorID;
		cameraData.Load();
	}
}

/// <summary>
/// 存档管理器，包含存档读档等与文件的交互，和一些存档时需要用到的方法
/// </summary>
public class SaveManager : Singleton<SaveManager>
{
	public delegate void CallBack();//利用委托回调可以先关闭UI，截取到没有UI的画面
	XmlDocument xml = new XmlDocument();

	void Start()
	{
		if (File.Exists("Saves/SaveInfo.xml"))
		{
			xml.Load("Saves/SaveInfo.xml");

		}
		else
		{
			xml = CreateSaveInfo();
			xml.Save("Saves/SaveInfo.xml");
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

		// DEBUG:强制生成新的XML
		if (Input.GetKeyDown(KeyCode.F10))
		{
			xml = CreateSaveInfo();
			xml.Save("Saves/SaveInfo.xml");
		}
	}

	public void Save(int saveID, string saveName)
	{
		SaveData savedata = SaveData.Create();

		if (!Directory.Exists("Saves")) Directory.CreateDirectory("Saves");

		BinaryFormatter formatter = new BinaryFormatter();
		FileStream saveFile = File.Create(
			string.Format("Saves/SaveData_" + "{0:D3}" + ".binary",saveID));
		formatter.Serialize(saveFile, savedata);
		saveFile.Close();

		StartCoroutine(ScreenShotTex(saveID));

		WriteSaveInfo(saveID, saveName);
		xml.Save("Saves/SaveInfo.xml");
	}

	public XmlDocument CreateSaveInfo()
	{
		XmlDocument xml = new XmlDocument();
		xml.AppendChild(xml.CreateXmlDeclaration("1.0", "UTF-8", null));
		XmlElement root = xml.CreateElement("Index");
		xml.AppendChild(root);
		for (var i = 0; i < 100; i++)
		{
			var node = root.AppendChild(xml.CreateElement("SavaData" + i.ToString()));
			node.AppendChild(xml.CreateElement("IsUsed")).InnerText = "";
			node.AppendChild(xml.CreateElement("SaveName")).InnerText = "";
			node.AppendChild(xml.CreateElement("SaveTime")).InnerText = "";
		}
		return xml;

	}

	private void WriteSaveInfo(int saveID, string saveName)
	{
		var node = xml.SelectSingleNode("Index").ChildNodes[saveID];
		node.ChildNodes[0].InnerText = "1";
		node.ChildNodes[1].InnerText = saveName;
		node.ChildNodes[2].InnerText = GetSaveTime();
	}

	/// <summary>
	/// 加载从start到end的存档信息，不包括end
	/// </summary>
	public List<SaveInfo> LoadSaveInfo(int? start=null,int? end=null)
	{
		var node = xml.SelectSingleNode("Index");
		List<SaveInfo> saveInfoList = new List<SaveInfo>();
		if (start == null) start = 0;
		if (end == null) end = 100;
		for (int i=start.Value; i < end.Value; i++)
		{
			saveInfoList.Add(new SaveInfo(
				node.ChildNodes[i].ChildNodes[0].InnerText,
				node.ChildNodes[i].ChildNodes[1].InnerText,
				node.ChildNodes[i].ChildNodes[2].InnerText,
				string.Format("Saves/SaveData_" + "{0:D3}" + ".binary", i)));
		}
		return saveInfoList;
	}

	private string GetSaveTime()
	{
		return string.Format("{0:D4}/{1:D2}/{2:D2}" + " " + "{3:D2}:{4:D2}",
			DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,
			DateTime.Now.Hour, DateTime.Now.Minute);
	}

	public void Load(int saveID)
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
		FileStream saveFile = File.Open(
			string.Format("Saves/SaveData_" + "{0:D3}" + ".binary", saveID), FileMode.Open);
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
	public IEnumerator ScreenShotTex(int saveID)
	{
		// 等待菜单关闭
		yield return null;
		// 等待帧结束，不然会报错
		yield return new WaitForEndOfFrame();
		Texture2D tex = ScreenCapture.CaptureScreenshotAsTexture();
		byte[] bytes = tex.EncodeToPNG();
		File.WriteAllBytes(string.Format("Saves/SaveImg_" + "{0:D3}" + ".png", saveID), bytes);
	}

	/// <summary>
	/// 读取文件为byte数组
	/// </summary>
	/// <param name="fileName">文件路径</param>
	/// <returns>byte数组</returns>
	public static byte[] FileContent(string fileName)
	{
		using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
		{
			try
			{
				byte[] buffur = new byte[fs.Length];
				fs.Read(buffur, 0, (int)fs.Length);
				return buffur;
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}
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

public struct SaveInfo
{
	public bool isUsed;
	public string saveName;
	public string saveTime;
	public byte[] bytes;

	public SaveInfo(string isUsed, string saveName, string saveTime,string imgPath)
	{
		if (isUsed == "1")
		{
			this.isUsed = true;
			this.saveName = saveName;
			this.saveTime = saveTime;
			bytes = SaveManager.FileContent(imgPath);
		}
		else
		{
			this.isUsed = false;
			this.saveName = "";
			this.saveTime = "";
			bytes = null;
		}
	}
}

/// <summary>
/// 实现该接口，则对象由唯一ID标识
/// </summary>
public interface IUniqueIdentity
{
	int ID { get; }
}