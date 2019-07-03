using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(BooleanMatrix), true)]
public class BooleanMatrixDrawer : PropertyDrawer
{
	private const float DEFAULT_HEIGHT = 16f;
	private float height = DEFAULT_HEIGHT;

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		EditorGUI.BeginChangeCheck();
		Object target = property.serializedObject.targetObject;
		BooleanMatrix boolMatrix = (BooleanMatrix)fieldInfo.GetValue(target);

		EditorGUI.BeginProperty(position, label, property);
		int indent = EditorGUI.indentLevel;
		EditorStyles.label.wordWrap = true;
		EditorStyles.label.fontStyle = FontStyle.Normal;

		Debug.Log(boolMatrix);

		EditorGUI.EndProperty();
		EditorGUI.EndChangeCheck();

		if (GUI.changed)
		{
			EditorUtility.SetDirty(target);
		}
	}
}
