using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ObjectInfo
{
    [System.NonSerialized]
    public List<ObjectInfo> m_ObjectChildren = new List<ObjectInfo>();
    public ObjectInfo m_ObjectParent = null;
    public List<ObjectInfo> m_Children = new List<ObjectInfo>();
    public ObjectInfo m_Parent = null;

    [SerializeField]
    public bool BaseParentExists
    {
        get { return m_Parent != null; }
    }

    public GameObject m_GameObject = null;

}
