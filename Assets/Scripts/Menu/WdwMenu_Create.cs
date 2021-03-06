﻿using System.Collections.Generic;
//using System.Runtime.Remoting.Messaging;
using UnityEngine;
using UnityEngine.UI;

public class WdwMenu_Create : MonoBehaviour
{

	[Header("直接创建")]
	public Button btn_RBox;
	public Button btn_Switch;
	public Button btn_DigV;
	public Button btn_DigA;
	public Button btn_Vmeter;
	public Button btn_Ameter;
	public Button btn_Gmeter;
	public Button btn_Solar;
	public Button btn_Thermistor;
	public Button btn_Triode;
	public Button btn_Opamp;
	public Button btn_UJ25;
	public Button btn_Diode;
	public Button btn_QJ45;
	[Header("参数创建")]
	public InputField iptNum_SliderR;
	public Button btn_SliderR;

	[Header("	误差电阻")]
	public InputField iptNum_R;
	public Button btn_R;
	public Dropdown dpd_Wucha;
	[Header("	预置电阻")]
	public Dropdown dpd_TypeOfR;
	public Button btn_NorminalR;

	public Dropdown dpdType_uA;
	public Button btn_uA;
	public Dropdown dpdType_Src;
	public Button btn_Src;
	[Header("3路电源")]
	public InputField iptNum_V0;
	public InputField iptNum_V1;
	public InputField iptNum_V2;
	public Button btn_Source;
	public Dropdown dpdSourceType;
	public Sprite imgSource_1;
	public Sprite imgSource_2;
	public Sprite imgSource_3;
	void Start()
	{
		btn_QJ45.onClick.AddListener(OnButton_Simple<QJ45>);
		btn_Diode.onClick.AddListener(OnButton_Simple<MyDiode>);
		btn_Opamp.onClick.AddListener(OnButton_Simple<OpAmp>);
		btn_UJ25.onClick.AddListener(OnButton_Simple<UJ25>);
		btn_Triode.onClick.AddListener(OnButton_Simple<Triode>);
		btn_Thermistor.onClick.AddListener(OnButton_Simple<Thermistor>);
		btn_RBox.onClick.AddListener(OnButton_Simple<RBox>);
		btn_Solar.onClick.AddListener(OnButton_Simple<Solar>);
		btn_Switch.onClick.AddListener(OnButton_Simple<Switch>);
		btn_DigV.onClick.AddListener(OnButton_Simple<DigtalVoltmeter>);
		btn_Vmeter.onClick.AddListener(OnButton_Simple<Voltmeter>);
		btn_DigA.onClick.AddListener(OnButton_Simple<DigtalAmmeter>);
		btn_Ameter.onClick.AddListener(OnButton_Simple<Ammeter>);
		btn_Gmeter.onClick.AddListener(OnButton_Simple<Gmeter>);
		btn_SliderR.onClick.AddListener(OnButtonSP_SliderR);
		btn_R.onClick.AddListener(OnButtonSP_R);
		btn_NorminalR.onClick.AddListener(OnButtonSP_NominalR);
		btn_uA.onClick.AddListener(OnButtonSP_uA);
		btn_Src.onClick.AddListener(OnButtonSP_Src);

		btn_Source.onClick.AddListener(OnButtonSP_Source);
		dpdSourceType.onValueChanged.AddListener(OnDropDown_Source);
		OnDropDown_Source(dpdSourceType.value);//更新
	}

