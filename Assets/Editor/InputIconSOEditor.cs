using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(InputIconSO), true)]
public class InputIconSOEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		EditorGUI.BeginDisabledGroup(serializedObject.isEditingMultipleObjects);
		if (GUILayout.Button("Add Selection"))
		{
			Object[] objects = Selection.objects;
			for (int i = 0; i < objects.Length; i++)
			{
				if (objects[i] is Texture2D)
				{
					Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GetAssetPath(objects[i]));
					((InputIconSO)target).AddToList(sprite);
				}
				else
				{
					Debug.Log($"{objects[i].name} is a type of {objects[i].GetType()}, not a {typeof(Texture2D)}");
				}
			}
			EditorUtility.SetDirty(target);
		}
		EditorGUI.EndDisabledGroup();
	}
}
