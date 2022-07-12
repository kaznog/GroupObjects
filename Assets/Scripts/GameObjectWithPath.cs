using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameObjectWithPath
{
    public string m_Name;
    public string m_Path;
    public GameObject m_GameObject;
    public GameObjectWithPath(GameObject gameObject)
    {
        var path = gameObject.name;
        var parent = gameObject.transform.parent;
        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        m_Path = path;
        m_Name = gameObject.name;
        m_GameObject = gameObject;
    }
}
