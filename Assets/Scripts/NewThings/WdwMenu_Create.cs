using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WdwMenu_Create : MonoBehaviour
{
	public Button btnCopies_RBox;
	public Button btnCopies_Switch;
	public InputField iptNum_SliderR;
	public InputField iptNum_R;
	public Button btnCopiesSP_SliderR;
	public Button btnCopiesSP_R;
	void Start()
    {
		btnCopies_RBox.onClick.AddListener(OnButtonCopy_RBox);
		btnCopies_Switch.onClick.AddListener(OnButtonCopy_Switch);
		btnCopiesSP_SliderR.onClick.AddListener(OnButtonCopySP_SliderR);
		btnCopiesSP_R.onClick.AddListener(OnButtonCopySP_R);
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
	void OnButtonCopy_RBox()
	{
		willBeSet = EntityCreator.CreateEntity<RBox>().gameObject;//复制物体
		NormalCreate();
	}
	void OnButtonCopy_Switch()
	{
		willBeSet = EntityCreator.CreateEntity<Switch>().gameObject;//复制物体//复制物体
		NormalCreate();
	}
	void OnButtonCopySP_SliderR()
	{
		string textStr = iptNum_R.text;
		textStr = textStr.Replace("k", "000");
		textStr = textStr.Replace("K", "000");
		textStr = textStr.Replace("m", "000000");
		textStr = textStr.Replace("M", "000000");
		if (double.TryParse(textStr, out double num))
		{
			iptNum_SliderR.text = "";
			SliderR sliderR = EntityCreator.CreateEntity<SliderR>();//复制物体
			willBeSet = sliderR.gameObject;
			if (sliderR)
			{
				sliderR.RMax = num;
				NormalCreate();
			}
			else
			{
				Debug.LogError("你家滑动变阻器没挂脚本");
			}
		}
	}
	void OnButtonCopySP_R()
	{
		string textStr = iptNum_R.text;
		textStr = textStr.Replace("k", "000");
		textStr = textStr.Replace("K", "000");
		textStr = textStr.Replace("m", "000000");
		textStr = textStr.Replace("M", "000000");
		if (double.TryParse(textStr, out double num))
		{
			iptNum_R.text = "";
			Resistance r = EntityCreator.CreateEntity<Resistance>();
			willBeSet = r.gameObject;
			if (r)
			{
				r.Value = num;
				NormalCreate();
			}
			else
			{
				Debug.LogError("你家电阻没挂脚本");
			}
		}
	}

	//下面是功能函数

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
