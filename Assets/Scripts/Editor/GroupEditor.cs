using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine.SceneManagement;

public class GroupEditor : EditorWindow
{
    private Vector2 m_GroupScrollPosition;
    private Vector2 m_ParameterScrollPosition;

    GroupObjectTreeView<GroupObjectTreeViewInfomation> m_GroupObjectTreeView;
    const string kGroupObjectChangeSessionStateKeyPrefix = "GO_TVS";

    class GroupTreeView : GroupEditorTreeView<GroupTreeInformation>
    {
        public GroupTreeView(TreeViewState state, GroupTreeModel<GroupTreeInformation> model) : base(state, model)
        {
            showBorder = true;
            showAlternatingRowBackgrounds = true;
        }
    }

    public const int GROUP_TREE_DRAG_AND_DROP_AREA_HIEGHT = 200;

    const string kGroupEditorSessionStateKeyPrefix = "GTVS";

    private GUIStyle DragAndDropAreaLabelStyle = new GUIStyle();

    GroupTreeView m_GroupTreeView;

    private Rect GroupDropArea;

    private GroupObjectInformationBehaviour m_GroupObjectInformationBehaviour = null;

    [MenuItem("Tools/GroupEditor", false, 50)]
    public static void OpenGroupEditor()
    {
        GetWindow<GroupEditor>();
    }

    private void OnEnable()
    {
        GameObject systemObject = GetSystemObject();
        m_GroupObjectInformationBehaviour = systemObject.GetComponent<GroupObjectInformationBehaviour>();
        if (m_GroupObjectInformationBehaviour == null)
        {
            m_GroupObjectInformationBehaviour = systemObject.AddComponent<GroupObjectInformationBehaviour>();
        }

        OnEnableGroupListView();
        OnEnableGroupObjectListView();
    }

    private void OnEnableGroupListView()
    {
        var gTreeViewState = new TreeViewState();
        var gJsonState = SessionState.GetString(kGroupEditorSessionStateKeyPrefix + this.GetInstanceID(), "");
        if (!string.IsNullOrEmpty(gJsonState))
        {
            JsonUtility.FromJsonOverwrite(gJsonState, gTreeViewState);
        }

        GroupTreeInformation objRoot = new GroupTreeInformation("Root", -1, 0);
        objRoot.parent = null;
        List<GroupTreeInformation> m_ObjData = new List<GroupTreeInformation>();
        m_ObjData.Add(objRoot);

        GroupTreeModel<GroupTreeInformation> groupTreeModel = new GroupTreeModel<GroupTreeInformation>(m_ObjData);
        m_GroupTreeView = new GroupTreeView(gTreeViewState, groupTreeModel);
        m_GroupTreeView.elementSelected += OnGroupSelected;
        ReloadGroupTreeInfo();

        if (groupTreeModel.Count > 1)
        {
            m_GroupTreeView.Reload();
        }
    }

    private void OnEnableGroupObjectListView()
    {
        var goTreeViewstate = new TreeViewState();
        var goJsonState = SessionState.GetString(kGroupObjectChangeSessionStateKeyPrefix + this.GetInstanceID(), "");
        if (string.IsNullOrEmpty(goJsonState))
        {
            JsonUtility.FromJsonOverwrite(goJsonState, goTreeViewstate);
        }

        GroupObjectTreeViewInfomation objRoot = new GroupObjectTreeViewInfomation("Root", -1, 0);
        objRoot.parent = null;
        List<GroupObjectTreeViewInfomation> m_objData = new List<GroupObjectTreeViewInfomation>();
        m_objData.Add(objRoot);
        var objTreeModel = new GroupObjectTreeViewModel<GroupObjectTreeViewInfomation>(m_objData);
        m_GroupObjectTreeView = new GroupObjectTreeView<GroupObjectTreeViewInfomation>(goTreeViewstate, objTreeModel);
        m_GroupObjectTreeView.elementSelected += OnGroupObjectSelected;
        if (objTreeModel.Count > 1)
        {
            m_GroupObjectTreeView.Reload();
        }
    }

