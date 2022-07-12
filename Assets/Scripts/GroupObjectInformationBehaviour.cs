using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupObjectInformationBehaviour : MonoBehaviour
{
    public int m_IdGenerator = 0;
    public List<GameObjectGroupInformation> m_GroupList = new List<GameObjectGroupInformation>();
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public GameObjectGroupInformation addGroup()
    {
        GameObjectGroupInformation info = new GameObjectGroupInformation("Group_" + m_IdGenerator, m_IdGenerator);
        m_GroupList.Add(info);
        m_IdGenerator++;
        return info;
    }

    public void deleteGroup(GameObjectGroupInformation info)
    {
        m_GroupList.Remove(info);
    }

    public bool isAliveGroup(GameObjectGroupInformation info)
    {
        bool alive = false;
        for (int i = 0; i < m_GroupList.Count; i++)
        {
            if (info == m_GroupList[i])
            {
                alive = true;
                break;
            }
        }
        return alive;
    }


}
