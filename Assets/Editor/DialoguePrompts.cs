using DialogueSystem;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ConversationWithActions), true)]
public class DialoguePromptsEditor : PropertyDrawer
{
	private float height;

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		Object target = property.serializedObject.targetObject;
		ConversationWithActions prompt = (ConversationWithActions)fieldInfo.GetValue(target);
		prompt.EnsureLength();

		EditorGUI.BeginProperty(position, label, property);
		position = EditorGUI.PrefixLabel(position, new GUIContent());
		int indent = EditorGUI.indentLevel;
		EditorStyles.label.wordWrap = true;
		EditorStyles.label.fontStyle = FontStyle.Normal;

		SerializedProperty conversationEventProperty
			= property.FindPropertyRelative("conversationEvent");
		EditorGUI.PropertyField(position, conversationEventProperty, label, true);
		height = EditorGUI.GetPropertyHeight(conversationEventProperty);
		EditorGUI.indentLevel++;

		if (prompt.conversationEvent != null)
		{
			SerializedProperty eventsListProperty = property.FindPropertyRelative("events");
			float foldoutHeight = EditorStyles.foldout.CalcHeight(label, position.width);
			Rect foldoutRect = new Rect(position.x, position.y + height, position.width, foldoutHeight);
			height += foldoutHeight;
			property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label);
			if (property.isExpanded && prompt != null)
			{
				int length = prompt.Length;
				for (int i = 0; i < length; i++)
				{
					float textWidth = position.width - 15f;
					string text = prompt.Lines[i].line;
					GUIContent textLabel = new GUIContent(text);
					float textHeight = EditorStyles.label.CalcHeight(textLabel, textWidth);
					Rect textRect = new Rect(position.x + 15f, position.y + height,
						textWidth, textHeight);
					EditorGUI.LabelField(textRect, textLabel);
					height += textHeight;

					SerializedProperty eventProperty = eventsListProperty.GetArrayElementAtIndex(i);
					float eventHeight = EditorGUI.GetPropertyHeight(eventProperty);
					Rect eventRect = new Rect(position.x + 15f, position.y + height,
						position.width - 20f, eventHeight);
					EditorGUI.PropertyField(eventRect, eventProperty);
					height += eventHeight;
				}

				height += EditorGUIUtility.singleLineHeight;
				GUIContent endLabel = new GUIContent("End of conversation action");
				float labelHeight = EditorStyles.label.CalcHeight(endLabel, position.width);
				Rect labelRect = new Rect(position.x, position.y + height,
					position.width, labelHeight);
				EditorGUI.LabelField(labelRect, endLabel);
				height += labelHeight;

				SerializedProperty endEventProperty = property.FindPropertyRelative("endEvent");
				float endEventHeight = EditorGUI.GetPropertyHeight(endEventProperty);
				Rect endEventRect = new Rect(position.x + 15f, position.y + height,
					position.width - 20f, endEventHeight);
				EditorGUI.PropertyField(endEventRect, endEventProperty);
				height += endEventHeight;
			}
		}

		EditorGUI.indentLevel = indent;
		property.serializedObject.ApplyModifiedProperties();
		EditorGUI.EndProperty();
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		return height;
	}
}
