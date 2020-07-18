using UnityEngine;

public class EntityCreator : MonoBehaviour
{
    public static T CreateEntity<T>(Vector3 pos)
	{
        GameObject TGameObject = (GameObject)Resources.Load(typeof(T).ToString());
        return Instantiate(TGameObject, pos, Quaternion.identity).GetComponent<T>();
    }

    public static T CreateEntity<T>()
    {
        GameObject TGameObject = (GameObject)Resources.Load(typeof(T).ToString());
        return Instantiate(TGameObject, new Vector3(0, 0, 0), Quaternion.identity).GetComponent<T>();
    }
}
