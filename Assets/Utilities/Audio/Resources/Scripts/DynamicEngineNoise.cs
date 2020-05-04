using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class DynamicEngineNoise : MonoBehaviour
{
	private AudioSource _source;
	private AudioSource Source => _source != null ? _source : (_source = GetComponent<AudioSource>());
	[SerializeField] private Vector2 pitchRange;
	[SerializeField] private float pitchMultiplier = 0.1f;
	[SerializeField] private float volume = 0.15f;

	private void Awake()
	{
		Source.playOnAwake = true;
		Source.loop = true;
		Source.spatialBlend = 1;
	}

	private void Update()
	{
		Source.volume = TimeController.IsStopped ? 0f : volume;
		Source.pitch = Mathf.Lerp(pitchRange.x, pitchRange.y, Speed * pitchMultiplier);
	}

	public float Speed { get; set; }
}