using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AudioSO), true)]
public class AudioSOEditor : Editor
{
	[SerializeField]
	private AudioSource previewer;

	public void OnEnable()
	{
		previewer = EditorUtility.CreateGameObjectWithHideFlags(
			"Audio Preview",
			HideFlags.HideAndDontSave,
			typeof(AudioSource)).GetComponent<AudioSource>();
	}

	private void OnDisable()
	{
		DestroyImmediate(previewer.gameObject);
	}

	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		EditorGUI.BeginDisabledGroup(serializedObject.isEditingMultipleObjects);
		if (GUILayout.Button("Preview"))
		{
			((AudioSO)target).Play(previewer);
		}
		EditorGUI.EndDisabledGroup();
	}
}