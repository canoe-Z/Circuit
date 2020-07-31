using System.Collections;
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
	[Header("3路电源")]
	public Button btn_StrangeSource;
	public Canvas cnvStrange;
	public InputField iptNum_V0;
	public InputField iptNum_V1;
	public InputField iptNum_V2;
	public Button btn_Source;
	void Start()
	{
		btn_RBox.onClick.AddListener(OnButton_RBox);
		btn_Solar.onClick.AddListener(OnButton_Solar);
		btn_Switch.onClick.AddListener(OnButton_Switch);
		btn_DigV.onClick.AddListener(OnButton_DigV);
		btn_DigA.onClick.AddListener(OnButton_DigA);
		btn_SliderR.onClick.AddListener(OnButtonSP_SliderR);
		btn_R.onClick.AddListener(OnButtonSP_R);
		btn_NominalR.onClick.AddListener(OnButtonSP_NominalR);
		btn_uA.onClick.AddListener(OnButtonSP_uA);

		btn_StrangeSource.onClick.AddListener(OnStrange_Source);
		btn_Source.onClick.AddListener(OnButtonSP_Source);
	}

	//创建
	void OnButtonSP_Source()
	{
		if(double.TryParse(iptNum_V0.text,out double num0)&&
			double.TryParse(iptNum_V1.text, out double num1)&& 
			double.TryParse(iptNum_V2.text, out double num2))
		{
			ThreeSource threeSource = EntityCreator.CreateEntity<ThreeSource>();
			threeSource.EMax[0] = num0;
			threeSource.EMax[1] = num1;
			threeSource.EMax[2] = num2;
			willBeSet = threeSource.gameObject;//复制物体
			NormalCreate();

			cnvStrange.enabled = false;//创建之后关闭选项卡
		}
		else
		{
			iptNum_V0.text = "15";
			iptNum_V1.text = "15";
			iptNum_V2.text = "5";
		}
	}
	//打开/关闭菜单
	void OnStrange_Source()
	{
		cnvStrange.enabled = !cnvStrange.enabled;
	}

	//下面全都是按钮
	void OnButton_RBox()
	{
		willBeSet = EntityCreator.CreateEntity<RBox>().gameObject;//复制物体
		NormalCreate();
	}
	void OnButton_Switch()
	{
		willBeSet = EntityCreator.CreateEntity<Switch>().gameObject;//复制物体//复制物体
		NormalCreate();
	}
	void OnButton_DigV()
	{
		willBeSet = EntityCreator.CreateEntity<DigtalVoltmeter>().gameObject;//复制物体//复制物体
		NormalCreate();
	}
	void OnButton_DigA()
	{
		willBeSet = EntityCreator.CreateEntity<DigtalAmmeter>().gameObject;//复制物体//复制物体
		NormalCreate();
	}
	void OnButton_Solar()
	{
		willBeSet = EntityCreator.CreateEntity<Solar>().gameObject;//复制物体//复制物体
		NormalCreate();
	}

	void OnButtonSP_SliderR()
	{
		if (ParseRNum(iptNum_SliderR.text, out double num))
		{
			iptNum_SliderR.text = "";
			SliderR sliderR = EntityCreator.CreateEntity<SliderR>();//复制物体
			willBeSet = sliderR.gameObject;
			sliderR.rMax = num;
			NormalCreate();
		}
	}
	void OnButtonSP_R()
	{
		if (ParseRNum(iptNum_R.text, out double num))
		{
			iptNum_R.text = "";
			Resistance r = EntityCreator.CreateEntity<Resistance>();
			willBeSet = r.gameObject;
			r.Value = num;
			NormalCreate();
		}
	}
	void OnButtonSP_NominalR()
	{
		NominalR nominalR = EntityCreator.CreateEntity<NominalR>();
		willBeSet = nominalR.gameObject;
		switch (dpdType_NominalR.value)
		{
			case 0: nominalR.NominalValue = 100; break;//100
			case 1: nominalR.NominalValue = 1000000; break;//1M
			case 2: nominalR.NominalValue = 0.1; break;//0.1
		}
		NormalCreate();
	}
	void OnButtonSP_uA()
	{
		NominaluA sampleuA = EntityCreator.CreateEntity<NominaluA>();
		willBeSet = sampleuA.gameObject;
		switch (dpdType_uA.value)
		{
			case 0: sampleuA.MyChangeToWhichType(50); break;//50
			case 1: sampleuA.MyChangeToWhichType(100); break;//100
			case 2: sampleuA.MyChangeToWhichType(200); break;//200
		}
		NormalCreate();
	}


	
	//
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
