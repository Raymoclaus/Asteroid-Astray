using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class DynamicEngineNoise : MonoBehaviour
{
	public AudioSource source;
	public Vector2 pitchRange;
	public float pitchMultiplier = 0.1f;
	public float volume = 0.15f;
	[SerializeField]
	private ShuttleTrackers shuttleTrackerSO;

	private void Awake()
	{
		source = source ?? GetComponent<AudioSource>();
		source.playOnAwake = true;
		source.loop = true;
		source.spatialBlend = 1;
	}

	private void Update()
	{
		source.enabled = shuttleTrackerSO;
		if (!shuttleTrackerSO) return;

		source.volume = Pause.IsPaused ? 0f : volume;
		source.pitch = Mathf.Lerp(pitchRange.x, pitchRange.y, shuttleTrackerSO.velocity.magnitude * pitchMultiplier);
	}
}