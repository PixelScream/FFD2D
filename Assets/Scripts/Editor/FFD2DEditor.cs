using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FFD2D))]
public class FFD2DEditor : Editor 
{
    FFD2D m_target;
    static bool _editing;

    static int _indexX = -1, _indexY = -1;
    static bool[][] _selected;

    private void OnEnable() {
        m_target = target as FFD2D;

        CheckOffsetsSize();
        CreateSelectionArray();
        SetEditPoint();

        
    }

    void ResetOffsets()
    {

        Undo.RecordObject(m_target, "creating offsets for " + m_target.name);
        //serializedObject.FindProperty("offsets").arraySize = cells * cells;
        m_target.offsets = new Vector3[cells * cells];
        serializedObject.Update();
        SceneView.RepaintAll();
        SetFFD();
    }
    void CreateSelectionArray()
    {
        _indexX = _indexY = -1;
        _selected = new bool[cells][];
        for (int i = 0; i < cells; i++)
        {

            _selected[i] = new bool[cells];
        }
        SceneView.RepaintAll();
    }

    void RandomiseOffsets()
    {

        Undo.RecordObject(m_target, "Randomizing offsets " + m_target.name);

        for (int x = 0; x < cells; x++)
        {
            for (int y = 0; y < cells; y++)
            {
                Vector3 r = Random.insideUnitCircle * 0.1f;
                m_target.offsets[m_target.CoordToIndex(x,y)] += r;
            }
        }
        serializedObject.Update();
        SceneView.RepaintAll();
        SetFFD();
    }

    void CheckOffsetsSize()
    {
        if(!m_target.offsets.IsCorrectLength(cells * cells))
        {
            ResetOffsets();
        }
    }
    private void OnDisable() {
        _editing = false;
        _indexX = _indexY = -1;
    }
    public override void OnInspectorGUI()
    {
        if(GUILayout.Button(_editing ? "Stop editing" : "Edit mode"))
            ToggleEditMode();

        if(_editing)
        {
            if(GUILayout.Button("Randomize"))
                RandomiseOffsets();

            if(GUILayout.Button("Reset"))
                ResetOffsets();

            if(GUILayout.Button("Clear Selection"))
                CreateSelectionArray();
        }
        else
        {
            if(GUILayout.Button("Change Cells Count from :" + m_target.cells))
                ChangeCellCount();
        }
    }

    void ChangeCellCount()
    {
        PropertiesWindow.Create(
            "Change cell count! warning: may(will) mess up your grid",
            new SerializedProperty[]{serializedObject.FindProperty("cells")},
            CheckOffsetsSize
        );
    }


