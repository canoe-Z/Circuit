using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 摄像机控制
/// </summary>
public class SmallCamManager : MonoBehaviour
{
	public static Camera MainCam { get; set; } = null;
	private static readonly Camera[] smallCams = new Camera[4];

	void Start()
	{
		MainCam = Camera.main;
		for (int i = 0; i < 4; i++)
		{
			smallCams[i] = Instantiate(Camera.main);
			smallCams[i].name = "smallCam" + i;
			smallCams[i].depth = 1;
			smallCams[i].enabled = false;
			Destroy(smallCams[i].gameObject.GetComponent<MoveController>());
		}

		// 初始化显示位置
		smallCams[0].rect = new Rect(0, 0.6f, 0.4f, 0.4f);
		smallCams[1].rect = new Rect(0.6f, 0.6f, 0.4f, 0.4f);
		smallCams[2].rect = new Rect(0f, 0, 0.4f, 0.4f);
		smallCams[3].rect = new Rect(0.6f, 0, 0.4f, 0.4f);
		MainCam.rect = new Rect(0, 0, 1, 1);
	}

	void Update()
	{
		bool IsShiftDown = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
		for (var i = 0; i < 4; i++)
		{
			if (smallCams[i].enabled)
			{
				if (Input.GetKeyDown(KeyCode.Alpha1 + i))
				{
					if (IsShiftDown)
					{
						smallCams[i].enabled = false;
					}
					else
					{
						Exchange(smallCams[i], MainCam);
					}
				}
			}
			else
			{
				if (Input.GetKeyDown(KeyCode.Alpha1 + i))
				{
					if (IsShiftDown)
					{
						smallCams[i].enabled = true;
						SetAs(smallCams[i], MainCam);
					}
				}
			}
		}
	}

	public static CameraData Save()
	{
		List<Camera> cameraList = new List<Camera>();
		cameraList.AddRange(smallCams);
		cameraList.Add(MainCam);
		return new CameraData(cameraList);
	}

	public static void Load(List<Float3> camPosList, List<Float4> camAngleList, List<bool> isCamEnableList)
	{
		for (var i = 0; i < 4; i++)
		{
			LoadCamera(smallCams[i], camPosList[i], camAngleList[i], isCamEnableList[i]);
		}
		LoadCamera(MainCam, camPosList[4], camAngleList[4], isCamEnableList[4]);
	}

	private static void LoadCamera(Camera camera, Float3 pos, Float4 angle, bool enabled)
	{
		camera.transform.position = pos.ToVector3();
		camera.transform.rotation = angle.ToQuaternion();
		camera.enabled = enabled;
	}

	/// <summary>
	/// 复制摄像机的位置
	/// </summary>
	/// <param name="which">将复制的摄像机</param>
	/// <param name="source">被覆盖的摄像机</param>
	static void SetAs(Camera which, Camera source)
	{
		Quaternion rot = source.transform.rotation;
		Vector3 pos = source.transform.position;
		which.transform.SetPositionAndRotation(pos, rot);
	}

	/// <summary>
	/// 交换两个摄像机
	/// </summary>
	/// <param name="one">摄像机1</param>
	/// <param name="two">摄像机2</param>
	static void Exchange(Camera one, Camera two)
	{
		Quaternion rot1 = one.transform.rotation;
		Vector3 pos1 = one.transform.position;
		Quaternion rot2 = two.transform.rotation;
		Vector3 pos2 = two.transform.position;
		one.transform.SetPositionAndRotation(pos2, rot2);
		two.transform.SetPositionAndRotation(pos1, rot1);
	}


}

[System.Serializable]
public class CameraData
{
	private readonly List<Float3> camPosList = new List<Float3>();
	private readonly List<Float4> camAngleList = new List<Float4>();
	private readonly List<bool> isCamEnableList = new List<bool>();

	public CameraData(List<Camera> cameras)
	{
		foreach (Camera camera in cameras)
		{
			camPosList.Add(camera.transform.position.ToFloat3());
			camAngleList.Add(camera.transform.rotation.ToFloat4());
			isCamEnableList.Add(camera.enabled);
		}
	}

	public void Load() => SmallCamManager.Load(camPosList, camAngleList, isCamEnableList);
}
