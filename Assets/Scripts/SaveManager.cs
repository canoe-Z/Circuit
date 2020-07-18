using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public List<LineData> LineDataList { get; set; } = new List<LineData>();
}

public class SaveManager : MonoBehaviour
{
    public void Save()
    {
        LineData lineData;
        SaveData savedata = new SaveData();
        foreach (CircuitLine line in CircuitCalculator.Lines)
        {
            lineData = line.Save();
            savedata.LineDataList.Add(lineData);
            Debug.LogError("1111");
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
        //Debug.LogError(GetItemById(1,CircuitCalculator.Ports).ID);
        if (Input.GetKeyDown(KeyCode.F5))
        {
            Save();
            var node = CircuitCalculator.Lines.First;
            while (node != null)
            {
                var next = node.Next;
                node.Value.DestroyRope();
                node = next;
            }
            CircuitCalculator.CalculateAll();
        }
        if (Input.GetKeyDown(KeyCode.F9))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream saveFile = File.Open("Saves/save.binary", FileMode.Open);
            SaveData datafromfile = (SaveData)formatter.Deserialize(saveFile);
            saveFile.Close();
            foreach (LineData data in datafromfile.LineDataList)
            {
                ConnectionManager.ConnectRope(GetItemById(data.startID, CircuitCalculator.Ports), GetItemById(data.endID, CircuitCalculator.Ports));
            }
            CircuitCalculator.CalculateAll();
        }
    }

    public static T GetItemById<T>(int PortID, IEnumerable<T> list) where T : IUniqueIdentity
    {
        IEnumerable<T> mainList = list;

        // SingleOrDefault:返回序列中的唯一记录；如果该序列为空，则返回默认值；如果该序列包含多个元素，则引发异常
        return mainList.SingleOrDefault(item => item.ID == PortID);

    }
}

public interface IUniqueIdentity
{
    int ID { get; }
}

interface ISaveble
{
    void Save();
}