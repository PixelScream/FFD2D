using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UniqueMesh : MonoBehaviour {
	[HideInInspector] int ownerID;

	MeshFilter _mf;
	MeshFilter mf {
		get {
			_mf = _mf == null ? GetComponent<MeshFilter>() : _mf;
			_mf = _mf == null ? gameObject.AddComponent<MeshFilter>() : _mf;
			return _mf;
		}
	}
	Mesh _mesh;
	public Mesh mesh { 
		get {
			bool isOwner = ownerID == gameObject.GetInstanceID();
			if(mf.sharedMesh == null || !isOwner)
			{
				mf.sharedMesh = _mesh = mf.sharedMesh == 
					null ? new Mesh() : mf.mesh;

				ownerID = gameObject.GetInstanceID();
				_mesh.name = 
					(_mesh.name == "" ? "Mesh" : _mesh.name) +
					" [" + ownerID + "]";
			}
			return _mesh;
		}
	}
}
