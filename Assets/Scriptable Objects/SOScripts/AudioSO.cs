using UnityEngine;
using UnityEditor;

[CreateAssetMenu(menuName = "Scriptable Objects/AudioSO")]
public class AudioSO : ScriptableObject
{
	public AudioClip[] sfx;
	public Vector2 volumeRange = Vector2.one;
	public Vector2 pitchRange = Vector2.one;
	private int lastPlayed = -1;

	public AudioClip PickRandomClip()
	{
		if (sfx.Length == 0) return null;
		if (sfx.Length == 1) return sfx[0];

		int choose = 0;
		do
		{
			choose = Random.Range(0, sfx.Length);
		} while (lastPlayed == choose);

		lastPlayed = choose;
		return sfx[choose];
	}

	public float PickRandomVolume()
	{
		return Random.Range(volumeRange.x, volumeRange.y);
	}

	public float PickRandomPitch()
	{
		return Random.Range(pitchRange.x, pitchRange.y);
	}

	public void Play(AudioSource source)
	{
		source.clip = PickRandomClip();
		source.volume = PickRandomVolume();
		source.pitch = PickRandomPitch();
		source.Play();
	}
}

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