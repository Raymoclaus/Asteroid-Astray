using UnityEngine;

[CreateAssetMenu]
public class AudioSO : ScriptableObject
{
	public AudioClip[] sfx;
	public Vector2 volumeRange = Vector2.one;
	public Vector2 pitchRange = Vector2.one;

	public AudioClip PickRandomClip()
	{
		if (sfx.Length == 0) return null;

		int choose = Random.Range(0, sfx.Length);
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
}
