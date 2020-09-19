using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
/// <summary>
/// 编译前的操作命令，用于批量处理奇怪的东西
/// </summary>
[ExecuteInEditMode]
public class QuickOperate : MonoBehaviour
{
	//public string path;
	public UnityEngine.Object o1;
	void Work()
	{
		//path = Application.dataPath + "/Resources";
		//renderer.material.SetFloat("Vector1_5B99BAC5", 0.5f);
	}

	void LoopWork()
	{
		if (o1)
		{
			Texture2D t1 = AssetPreview.GetAssetPreview(o1);
			MyFunction.MyWriteFile("C:/Users/vc/Desktop/PNGS/" + o1.name + ".png", t1.EncodeToPNG());
			Debug.Log(o1.name);
			o1 = null;
		}
	}


	public bool boolStartWork = false;
	void Update()
	{
		LoopWork();
		if (boolStartWork)
		{
			boolStartWork = false;
			Debug.Log("开始");
			Work();
			Debug.Log("结束");
		}
	}
}

public static class MyFunction
{

	/// <summary>
	/// 得到某路径下所有文件/文件夹，由参数控制
	/// </summary>
	public static bool MyGetFileIndex(string fullPath, out List<string> strList, bool returnFullPath = false, bool isFile = true)
	{
		try
		{
			if (Directory.Exists(fullPath))//是否存在路径
			{
				DirectoryInfo direction = new DirectoryInfo(fullPath);
				strList = new List<string>();

				if (isFile)
				{
					//文件
					FileInfo[] fileInfos = direction.GetFiles("*");
					foreach (var fi in fileInfos)
					{
						if (returnFullPath)
							strList.Add(fullPath + "/" + fi.Name);
						else
							strList.Add(fi.Name);
					}
				}
				else
				{
					//文件夹
					DirectoryInfo[] directoryInfos = direction.GetDirectories("*");
					foreach (var fi in directoryInfos)
					{
						if (returnFullPath)
							strList.Add(fullPath + "/" + fi.Name);
						else
							strList.Add(fi.Name);
					}
				}
				return true;
			}
			else
			{
				strList = null;
				return false;
			}
		}
		catch (Exception e)
		{
			strList = null;
			return false;
		}
	}


	public static void MyWriteFile(string path, byte[] data)
	{
		FileStream fs = new FileStream(path, FileMode.Create);
		fs.Write(data, 0, data.Length);
		fs.Close();
	}
}