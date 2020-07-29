using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WdwMenu_Create : MonoBehaviour
{
	public Button btn_RBox;
	public Button btn_Switch;
	public Button btn_DigV;
	public Button btn_DigA;
	public InputField iptNum;
	public Button btn_SliderR;
	public Button btn_R;
	public Button btn_NominalR;
	public Dropdown dpdType_uA;
	public Button btn_uA;
	public Button btn_Solar;
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
	}

    // Update is called once per frame
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
		if (ParseRNum(iptNum.text, out double num))
		{
			iptNum.text = "";
			SliderR sliderR = EntityCreator.CreateEntity<SliderR>();//复制物体
			willBeSet = sliderR.gameObject;
			sliderR.rMax = num;
			NormalCreate();
		}
	}
	void OnButtonSP_R()
	{
		if (ParseRNum(iptNum.text, out double num))
		{
			iptNum.text = "";
			Resistance r = EntityCreator.CreateEntity<Resistance>();
			willBeSet = r.gameObject;
			r.Value = num;
			NormalCreate();
		}
	}
	void OnButtonSP_NominalR()
	{
		if (ParseRNum(iptNum.text, out double num))
		{
			iptNum.text = "";
			NominalR nominalR = EntityCreator.CreateEntity<NominalR>();
			willBeSet = nominalR.gameObject;
			nominalR.NominalValue = num;
			NormalCreate();
		}
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
