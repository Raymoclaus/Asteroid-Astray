using UnityEngine;
using UnityEditor;

namespace ValueComponents.CustomisedEditor
{
	[CustomPropertyDrawer(typeof(IntComponent), true)]
	public class IntComponentCustomisedEditor : PropertyDrawer
	{
		private float height;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);


			float objectFieldHeight = EditorStyles.objectField.CalcHeight(label, position.width);
			Rect objectFieldRect = new Rect(position.x, position.y, position.width, objectFieldHeight);
			EditorGUI.ObjectField(objectFieldRect, property, label);
			height = objectFieldHeight;

			Object target = property.serializedObject.targetObject;
			IntComponent comp = (IntComponent) fieldInfo.GetValue(target);

			if (comp != null)
			{
				height += EditorGUIUtility.standardVerticalSpacing;

				GUIContent valueLabel = new GUIContent(comp.ToString());
				float valueLabelHeight = EditorStyles.label.CalcHeight(valueLabel, position.width);
				Rect valueLabelRect = new Rect(position.x, position.y + height,
					position.width, valueLabelHeight);
				EditorGUI.LabelField(valueLabelRect, valueLabel);
				height += valueLabelHeight;
			}

			EditorGUI.EndProperty();
			property.serializedObject.ApplyModifiedProperties();
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return height;
		}
	}
}