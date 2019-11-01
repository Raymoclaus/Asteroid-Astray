using InputHandlerSystem;
using UnityEngine;
using UnityEditor;

namespace InputHandlerSystem.CustomisedEditor
{
	[CustomPropertyDrawer(typeof(InputCode), true)]
	public class InputCodePropertyDrawer : PropertyDrawer
	{
		private const float LINE_HEIGHT = 16f;
		private int lineCount = 1;

		public override void OnGUI(Rect r, SerializedProperty prop, GUIContent label)
		{
			EditorGUI.BeginProperty(r, label, prop);

			r = EditorGUI.PrefixLabel(r, GUIUtility.GetControlID(FocusType.Passive), label);
			lineCount = 1;
			int indent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;

			SerializedProperty inputTypeProp = prop.FindPropertyRelative("inputType");
			InputCode.InputType inputType = (InputCode.InputType)inputTypeProp.enumValueIndex;
			float popupWidth = EditorStyles.popup.CalcSize(
				new GUIContent(inputType.ToString())).x;
			Rect inputTypeRect = new Rect(r.x, r.y, popupWidth, r.height);
			r.x += popupWidth;
			r.width -= popupWidth;
			inputTypeProp.enumValueIndex = EditorGUI.Popup(
				inputTypeRect, string.Empty,
				inputTypeProp.enumValueIndex, inputTypeProp.enumNames);

			switch (inputType)
			{
				default: break;
				case InputCode.InputType.Button:
					{
						SerializedProperty buttonCodeProp = prop.FindPropertyRelative("buttonCode");
						buttonCodeProp.enumValueIndex = EditorGUI.Popup(
							r, string.Empty, buttonCodeProp.enumValueIndex,
							buttonCodeProp.enumNames);
						break;
					}
				case InputCode.InputType.Axis:
					{
						SerializedProperty axisPositiveProp = prop.FindPropertyRelative("axisPositiveDirection");
						GUIContent toggleText = new GUIContent("Positive Axis");
						float toggleWidth = EditorStyles.toggle.CalcSize(toggleText).x;
						Rect toggleRect = new Rect(
							r.x + r.width - toggleWidth, r.y, toggleWidth, LINE_HEIGHT);
						r.width -= toggleWidth;
						axisPositiveProp.boolValue = EditorGUI.ToggleLeft(
							toggleRect, toggleText, axisPositiveProp.boolValue);

						SerializedProperty axisNameProp = prop.FindPropertyRelative("axisName");
						axisNameProp.stringValue = EditorGUI.TextArea(r, axisNameProp.stringValue);
						break;
					}
			}

			EditorGUI.indentLevel = indent;
			EditorGUI.EndProperty();
		}

		private float Height => lineCount * LINE_HEIGHT;

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return Height;
		}
	}
}
