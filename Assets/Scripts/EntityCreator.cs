using System.Collections.Generic;
using UnityEngine;

public class EntityCreator : MonoBehaviour
{
    /// <summary>
    /// 创建元件
    /// </summary>
    /// <typeparam name="T">元件种类</typeparam>
    /// <returns>元件脚本</returns>
    public static T CreateEntity<T>()
    {
        GameObject TGameObject = (GameObject)Resources.Load(typeof(T).ToString());
        return Instantiate(TGameObject, new Vector3(0, 0, 0), Quaternion.identity).GetComponent<T>();
    }

    /// <summary>
    /// 创建元件
    /// </summary>
    /// <typeparam name="T">元件种类</typeparam>
    /// <param name="pos">位置</param>
    /// <returns>元件脚本</returns>
    public static T CreateEntity<T>(Vector3 pos)
	{
        GameObject TGameObject = (GameObject)Resources.Load(typeof(T).ToString());
        return Instantiate(TGameObject, pos, Quaternion.identity).GetComponent<T>();
    }

    /// <summary>
    /// 创建元件
    /// </summary>
    /// <typeparam name="T">元件种类</typeparam>
    /// <param name="pos">位置</param>
    /// <returns>元件脚本</returns>
    public static T CreateEntity<T>(Float3 pos)
    {
        GameObject TGameObject = (GameObject)Resources.Load(typeof(T).ToString());
        return Instantiate(TGameObject, new Vector3(pos.x, pos.y, pos.z), Quaternion.identity).GetComponent<T>();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="pos"></param>
    /// <param name="IDlist"></param>
    /// <returns></returns>
    public static T CreateEntity<T>(Float3 pos,List<int> IDlist)
    {
        //Debug.LogError("创建成功");
        GameObject TGameObject = (GameObject)Resources.Load(typeof(T).ToString());
        T t = Instantiate(TGameObject, new Vector3(pos.x, pos.y, pos.z), Quaternion.identity).GetComponent<T>();
        for (var i =0; i< (t as EntityBase).PortNum; i++)
		{
            (t as EntityBase).ChildPortID = IDlist;
            (t as EntityBase).ChildPorts[i].ID = IDlist[i];
		}
        (t as EntityBase).IsIDSet = true;
        return t;
    }
}
