using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FFD2D))]
public class FFD2DEditor : Editor 
{
    FFD2D m_target;

    private void OnEnable() {
        m_target = target as FFD2D;
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
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

    void OnSceneGUI()
    {
        corners.UpdateCorners(m_target);

        Handles.color = Color.white;
        Handles.DrawLine(corners.topLeft, corners.topRight);
        Handles.DrawLine(corners.bottomLeft, corners.bottomRight);

        Handles.color = new Color(1,1,1, 0.5f);
        DrawLineHor(0.5f);
        DrawLineVer(0.5f);

    }

    void DrawLineHor(float y)
    {
        Vector3 p0 = Vector3.Lerp(corners.topLeft, corners.bottomLeft, y);
        Vector3 p1 = Vector3.Lerp(corners.topRight, corners.bottomRight, y);
        
        Handles.DrawLine(p0, p1);
    }
    void DrawLineVer(float x)
    {
        Vector3 p0 = Vector3.Lerp(corners.topLeft, corners.topRight, x);
        Vector3 p1 = Vector3.Lerp(corners.bottomLeft, corners.bottomRight, x);
        
        Handles.DrawLine(p0, p1);
    }

}
