using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ConversationEvent), true)]
public class ConversationEventEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		EditorGUI.BeginDisabledGroup(serializedObject.isEditingMultipleObjects);
		if (GUILayout.Button("Load From File"))
		{
			((ConversationEvent)target).Load();
		}
		if (GUILayout.Button("Save To File"))
		{
			((ConversationEvent)target).Save();
		}
		EditorGUI.EndDisabledGroup();
	}
}
