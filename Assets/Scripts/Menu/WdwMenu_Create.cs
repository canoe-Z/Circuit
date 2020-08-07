﻿using System.Collections;
using System.Collections.Generic;
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
	public Button btn_Solar;
	[Header("参数创建")]
	public InputField iptNum_SliderR;
	public Button btn_SliderR;
	public InputField iptNum_R;
	public Button btn_R;
	public Dropdown dpdType_NominalR;
	public Button btn_NominalR;
	public Dropdown dpdType_uA;
	public Button btn_uA;
	public Dropdown dpdType_Src;
	public Button btn_Src;
	[Header("3路电源")]
	public Button btn_StrangeSource;
	public Canvas cnvStrange;
	public InputField iptNum_V0;
	public InputField iptNum_V1;
	public InputField iptNum_V2;
	public Button btn_Source;
	public Dropdown dpdSourceType;
	void Start()
	{
		btn_RBox.onClick.AddListener(OnButton_Simple<RBox>);
		btn_Solar.onClick.AddListener(OnButton_Simple<Solar>);
		btn_Switch.onClick.AddListener(OnButton_Simple<Switch>);
		btn_DigV.onClick.AddListener(OnButton_Simple<DigtalVoltmeter>);
		btn_Vmeter.onClick.AddListener(OnButton_Simple<Voltmeter>);
		btn_DigA.onClick.AddListener(OnButton_Simple<DigtalAmmeter>);
		btn_Ameter.onClick.AddListener(OnButton_Simple<Ammeter>);
		btn_SliderR.onClick.AddListener(OnButtonSP_SliderR);
		btn_R.onClick.AddListener(OnButtonSP_R);
		btn_NominalR.onClick.AddListener(OnButtonSP_NominalR);
		btn_uA.onClick.AddListener(OnButtonSP_uA);
		btn_Src.onClick.AddListener(OnButtonSP_Src);

		btn_StrangeSource.onClick.AddListener(OnStrange_Source);
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
			NormalCreate();
			cnvStrange.enabled = false;//创建之后关闭选项卡
		}
		else
		{
			if (iptNum_V0.text == "") iptNum_V0.text = "15";
			if (iptNum_V1.text == "") iptNum_V1.text = "15";
			if (iptNum_V2.text == "") iptNum_V2.text = "5";
		}
	}
	//打开/关闭菜单
	void OnStrange_Source()
	{
		cnvStrange.enabled = !cnvStrange.enabled;
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
				break;
			case 1:
				willType = ThreeSource.SourceMode.twoOfThree;
				iptNum_V0.text = "";
				iptNum_V0.gameObject.SetActive(true);
				iptNum_V1.text = "";
				iptNum_V1.gameObject.SetActive(true);
				iptNum_V2.text = "5";
				iptNum_V2.gameObject.SetActive(false);
				break;
			case 2:
				willType = ThreeSource.SourceMode.three;
				iptNum_V0.text = "";
				iptNum_V0.gameObject.SetActive(true);
				iptNum_V1.text = "";
				iptNum_V1.gameObject.SetActive(true);
				iptNum_V2.text = "";
				iptNum_V2.gameObject.SetActive(true);
				break;
		}
	}

	//下面全都是按钮
	void OnButton_Simple<T>() where T : EntityBase
	{
		willBeSet = EntityBase.SimpleCreate<T>().gameObject;//复制物体
		NormalCreate();
	}

	void OnButtonSP_SliderR()
	{
		if (ParseRNum(iptNum_SliderR.text, out double num))
		{
			iptNum_SliderR.text = "";
			willBeSet = SliderR.Create(num).gameObject;
			NormalCreate();
		}
	}

	void OnButtonSP_R()
	{
		if (ParseRNum(iptNum_R.text, out double num))
		{
			iptNum_R.text = "";
			willBeSet = Resistance.Create(num).gameObject;
			NormalCreate();
		}
	}

	void OnButtonSP_NominalR()
	{
		switch (dpdType_NominalR.value)
		{
			case 0: willBeSet = NominalR.Create(100, "待测"); break;
			case 1: willBeSet = NominalR.Create(1e6, "待测"); break;
			case 2: willBeSet = NominalR.Create(0.1, "待测"); break;
			case 3: willBeSet = NominalR.Create(120, "标称"); break;
		}
		NormalCreate();
	}

	void OnButtonSP_uA()
	{
		switch (dpdType_uA.value)
		{
			case 0: willBeSet = NominaluA.Create(50, 2); break;//50
			case 1: willBeSet = NominaluA.Create(100, 1); break;//100
			case 2: willBeSet = NominaluA.Create(200, 0.5); break;//200
		}
		NormalCreate();
	}
	void OnButtonSP_Src()
	{
		switch (dpdType_Src.value)
		{
			case 0:
				willBeSet = Source.Create(1.01865, 100);
				break;//1.01865
			case 1:
				willBeSet = NominalSource.Create(1.54652, 100, "待测电源");
				break;//待测
		}
		NormalCreate();
	}

	void Update()
	{
		if (willBeSet)//如果带了一个物体
		{
			RaycastHit info;
			Transform camTr = SmallCamManager.MainCam.gameObject.transform;//主摄像机
			if (Physics.Raycast(camTr.position, camTr.forward, out info, 2000, 1 << 0))//0层碰撞
			{
				willBeSet.transform.position = info.point;
			}
			if (Input.GetMouseButtonDown(0))//左键放下物体
			{
				OpenColl(willBeSet);//打开碰撞体
				willBeSet = null;
				MoveController.CanOperate = true;//可以操作物体了
			}
			else if (Input.GetMouseButtonDown(1))
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
	void NormalCreate()
	{
		CloseColl(willBeSet);//关闭碰撞体
		Wdw_Menu.shouldCloseMenu = true;//关闭菜单
		MoveController.CanOperate = false;//禁止操作物体
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
	//关闭这东西的碰撞体
	static void CloseColl(GameObject operate)
	{
		Collider[] colliders = operate.GetComponentsInChildren<Collider>();
		foreach (var coll in colliders)
		{
			coll.enabled = false;
		}
		Renderer[] renderers = operate.GetComponentsInChildren<Renderer>();
		foreach (var rend in renderers)
		{
			foreach (var mat in rend.materials)
			{
				{//变为Fade
					mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
					mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
					mat.SetInt("_ZWrite", 0);
					mat.EnableKeyword("_ALPHABLEND_ON");
					mat.renderQueue = 3000;
				}
				Color color = mat.color;
				color.a = 0.5f;
				mat.color = color;
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
		Renderer[] renderers = operate.GetComponentsInChildren<Renderer>();
		foreach (var rend in renderers)
		{
			foreach (var mat in rend.materials)
			{
				{//恢复Opaque
					mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
					mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
					mat.SetInt("_ZWrite", 1);
					mat.DisableKeyword("_ALPHABLEND_ON");
					mat.renderQueue = -1;
				}
				Color color = mat.color;
				color.a = 1f;
				mat.color = color;
			}
		}

	}
}