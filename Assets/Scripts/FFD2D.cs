using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FFD2D : MonoBehaviour {

	public Vector2 offset;
	Bounds bounds = new Bounds();
	Vector3[][] cachedPoints =  
		new Vector3[][]{ new Vector3[cellsInt], new Vector3[cellsInt], new Vector3[cellsInt]};
	const float cells = 3;
	const int cellsInt = 3;
	const float cellsMO = cells - 1;
	const int cellsIntMO = cellsInt - 1;

	public bool m_activeUpdate = false;

	// Use this for initialization
	void Start () {
		
		Set();
		
		_cachedTransform = new CachedTransform();
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

	void CachePoints()
	{
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
		v = v * cellsMO;
		weighting = v % 1;
		return (int)v;
	}

	private void OnValidate() {
		if(Application.isPlaying && m_activeUpdate)
		{
			Set();
		}
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
