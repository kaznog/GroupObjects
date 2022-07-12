using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GroupObjectTreeViewInfomation
{
    const int ITEM_ID_NONE = -1;
    public int m_Id = ITEM_ID_NONE;
    public int id
    {
        get { return m_Id; }
        set { m_Id = value; }
    }

    [System.NonSerialized]
    public List<GroupObjectTreeViewInfomation> m_ObjectChildren = new List<GroupObjectTreeViewInfomation>();
    public GroupObjectTreeViewInfomation m_ObjectParent = null;
    public List<GroupObjectTreeViewInfomation> m_Children = new List<GroupObjectTreeViewInfomation>();
    public GroupObjectTreeViewInfomation m_Parent = null;
    private int m_Depth;
    public int depth { get { return m_Depth; } set { m_Depth = value; } }

    [SerializeField]
    public bool BaseParentExists
    {
        get { return m_Parent != null; }
    }

    public GameObject m_GameObject = null;

    public GroupObjectTreeViewInfomation parent
    {
        get { return m_Parent; }
        set { m_Parent = value; }
    }

    public List<GroupObjectTreeViewInfomation> children
    {
        get { return m_Children; }
        set { m_Children = value; }
    }

    public bool hasChildren
    {
        get { return children != null && children.Count > 0; }
    }

    public string m_FunctionName;

    public string name
    {
        get { return m_FunctionName; }
        set { m_FunctionName = value; }
    }

    public GroupObjectTreeViewInfomation(string name, int depth, int id)
    {
        m_Id = id;
        m_FunctionName = name;
        m_Depth = depth;
    }
}
