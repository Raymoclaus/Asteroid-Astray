using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class DynamicEngineNoise : MonoBehaviour
{
	public AudioSource source;
	public Vector2 pitchRange;
	public float pitchMultiplier = 0.1f;

	private void Awake()
	{
		source = source ?? GetComponent<AudioSource>();
		source.playOnAwake = true;
		source.loop = true;
		source.spatialBlend = 1;
	}

	private void Update()
	{
		source.pitch = Mathf.Lerp(pitchRange.x, pitchRange.y, Shuttle.singleton._vel.magnitude * pitchMultiplier);
	}
}