using UnityEngine;

public class SmallCamManager : MonoBehaviour
{
	public static Camera MainCam { get; set; } = null;
	private static readonly Camera[] smallCam = new Camera[4];

	void Start()
    {
		MainCam = Camera.main;
		for (int i = 0; i < 4; i++)
		{
			smallCam[i] = Instantiate(Camera.main);
			smallCam[i].name = "smallCam" + i;
			smallCam[i].depth = 1;
			smallCam[i].enabled = false;
			Destroy(smallCam[i].gameObject.GetComponent<CharacterController>());
		}
		smallCam[0].rect = new Rect(0, 0.6f, 0.4f, 0.4f);
		smallCam[1].rect = new Rect(0.6f, 0.6f, 0.4f, 0.4f);
		smallCam[2].rect = new Rect(0f, 0, 0.4f, 0.4f);
		smallCam[3].rect = new Rect(0.6f, 0, 0.4f, 0.4f);
		// 全屏显示
		MainCam.rect = new Rect(0, 0, 1, 1);
	}

    void Update()
    {
		bool IsShiftDown = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
		for (int i = 0; i < 4; i++)
		{
			if (smallCam[i].enabled)
			{
				if (Input.GetKeyDown(KeyCode.Alpha1 + i))
				{
					if (IsShiftDown)
					{
						smallCam[i].enabled = false;
					}
					else
					{
						Exchange(smallCam[i], MainCam);
					}
				}
			}
			else
			{
				if (Input.GetKeyDown(KeyCode.Alpha1 + i))
				{
					if (IsShiftDown)
					{
						smallCam[i].enabled = true;
						SetAs(smallCam[i], MainCam);
					}
				}
			}
		}
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
