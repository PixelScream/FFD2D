using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public class UniqueMeshManager : MonoBehaviour {

	public static void Init()
	{
		EditorSceneManager.sceneSaving += OnSaving;
		EditorSceneManager.sceneSaved += OnSaved;

	}
	public static  void OnSaving(Scene scene, string path)
	{
		UniqueMesh[] meshes = FindObjectsOfType<UniqueMesh>();

		foreach (UniqueMesh m in meshes)
		{
			m.Reset();
		}
	}
	public static void OnSaved(Scene scene)
	{
		Apply();
	}
	public static void OnSceneLoad(Scene scene, OpenSceneMode mode)
	{
		Apply();
	}

	public static void Apply()
	{
		UniqueMesh[] meshes = FindObjectsOfType<UniqueMesh>();

		foreach (UniqueMesh m in meshes)
		{
			m.Reapply();
		}
	}

}
