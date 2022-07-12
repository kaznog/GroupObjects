using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupTreeModel<T> where T : GroupTreeInformation
{
    public void TreeToList<T>(T root, IList<T> result) where T : GroupTreeInformation
    {
        if (result == null)
        {
            throw new NullReferenceException("The input 'IList<T> result' list is null");
        }

        result.Clear();

        Stack<T> stack = new Stack<T>();
        stack.Push(root);

        while (stack.Count > 0)
        {
            T current = stack.Peek();
            result.Add(current);
            stack.Pop();
            if (current == null) return;

            if (current.children != null && current.children.Count > 0)
            {
                for (int i = current.children.Count - 1; i >= 0; i--)
                {
                    stack.Push((T)current.children[i]);
                }
            }
        }
    }

    public T ListToTree<T>(IList<T> list) where T : GroupTreeInformation
    {
        // Validate input
        ValidateDepthValues(list);

        // Clear old states
        foreach (var element in list)
        {
            element.parent = null;
            element.children = null;
        }

        // Set Child and parent preferences using depth info
        for (int parentIndex = 0; parentIndex < list.Count; parentIndex++)
        {
            var parent = list[parentIndex];
            bool alreadyHasValidChildren = parent.children != null;
            if (alreadyHasValidChildren)
                continue;

            int parentDepth = parent.depth;
            int childCount = 0;

            // Count Children based depth value. we are looking at children until it's the same depth as this object
            for (int i = parentIndex + 1; i < list.Count; i++)
            {
                if (list[i].depth == parentDepth + 1)
                    childCount++;
                if (list[i].depth <= parentDepth)
                    break;
            }

            // Fill child array
            List<GroupTreeInformation> childList = null;
            if (childCount != 0)
            {
                childList = new List<GroupTreeInformation>(childCount);    // Allocate one
                childCount = 0;
                for (int i = parentIndex + 1; i < list.Count; i++)
                {
                    if (list[i].depth == parentDepth + 1)
                    {
                        list[i].parent = parent;
                        childList.Add(list[i]);
                        childCount++;
                    }

                    if (list[i].depth <= parentDepth)
                    {
                        break;
                    }
                }
            }

            parent.children = childList;
        }
        return list[0];
    }

    public void ValidateDepthValues<T>(IList<T> list) where T : GroupTreeInformation
    {
        if (list.Count == 0)
        {
            throw new ArgumentException("list should have items, count is 0, check before calling ValidateDepthValues", "list");
        }

        if (list[0].depth != -1)
        {
            throw new ArgumentException("list item at index 0 should have a depth of -1 (since this should be the hidden root of the tree). Depth is: " + list[0].depth, "list");
        }

        for (int i = 0; i < list.Count - 1; i++)
        {
            int depth = list[i].depth;
            int nextDepth = list[i + 1].depth;
            if (nextDepth > depth && nextDepth - depth > 1)
            {
                throw new ArgumentException(string.Format("Invalid depth info in input list. Depth cannot increase more than 1 per row. Index {0} has depth {1} while index {2} has depth {3}", i, depth, i + 1, nextDepth));
            }
        }

        for (int i = 1; i < list.Count; i++)
        {
            if (list[i].depth < 0)
            {
                throw new ArgumentException("Invalid depth value for item at index " + i + ". Only the first item (the root) should have depth below 0.");
            }
        }

        if (list.Count > 1 && list[1].depth != 0)
        {
            throw new ArgumentException("Input list item at index 1 is assumed to have a depth of 0", "list");
        }
    }

    public void UpdateDepthValues<T>(T root) where T : GroupTreeInformation
    {
        if (root == null)
        {
            throw new ArgumentNullException("root", "This root is null");
        }

        if (!root.hasChildren)
            return;

        Stack<GroupTreeInformation> stack = new Stack<GroupTreeInformation>();
        stack.Push(root);

        try
        {
            while (stack.Count > 0)
            {
                GroupTreeInformation current = stack.Pop();
                if (current.children == null)
                {
                    foreach (var child in current.children)
                    {
                        child.depth = current.depth + 1;
                        stack.Push(child);
                    }
                }
            }
        }
        catch (InvalidOperationException ioe)
        {
            Debug.Log(ioe);
        }
    }

    public bool IsChildOf<T>(T child, IList<T> elements) where T : GroupTreeInformation
    {
        while (child != null)
        {
            child = (T)child.parent;
            if (elements.Contains(child))
                return true;
        }
        return false;
    }

    public IList<T> FindCommonAncestorsWithinList<T>(IList<T> elements) where T : GroupTreeInformation
    {
        if (elements.Count == 1)
            return new List<T>(elements);

        List<T> result = new List<T>(elements);
        result.RemoveAll(g => IsChildOf(g, elements));
        return result;
    }

    IList<T> m_Data;
    T m_Root;
    int m_MaxID;

    public T root { get { return m_Root; } set { m_Root = value; } }
    public event Action modelChanged;
    public int numberOfDataElements
    {
        get { return m_Data.Count; }
    }

    public GroupTreeModel(IList<T> data)
    {
        SetData(data);
    }

    public void SetData(IList<T> data)
    {
        Init(data);
    }

    public void Init(IList<T> data)
    {
        if (data == null)
        {
            throw new ArgumentNullException("data", "Input data is null. Ensure input is a non-null list.");
        }

        m_Data = data;
        if (m_Data.Count > 0)
        {
            m_Root = ListToTree(data);
            m_MaxID = m_Data.Max(e => e.id);
        }
        else
        {
            m_MaxID = 0;
        }
    }

    public int Count
    {
        get { return m_Data.Count; }
    }

    public T Find(int id)
    {
        return m_Data.FirstOrDefault(element => element.id == id);
    }

    public T At(int index)
    {
        return m_Data[index];
    }

    public int GenerateUniqueID()
    {
        return ++m_MaxID;
    }

    public IList<int> GetAncestors(int id)
    {
        var parents = new List<int>();
        GroupTreeInformation T = Find(id);
        if (T != null)
        {
            while (T.parent != null)
            {
                parents.Add(T.parent.id);
                T = T.parent;
            }
        }
        return parents;
    }

    public IList<int> GetDescendantsThatHaveChildren(int id)
    {
        T searchFromThis = Find(id);
        if (searchFromThis != null)
        {
            return GetParentsBelowStackBased(searchFromThis);
        }
        return new List<int>();
    }

    public IList<int> GetParentsBelowStackBased(GroupTreeInformation searchFromThis)
    {
        Stack<GroupTreeInformation> stack = new Stack<GroupTreeInformation>();
        stack.Push(searchFromThis);

        var parentBelow = new List<int>();
        while (stack.Count > 0)
        {
            GroupTreeInformation current = stack.Pop();
            if (current.hasChildren)
            {
                parentBelow.Add(current.id);
                foreach (var T in current.children)
                {
                    stack.Push(T);
                }
            }
        }

        return parentBelow;
    }

    public void RemoveElements()
    {
        IList<T> elements = m_Data.Where(element => element.depth > -1).ToList();
        RemoveElements(elements);
    }

    public void RemoveElements(IList<int> elementIDs)
    {
        IList<T> elements = m_Data.Where(element => elementIDs.Contains(element.id)).ToArray();
        RemoveElements(elements);
    }

    public void RemoveElements(IList<T> elements)
    {
        foreach (var element in elements)
        {
            if (element == m_Root)
            {
                throw new ArgumentException("it is not allowed to remove the root element");
            }
        }

        var commonAncestors = FindCommonAncestorsWithinList(elements);

        foreach (var element in commonAncestors)
        {
            element.parent.children.Remove(element);
            element.parent = null;
        }

        TreeToList(m_Root, m_Data);

        Changed();
    }

    public void AddElements(IList<T> elements, GroupTreeInformation parent, int insertPosition)
    {
        if (elements == null)
        {
            throw new ArgumentNullException("elements", "elements is null");
        }
        if (elements.Count == 0)
        {
            throw new ArgumentNullException("elements", "elements Count is 0: nothing to add");
        }
        if (parent == null)
        {
            throw new ArgumentNullException("parent", "parent is null");
        }

        if (parent.children == null)
        {
            parent.children = new List<GroupTreeInformation>();
        }

        parent.children.InsertRange(insertPosition, elements.Cast<GroupTreeInformation>());
        foreach (var element in elements)
        {
            element.parent = parent;
            element.depth = parent.depth + 1;
            UpdateDepthValues(element);
        }

        TreeToList(m_Root, m_Data);

        Changed();
    }

    public void AddRoot(T root)
    {
        if (root == null)
        {
            throw new ArgumentNullException("root", "root is null");
        }

        if (m_Data == null)
        {
            throw new InvalidOperationException("Internal Error: data list is null");
        }

        if (m_Data.Count != 0)
        {
            throw new InvalidOperationException("AddRoot is only allowed on empty data list");
        }

        root.id = GenerateUniqueID();
        root.depth = -1;
        m_Data.Add(root);
    }

    public void AddElement(T element, GroupTreeInformation parent, int insertPosition)
    {
        if (element == null)
        {
            throw new ArgumentNullException("element", "element is null");
        }
        if (parent == null)
        {
            throw new ArgumentNullException("parent", "parent is null");
        }
        if (parent.children == null)
        {
            parent.children = new List<GroupTreeInformation>();
        }

        if (insertPosition < 0)
        {
            m_Root.children.Add(element);
            element.parent = m_Root;
        }
        else
        {
            parent.children.Insert(insertPosition, element);
            element.parent = parent;
        }

        UpdateDepthValues(parent);
        TreeToList(m_Root, m_Data);

        Changed();
    }

    public void MoveElements(GroupTreeInformation parentElement, int insertionIndex, List<GroupTreeInformation> elements)
    {

    }

    public void Changed()
    {
        if (modelChanged != null)
            modelChanged();
    }
}
