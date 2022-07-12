using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupTreeInformation : GameObjectGroupInformation
{
    public List<GroupTreeInformation> m_Children = new List<GroupTreeInformation>();
    public GroupTreeInformation m_Parent = null;

    public GroupTreeInformation parent
    {
        get { return m_Parent; }
        set { m_Parent = value; }
    }

    public List<GroupTreeInformation> children
    {
        get { return m_Children; }
        set { m_Children = value; }
    }

    public bool hasChildren
    {
        get { return children != null && children.Count > 0; }
    }

    public string name
    {
        get { return m_Title; }
        set { m_Title = value; }
    }

    public int id
    {
        get { return m_Id; }
        set { m_Id = value; }
    }

    int m_Depth;
    public int depth
    {
        get { return m_Depth; }
        set { m_Depth = value; }
    }

    public GroupTreeInformation(string name, int depth, int id) : base(name, id)
    {
        m_Depth = depth;
        parent = null;
    }
}
