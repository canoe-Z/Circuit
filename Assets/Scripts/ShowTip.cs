using UnityEngine;

/// <summary>
/// 根据鼠标的不同情况显示文字
/// </summary>
public class ShowTip : MonoBehaviour
{
	public static bool IsTipShowed = false;
	/// <summary>
	/// 鼠标置于某端口上
	/// </summary>
	/// <param name="which">鼠标置于的端口</param>
	public static void OverPort(CircuitPort which)
	{
		if (Global.Other.prePort == null) CamMain.ShowTips("单击以连接导线。\n", 0);
		else
		{
			if (Global.Other.prePort == which) CamMain.ShowTips("在其它接线柱单击，完成连接导线。\n按 鼠标右键 清除当前连接。\n", 0);
			else CamMain.ShowTips("在这里单击，完成连接导线。\n按 鼠标右键 清除当前连接。\n", 0);
		}
		CamMain.ShowTips(null, 1);
		CamMain.ShowTips(null, 2);
		CamMain.ShowTips(null, 3);
		IsTipShowed = true;
	}
	/// <summary>
	/// 鼠标置于某元件上
	/// </summary>
	/// <param name="which">鼠标置于的元件</param>
	public static void OverItem(EntityBase which)
	{
		if (Global.Other.prePort == null) CamMain.ShowTips("捕捉到接线柱并单击，开始连接导线。\n", 0);
		else CamMain.ShowTips("在接线柱处单击，继续连接导线。\n按 鼠标右键 清除当前连接。\n", 0);
		CamMain.ShowTips("鼠标拖动，移动这个元件。不要扔到桌子外面哦！\n按 鼠标中键 将这个元件的方向正回来。（如果被你玩飞了的话AwA）\n", 1);
		if (Input.GetMouseButtonDown(2)) which.Straighten();
		CamMain.ShowTips(null, 2);
		CamMain.ShowTips(null, 3);
		IsTipShowed = true;
	}
	/// <summary>
	/// 鼠标置于其他位置
	/// </summary>
	public static void OverElse()
	{
		if (Global.Other.prePort == null) CamMain.ShowTips("捕捉到接线柱并单击，开始连接导线。\n", 0);
		else CamMain.ShowTips("在接线柱处单击，继续连接导线。\n按 鼠标右键 清除当前连接。\n", 0);
		CamMain.ShowTips(null, 1);
		CamMain.ShowTips(null, 2);
		CamMain.ShowTips(null, 3);
	}
	/// <summary>
	/// 鼠标置于导线上
	/// </summary>
	public static void OverChain()
	{
		if (Global.Other.prePort == null) CamMain.ShowTips("捕捉到接线柱并单击，开始连接导线。\n", 0);
		else CamMain.ShowTips("在接线柱处单击，继续连接导线。\n按 鼠标右键 清除当前连接。\n", 0);
		CamMain.ShowTips(null, 1);
		CamMain.ShowTips("按 鼠标右键 删除这个导线。\n", 2);
		CamMain.ShowTips(null, 3);
		IsTipShowed = true;
	}
	/// <summary>
	/// 鼠标置于滑块上
	/// </summary>
	public static void OverSlider()
	{
		if (Global.Other.prePort == null) CamMain.ShowTips("捕捉到接线柱并单击，开始连接导线。\n", 0);
		else CamMain.ShowTips("在接线柱处单击，继续连接导线。\n按 鼠标右键 清除当前连接。\n", 0);
		CamMain.ShowTips(null, 1);
		CamMain.ShowTips(null, 2);
		CamMain.ShowTips("滑动以调节参数。\n", 3);
		IsTipShowed = true;
	}

	private void Update()
	{
		if(!IsTipShowed)
		{
			OverElse();
		}
	}
}



