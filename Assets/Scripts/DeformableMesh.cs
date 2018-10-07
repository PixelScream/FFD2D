using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeformableMesh : UniqueMesh
{
	Vector3[] _origonalVerts;
	public bool _debug;
	
	public Vector3[] OrigonalVerts
	{
		get
		{
			if(_origonalVerts == null || _origonalVerts.Length != mesh.vertexCount)
			{
				_origonalVerts = mesh.vertices;

			}
			return _origonalVerts;

		}
	}

	public Vector3[] GetVertsWS()
	{
		if(_debug)
		{
			return new Vector3[]{transform.position};
		}

		Vector3[] verts = (Vector3[]) OrigonalVerts.Clone();
		for (int i = 0; i < verts.Length; i++)
		{
			verts[i] = transform.TransformPoint(verts[i]);
		}
		return verts;
	}
	public void SetVertsWS(Vector3[] verts)
	{
		if(_debug)
		{
			deformedPoint = verts[0];
			return;
		}

		for (int i = 0; i < verts.Length; i++)
		{
			verts[i] = transform.InverseTransformPoint(verts[i]);
		}
		mesh.vertices = verts;
	}

	Vector3 deformedPoint;
	private void OnDrawGizmos() {
		if(!Application.isPlaying || !_debug)
			return;

		Gizmos.color = Color.red;	
		Gizmos.DrawSphere((deformedPoint), 0.1f);
	}

}
