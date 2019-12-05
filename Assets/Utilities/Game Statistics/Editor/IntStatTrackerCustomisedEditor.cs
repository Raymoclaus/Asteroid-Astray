using UnityEditor;
using UnityEngine;

namespace StatisticsTracker.CustomisedEditor
{
	[CustomEditor(typeof(IntStatTracker), true)]
	public class IntStatTrackerCustomisedEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			SerializedProperty valueProp = serializedObject.FindProperty("value");

			GUIContent label = new GUIContent(serializedObject.targetObject.name);
			EditorGUILayout.PropertyField(valueProp, label, true);

			SerializedProperty defaultValueProp = serializedObject.FindProperty("defaultValue");
			EditorGUILayout.PropertyField(defaultValueProp, true);

			serializedObject.ApplyModifiedProperties();
		}
	}
}
