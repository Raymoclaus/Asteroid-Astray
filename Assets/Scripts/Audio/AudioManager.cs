using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
	public static AudioManager singleton;
	public AudioMixerGroup musicMixer, sfxMixer;

	private void Awake()
	{
		if (singleton == null)
		{
			singleton = this;
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			Destroy(gameObject);
		}
	}

	public static void PlaySFX(AudioClip clip, Vector3 position, Transform parent = null, float volume = 1f, float pitch = 1f)
	{
		GameObject obj = new GameObject(clip.name);
		obj.transform.position = position;
		obj.transform.parent = parent;
		AudioSource src = obj.AddComponent<AudioSource>();
		src.clip = clip;
		src.outputAudioMixerGroup = singleton.sfxMixer;
		src.volume = volume;
		src.pitch = pitch;
		src.spatialBlend = 1f;
		src.Play();
		singleton.StartCoroutine(Lifetime(obj, clip.length));
	}

	private static IEnumerator Lifetime(GameObject obj, float time)
	{
		while (time > 0f)
		{
			time -= Time.deltaTime;
			yield return null;
		}
		Destroy(obj);
	}
}