    private void OnGroupSelected(int obj)
    {
        GroupTreeInformation[] selection = GetSelectedGroupInfos().ToArray();
        if (selection.Length > 0)
        {
            GroupTreeInformation grouptree_info = selection[0];
            ReloadGroupObjectTreeElements(grouptree_info);
        }
    }

    List<GroupTreeInformation> GetSelectedGroupInfos()
    {
        List<GroupTreeInformation> selectedObjects = new List<GroupTreeInformation>();
        var selection = m_GroupTreeView.GetSelection();
        foreach (int i in selection)
        {
            GroupTreeInformation info = m_GroupTreeView.treeModel.Find(i);
            selectedObjects.Add(info);
        }
        return selectedObjects;
    }

    private void OnGroupObjectSelected(int obj)
    {
        GameObject[] selection = GetSelectedObjects().ToArray();
        if (selection.Length > 0)
        {
            Selection.objects = selection;
        }
    }

    private List<GameObject> GetSelectedObjects()
    {
        List<GameObject> selectObjects = new List<GameObject>();
        var selection = m_GroupObjectTreeView.GetSelection();
        foreach (int i in selection)
        {
            GroupObjectTreeViewInfomation info = m_GroupObjectTreeView.treeModel.Find(i);
            if (info.m_GameObject != null)
            {
                selectObjects.Add(info.m_GameObject);
            }
        }
        return selectObjects;
    }

    private void ReloadGroupTreeInfo()
    {
        m_GroupTreeView.treeModel.RemoveElements();
        for (int i = 1; i <= m_GroupObjectInformationBehaviour.m_GroupList.Count; i++)
        {
            GameObjectGroupInformation objinfo = m_GroupObjectInformationBehaviour.m_GroupList[i - 1];
            GroupTreeInformation groupTreeInfo = new GroupTreeInformation(objinfo.GetTitle(), 0, i);
            foreach (GameObjectWithPath go in objinfo.m_ObjectList)
            {
                if (string.IsNullOrEmpty(go.m_Name))
                {
                    go.m_Name = go.m_GameObject.name;
                }
                if (string.IsNullOrEmpty(go.m_Path))
                {
                    go.m_Path = GetHierarchyPath(go.m_GameObject.transform);
                }
                groupTreeInfo.AddObject(go.m_GameObject);
            }
            m_GroupTreeView.treeModel.AddElement(groupTreeInfo, m_GroupTreeView.treeModel.root, -1);
        }
        m_GroupTreeView.Reload();
    }

    private void ReloadGroupObjectTreeElements(GroupTreeInformation grouptree_info)
    {
        m_GroupObjectTreeView.treeModel.RemoveElements();
        for (int i = 1; i <= grouptree_info.m_ObjectList.Count; i++)
        {
            GameObject go = grouptree_info.m_ObjectList[i - 1].m_GameObject;
            GroupObjectTreeViewInfomation info = new GroupObjectTreeViewInfomation(go.name, 0, i);
            info.m_GameObject = go;
            m_GroupObjectTreeView.treeModel.AddElement(info, m_GroupObjectTreeView.treeModel.root, -1);
        }
        m_GroupObjectTreeView.Reload();

        m_GroupTreeView.Reload();
    }

    void RemoveSelectedTreeElements(GroupTreeInformation grouptree_info, IList<int> selection)
    {
        List<GameObjectWithPath> removeObjectList = new List<GameObjectWithPath>();
        foreach (int index in selection)
        {
            GroupObjectTreeViewInfomation info = m_GroupObjectTreeView.treeModel.Find(index);
            GameObjectWithPath go = grouptree_info.m_ObjectList.Where(element => element.m_GameObject.Equals(info.m_GameObject)).FirstOrDefault();
            if (go != null)
            {
                removeObjectList.Add(go);
            }
        }
        foreach (GameObjectWithPath obj in removeObjectList)
        {
            grouptree_info.m_ObjectList.Remove(obj);
        }
    }

