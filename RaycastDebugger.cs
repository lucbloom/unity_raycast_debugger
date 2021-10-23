#if UNITY_EDITOR
using StoryGiant.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RaycastDebugger : EditorWindow
{
    PointerEventData m_PointerEventData;
    EventSystem m_EventSystem;
    List<GameObject> m_EventSystemResults = new List<GameObject>();
    List<GameObject> m_AllObjects = new List<GameObject>();
    //RaycastHit2D[] m_RaycastResults = new RaycastHit2D[0];
    Vector2 m_MeasuredMousePosition;
    GameObject m_SelectedGameObject;

    int m_UpdateWhen;
    bool m_IncludeInactive;
    //bool m_DrawAllRaycastTargets;
    //bool m_DrawRaycastTargetsUnderCursor;
    //bool m_DrawAllUnderCursor;

    bool m_AllObjectsFoldout = true;
    bool m_EventSystemHitsFoldout = true;

    bool m_IsUpdating;
    bool m_IsDrawing;

    [MenuItem("𝓢𝓽𝓸𝓻𝔂 𝓖𝓲𝓪𝓷𝓽/🛠 Tools/Raycast Debugger")]
    static void ShowWindow()
    {
        var window = CreateInstance<RaycastDebugger>();
        window.titleContent = new GUIContent("Raycast Debugger");
        window.UpdateHits(true);
        window.Show();
    }

    void OnHierarchyChange()
    {
        UpdateHits();
    }

    //void OnFocus()
    //{
    //    SceneView.duringSceneGui -= OnSceneGUI;
    //    SceneView.duringSceneGui += OnSceneGUI;
    //}
    //
    //void OnDestroy()
    //{
    //    SceneView.duringSceneGui -= OnSceneGUI;
    //}
    //
    //private void OnSceneGUI(SceneView obj)
    //{
    //    if (m_DrawRaycastTargetsUnderCursor || m_DrawAllUnderCursor)
    //    {
    //        Handles.BeginGUI();
    //        var ind = new int[] { 0, 1, 1, 2, 2, 3, 3, 0 };
    //        m_EventSystemResults.ForEach(go => {
    //            var rt = go.GetComponent<RectTransform>();
    //            //Handles.DrawLines(new Vector3[]{
    //            //    new Vector3(rt.rect.xMin, rt.rect.yMin, 0),
    //            //    new Vector3(rt.rect.xMax, rt.rect.yMin, 0),
    //            //    new Vector3(rt.rect.xMax, rt.rect.yMax, 0),
    //            //    new Vector3(rt.rect.xMin, rt.rect.yMax, 0),
    //            //}, ind);
    //            GUI.Box(rt.rect, "");
    //        });
    //        Handles.EndGUI();
    //    }
    //}

    private void OnGUI()
    {
        UpdateHits();

        m_IsDrawing = true;

        var updateOptions = new string[] { "Continuous", "Right mouse button is down", "Shift is down" };
        m_UpdateWhen = EditorGUILayout.Popup("Update when", m_UpdateWhen, updateOptions);
        m_IncludeInactive = EditorGUILayout.Toggle("Include inactive?", m_IncludeInactive);
        //m_DrawRaycastTargetsUnderCursor = EditorGUILayout.Toggle("Draw Raycast Targets?", m_DrawRaycastTargetsUnderCursor);
        //m_DrawAllUnderCursor = EditorGUILayout.Toggle("Draw all?", m_DrawAllUnderCursor);

        if (!EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorGUILayout.HelpBox("This window only works when the game is running.", MessageType.Info);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            if (GUILayout.Button("Run"))
            {
                EditorApplication.isPlaying = true;
            }
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();
        }
        else if (EditorApplication.isPlaying)
        {
            EditorGUILayout.LabelField($"Mouse position: [{Input.mousePosition.x}, {Input.mousePosition.y}]");

            EditorGUILayout.Space();

            EditorGUILayout.LabelField($"Selected GameObject:");
            EditorGUILayout.ObjectField(m_SelectedGameObject, typeof(GameObject), true);

            EditorGUILayout.Space();

            if (m_EventSystemHitsFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(m_EventSystemHitsFoldout, "EventSystem hits"))
            {
                if (m_EventSystemResults.Empty())
                {
                    EditorGUILayout.HelpBox("No EventSystem hits found.", MessageType.Info);
                }
                else
                {
                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    m_EventSystemResults.ForEach(go => {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.ObjectField(go, typeof(GameObject), true);
                        //if (m_IncludeInactive)
                        //{
                        //    if (EditorGUILayout.Toggle(go.activeSelf, GUILayout.Width(16)) != go.activeSelf) { go.SetActive(!go.activeSelf); }
                        //}
                        EditorGUILayout.EndHorizontal();
                    });
                    EditorGUILayout.EndVertical();
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            //if (m_RaycastResultsFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(m_RaycastResultsFoldout, "2D Raycast hits"))
            //{
            //    if (m_RaycastResults.Empty())
            //    {
            //        EditorGUILayout.HelpBox("No 2D Raycast targets found.", MessageType.Info);
            //    }
            //    else
            //    {
            //        EditorGUILayout.BeginVertical(GUI.skin.box);
            //        m_RaycastResults.ForEach(rcr => EditorGUILayout.ObjectField(rcr.transform, typeof(Transform), true));
            //        EditorGUILayout.EndVertical();
            //    }
            //}
            //EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.Space();

            if (m_AllObjectsFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(m_AllObjectsFoldout, "All Objects"))
            {
                if (m_AllObjects.Empty())
                {
                    EditorGUILayout.HelpBox("No objects found.", MessageType.Info);
                }
                else
                {
                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    m_AllObjects.ForEach(go => {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.ObjectField(go, typeof(Transform), true);
                        //if (m_IncludeInactive)
                        //{
                        //    if (EditorGUILayout.Toggle(go.activeSelf, GUILayout.Width(16)) != go.activeSelf) { go.SetActive(!go.activeSelf); }
                        //}
                        EditorGUILayout.EndHorizontal();
                    });
                    EditorGUILayout.EndVertical();
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        m_IsDrawing = false;
    }

    private bool ShouldUpdate()
    {
        return m_UpdateWhen == 0 ||
            (m_UpdateWhen == 1 && Input.GetMouseButton(1)) ||
            (m_UpdateWhen == 2 && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)));
    }

    private void Update()
    {
        if (WillUpdate())
        {
            var oldPos = m_MeasuredMousePosition;
            var oldList1 = m_EventSystemResults;
            var oldList2 = m_AllObjects;
            m_EventSystemResults = new List<GameObject>();
            m_AllObjects = new List<GameObject>();
            var oldSelectedGameObject = m_SelectedGameObject;
            UpdateHits();
            bool same =
                oldList1.Count == m_EventSystemResults.Count &&
                oldList2.Count == m_AllObjects.Count &&
                oldPos == m_MeasuredMousePosition &&
                m_SelectedGameObject == oldSelectedGameObject;
            if (same)
            {
                oldList1.For((i, go) => same &= go == m_EventSystemResults[i]);
            }
            if (same)
            {
                oldList2.For((i, go) => same &= go == m_AllObjects[i]);
            }
            if (!same)
            {
                Repaint();
            }
        }
    }

    public bool WillUpdate(bool force = false) => !m_IsUpdating && (ShouldUpdate() || force);

    public void UpdateHits(bool force = false)
    {
        if (!WillUpdate(force))
        {
            return;
        }

        m_IsUpdating = true;

        m_EventSystemResults.Clear();
        m_AllObjects.Clear();

        m_EventSystem = EventSystem.current;
        if (m_EventSystem)
        {
            //if (m_PointerEventData == null)
            {
                m_PointerEventData = new PointerEventData(m_EventSystem);
            }
            m_MeasuredMousePosition = Input.mousePosition;
            m_PointerEventData.position = m_MeasuredMousePosition;
            var tmp = new List<RaycastResult>();
            m_EventSystem.RaycastAll(m_PointerEventData, tmp);
            m_EventSystemResults.AddRange(tmp.Select(rcr => rcr.gameObject));
            m_SelectedGameObject = m_EventSystem.currentSelectedGameObject;

            //var rayPos = Camera.main.ScreenToWorldPoint(m_MeasuredMousePosition);
            //var ray = new Ray(rayPos, Vector2.zero);
            //m_RaycastResults = Physics2D.RaycastAll(rayPos, Vector2.left, LayerMask.);

            var allObjects = FindObjectsOfType<GameObject>().ToList();
            if (StageUtility.GetCurrentStageHandle() != StageUtility.GetMainStageHandle())
            {
                allObjects.AddRange(StageUtility.GetCurrentStageHandle().FindComponentsOfType<Transform>().Select(t => t.gameObject));
            }
            allObjects.ForEach(obj => {
                if (!m_IncludeInactive && !obj.activeInHierarchy)
                {
                    return;
                }

                var rectTransform = obj.GetComponent<RectTransform>();
                if (rectTransform)
                {
                    Vector2 localMousePosition = rectTransform.InverseTransformPoint(m_MeasuredMousePosition);
                    if (rectTransform.rect.Contains(localMousePosition))
                    {
                        m_AllObjects.Add(obj);
                        if (!m_EventSystemResults.Contains(obj))
                        {
                            var img = obj.GetComponent<Graphic>();
                            var btn = obj.GetComponent<Selectable>();
                            if ((img && img.raycastTarget) ||
                                (btn && btn.interactable))
                            {
                                m_EventSystemResults.Add(obj);
                            }
                        }
                    }
                }
                else
                {
                }
            });
        }

        m_IsUpdating = false;
    }
}
#endif // UNITY_EDITOR
