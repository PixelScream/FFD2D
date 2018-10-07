using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[ExecuteInEditMode]
public class FFD2D : MonoBehaviour {

	public Vector2 offset;
	Bounds bounds = new Bounds();
	public Vector3[][] cachedPoints;
	public int cells = 3;

	public bool m_activeUpdate = false;

	// Use this for initialization
	void Start () {
		Init();
		Set();
	}

	struct CachedTransform
	{
		public Vector3 position, scale;
		Quaternion rotation;
		public bool Matching(Transform t)
		{
			bool b = 	t.position == position &&
						t.rotation == rotation &&
						t.localScale == scale;
						
			position = t.position;
			rotation = t.rotation;
			scale = t.localScale;

			return b;
		}
	}
	CachedTransform _cachedTransform;

	private void Update() {
		if(m_activeUpdate)
		{
			if(!_cachedTransform.Matching(transform))
			{
				Set();
			}
		}
	}

	void Init()
	{
		if(cachedPoints != null && cachedPoints.Length == cells)
			return;

		cachedPoints = new Vector3[cells][];
		for (int i = 0; i < cells; i++)
		{
			cachedPoints[i] = new Vector3[cells];
		}
		_cachedTransform = new CachedTransform();
	}

	public void CachePoints()
	{
		Init();
		bounds.size = transform.localScale * 2;

		cachedPoints[1][1] = offset * transform.localScale;
		
	}

	Vector3 LocalPoint(float x, float y)
	{
		return LocalPoint(new Vector3(x,y));
	}
	public Vector3 LocalPoint(Vector3 p)
	{
		return transform.InverseTransformPoint(p);
	}
	public Vector3 WorldPoint(Vector3 p)
	{
		return transform.TransformPoint(p);
	}
	

	public void Set()
	{
		CachePoints();
		DeformableMesh[] meshes = FindObjectsOfType<DeformableMesh>();

		foreach (DeformableMesh d in meshes)
		{
			Set(d);
		}
	}

	public void Set(DeformableMesh deformable)
	{
		Vector3[] verts = deformable.GetVertsWS();

		for (int v = 0; v < verts.Length; v++)
		{
			// check if in bounds
			Vector3 localV = LocalPoint(verts[v]);
			if(bounds.Contains(localV))
			{
				// find vert's grid cell
				float weightingX, weightingY;
				//Debug.Log(localV.x);
				int x = PointToGrid(localV.x, out weightingX);
				int y = PointToGrid(localV.y, out weightingY);
				//Debug.Log("x: " + x + ", y: " + y);
				// deform
				localV += BilinearFilter(x, y, weightingX, weightingY);

				verts[v] = WorldPoint(localV);
			}
		}
		deformable.SetVertsWS(verts);

	}

	int PointToGrid(float v, out float weighting)
	{
		v = v * 0.5f + 0.5f;
		v = v * (cells - 1);
		weighting = v % 1;
		return (int)v;
	}


	Vector3 BilinearFilter(int x, int y, float u, float v)
	{
		Vector3 delta1 = Vector3.Lerp(
			cachedPoints[x][y], cachedPoints[x + 1][y], u
		);
		Vector3 delta2 = Vector3.Lerp(
			cachedPoints[x][y+1], cachedPoints[x + 1][y + 1], u
		);
		return Vector3.Lerp(delta1, delta2, v);
	}
}
