using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
	public static AudioManager singleton;
	public AudioMixerGroup musicMixer, sfxMixer;

	private Queue<AudioSource> pool = new Queue<AudioSource>(poolReserve);
	private List<AudioSource> active = new List<AudioSource>(poolReserve);
	private const int poolReserve = 30;

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

		SetUpPoolReserve();
	}

	public static void PlaySFX(AudioClip clip, Vector3 position, Transform parent = null, float volume = 1f, float pitch = 1f)
	{
		AudioSource src = singleton.pool.Dequeue();
		singleton.active.Add(src);
		GameObject obj = src.gameObject;
		obj.SetActive(true);
		obj.transform.position = position;
		obj.transform.parent = parent == null ? singleton.transform : parent;
		src.clip = clip;
		src.volume = volume;
		src.pitch = pitch;
		src.Play();
		singleton.StartCoroutine(Lifetime(src, clip.length));
	}

	private static IEnumerator Lifetime(AudioSource src, float time)
	{
		while (time > 0f)
		{
			time -= Time.deltaTime;
			yield return null;
		}
		src.gameObject.SetActive(false);
		src.transform.parent = singleton.transform;
		singleton.active.Remove(src);
		singleton.pool.Enqueue(src);
	}

	private void SetUpPoolReserve()
	{
		for (int i = 0; i < poolReserve; i++)
		{
			GameObject go = new GameObject();
			go.SetActive(false);
			go.transform.parent = transform;
			AudioSource src = go.AddComponent<AudioSource>();
			src.outputAudioMixerGroup = sfxMixer;
			src.spatialBlend = 1f;
			pool.Enqueue(src);
		}
	}
}