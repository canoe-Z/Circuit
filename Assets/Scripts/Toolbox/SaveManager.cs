﻿using System.Collections;
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
	public List<EntityData> EntityDataList { get; set; } = new List<EntityData>();
	public List<LineData> LineDataList { get; set; } = new List<LineData>();

	//TODO:保存摄像机位置，小窗状态
	//TODO:保存导线颜色配置
	//TODO:保存用户设定
}

/// <summary>
/// 存档管理器，包含存档读档等与文件的交互，和一些存档时需要用到的方法
/// </summary>
public class SaveManager : MonoBehaviour
{
	public delegate void CallBack();//利用委托回调可以先关闭UI，截取到没有UI的画面
	public void Save()
	{
		SaveData savedata = new SaveData();

		foreach (EntityBase entity in CircuitCalculator.Entities)
		{
			savedata.EntityDataList.Add(entity.Save());
		}

		foreach (CircuitLine line in CircuitCalculator.Lines)
		{
			savedata.LineDataList.Add(line.Save());
		}

		if (!Directory.Exists("Saves"))
			Directory.CreateDirectory("Saves");

		BinaryFormatter formatter = new BinaryFormatter();
		FileStream saveFile = File.Create("Saves/save.binary");
		formatter.Serialize(saveFile, savedata);
		saveFile.Close();

		// 调用UnityEngine自带截屏Api
		ScreenCapture.CaptureScreenshot("Saves/save.png");
		StartCoroutine(ScreenShotTex());
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.F5))
		{
			Save();
		}

		if (Input.GetKeyDown(KeyCode.F9))
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
			FileStream saveFile = File.Open("Saves/save.binary", FileMode.Open);
			SaveData datafromfile = (SaveData)formatter.Deserialize(saveFile);
			saveFile.Close();

			foreach (EntityData entitydata in datafromfile.EntityDataList)
			{
				entitydata.Load();
			}

			foreach (LineData linedata in datafromfile.LineDataList)
			{
				linedata.Load();
			}

			// 下一帧计算，直接调用计算会在Start()之前执行计算，丢失引用
			CircuitCalculator.NeedCalculate = true;
		}
	}

	/// <summary>
	/// UnityEngine自带截屏Api，只能截全屏
	/// </summary>
	/// <param name="fileName">文件名</param>
	/// <param name="callBack">截图完成回调</param>
	/// <returns>协程</returns>
	public IEnumerator ScreenShotTex(CallBack callBack = null)
	{
		yield return new WaitForEndOfFrame();//等到帧结束，不然会报错
		Texture2D tex = ScreenCapture.CaptureScreenshotAsTexture();//截图返回Texture2D对象
		byte[] bytes = tex.EncodeToPNG();//将纹理数据，转化成一个png图片
		File.WriteAllBytes("Saves/save2.png", bytes);//写入数据
		callBack?.Invoke();
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

/// <summary>
/// 实现该接口，则对象由唯一ID标识
/// </summary>
public interface IUniqueIdentity
{
	int ID { get; }
}

/// <summary>
/// 脚本内有需要保存的数据，则应实现ISave接口
/// </summary>
public interface ISave
{
	/// <summary>
	/// 保存
	/// </summary>
	/// <returns>实现ILoad接口的数据</returns>
	ILoad Save();
}

/// <summary>
/// 被保存的数据实现ILoad接口
/// </summary>
public interface ILoad
{
	/// <summary>
	/// 读档
	/// </summary>
	void Load();
}