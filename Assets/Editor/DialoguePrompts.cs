using DialogueSystem;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ConversationWithActions), true)]
public class DialoguePromptsEditor : PropertyDrawer
{
	private const float DEFAULT_HEIGHT = 16f;
	private float height = DEFAULT_HEIGHT;

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		EditorGUI.BeginChangeCheck();
		Object target = property.serializedObject.targetObject;
		ConversationWithActions prompt = (ConversationWithActions)fieldInfo.GetValue(target);
		prompt.EnsureLength();

		EditorGUI.BeginProperty(position, label, property);
		int indent = EditorGUI.indentLevel;
		EditorStyles.label.wordWrap = true;
		EditorStyles.label.fontStyle = FontStyle.Normal;

		SerializedProperty conversationEventProperty
			= property.FindPropertyRelative("conversationEvent");
		EditorGUI.PropertyField(position, conversationEventProperty, label, true);
		height = DEFAULT_HEIGHT;
		EditorGUI.indentLevel++;

		if (prompt.conversationEvent != null)
		{
			SerializedProperty eventsListProperty = property.FindPropertyRelative("events");
			Rect foldoutRect = new Rect(position.x, position.y + height, position.width, DEFAULT_HEIGHT);
			height += DEFAULT_HEIGHT;
			property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label);
			if (property.isExpanded && prompt != null)
			{
				int length = prompt.Length;
				for (int i = 0; i < length; i++)
				{
					float textWidth = position.width - 15f;
					Rect textRect = new Rect(position.x + 15f, position.y + height,
						textWidth, DEFAULT_HEIGHT * 3f);

					string text = prompt.GetLines()[i].line;
					GUIContent textLabel = new GUIContent(text);
					EditorGUI.LabelField(textRect, textLabel);
					height += EditorStyles.label.CalcHeight(textLabel, textWidth);

					SerializedProperty eventProperty = eventsListProperty.GetArrayElementAtIndex(i);
					Rect eventRect = new Rect(position.x + 15f, position.y + height,
						position.width - 20f, DEFAULT_HEIGHT * 5);
					EditorGUI.PropertyField(eventRect, eventProperty);
					height += EditorGUI.GetPropertyHeight(eventProperty);
				}

				height += DEFAULT_HEIGHT;
				Rect labelRect = new Rect(position.x, position.y + height,
					position.width, DEFAULT_HEIGHT);
				EditorGUI.LabelField(labelRect, "End of conversation action");
				height += DEFAULT_HEIGHT;

				Rect endEventRect = new Rect(position.x + 15f, position.y + height,
					position.width - 20f, DEFAULT_HEIGHT * 5);
				SerializedProperty endEventProperty = property.FindPropertyRelative("endEvent");
				EditorGUI.PropertyField(endEventRect, endEventProperty);
				height += EditorGUI.GetPropertyHeight(endEventProperty);

				//convoEventObj.ApplyModifiedProperties();
				//convoObject.ApplyModifiedProperties();
				//endEventObj.ApplyModifiedProperties();
			}
		}

		EditorGUI.indentLevel = indent;
		property.serializedObject.ApplyModifiedProperties();
		EditorGUI.EndProperty();
		EditorGUI.EndChangeCheck();

		if (GUI.changed)
		{
			EditorUtility.SetDirty(target);
		}
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		return height;
	}
}
