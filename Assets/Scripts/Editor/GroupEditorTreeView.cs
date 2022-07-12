using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.IMGUI.Controls;

public class GroupEditorTreeViewItem<T> : UnityEditor.IMGUI.Controls.TreeViewItem where T : GroupTreeInformation
{
    public T data { get; set; }

    public GroupEditorTreeViewItem(int id, int depth, string displayName, T data) : base(id, depth, displayName)
    {
        this.data = data;
    }
}
public class GroupEditorTreeView<T> : UnityEditor.IMGUI.Controls.TreeView where T : GroupTreeInformation
{
    GroupTreeModel<T> m_TreeModel;
    List<UnityEditor.IMGUI.Controls.TreeViewItem> m_Rows = new List<TreeViewItem>();
    public event Action treeChanged;
    public event Action<int> elementSelected;
    public GroupTreeModel<T> treeModel { get { return m_TreeModel; } }

    public GroupEditorTreeView(TreeViewState state, GroupTreeModel<T> model) : base(state)
    {
        Init(model);
    }

    public GroupEditorTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader, GroupTreeModel<T> model) : base(state, multiColumnHeader)
    {
        Init(model);
    }

    void Init(GroupTreeModel<T> model)
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

        return m_TreeModel.root == null ? null : new GroupEditorTreeViewItem<T>(id:0, depth:depthForHiddenRoot, "root", m_TreeModel.root);
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
            var item = new GroupEditorTreeViewItem<T>(child.id, depth, child.name, child);
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
        return true;
    }

    protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
    {
        UnityEditor.DragAndDrop.PrepareStartDrag();
        string[] draggedItemIdArray = args.draggedItemIDs.Select(i => i.ToString()).ToArray();
        UnityEditor.DragAndDrop.paths = draggedItemIdArray;
        List<GroupTreeInformation> draggedItems = new List<GroupTreeInformation>();
        foreach (int index in args.draggedItemIDs)
        {
            draggedItems.Add(m_TreeModel.Find(index));
        }
        UnityEditor.DragAndDrop.SetGenericData("ListViewEditor:ItemInfos", draggedItems);
        string title = draggedItems.Count == 1 ? draggedItems[0].GetTitle() : "< Multiple >";
        UnityEditor.DragAndDrop.StartDrag(title);
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
