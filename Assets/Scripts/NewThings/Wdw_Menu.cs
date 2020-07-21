using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Wdw_Menu : MonoBehaviour
{
	public GameObject mainThings;
	public Button exitGame;
	public Button continueGame;
	public Button btnCopies_RBox;
	public Button btnCopies_Switch;
	public GameObject gmObjSrc_RBox;
	public GameObject gmObjSrc_Switch;
	public InputField iptNum_SliderR;
	public InputField iptNum_R;
	public Button btnCopiesSP_SliderR;
	public Button btnCopiesSP_R;
	public GameObject gmObjSrcSP_SliderR;
	public GameObject gmObjSrcSP_R;
	void Start()
	{
		if (mainThings == null) Debug.LogError("这里没挂");
		if (exitGame == null) Debug.LogError("这里没挂");
		if (continueGame == null) Debug.LogError("这里没挂");
		CloseMenu();
		exitGame.onClick.AddListener(OnYesButton);
		continueGame.onClick.AddListener(OnNoButton);

		btnCopies_RBox.onClick.AddListener(OnButtonCopy_RBox);
		btnCopies_Switch.onClick.AddListener(OnButtonCopy_Switch);
		btnCopiesSP_SliderR.onClick.AddListener(OnButtonCopySP_SliderR);
		btnCopiesSP_R.onClick.AddListener(OnButtonCopySP_R);
	}

	void Update()
	{
		UpdateCopyThings();


		if (Input.GetKeyDown(KeyCode.Escape))//开启或者关闭菜单，取决于是否进行过无法移动锁定
		{
			if (MoveController.CanOperate) OpenMenu();
			else CloseMenu();
		}
	}

	//打开菜单
	void OpenMenu()
	{
		Cursor.lockState = CursorLockMode.None;//解除鼠标锁定
		Cursor.visible = true;
		MoveController.CanOperate = false;//不允许移动视角
		MoveController.CanTurn = false;
		mainThings.SetActive(true);
	}
	//关闭菜单
	void CloseMenu()
	{
		Cursor.lockState = CursorLockMode.Locked;//锁定鼠标于中央
		Cursor.visible = false;
		MoveController.CanOperate = true;//允许移动视角
		MoveController.CanTurn = true;
		mainThings.SetActive(false);
	}

	void OnYesButton()
	{
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
	}

	void OnNoButton()
	{
		CloseMenu();
	}

	/// <summary>
	/// 将物体复制并且移除碰撞体的系统
	/// </summary>
	void UpdateCopyThings()
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
				EntityBase entityBase = willBeSet.GetComponent<EntityBase>();
				entityBase.DestroyEntity();
				willBeSet = null;
				MoveController.CanOperate = true;//可以操作物体了
			}
		}
	}
	GameObject willBeSet;//即将会被扔到桌子上的物体
	void CloseColl(GameObject operate)//关闭上面那东西的碰撞体
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
	void OpenColl(GameObject operate)//打开上面那东西的碰撞体
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
	//下面全都是按钮
	void OnButtonCopy_RBox()
	{
		GameObject digtalVoltmeter = (GameObject)Resources.Load("DigtalVoltmeter");
		willBeSet = Instantiate(digtalVoltmeter);//复制物体
		CloseColl(willBeSet);//关闭碰撞体
		CloseMenu();//关闭菜单
		MoveController.CanOperate = false;//禁止操作物体
	}
	void OnButtonCopy_Switch()
	{
		willBeSet = Instantiate(gmObjSrc_Switch, new Vector3(0, 0, 0), Quaternion.identity);//复制物体
		CloseColl(willBeSet);//关闭碰撞体
		CloseMenu();//关闭菜单
		MoveController.CanOperate = false;//禁止操作物体
	}
	
	void OnButtonCopySP_SliderR()
	{
		if (double.TryParse(iptNum_SliderR.text, out double num))
		{
			iptNum_SliderR.text = "";
			willBeSet = Instantiate(gmObjSrcSP_SliderR, new Vector3(0, 0, 0), Quaternion.identity);//复制物体
			SliderR sliderR = willBeSet.GetComponent<SliderR>();
			if (sliderR)
			{
				sliderR.Rmax = num;
				CloseColl(willBeSet);//关闭碰撞体
				CloseMenu();//关闭菜单
				MoveController.CanOperate = false;//禁止操作物体
			}
			else
			{
				Debug.LogError("你家滑动变阻器没挂脚本");
			}
		}
	}
	
	void OnButtonCopySP_R()
	{
		if (double.TryParse(iptNum_R.text, out double num))
		{
			Resistance r = EntityCreator.CreateEntity<Resistance>();
			willBeSet = r.gameObject;
			if (r)
			{
				r.Rnum = num;
				CloseColl(willBeSet);//关闭碰撞体
				CloseMenu();//关闭菜单
				MoveController.CanOperate = false;//禁止操作物体
			}
			else
			{
				Debug.LogError("你家电阻没挂脚本");
			}
		}
	}
}
