using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PropertiesWindow : EditorWindow {

	public static PropertiesWindow Create(
		string message, SerializedProperty[] properties,
		System.Action callback = null
	)
	{
		PropertiesWindow window = EditorWindow.GetWindow(typeof(PropertiesWindow))
			as PropertiesWindow;

		window._message = message;
		window._properties = properties;
		window._callback = callback;

		return window;
	}

	public string _message;
	public SerializedProperty[] _properties;
	public System.Action _callback;

	private void OnGUI() {
		GUILayout.Label(_message);
		foreach(SerializedProperty property in _properties)
		{
			EditorGUILayout.PropertyField(property);
		}
		if(GUILayout.Button("Enter"))
		{
			Enter();
		}
	}

	void Enter() {
		_properties[0].serializedObject.ApplyModifiedProperties();

		if(_callback != null)
			_callback.Invoke();

		Close();
	}
}
