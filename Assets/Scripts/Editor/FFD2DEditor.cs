using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FFD2D))]
public class FFD2DEditor : Editor 
{
    FFD2D m_target;
    static bool _editing;

    private void OnEnable() {
        m_target = target as FFD2D;
    }
    private void OnDisable() {
        _editing = false;
    }
    public override void OnInspectorGUI()
    {
        if(GUILayout.Button(_editing ? "Stop editing" : "Edit mode"))
            ToggleEditMode();

        base.OnInspectorGUI();
        
    }

    Tool cachedTool;
    void ToggleEditMode()
    {
        _editing = !_editing;

        if(_editing)
        {
            cachedTool = Tools.current;
            Tools.current = Tool.None;
        }
        else
        {
            Tools.current = cachedTool;
        }
        SceneView.RepaintAll();
    }

    struct GridCorners
    {
        public Vector3 topLeft, topRight, bottomLeft, bottomRight;

        public void UpdateCorners(FFD2D target)
        {
            topLeft = target.WorldPoint(new Vector3(-1,1));
            topRight = target.WorldPoint(new Vector3(1,1));
            bottomLeft = target.WorldPoint(new Vector3(-1,-1));
            bottomRight = target.WorldPoint(new Vector3(1,-1));
        }
    }
    GridCorners corners = new GridCorners();

    Vector3[][] cachedPointsWS;

    void DrawGrid()
    {

        //Handles.color = new Color(1,1,1, 0.5f);
        //DrawLineHor(0.5f);
        //DrawLineVer(0.5f);



        for (int x = 0; x < m_target.cachedPoints.Length; x++)
        {
            Handles.color = new Color(1,1,1, 0.5f);
            DrawLineHor(x);
            for (int y = 0; y < m_target.cachedPoints[0].Length; y++)
            {
                DrawLineVer(y);
                Handles.color = Color.green;
                if(x != m_target.cachedPoints.Length - 1)
                    Handles.DrawLine(cachedPointsWS[x][y], cachedPointsWS[x+1][y]);

                if(y != m_target.cachedPoints[0].Length - 1)
                    Handles.DrawLine(cachedPointsWS[x][y], cachedPointsWS[x][y+1]);
            }
        }

    }

    void OnSceneGUI()
    {
        corners.UpdateCorners(m_target);
        m_target.CachePoints();
        cachedPointsWS = new Vector3[m_target.cachedPoints.Length][];
        for (int x = 0; x < m_target.cachedPoints.Length; x++)
        {
            cachedPointsWS[x] = new Vector3[m_target.cachedPoints[x].Length];
            for (int y = 0; y < cachedPointsWS[0].Length; y++)
            {
                // TODO: have I buggered up the x and y order?
                Vector3 gridPos = new Vector3( 
                    GridToScalar(x, m_target.cells), GridToScalar(y, m_target.cells)
                );
                cachedPointsWS[x][y] = 
                    m_target.transform.TransformPoint(
                        m_target.cachedPoints[x][y] + gridPos
                    );
            }
        }
        Edit();
        DrawGrid();

    }

    void Edit()
    {
        if(!_editing)
            return;

        EditorGUI.BeginChangeCheck();
        cachedPointsWS[1][1] = 
            Handles.PositionHandle(cachedPointsWS[1][1], m_target.transform.rotation);
        if(EditorGUI.EndChangeCheck())
        {
            Vector2 offset = m_target.transform.InverseTransformPoint(cachedPointsWS[1][1]);
            serializedObject.FindProperty("offset").vector2Value = offset;
            serializedObject.ApplyModifiedProperties();

            m_target.Set();
        }

    }

    float GridToScalar(int v, float grid)
    {
        return (v / (grid - 1)) * 2 - 1;
    }

    void DrawLineHor(int y)
    {
        DrawLineHor(y / (m_target.cells - 1.0f));
    }
    void DrawLineHor(float y)
    {
        Vector3 p0 = Vector3.Lerp(corners.topLeft, corners.bottomLeft, y);
        Vector3 p1 = Vector3.Lerp(corners.topRight, corners.bottomRight, y);
        
        Handles.DrawDottedLine(p0, p1, 5);
    }
    void DrawLineVer(int x)
    {
        DrawLineVer(x / (m_target.cells - 1.0f));
    }
    void DrawLineVer(float x)
    {
        Vector3 p0 = Vector3.Lerp(corners.topLeft, corners.topRight, x);
        Vector3 p1 = Vector3.Lerp(corners.bottomLeft, corners.bottomRight, x);
        
        Handles.DrawDottedLine(p0, p1, 5);
    }

}
