using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.IMGUI.Controls;

public class GroupObjectTreeViewItem<T> : UnityEditor.IMGUI.Controls.TreeViewItem where T : GroupObjectTreeViewInfomation
{
    public T data { get; set; }

    public GroupObjectTreeViewItem(int id, int depth, string displayName, T data) : base(id, depth, displayName)
    {
        this.data = data;
    }
}

public class GroupObjectTreeView<T> : UnityEditor.IMGUI.Controls.TreeView where T : GroupObjectTreeViewInfomation
{
    GroupObjectTreeViewModel<T> m_TreeModel;
    List<UnityEditor.IMGUI.Controls.TreeViewItem> m_Rows = new List<TreeViewItem>();
    public event Action treeChanged;
    public event Action<int> elementSelected;
    public GroupObjectTreeViewModel<T> treeModel { get { return m_TreeModel; } }

    public GroupObjectTreeView(TreeViewState state, GroupObjectTreeViewModel<T> model) : base(state)
    {
        Init(model);
    }

    public GroupObjectTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader, GroupObjectTreeViewModel<T> model) : base(state, multiColumnHeader)
    {
        Init(model);
    }

    void Init(GroupObjectTreeViewModel<T> model)
    {
        m_TreeModel = model;
        m_TreeModel.modelChanged += ModelChanged;

        rowHeight = 26;
        showBorder = true;
        showAlternatingRowBackgrounds = true;
    }

    void ModelChanged()
    {
        if (treeChanged != null)
            treeChanged();

        Reload();
    }

    protected override void SelectionChanged(IList<int> selectedIds)
    {
        base.SelectionChanged(selectedIds);
        if (selectedIds.Count > 0)
        {
            if (elementSelected != null)
            {
                elementSelected(selectedIds[0]);
            }
        }
    }

    protected override UnityEditor.IMGUI.Controls.TreeViewItem BuildRoot()
    {
        int depthForHiddenRoot = -1;

        return m_TreeModel.root == null ? null : new GroupObjectTreeViewItem<T>(m_TreeModel.root.id, depthForHiddenRoot, m_TreeModel.root.name, m_TreeModel.root);
    }

    protected override IList<UnityEditor.IMGUI.Controls.TreeViewItem> BuildRows(UnityEditor.IMGUI.Controls.TreeViewItem root)
    {
        if (m_TreeModel.root == null)
        {
            Debug.Log("tree model root is null. did you call SetData()?");
        }

        m_Rows.Clear();
        if (m_TreeModel.root.hasChildren)
        {
            AddChildrenRecursive(m_TreeModel.root, 0, m_Rows);
        }

        SetupParentsAndChildrenFromDepths(root, m_Rows);

        return m_Rows;
    }

    void AddChildrenRecursive(T parent, int depth, IList<UnityEditor.IMGUI.Controls.TreeViewItem> newRows)
    {
        foreach (T child in parent.children)
        {
            Debug.Log(string.Format("AddChildrenRecursive id:{0}, deoth:{1}, name:{2}", child.id, child.depth, child.name));
            var item = new GroupObjectTreeViewItem<T>(child.id, depth, child.name, child);
            newRows.Add(item);

            if (child.hasChildren)
            {
                if (IsExpanded(child.id))
                {
                    AddChildrenRecursive(child, depth + 1, newRows);
                }
                else
                {
                    item.children = CreateChildListForCollapsedParent();
                }
            }
        }
    }

    protected override IList<int> GetAncestors(int id)
    {
        return m_TreeModel.GetAncestors(id);
    }

    protected override IList<int> GetDescendantsThatHaveChildren(int id)
    {
        return m_TreeModel.GetDescendantsThatHaveChildren(id);
    }

    protected override void RowGUI(RowGUIArgs args)
    {
        base.RowGUI(args);
    }

    protected override bool CanMultiSelect(TreeViewItem item)
    {
        return true;
    }

    protected override bool CanRename(TreeViewItem item)
    {
        return false;
    }

    protected override void RenameEnded(RenameEndedArgs args)
    {
    }

    protected override bool CanStartDrag(CanStartDragArgs args)
    {
        return false;
    }

    protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
    {
    }

    protected override bool CanBeParent(TreeViewItem item)
    {
        return false;
    }

    protected override bool CanChangeExpandedState(TreeViewItem item)
    {
        return false;
    }

    protected override bool DoesItemMatchSearch(TreeViewItem item, string search)
    {
        return false;
    }
}
