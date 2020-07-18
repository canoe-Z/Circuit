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
    public List<ILoad> DataList { get; set; } = new List<ILoad>();
}

/// <summary>
/// 存档管理器，包含存档读档等与文件的交互，和一些存档时需要用到的方法
/// </summary>
public class SaveManager : MonoBehaviour
{
    public void Save()
    {
        List<ISave> Savelist = FindAllTypes<ISave>();
        SaveData savedata = new SaveData();
        foreach(ISave toSave in Savelist)
		{
            savedata.DataList.Add(toSave.Save());
		}
        if (!Directory.Exists("Saves"))
            Directory.CreateDirectory("Saves");
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream saveFile = File.Create("Saves/save.binary");
        formatter.Serialize(saveFile, savedata);
        saveFile.Close();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            Save();
        }

        if (Input.GetKeyDown(KeyCode.F9))
        {
            List<ISave> Savelist = FindAllTypes<ISave>();
            foreach (ISave toSave in Savelist)
            {
                if (toSave is DigtalVoltmeter)
                {
                    (toSave as DigtalVoltmeter).DestroyEntity();
                }
            }

            var node = CircuitCalculator.Lines.First;
            while (node != null)
            {
                var next = node.Next;
                node.Value.DestroyRope();
                node = next;
            }

            BinaryFormatter formatter = new BinaryFormatter();
            FileStream saveFile = File.Open("Saves/save.binary", FileMode.Open);
            SaveData datafromfile = (SaveData)formatter.Deserialize(saveFile);
            saveFile.Close();
            foreach (ILoad data in datafromfile.DataList)
            {
                if (data is DigtalVoltmeterData)
                {
                    data.Load();
                }
            }

            foreach (ILoad data in datafromfile.DataList)
            {
                if (data is LineData)
                {
                    data.Load();
                }
            }

            CircuitCalculator.CalculateAll();
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

    public static Vector3 ToVector3(ThreeFloat pos)
	{
        return new Vector3(pos.x, pos.y, pos.z);
    }

    public static ThreeFloat ToThreeFloat(Vector3 pos)
    {
        return new ThreeFloat(pos.x, pos.y, pos.z);
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

public class ThreeFloat
{
    public float x, y, z;

	public ThreeFloat(float x, float y, float z)
	{
		this.x = x;
		this.y = y;
		this.z = z;
	}
}