    public void OnGUI()
    {
        using (GUILayout.ScrollViewScope group_scroll = new GUILayout.ScrollViewScope(m_GroupScrollPosition, EditorStyles.helpBox, GUILayout.ExpandHeight(true)))
        {
            m_GroupScrollPosition = group_scroll.scrollPosition;
            GUIStyle style = new GUIStyle();
            style.margin = new RectOffset(10, 2, 2, 2);
            style.alignment = TextAnchor.MiddleLeft;
            style.fontSize = EDITOR_TITLE_FONT_SIZE;
            style.normal.textColor = Color.white;

            GUILayout.Space(SPACE_HEIGHT * 2);
            GUILayout.Label("Create Group", style);
            GUILayout.Space(SPACE_HEIGHT * 2);
            drawSeparatorLine();
            if (m_GroupTreeView == null || m_GroupTreeView.treeModel.Count < 1) return;
            const float topToolbarHeight = 20f;
            const float spacing = 2f;
            float totalHeight = m_GroupTreeView.totalHeight + topToolbarHeight + 2 * spacing;
            Rect rect = GUILayoutUtility.GetRect(0, 10000, 0, totalHeight);
            Rect toolbarRect = new Rect(rect.x, rect.y, rect.width, topToolbarHeight);
            Rect multiColumnTreeViewRect = new Rect(rect.x, rect.y + spacing, rect.width, rect.height - 2 * spacing);
            DoTreeView(multiColumnTreeViewRect);
        }
        GUILayout.Space(5f);
        ToolBar();
        GUILayout.Space(5f);
        using (GUILayout.ScrollViewScope param_scroll = new GUILayout.ScrollViewScope(m_ParameterScrollPosition, EditorStyles.helpBox, GUILayout.ExpandWidth(true)))
        {
            m_ParameterScrollPosition = param_scroll.scrollPosition;
            var selection = m_GroupTreeView.GetSelection();
            if (selection.Count > 0)
            {
                GroupTreeInformation grouptree_info = m_GroupTreeView.treeModel.Find(selection[0]);
                if (grouptree_info != null)
                {
                    using (var check = new EditorGUI.ChangeCheckScope())
                    {
                        string getStr = EditorGUILayout.TextField("GroupName", grouptree_info.GetTitle(), GUILayout.ExpandWidth(true));
                        if (check.changed)
                        {
                            if (!grouptree_info.GetTitle().Equals(getStr))
                            {
                                grouptree_info.SetTitle(getStr);
                            }
                            m_GroupTreeView.Reload();
                            updateGroupInfo(grouptree_info);
                        }
                    }

                    EditorGUILayout.LabelField("ObjectEntry", GUI.skin.box, GUILayout.ExpandWidth(true));
                    GroupDropArea = GUILayoutUtility.GetRect(0f, GROUP_TREE_DRAG_AND_DROP_AREA_HIEGHT, GUILayout.ExpandWidth(true));
                    HandlingDragAndDrop("Drag & Drop Entry", grouptree_info, GroupDropArea);
                    if (grouptree_info.m_ObjectList.Count == 0 && m_GroupObjectTreeView.treeModel.Count == 1)
                    {
                        GUI.Box(GroupDropArea, "Drag & Drop Entry", DragAndDropAreaLabelStyle);
                    }
                    else
                    {
                        if (m_GroupObjectTreeView.treeModel.Count == 1)
                        {
                            ReloadGroupObjectTreeElements(grouptree_info);
                        }
                        if (m_GroupObjectTreeView.treeModel.Count > 1)
                        {
                            m_GroupObjectTreeView.OnGUI(GroupDropArea);
                        }
                    }
                    EditorGUI.BeginDisabledGroup(grouptree_info.m_ObjectList.Count == 0);
                    if (GUILayout.Button("Remove", GUILayout.ExpandWidth(true)))
                    {
                        var groupObjectSelection = m_GroupObjectTreeView.GetSelection();
                        if (groupObjectSelection.Count > 0)
                        {
                            RemoveSelectedTreeElements(grouptree_info, groupObjectSelection);
                            m_GroupObjectTreeView.treeModel.RemoveElements(groupObjectSelection);
                            updateGroupInfo(grouptree_info);
                            m_GroupObjectTreeView.Reload();
                            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
                        }
                    }
                    EditorGUI.EndDisabledGroup();
                }
            }
        }

    }

