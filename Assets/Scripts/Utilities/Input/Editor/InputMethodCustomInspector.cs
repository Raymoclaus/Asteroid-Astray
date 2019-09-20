using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace InputHandler.InputEditor
{
	[CustomEditor(typeof(InputMethod), true)]
	public class InputMethodCustomInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			return;

			InputMethod method = (InputMethod)target;
			InputContext context = method.context;

			SerializedProperty contextProperty = serializedObject.FindProperty("context");
			EditorGUILayout.PropertyField(contextProperty);

			float labelWidth = 80f;
			float enumPopupWidth = 120f;

			GUILayout.BeginHorizontal();
			GUILayout.Label("Input Mode", GUILayout.Width(labelWidth));
			InputMode mode = method.inputMode;
			InputMode newMode = (InputMode)EditorGUILayout.EnumPopup(mode, GUILayout.Width(enumPopupWidth));
			method.inputMode = newMode;
			GUILayout.EndHorizontal();

			bool filled = context != null;
			if (filled)
			{

				List<string> actions = context.actions;

				method.SetCombinationsLength(actions.Count);
				GUILayout.Label($"Context Actions for {context.contextName}", EditorStyles.boldLabel);
				DrawContextActions(method, serializedObject, labelWidth, enumPopupWidth, actions, false);
				GUILayout.Label($"Default Context Actions for {context.contextName}", EditorStyles.boldLabel);
				DrawContextActions(method, serializedObject, labelWidth, enumPopupWidth, actions, true);
			}
			else
			{
				GUILayout.Label("Assign an Input Context", EditorStyles.boldLabel);
			}
			

			serializedObject.ApplyModifiedProperties();
		}

		private static void DrawContextActions(InputMethod method, SerializedObject serializedObject, float labelWidth, float enumPopupWidth, List<string> actions, bool drawDefault)
		{
			SerializedProperty combinationsProperty = serializedObject.FindProperty(drawDefault ? "defaultCombinations" : "currentCombinations");

			for (int i = 0; i < actions.Count; i++)
			{
				GUILayout.Label(actions[i], EditorStyles.miniBoldLabel);
				InputCombination combination = drawDefault ? method.defaultCombinations[i] : method.currentCombinations[i];

				List<InputCode> inputs = combination.inputs;
				for (int j = 0; j < inputs.Count; j++)
				{
					GUILayout.BeginHorizontal();
					GUILayout.Label("Input Type", GUILayout.Width(labelWidth));
					InputCode input = inputs[j];
					InputCode.InputType type = input.inputType;
					InputCode.InputType newType = (InputCode.InputType)
						EditorGUILayout.EnumPopup(type, GUILayout.Width(enumPopupWidth));
					input.inputType = newType;
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					switch (newType)
					{
						case InputCode.InputType.Button:
							GUILayout.Label("Button Code", GUILayout.Width(labelWidth));
							GUIContent keyCodeContent = new GUIContent(
								input.buttonCode.ToString());
							float width = EditorStyles.textArea.CalcSize(keyCodeContent).x;
							KeyCode key = (KeyCode)EditorGUILayout.EnumPopup(
								input.buttonCode, GUILayout.Width(enumPopupWidth));
							input.buttonCode = key;
							break;
						case InputCode.InputType.Axis:
							GUILayout.Label("Axis Name", GUILayout.Width(labelWidth));
							string axisName = GUILayout.TextField(input.axisName);
							input.axisName = axisName;
							bool positiveAxis = input.axisPositiveDirection;
							bool toggle = GUILayout.Toggle(
								positiveAxis, "Expect Positive Axis");
							input.axisPositiveDirection = toggle;
							break;
					}
					GUILayout.EndHorizontal();
				}

				float buttonWidth = 20f;
				GUILayout.BeginHorizontal();
				GUIContent addButtonInputButton = new GUIContent("+");
				if (GUILayout.Button(addButtonInputButton, GUILayout.Width(buttonWidth)))
				{
					combination.AddCode(InputCode.InputType.Button);
				}

				GUIStyle style = new GUIStyle();
				GUIContent removeButtonInputButton = new GUIContent("-");
				if (GUILayout.Button(removeButtonInputButton, GUILayout.Width(buttonWidth)))
				{
					combination.RemoveLastCode();
				}
				GUILayout.EndHorizontal();
			}
		}
	}
}
