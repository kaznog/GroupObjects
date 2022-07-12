using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameObjectGroupInformation
{
    public int m_Id;
    public string m_Title;
    public void SetTitle(string title)
    {
        m_Title = title;
    }
    public string GetTitle()
    {
        return m_Title;
    }

    public List<GameObjectWithPath> m_ObjectList = new List<GameObjectWithPath>();

    public GameObjectGroupInformation()
    {

    }

    public GameObjectGroupInformation(string title, int id)
    {
        SetTitle(title);
        m_Id = id;
    }

    public void AddObject(GameObject obj)
    {
        bool isContain = false;
        for (int i = 0; i < GetObjCount(); i++)
        {
            if (GetObject(i).m_GameObject == obj)
            {
                isContain = true;
                break;
            }
        }

        if (isContain == false)
        {
            m_ObjectList.Add(new GameObjectWithPath(obj));
        }
    }

    public void RemoveObject(GameObjectWithPath obj)
    {
        m_ObjectList.Remove(obj);
    }

    public void RemoveObjectAt(int index)
    {
        m_ObjectList.Remove(m_ObjectList[index]);
    }

    public void RemoveObjectAt(List<int> indexs)
    {
        List<GameObjectWithPath> removeList = new List<GameObjectWithPath>();
        for (int i = 0; i < indexs.Count; i++)
        {
            removeList.Add(GetObject(indexs[i]));
        }

        for (int i = 0; i < removeList.Count; i++)
        {
            RemoveObject(removeList[i]);
        }
    }

    public int GetObjCount()
    {
        Refresh();
        return m_ObjectList.Count;
    }

    public GameObjectWithPath GetObject(int index)
    {
        return m_ObjectList[index];
    }

    public void Refresh()
    {
        for (int i = 0; i < m_ObjectList.Count; i++)
        {
            if (m_ObjectList[i] == null)
            {
                m_ObjectList.RemoveAt(i);
                i--;
            }
        }
    }
}