    Tool cachedTool;
    void ToggleEditMode()
    {
        _editing = !_editing;

        if(_editing)
        {
            cachedTool = Tools.current;
            Tools.current = Tool.None;
            SetEditPoint();
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

        Color gridColor = new Color(1,1,1, 0.25f);
        Color latticeColor = new Color(0.5f,1,0.5f,0.5f);

        for (int x = 0; x < cells; x++)
        {
            Handles.color = gridColor;
            DrawLineHor(x);
            DrawLineVer(x);
            for (int y = 0; y < cells; y++)
            {
                Handles.color = gridColor;
                
                Handles.color = latticeColor;
                if(x != m_target.cellsM1)
                    Handles.DrawDottedLine(cachedPointsWS[x][y], cachedPointsWS[x+1][y], 7);

                if(y !=  m_target.cellsM1)
                    Handles.DrawDottedLine(cachedPointsWS[x][y], cachedPointsWS[x][y+1], 7);
            }
        }

    }
    int cells { get { return m_target.cells; } }

    void CachePoints()
    {
        m_target.CachePoints();
        cachedPointsWS = new Vector3[cells][];
        for (int x = 0; x < cells; x++)
        {
            cachedPointsWS[x] = new Vector3[cells];
            for (int y = 0; y < cells; y++)
            {
                // TODO: have I buggered up the x and y order?
                Vector3 gridPos = new Vector3( 
                    GridToScalar(x, cells), GridToScalar(y, cells)
                );
                cachedPointsWS[x][y] = 
                    m_target.transform.TransformPoint(
                        m_target.offsets[m_target.CoordToIndex(x, y)] + gridPos
                    );
            }
        }
    }

    void CheckTool()
    {
        if(_editing && Tools.current != Tool.None)
        {
            _editing = false;
        }
    }

     void OnSceneGUI()
     {
        CheckTool();
        corners.UpdateCorners(m_target);
        CachePoints();
        Edit();
        DrawGrid();

     }

    void SetEditPoint()
    {
        CachePoints();
        if(_indexX != -1 && _indexY != -1)
            editPoint = cachedEditPoint =  cachedPointsWS[_indexX][_indexY];
    }
    

    static Vector3 editPoint, cachedEditPoint;
    const float buttonSize = 0.05f, selectedButtonSize = 0.08f, pickSize = 0.1f;

    void Edit()
    {
        if(!_editing)
            return;

        for (int x = 0; x < cells; x++)
        {
            for (int y = 0; y < cells; y++)
            {
                Handles.color = _selected[x][y] ? Color.yellow : Color.white;
                if (Handles.Button(
                    cachedPointsWS[x][y], Quaternion.identity, 
                    _selected[x][y] ? selectedButtonSize : buttonSize, 
                    pickSize, Handles.CubeHandleCap
                ))
                {
                    if(Event.current.shift == true)
                    {
                        _selected[x][y] = false;
                    }
                    else
                    {
                        _selected[x][y] = true;
                        _indexX = x;
                        _indexY = y;
                        SetEditPoint();
                    }
                }
                // Handles.CubeHandleCap(
                //     m_target.CoordToIndex(x,y), cachedPointsWS[x][y], m_target.transform.rotation,
                //     0.1f, EventType.Repaint
                // );
            }
        }
        if(_indexX != -1 && _indexY != -1)
        {
            EditorGUI.BeginChangeCheck();
            editPoint = 
                Handles.PositionHandle(editPoint, m_target.transform.rotation);
            if(EditorGUI.EndChangeCheck())
            {
                Vector3 offset = m_target.transform.InverseTransformVector((editPoint - cachedEditPoint) / (cells - 2));


                SerializedProperty offsets =  serializedObject.FindProperty("offsets");
                
                for (int x = 0; x < cells; x++)
                {
                    for (int y = 0; y < cells; y++)
                    {
                        if(_selected[x][y])
                        {
                            offsets.GetArrayElementAtIndex(m_target.CoordToIndex(x,y)).vector3Value += offset;
                        }
                    }
                }
                
                serializedObject.ApplyModifiedProperties();

                cachedEditPoint = editPoint;

                SetFFD();
            }
        }

    }

    void SetFFD()
    {
        if(Application.isPlaying)
            m_target.Set();
    }

    float GridToScalar(int v, float grid)
    {
        return (v / (grid - 1)) * 2 - 1;
    }

    void DrawLineHor(int y)
    {
        DrawLineHor(y / (float)m_target.cellsM1);
    }
    void DrawLineHor(float y)
    {
        Vector3 p0 = Vector3.Lerp(corners.topLeft, corners.bottomLeft, y);
        Vector3 p1 = Vector3.Lerp(corners.topRight, corners.bottomRight, y);
        
        Handles.DrawDottedLine(p0, p1, 5);
    }
    void DrawLineVer(int x)
    {
        DrawLineVer(x / (float)m_target.cellsM1);
    }
    void DrawLineVer(float x)
    {
        Vector3 p0 = Vector3.Lerp(corners.topLeft, corners.topRight, x);
        Vector3 p1 = Vector3.Lerp(corners.bottomLeft, corners.bottomRight, x);
        
        Handles.DrawDottedLine(p0, p1, 5);
    }

}
