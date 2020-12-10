using UnityEngine;

/// <summary>
/// 元件按动开关
/// 挂在包含有碰撞体和刚体的物体上，令pos.y变动
/// </summary>
public class MyButton : MonoBehaviour
{
    // 按钮状态变化事件
    public delegate void ButtonEventHandler();
    public event ButtonEventHandler ButtonEvent;

    private const float posYRange = 0.08f;

    private Transform Sw;
    private Vector3 basicPos;

    /// <summary>
    /// 按钮状态
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

    void OnMouseOver()
    {
        if (!MoveController.CanOperate) return;
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            IsOn = !IsOn;
            ChangeState();
            ButtonEvent?.Invoke();
            CircuitCalculator.NeedCalculate = true;
        }
    }

    // 根据开关状态修改颜色和位置
    private void ChangeState()
    {
        if (Sw == null)
        {
            Sw = transform.FindComponent_DFS<Transform>("Switch");
            basicPos = Sw.transform.localPosition;
        }
        if (IsOn)
        {
            ChangeMat(Sw, Color.green);
            Sw.transform.localPosition = basicPos + new Vector3(0, 0, 0);
        }
        else
        {
            ChangeMat(Sw, Color.red);
            Sw.transform.localPosition = basicPos + new Vector3(0, posYRange, 0);
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
