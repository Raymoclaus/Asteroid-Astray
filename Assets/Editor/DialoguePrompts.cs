using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ConversationEvent), true)]
public class DialoguePromptsEditor : PropertyDrawer
{
	private const float DEFAULT_HEIGHT = 16f;
	private float height = DEFAULT_HEIGHT;

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		property.serializedObject.Update();
		Object target = property.serializedObject.targetObject;
		ConversationEvent prompt = (ConversationEvent)fieldInfo.GetValue(target);

		EditorGUI.BeginProperty(position, label, property);
		int indent = EditorGUI.indentLevel;
		EditorStyles.label.wordWrap = true;
		EditorStyles.label.fontStyle = FontStyle.Normal;

		EditorGUI.PropertyField(position, property, label, true);
		EditorGUI.indentLevel++;

		if (prompt != null)
		{
			int length = prompt.conversation.Length;
			height = DEFAULT_HEIGHT;
			for (int i = 0; i < length; i++)
			{
				Rect toggleRect = new Rect(position.x, position.y + height,
					position.width, DEFAULT_HEIGHT);
				float textWidth = position.width - 15f;
				Rect textRect = new Rect(position.x + 15f, position.y + height,
					textWidth, DEFAULT_HEIGHT * 3f);

				bool hasAction = prompt.conversation[i].hasAction;
				if (EditorGUI.Toggle(toggleRect, hasAction) != hasAction)
				{
					prompt.conversation[i].hasAction = !hasAction;
				}
				string text = prompt.conversation[i].GetLine(0);
				GUIContent textLabel = new GUIContent(text);
				EditorGUI.LabelField(textRect, textLabel);
				height += EditorStyles.label.CalcHeight(textLabel, textWidth);

				if (hasAction)
				{
					SerializedObject actionObject = new SerializedObject(prompt.conversation[i]);

					Rect eventRect = new Rect(position.x + 15f, position.y + height,
						position.width - 20f, DEFAULT_HEIGHT * 5);
					SerializedProperty actionProperty = actionObject.FindProperty("action");
					EditorGUI.PropertyField(eventRect, actionProperty, true);
					height += EditorGUI.GetPropertyHeight(actionProperty);

					Rect skipEventRect = new Rect(position.x + 15f, position.y + height,
						position.width - 20f, DEFAULT_HEIGHT * 5);
					SerializedProperty skipActionProperty = actionObject.FindProperty("skipAction");
					EditorGUI.PropertyField(skipEventRect, skipActionProperty, true);
					height += EditorGUI.GetPropertyHeight(skipActionProperty);

					actionObject.ApplyModifiedProperties();
				}
			}

			height += DEFAULT_HEIGHT;
			Rect labelRect = new Rect(position.x, position.y + height,
				position.width, DEFAULT_HEIGHT);
			EditorGUI.LabelField(labelRect, "End of conversation action");
			height += DEFAULT_HEIGHT;

			Rect endEventRect = new Rect(position.x + 15f, position.y + height,
				position.width - 20f, DEFAULT_HEIGHT * 5);
			SerializedObject endActionObject = new SerializedObject(prompt);
			SerializedProperty eventProperty = endActionObject
				.FindProperty("conversationEndAction");
			EditorGUI.PropertyField(endEventRect, eventProperty);
			height += EditorGUI.GetPropertyHeight(eventProperty);

			endActionObject.ApplyModifiedProperties();
		}

		EditorGUI.indentLevel = indent;
		EditorGUI.EndProperty();
		property.serializedObject.ApplyModifiedProperties();
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		return height;
	}
}
