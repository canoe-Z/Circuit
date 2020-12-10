using UnityEngine;
using UnityEngine.UI;

public class AirComditioner : MonoBehaviour
{
	public Text screenText;
	public Transform tr;
	public Vector3 startEular;
	public Vector3 endEular;
	float nowSpeedPS = 0;//每秒速度，当前的
	float nowPos = 1.0f / 3;//当前位置，0-1

	private void OnMouseOver()
	{
		//左键增加
		if (Input.GetMouseButton(0)) nowSpeedPS += 1f * Time.deltaTime;
		//右键减小
		else if (Input.GetMouseButton(1)) nowSpeedPS -= 1f * Time.deltaTime;
		//否则清零
		else nowSpeedPS = 0;
		nowPos += nowSpeedPS * Time.deltaTime;
		if (nowPos > 1) nowPos = 1;
		else if (nowPos < 0) nowPos = 0;
	}
	float willTemperature;
	const float upSpeedK = 0.1f;
	void Update()
	{
		tr.localEulerAngles = nowPos * endEular + (1 - nowPos) * startEular;
		willTemperature = nowPos * 15 + 15;

		MySettings.roomTemperature += (willTemperature - MySettings.roomTemperature) * upSpeedK * Time.deltaTime;
		screenText.text = "室温：" + MySettings.roomTemperature.ToString("0.0") + "℃";
	}
}
