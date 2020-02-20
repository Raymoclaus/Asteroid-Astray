using DialogueSystem;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ConversationWithActions), true)]
public class ConversationWithActionsEditor : PropertyDrawer
{
	private float height;

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		if (position.width == 1f)
		{
			return;
		}
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

					float delayWidth = position.width - 15f;
					GUIContent delayLabel = new GUIContent("Delay");
					float delayHeight = EditorStyles.numberField.CalcHeight(delayLabel, delayWidth);
					Rect delayRect = new Rect(position.x + 15f, position.y + height,
						delayWidth, delayHeight);
					float getDelay = EditorGUI.FloatField(delayRect, delayLabel, prompt.GetDelay(i));
					prompt.SetDelay(i, getDelay);
					height += delayHeight;

					float delayEventWidth = position.width - 15f;
					GUIContent delayEventLabel = new GUIContent("Also delay event invocation?");
					float delayEventHeight = EditorStyles.toggle.CalcHeight(delayEventLabel, delayEventWidth);
					Rect delayEventRect = new Rect(position.x + 15f, position.y + height,
						delayEventWidth, delayEventHeight);
					bool getDelayEventBool = EditorGUI.Toggle(delayEventRect, delayEventLabel,
						prompt.GetAlsoDelayEvent(i));
					prompt.SetToAlsoDelayEvent(i, getDelayEventBool);
					height += delayEventHeight;

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
