using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// 鼠标操控
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Camera))]
public class MoveController : MonoBehaviour
{
    [DllImport("user32.dll")]// 用于获取CapsLock的状态
    static extern short GetKeyState(int keyCode);
    [DllImport("user32.dll")]// 用于获取CapsLock的状态
    static extern int SetCursorPos(int x, int y);
    /// <summary>
    /// 可以操纵场景内已有对象
    /// </summary>
    public static bool CanOperate { get; set; } = true;

    /// <summary>
    /// 可以操纵角色
    /// </summary>
    public static bool CanControll { get; set; } = true;

    //***转头参数***

    //转头变量
    Vector3 eularAngles = new Vector3(0, -90, 0);

    //***移动参数***
    const float moveSpeedMax = 20;//速度上限dm/s
    const float moveAcceleration = 60f;//加速度dm/s2




    Rigidbody rigidBody;
    Camera cam;

    void Start()
    {
        // 锁定光标并隐藏
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;

        rigidBody = GetComponent<Rigidbody>();
        rigidBody.velocity = Vector3.zero;//速度清零
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        Rotate();
        Move();
        RopeUpdate();
    }

    /// <summary>
    /// 视角旋转
    /// </summary>
    private void Rotate()
    {
        // 在菜单的时候禁止转头
        if (!CanControll)
        {
            Cursor.lockState = CursorLockMode.None;
            return;
        }



        // 鼠标移动距离
        float dx = Input.GetAxis("Mouse X");
        float dy = Input.GetAxis("Mouse Y");
        bool canMoveScreenX = false;
        bool canMoveScreenY = false;

        if (MySettings.lockCursor)
        {
            canMoveScreenX = true;
            canMoveScreenY = true;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Confined;
        }


        Vector3 nowPosition = Input.mousePosition;
        if (nowPosition.x > Screen.width - MySettings.xLimitCursorRange && dx > 0)
        {
            canMoveScreenX = true;
        }
        else if (nowPosition.x < MySettings.xLimitCursorRange && dx < 0)
        {
            canMoveScreenX = true;
        }
        if (nowPosition.y > Screen.height - MySettings.yLimitCursorRange && dy > 0)
        {
            canMoveScreenY = true;
        }
        else if (nowPosition.y < MySettings.yLimitCursorRange && dy < 0)
        {
            canMoveScreenY = true;
        }

        float speed = 1;
        // 大写锁定打开时，转头速度乘以0.8
        // (((ushort)GetKeyState(0x14)) & 0xffff) != 0 -->大写锁定已打开
        if ((((ushort)GetKeyState(0x14)) & 0xffff) != 0)
        {
            speed *= 0.8f;
        }
        if (canMoveScreenY)
            eularAngles.x -= dy * speed * MySettings.turnRatio;
        if (canMoveScreenX)
            eularAngles.y += dx * speed * MySettings.turnRatio;

        if (eularAngles.x > 89) eularAngles.x = 89;
        else if (eularAngles.x < -89) eularAngles.x = -89;
        if (eularAngles.y > 360) eularAngles.y -= 360;
        else if (eularAngles.y < 0) eularAngles.y += 360;

        transform.eulerAngles = eularAngles;
    }

    /// <summary>
    /// 移动
    /// </summary>
    private void Move()
    {
        // 在菜单的时候禁止移动
        if (!CanControll)
        {
            rigidBody.velocity = Vector3.zero;
            return;
        }


        float speed = moveSpeedMax;
        // 大写锁定打开时，移动速度变为十分之一
        // (((ushort)GetKeyState(0x14)) & 0xffff) != 0 -->大写锁定已打开
        if ((((ushort)GetKeyState(0x14)) & 0xffff) != 0)
        {
            speed /= 10;
        }

        //得到移动方向
        Vector3 localForward = Vector3.zero;
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) localForward.z += 1;
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) localForward.x += 1;
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) localForward.z -= 1;
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) localForward.x -= 1;
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) localForward.y -= 1;
        if (Input.GetKey(KeyCode.Space)) localForward.y += 1;


        if (localForward.magnitude > 0.01f)//移动了
        {
            localForward.Normalize();//单位化
            Vector3 control = transform.TransformDirection(localForward);//变到世界坐标系

            rigidBody.velocity += control * moveAcceleration * Time.deltaTime * Mathf.Pow(10, MySettings.moveARatio);//获得加速度

            if (rigidBody.velocity.magnitude > speed * MySettings.moveRatio)//限制最大速度
            {
                rigidBody.velocity = rigidBody.velocity.normalized * speed * MySettings.moveRatio;
            }
        }
        else
        {
            rigidBody.velocity = rigidBody.velocity * Time.deltaTime * (-MySettings.moveARatio + 1) / 2;//速度衰减
        }
    }

    /// <summary>
    /// 绳子操作
    /// </summary>
    private void RopeUpdate()
    {
        if (!CanOperate) return;

        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            // 鼠标操作时更新所有导线的碰撞体
            foreach (CircuitLine line in CircuitCalculator.Lines)
            {
                line.GetComponent<MeshCollider>().sharedMesh = line.GetComponent<MeshFilter>().sharedMesh;
                //line.GetComponent<MeshCollider>().convex = true;
            }

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hitInfo))
            {
                GameObject hitObject = hitInfo.collider.gameObject;
                if (hitObject.name == "Rope")
                {
                    // 右键删除
                    if (Input.GetMouseButtonDown(1))
                    {
                        hitObject.GetComponent<CircuitLine>().DestroyRope();
                    }
                }
            }
        }
    }
}