    void HandlingDragAndDrop(string dropAreaLabel, GroupTreeInformation grouptree_info, Rect dropArea)
    {
        if (Event.current == null) return;
        switch (Event.current.type)
        {
            case EventType.DragUpdated:
            case EventType.DragPerform:
                if (!dropArea.Contains(Event.current.mousePosition))
                {
                    break;
                }

                DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
                if (Event.current.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    foreach (GameObject gameObject in DragAndDrop.objectReferences)
                    {
                        GameObjectWithPath exists_go = grouptree_info.m_ObjectList.Where(element => element.m_GameObject.Equals(gameObject)).FirstOrDefault();
                        if (exists_go == null)
                        {
                            grouptree_info.m_ObjectList.Add(new GameObjectWithPath(gameObject));

                            int id = m_GroupObjectTreeView.treeModel.GenerateUniqueID();
                            GroupObjectTreeViewInfomation info = new GroupObjectTreeViewInfomation(gameObject.name, 0, id);
                            info.m_GameObject = gameObject;
                            m_GroupObjectTreeView.treeModel.AddElement(info, m_GroupObjectTreeView.treeModel.root, -1);
                            m_GroupObjectTreeView.Reload();
                        }
                    }
                    Event.current.Use();
                    m_GroupTreeView.Reload();
                    updateGroupInfo(grouptree_info);
                    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
                }
                Event.current.Use();
                break;
            case EventType.DragExited:
                if (!dropArea.Contains(Event.current.mousePosition))
                {
                    break;
                }
                Event.current.Use();
                break;
        }
    }

    const int EDITOR_BUTTON_HEIGHT = 30;
    const int EDITOR_TITLE_FONT_SIZE = 13;
    const int MARGIN = 5;
    const int SPACE_HEIGHT = 3;
    const int SEPARATOR_HEIGHT = 1;
    const int INDENT_WIDTH = 16;

