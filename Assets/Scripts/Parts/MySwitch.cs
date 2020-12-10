using UnityEngine;

/// <summary>
/// 元件开关
/// 挂在包含有碰撞体和刚体的物体上，令localEular.z变动
/// </summary>
public class MySwitch : MonoBehaviour
{
    // 开关状态变化事件
    public delegate void SwitchEventHandler();
    public event SwitchEventHandler SwitchEvent;

    /// <summary>
    /// 开关状态
    /// </summary>
    private bool isOn = true;
    public bool IsOn
    {
        get
        {
            return isOn;
        }
        set
        {
            isOn = value;
            ChangeState();
        }
    }

    private bool isAwaked = false;

    private readonly float angleRange = 15;

    private Transform Sw;
    private Vector3 basicPos;

    void OnMouseOver()
    {
        if (!MoveController.CanOperate) return;
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            IsOn = !IsOn;
            ChangeState();
            SwitchEvent?.Invoke();
            CircuitCalculator.NeedCalculate = true;
        }
    }

    // 根据开关状态修改颜色和位置
    private void ChangeState()
    {
        if (Sw == null)
        {
            Sw = transform.FindComponent_DFS<Transform>("Sw");
            basicPos = Sw.transform.localEulerAngles;
        }
        if (IsOn)
        {
            ChangeMat(Sw, Color.green);
            Sw.transform.localEulerAngles = basicPos + new Vector3(angleRange, 0, 0);
        }
        else
        {
            ChangeMat(Sw, Color.red);
            Sw.transform.localEulerAngles = basicPos + new Vector3(-angleRange, 0, 0);
        }
    }

    // 修改颜色
    private void ChangeMat(Transform trans, Color color)
    {
        Renderer[] renderers = trans.GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            foreach (var material in renderer.materials)
            {
                material.SetColor("Color_51411BA8", color);
            }
        }
    }
}
