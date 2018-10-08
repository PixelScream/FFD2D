using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * modified from https://www.youtube.com/watch?v=o9RK6O2kOKo
 */
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
	[SerializeField] Mesh _origonal;
	public Mesh Origonal { get { return _origonal; } }

	[SerializeField] Mesh _mesh;
	public Mesh mesh { 
		get {
			bool isOwner = ownerID == gameObject.GetInstanceID();
			if(mf.sharedMesh == null || _mesh == null || !isOwner)
			{
				ownerID = gameObject.GetInstanceID();
				if(_origonal != null)
				{
					UpdateFromOrigonal();
				}
				else if(mf.sharedMesh != null)
				{
					_origonal = mf.sharedMesh;
					UpdateFromOrigonal();
				}
				else
				{
					_mesh = new Mesh();
					SetName("Mesh");
				}
				mf.sharedMesh = _mesh;


			}
			return _mesh;
		}
	}

	public virtual void UpdateFromOrigonal()
	{
		if(_origonal != null)
		{
			_mesh = Instantiate(_origonal);
			SetName(_origonal.name);
		}
	}

	void SetName(string n)
	{
		_mesh.name = n +  " [" + ownerID + "]";
	}
	public virtual void Reset()
	{
		if(_origonal != null)
			mf.sharedMesh = _origonal;
	}
	public virtual void Reapply()
	{
		if(_mesh != null)
			mf.sharedMesh = _mesh;
	}
}