	//创建三路电源
	ThreeSource.SourceMode willType = ThreeSource.SourceMode.three;
	void OnButtonSP_Source()
	{
		if (double.TryParse(iptNum_V0.text, out double num0) &&
			double.TryParse(iptNum_V1.text, out double num1) &&
			double.TryParse(iptNum_V2.text, out double num2))
		{
			willBeSet = ThreeSource.Create(willType, new List<double> { num0, num1, num2 }).gameObject;
			NormalStartCreate();
		}
		else
		{
			if (iptNum_V0.text == "") iptNum_V0.text = "15";
			if (iptNum_V1.text == "") iptNum_V1.text = "15";
			if (iptNum_V2.text == "") iptNum_V2.text = "5";
		}
	}
	//切换电源模式
	void OnDropDown_Source(int value)
	{
		switch (value)
		{
			case 0:
				willType = ThreeSource.SourceMode.one;
				iptNum_V0.text = "";
				iptNum_V0.gameObject.SetActive(true);
				iptNum_V1.text = "15";
				iptNum_V1.gameObject.SetActive(false);
				iptNum_V2.text = "5";
				iptNum_V2.gameObject.SetActive(false);
				btn_Source.GetComponent<Image>().sprite = imgSource_1;
				break;
			case 1:
				willType = ThreeSource.SourceMode.twoOfThree;
				iptNum_V0.text = "";
				iptNum_V0.gameObject.SetActive(true);
				iptNum_V1.text = "";
				iptNum_V1.gameObject.SetActive(true);
				iptNum_V2.text = "5";
				iptNum_V2.gameObject.SetActive(false);
				btn_Source.GetComponent<Image>().sprite = imgSource_2;
				break;
			case 2:
				willType = ThreeSource.SourceMode.three;
				iptNum_V0.text = "";
				iptNum_V0.gameObject.SetActive(true);
				iptNum_V1.text = "";
				iptNum_V1.gameObject.SetActive(true);
				iptNum_V2.text = "";
				iptNum_V2.gameObject.SetActive(true);
				btn_Source.GetComponent<Image>().sprite = imgSource_3;
				break;
		}
	}

	//下面全都是按钮
	void OnButton_Simple<T>() where T : EntityBase
	{
		willBeSet = EntityBase.BaseCreate<T>().gameObject;//复制物体
		NormalStartCreate();
	}

	void OnButtonSP_SliderR()
	{
		if (ParseRNum(iptNum_SliderR.text, out double num))
		{
			iptNum_SliderR.text = "";
			willBeSet = SliderR.Create(num).gameObject;
			NormalStartCreate();
		}
		else if (iptNum_SliderR.text == "")
		{
			iptNum_SliderR.text = "100";
		}
	}

	void OnButtonSP_R()
	{
		if (ParseRNum(iptNum_R.text, out double RValue))
		{
			iptNum_R.text = "";

			string strLast = "";
			// 根据阻值确定显示方式
			if (RValue >= 1e6)
			{
				strLast = (RValue / 1e6).ToString() + "MΩ";
			}
			else if (RValue >= 1e3)
			{
				strLast = (RValue / 1e3).ToString() + "kΩ";
			}
			else
			{
				strLast = RValue.ToString() + "Ω";
			}

			string strFront = "";
			switch (dpd_Wucha.value)
			{
				case 0:
					strFront = "10%误差\n";
					RValue *= UnityEngine.Random.Range(0.9f, 1.1f);
					break;//10%误差
				case 1:
					strFront = "5%误差\n";
					RValue *= UnityEngine.Random.Range(0.95f, 1.05f);
					break;//5%误差
				case 2:
					strFront = "无误差\n";
					break;//0%误差
			}

			willBeSet = MyResistor.Create(RValue, strFront + strLast).gameObject;
			NormalStartCreate();
		}
		else if (iptNum_R.text == "")
		{
			iptNum_R.text = "100";
		}
	}

	void OnButtonSP_NominalR()
	{
		switch (dpd_TypeOfR.value)
		{
			case 0: willBeSet = MyResistor.Create(100 * Random.Range(0.9f, 1.1f), "待测\n100Ω"); break;
			case 1: willBeSet = MyResistor.Create(1e6 * Random.Range(0.9f, 1.1f), "待测\n1MΩ"); break;
			case 2: willBeSet = MyResistor.Create(0.1 * Random.Range(0.9f, 1.1f), "待测\n0.1Ω"); break;
			case 3: willBeSet = MyResistor.Create(120 * Random.Range(0.9f, 1.1f), "标称\n120Ω"); break;
		}
		NormalStartCreate();
	}

