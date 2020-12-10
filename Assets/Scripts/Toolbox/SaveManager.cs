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
    private readonly List<EntityData> entityDataList = new List<EntityData>();  // 元件信息
    private readonly List<LineData> lineDataList = new List<LineData>();        // 导线信息
    private int colorID;            // 颜色配置
    private CameraData cameraData;  // 窗口状态
    public byte[] Bytes;            // 截图字节数组
    public int PortNum;

    // 禁止使用构造函数创建
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
        savedata.PortNum = CircuitCalculator.PortNum;
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
        CircuitCalculator.PortNum = PortNum;
        DisplayController.MyColorID = colorID;
        cameraData.Load();
    }
}

/// <summary>
/// 存档信息
/// </summary>
public struct SaveInfo
{
    public bool isUsed;
    public string saveName;
    public string saveTime;
    public byte[] bytes;

    public SaveInfo(bool isUsed, string saveName, string saveTime, byte[] bytes)
    {
        this.isUsed = isUsed;
        this.saveName = saveName;
        this.saveTime = saveTime;
        this.bytes = bytes;
    }
}

/// <summary>
/// 存档管理器，包含存档读档等与文件的交互，和一些存档时需要用到的方法
/// </summary>
public class SaveManager : Singleton<SaveManager>
{
    XmlDocument xml = new XmlDocument();

    void Awake()
    {
        // 创建相关文件夹
        if (!Directory.Exists("Saves")) Directory.CreateDirectory("Saves");
        if (!Directory.Exists("Export")) Directory.CreateDirectory("Export");
        if (!Directory.Exists("Import")) Directory.CreateDirectory("Import");

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

    private SaveData LoadDataFromFile(int saveID)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream saveFile = File.Open(
            string.Format("Saves/SaveData_" + "{0:D3}" + ".binary", saveID), FileMode.Open);
        SaveData saveData = (SaveData)formatter.Deserialize(saveFile);
        saveFile.Close();
        return saveData;
    }

    /// <summary>
    /// 写入存档
    /// </summary>
    /// <param name="saveID">存档ID</param>
    /// <param name="saveName">存档名称</param>
    /// <param name="saveTime">存档时间</param>
    /// <param name="bytes">存档截图</param>
    /// <param name="saveData">可选已有的SaveData</param>
    /// <returns></returns>
    public bool MySave(int saveID, string saveName, string saveTime, byte[] bytes, SaveData saveData = null)
    {
        WriteSaveInfo(saveID, saveName, saveTime);

        if (saveData == null)
        {
            saveData = SaveData.Create();
            saveData.Bytes = bytes;
        }

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream saveFile = File.Create(
            string.Format("Saves/SaveData_" + "{0:D3}" + ".binary", saveID));
        formatter.Serialize(saveFile, saveData);
        saveFile.Close();
        return true;
    }

    /// <summary>
    /// 清空场景，导入指定存档
    /// </summary>
    /// <param name="saveID">存档位置</param>
    public void MyLoad(int saveID)
    {
        // 删除场景内所有元件，通过委托调用也将删除所有端口和导线
        var node = CircuitCalculator.Entities.First;
        while (node != null)
        {
            var next = node.Next;
            node.Value.DestroyEntity();
            node = next;
        }
        if (CircuitCalculator.Lines.Count != 0)
        {
            Debug.LogError(CircuitCalculator.Lines.Count);
        }

        LoadDataFromFile(saveID).Load();

        // 下一帧计算，直接调用计算会在Start()之前执行计算，丢失引用
        CircuitCalculator.NeedCalculate = true;
    }

    /// <summary>
    /// 创建初始XML
    /// </summary>
    /// <returns>初始XML</returns>
    private XmlDocument CreateSaveInfo()
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

    /// <summary>
    /// 向XML写入存档信息
    /// </summary>
    /// <param name="saveID">存档位置</param>
    /// <param name="saveName">存档名称</param>
    private void WriteSaveInfo(int saveID, string saveName, string saveTime)
    {
        var node = xml.SelectSingleNode("Index").ChildNodes[saveID];
        node.ChildNodes[0].InnerText = "1";
        node.ChildNodes[1].InnerText = saveName;
        node.ChildNodes[2].InnerText = saveTime;
        xml.Save("Saves/SaveInfo.xml");
    }

    /// <summary>
    /// 从文件加载从start到end的存档信息
    /// for (int i = start; i < end; i++)
    /// </summary>
    public List<SaveInfo> MyLoadSaveInfo()
    {
        var node = xml.SelectSingleNode("Index");
        List<SaveInfo> saveInfoList = new List<SaveInfo>();

        for (int i = 0; i < 100; i++)
        {
            if (node.ChildNodes[i].ChildNodes[0].InnerText == "1")
            {
                saveInfoList.Add(new SaveInfo(
                    true,
                    node.ChildNodes[i].ChildNodes[1].InnerText,
                    node.ChildNodes[i].ChildNodes[2].InnerText,
                    LoadDataFromFile(i).Bytes));
            }
            else
            {
                saveInfoList.Add(new SaveInfo(false, null, null, null));
            }
        }
        return saveInfoList;
    }

    /// <summary>
    /// 删除指定存档
    /// </summary>
    /// <param name="saveID">存档位置</param>
    /// <param name="isDataCleared">二进制数据是否被正常删除</param>
    /// <returns></returns>
    public void MyClear(int saveID)
    {
        string saveDataPath = string.Format(
            "Saves/SaveData_" + "{0:D3}" + ".binary", saveID);

        if (File.Exists(saveDataPath))
        {
            File.Delete(saveDataPath);
        }

        ClearXml(saveID);
    }

    private void ClearXml(int saveID)
    {
        var node = xml.SelectSingleNode("Index").ChildNodes[saveID];
        node.ChildNodes[0].InnerText = "";
        node.ChildNodes[1].InnerText = "";
        node.ChildNodes[2].InnerText = "";
        xml.Save("Saves/SaveInfo.xml");
    }

    [System.Serializable]
    public class ExportData
    {
        public SaveData saveData;
        public string saveName;
        public string saveTime;

        public ExportData(int saveID, SaveInfo saveInfo)
        {
            saveData = Instance.LoadDataFromFile(saveID);
            saveName = saveInfo.saveName;
            saveTime = saveInfo.saveTime;
        }


        public void Import(int saveID) =>
            Instance.MySave(saveID, saveName, saveTime, saveData.Bytes, saveData);
    }

    /// <summary>
    /// 导出读档
    /// </summary>
    /// <param name="saveID">需要导出的存档位置</param>
    /// <returns>导出成功</returns>
    public void MyExport(int saveID, SaveInfo saveInfo)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream exportFile = File.Create("Export/Data.binary");
        formatter.Serialize(exportFile, new ExportData(saveID, saveInfo));
        exportFile.Close();
    }

    /// <summary>
    /// 导入存档
    /// </summary>
    /// <param name="saveID">导入到的存档位置</param>
    /// <returns>导入成功</returns>
    public bool MyImport(int saveID, out ExportData exportData)
    {
        try
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream exportFile = File.Open("Import/Data.binary", FileMode.Open);
            exportData = (ExportData)formatter.Deserialize(exportFile);
            exportFile.Close();

            exportData.Import(saveID);
            return true;
        }
        catch
        {
            exportData = null;
            return false;
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
}

/// <summary>
/// 实现该接口，则对象由唯一ID标识
/// </summary>
public interface IUniqueIdentity
{
    int ID { get; }
}