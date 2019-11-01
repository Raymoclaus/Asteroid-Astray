using System.Collections.Generic;
using InputHandlerSystem;
using UnityEngine;
using UnityEditor;

namespace InputHandlerSystem.CustomisedEditor
{
	[CustomEditor(typeof(InputMethod), true)]
	public class InputMethodCustomInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			InputMethod obj = (InputMethod)target;

			if (!obj.MatchesContextActionPool)
			{
				if (GUILayout.Button("Update Action List"))
				{
					obj.UpdateInputs();
				}
			}

			obj.inputMode = (InputMode)EditorGUILayout.EnumPopup(
				"Input Mode", obj.inputMode);
			SerializedProperty contextProp = serializedObject.FindProperty("context");
			EditorGUILayout.PropertyField(contextProp, new GUIContent("Context"));

			List<ActionCombination> combs = obj.combinations;
			SerializedProperty combsProp = serializedObject.FindProperty("combinations");
			int indentLevel0 = EditorGUI.indentLevel;
			for (int i = 0; i < combs.Count; i++)
			{
				EditorGUI.indentLevel = indentLevel0;
				ActionCombination comb = combs[i];
				SerializedProperty combProp = combsProp.GetArrayElementAtIndex(i);
				DrawAction(comb, combProp);
			}

			serializedObject.ApplyModifiedProperties();

			//base.OnInspectorGUI();
		}

		private static void DrawAction(ActionCombination comb, SerializedProperty combProp)
		{
			string actionName = comb.actionName;
			GUIContent actionNameContent = new GUIContent(actionName);
			float labelWidth = EditorStyles.label.CalcSize(actionNameContent).x;

			EditorGUILayout.LabelField(actionName);

			int indentLevel1 = ++EditorGUI.indentLevel;
			DrawBindings(comb.GetDefaultCombination(), combProp,
				"Default", "defaultCombination");

			EditorGUI.indentLevel = indentLevel1;
			DrawBindings(comb.GetCurrentCombination(), combProp,
				"Current", "currentCombination");
		}

		private static void DrawBindings(InputCombination comb, SerializedProperty combProp, string labelName, string propertyName)
		{
			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(labelName);
			SerializedProperty defaultCombProp = combProp.FindPropertyRelative(propertyName);
			GUIContent addButtonContent = new GUIContent("+");
			float addButtonWidth = EditorStyles.miniButton.CalcSize(addButtonContent).x;
			if (GUILayout.Button(addButtonContent, GUILayout.Width(addButtonWidth)))
			{
				comb.AddCode();
			}
			GUILayout.EndHorizontal();
			EditorGUI.indentLevel++;
			int indentLevel2 = EditorGUI.indentLevel;
			List<InputCode> inputs = comb.inputs;
			SerializedProperty codesProp = defaultCombProp.FindPropertyRelative("inputs");
			for (int j = 0; j < codesProp.arraySize && j < inputs.Count; j++)
			{
				GUILayout.BeginHorizontal();
				InputCode code = inputs[j];
				SerializedProperty codeProp = codesProp.GetArrayElementAtIndex(j);
				EditorGUILayout.PropertyField(codeProp, GUIContent.none);
				GUIContent removeButtonContent = new GUIContent("-");
				float removeButtonWidth = EditorStyles.miniButton.CalcSize(removeButtonContent).x;
				if (GUILayout.Button(removeButtonContent, GUILayout.Width(removeButtonWidth)))
				{
					comb.RemoveAtIndex(j);
					codesProp = defaultCombProp.FindPropertyRelative("inputs");
				}
				GUILayout.EndHorizontal();
			}
		}
	}
}