	void OnButtonSP_uA()
	{
		switch (dpdType_uA.value)
		{
			case 0: willBeSet = NominaluA.Create(50, 2); break;//50
			case 1: willBeSet = NominaluA.Create(100, 1); break;//100
			case 2: willBeSet = NominaluA.Create(200, 0.5); break;//200
		}
		NormalStartCreate();
	}
	void OnButtonSP_Src()
	{
		switch (dpdType_Src.value)
		{
			case 0://1.01860
				willBeSet = Source.Create(1.01860, 100, "标准电池");
				break;
			case 1:
				double willE = UnityEngine.Random.Range(1.48f, 1.52f);//随机数
				willBeSet = Source.Create(willE, 100, "待测电池");
				break;//待测
		}
		NormalStartCreate();
	}

	void Update()
	{
		if (willBeSet)//如果带了一个物体
		{
			RaycastHit info;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray, out info, 2000, (1 << 11) | (1 << 0)))//0层碰撞
			{
				willBeSet.transform.position = info.point;
			}
			if (Input.GetMouseButtonDown(0))//左键放下物体
			{
				OpenColl(willBeSet);//打开碰撞体
				willBeSet = null;
				MoveController.CanOperate = true;//可以操作物体了
			}
			else if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
			{
				OpenColl(willBeSet);//打开碰撞体
				EntityBase entityBase = willBeSet.GetComponent<EntityBase>();//销毁
				entityBase.DestroyEntity();
				willBeSet = null;
				MoveController.CanOperate = true;//可以操作物体了
			}
		}
	}
	GameObject willBeSet;//即将会被扔到桌子上的物体

	//正常创建一个物体，减少重复代码的使用
	void NormalStartCreate()
	{
		MyPin theMyPin = willBeSet.GetComponentInChildren<MyPin>();//只要有这东西
		if (theMyPin) theMyPin.myRandomFlag = true;//就设置随机值

		CloseColl(willBeSet);//关闭碰撞体
		Wdw_Menu.Instance.MyCloseMenu();//关闭菜单
		MoveController.CanOperate = false;//禁止操作物体
		MoveController.CanControll = true;
	}

	//
	//
	//下面是功能函数

	//解析成为电阻阻值
	static bool ParseRNum(string toParse, out double num)
	{
		toParse = toParse.Replace("k", "000");
		toParse = toParse.Replace("K", "000");
		toParse = toParse.Replace("m", "000000");
		toParse = toParse.Replace("M", "000000");
		if (double.TryParse(toParse, out num))
		{
			return true;
		}
		else
		{
			return false;
		}
	}


	//
	//关闭这东西的碰撞体
	static void CloseColl(GameObject operate)
	{
		Collider[] colliders = operate.GetComponentsInChildren<Collider>();
		foreach (var coll in colliders)
		{
			coll.enabled = false;
		}
		//得到引用
		Renderer[] transparentRenderers = operate.GetComponentsInChildren<Renderer>();
		//模型
		foreach (var rend in transparentRenderers)
		{
			foreach (var m in rend.materials)
			{
				if (m.name.Remove(5) != "Glass")
					m.SetFloat("Vector1_A623D23A", 0.5f);
			}
		}
	}
	//打开这东西的碰撞体
	static void OpenColl(GameObject operate)
	{
		Collider[] colliders = operate.GetComponentsInChildren<Collider>();
		foreach (var coll in colliders)
		{
			coll.enabled = true;
		}
		//得到引用
		Renderer[] transparentRenderers = operate.GetComponentsInChildren<Renderer>();
		//模型
		foreach (var rend in transparentRenderers)
		{
			foreach (var m in rend.materials)
			{
				if (m.name.Remove(5) != "Glass")
					m.SetFloat("Vector1_A623D23A", 1f);
			}
		}
		//清除引用
	}
}
