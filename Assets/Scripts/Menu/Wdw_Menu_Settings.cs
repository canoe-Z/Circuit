using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;

public static class MySettings
{
    public static bool openMyPinDamping = true;
    public static bool isEmissionWhenOnLine = true;
    public static bool lockCursor = false;//是否将鼠标光标锁定在屏幕中央
    public static float moveRatio = 1f;//移动速度
    public static float turnRatio = 1f;//转动速度
    public static float moveARatio = 1f;//移动速度

    //未保存程序
    public static float roomTemperature = 20;//室温
    public static float hotRInterval = 0.1f;//热敏电阻计算间隔
    public static float xLimitCursorRange = 100;//鼠标fixed的位置离屏幕距离
    public static float yLimitCursorRange = 100;
}
public class Wdw_Menu_Settings : MonoBehaviour
{
    public Slider sldMove;
    public Slider sldMoveA;
    public Slider sldTurn;
    public Toggle tglLine;
    public Toggle tglMyPinDamping;
    public Toggle tglCursor;
    void Awake()
    {
        LoadToSettings();//加载存档

        sldMove.onValueChanged.AddListener((float value) => MySettings.moveRatio = value);
        sldMoveA.onValueChanged.AddListener((float value) => MySettings.moveARatio = value);
        sldTurn.onValueChanged.AddListener((float value) => MySettings.turnRatio = value);
        tglCursor.onValueChanged.AddListener((bool value) => MySettings.lockCursor = value);
        tglLine.onValueChanged.AddListener((bool value) => MySettings.isEmissionWhenOnLine = value);
        tglMyPinDamping.onValueChanged.AddListener((bool value) => MySettings.openMyPinDamping = value);

        SettingsToMenu();
    }
    public void SettingsToMenu()
    {
        sldMove.value = MySettings.moveRatio;
        sldMoveA.value = MySettings.moveARatio;
        sldTurn.value = MySettings.turnRatio;
        tglLine.isOn = MySettings.isEmissionWhenOnLine;
        tglMyPinDamping.isOn = MySettings.openMyPinDamping;
        tglCursor.isOn = MySettings.lockCursor;
    }



    void OnApplicationQuit()
    {
        SaveSettings();//保存存档
    }




    static void SaveSettings()
    {
        if (!Directory.Exists("Saves"))
            Directory.CreateDirectory("Saves");

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream saveFile = File.Create("Saves/settings.binary");
        formatter.Serialize(saveFile, new MySettingsData());
        saveFile.Close();
    }
    static void LoadToSettings()
    {
        string saveDirectory = "Saves/";
        string saveFile = "settings.binary";
        if (!Directory.Exists(saveDirectory))
            Directory.CreateDirectory(saveDirectory);
        if (File.Exists(saveDirectory + saveFile))
        {
            try//抓取可能出现的错误
            {
                FileStream fs = File.Open(saveDirectory + saveFile, FileMode.Open);
                BinaryFormatter formatter = new BinaryFormatter();
                try//尝试反序列化
                {
                    MySettingsData datafromfile = (MySettingsData)formatter.Deserialize(fs);
                    datafromfile.LoadToSettings();
                    fs.Close();
                }
                catch (System.Runtime.Serialization.SerializationException)//反序列化失败，处理掉这个文件
                {
                    fs.Close();
                    File.Delete(saveDirectory + saveFile);
                }
            }
            catch (Exception e)
            {
#if UNITY_EDITOR
                Debug.LogError(e);
#endif
            }
        }
    }

    [System.Serializable]
    private class MySettingsData
    {
        private bool openMyPinDamping;
        private bool isEmission;
        private bool lockCursor;
        private float moveRatio;
        private float moveARatio;
        private float turnRatio;


        public MySettingsData()
        {
            openMyPinDamping = MySettings.openMyPinDamping;
            isEmission = MySettings.isEmissionWhenOnLine;
            lockCursor = MySettings.lockCursor;
            moveRatio = MySettings.moveRatio;
            moveARatio = MySettings.moveARatio;
            turnRatio = MySettings.turnRatio;
        }
        public void LoadToSettings()
        {
            MySettings.openMyPinDamping = openMyPinDamping;
            MySettings.isEmissionWhenOnLine = isEmission;
            MySettings.lockCursor = lockCursor;
            MySettings.moveRatio = moveRatio;
            MySettings.moveARatio = moveARatio;
            MySettings.turnRatio = turnRatio;
        }
    }
}