    private void ToolBar()
    {
        GUIStyle style = GUI.skin.button;
        style.padding = EditorStyles.toolbarButton.padding;
        style.fixedHeight = EDITOR_BUTTON_HEIGHT;
        style.margin = new RectOffset(4, 4, 4, 4);
        style.alignment = TextAnchor.MiddleCenter;
        style.fontSize = EDITOR_TITLE_FONT_SIZE;
        style.normal.textColor = Color.white;
        var selection = m_GroupTreeView.GetSelection();
        bool NotFocusAndNotSelected = (!m_GroupTreeView.HasFocus() || selection.Count == 0);
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("New", style))
            {
                int newNumber = ++m_GroupObjectInformationBehaviour.m_IdGenerator;
                GameObjectGroupInformation objinfo = new GameObjectGroupInformation(string.Format("Group_{0}", newNumber), newNumber);
                m_GroupObjectInformationBehaviour.m_GroupList.Add(objinfo);
                GroupTreeInformation groupTreeinfo = new GroupTreeInformation(objinfo.GetTitle(), 0, newNumber);
                m_GroupTreeView.treeModel.AddElement(groupTreeinfo, m_GroupTreeView.treeModel.root, -1);
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            }
            EditorGUI.BeginDisabledGroup(NotFocusAndNotSelected);
            if (GUILayout.Button("Remove", style))
            {
                List<GroupTreeInformation> removeList = new List<GroupTreeInformation>();
                foreach (int index in selection)
                {
                    GroupTreeInformation groupTreeinfo = m_GroupTreeView.treeModel.Find(index);
                    removeList.Add(groupTreeinfo);
                    GameObjectGroupInformation objinfo = m_GroupObjectInformationBehaviour.m_GroupList.Where(element => element.m_Id == groupTreeinfo.m_Id).FirstOrDefault();
                    if (objinfo != null)
                    {
                        m_GroupObjectInformationBehaviour.m_GroupList.Remove(objinfo);
                    }
                }
                m_GroupTreeView.treeModel.RemoveElements(removeList);
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            }
            EditorGUI.EndDisabledGroup();
        }
    }

    private void updateGroupInfo(GroupTreeInformation updateinfo)
    {
        foreach (GameObjectGroupInformation info in m_GroupObjectInformationBehaviour.m_GroupList)
        {
            if (info.m_Id == updateinfo.m_Id)
            {
                info.m_Title = updateinfo.m_Title;
                info.m_ObjectList.Clear();
                foreach (GameObjectWithPath obj in updateinfo.m_ObjectList)
                {
                    info.m_ObjectList.Add(obj);
                }
            }
        }
    }

    void DoTreeView(Rect rect)
    {
        m_GroupTreeView.OnGUI(rect);
    }

    public void drawSeparatorLine()
    {
        Rect drawArea = GUILayoutUtility.GetRect(0.0f, SEPARATOR_HEIGHT, GUILayout.ExpandWidth(true));

        Vector2 pointA = new Vector2(drawArea.x, drawArea.y);
        Vector2 pointB = new Vector2(drawArea.x + drawArea.width, drawArea.y);
        DrawLine(pointA, pointB);
    }

    public static Texture2D lineTex;

    public void DrawLine(Vector2 pointA, Vector2 pointB)
    {
        DrawLine(pointA, pointB, GUI.contentColor, 1.0f);
    }

    public void DrawLine(Vector2 pointA, Vector2 pointB, Color color, float width)
    {
        pointA.x = (int)pointA.x;
        pointA.y = (int)pointA.y;
        pointB.x = (int)pointB.x;
        pointB.y = (int)pointB.y;

        if (!lineTex) { lineTex = new Texture2D(1, 1); }
        Color saveColor = GUI.color;
        GUI.color = color;

        Matrix4x4 matrixBaclup = GUI.matrix;

        float angle = Mathf.Atan2(pointB.y - pointA.y, pointB.x - pointA.x) * 180f / Mathf.PI;
        float length = (pointA - pointB).magnitude;
        GUIUtility.RotateAroundPivot(angle, pointA);
        GUI.DrawTexture(new Rect(pointA.x, pointA.y, length, width), lineTex);

        GUI.matrix = matrixBaclup;
        GUI.color = saveColor;
    }

    public GameObject GetSystemObject()
    {
        string SceneName;
        Scene scene;
        if (Application.isPlaying)
        {
            SceneName = SceneManager.GetActiveScene().name;
            scene = SceneManager.GetSceneByName(SceneName);
            return FindRootGameObject(scene, "SystemObject");
        }
        else
        {
#if UNITY_EDITOR
            if (UnityEditor.SceneManagement.EditorSceneManager.loadedRootSceneCount != 0)
            {
                scene = UnityEditor.SceneManagement.EditorSceneManager.GetSceneAt(0);
                return FindRootGameObject(scene, "SystemObject");
            }
            else
            {
                return GameObject.Find("SystemObject");
            }
#else
            return GameObject.Find("SystemObject");
#endif
        }
    }

    public GameObject FindRootGameObject(Scene scene, string findObjectName)
    {
        if (Application.isPlaying)
        {
            GameObject[] rootObjects = scene.GetRootGameObjects();
            foreach (GameObject gameObject in rootObjects)
            {
                if (gameObject.name.Equals(findObjectName))
                {
                    return gameObject;
                }
            }
        }
        else
        {
#if UNITY_EDITOR
            GameObject[] rootObjects = scene.GetRootGameObjects();
            foreach (GameObject gameObject in rootObjects)
            {
                if (gameObject.name.Equals(findObjectName))
                {
                    return gameObject;
                }
            }
#else
            return GameObject.Find(findObjectName);
#endif
        }
        return null;
    }

    private string GetHierarchyPath(Transform obj)
    {
        var path = obj.gameObject.name;
        var parent = obj.parent;
        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        return path;
    }
}
