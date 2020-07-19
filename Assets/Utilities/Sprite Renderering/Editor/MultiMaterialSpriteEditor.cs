using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MultiMaterialSpriteController), true)]
public class MultiMaterialSpriteEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		bool changesMade = false;
		MultiMaterialSpriteController obj = (MultiMaterialSpriteController) target;

		//show button to apply changes if any changes have been made
		if (!obj.MaterialListMatchesSpriteRendererMaterialArray)
		{
			if (GUILayout.Button("Apply Changes"))
			{
				obj.ApplyMaterials();
				changesMade = true;
			}

			if (GUILayout.Button("Revert"))
			{
				obj.RevertToArray();
				changesMade = true;
			}
		}

		if (changesMade)
		{
			serializedObject.ApplyModifiedProperties();
		}
	}
}
