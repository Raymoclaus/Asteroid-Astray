using System.Collections;
using System.Collections.Generic;
using GenericExtensions;
using UnityEditor;
using UnityEngine;

namespace InputHandlerSystem.CustomisedEditor
{
	[CustomPropertyDrawer(typeof(InputAction), true)]
	public class InputActionPropertyDrawer : PropertyDrawer
	{
		private float height;

		public override void OnGUI(Rect r, SerializedProperty prop, GUIContent label)
		{
			EditorGUI.BeginProperty(r, label, prop);
			int originalIndentLevel = EditorGUI.indentLevel;

			float objectFieldHeight = EditorStyles.objectField.CalcHeight(label, r.width);
			Rect objectFieldRect = new Rect(r.x, r.y, r.width, objectFieldHeight);
			EditorGUI.ObjectField(objectFieldRect, prop, label);
			height = objectFieldHeight;

			Object target = prop.serializedObject.targetObject;
			if (fieldInfo.FieldType == typeof(InputAction))
			{
				InputAction action = (InputAction)fieldInfo.GetValue(target);

				if (action != null)
				{
					height += EditorGUIUtility.standardVerticalSpacing;
					EditorGUI.indentLevel++;

					GUIContent actionLabel = new GUIContent($"{action.ActionName}|{action.IntendedContext.contextName}");
					float valueLabelHeight = EditorStyles.label.CalcHeight(actionLabel, r.width);
					Rect valueLabelRect = new Rect(r.x, r.y + height,
						r.width, valueLabelHeight);
					EditorGUI.LabelField(valueLabelRect, actionLabel);
					height += valueLabelHeight;
				}
			}

			EditorGUI.indentLevel = originalIndentLevel;
			EditorGUI.EndProperty();
			prop.serializedObject.ApplyModifiedProperties();
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return height;
		}
	} 
}
