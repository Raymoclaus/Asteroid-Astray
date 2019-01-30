using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
	public AudioMixerGroup masterMixer, musicMixer, sfxMixer;

	private const int poolReserve = 400;
	private Queue<AudioSource> pool = new Queue<AudioSource>(poolReserve);
	private List<SourceManager> active = new List<SourceManager>(poolReserve);
	private const float MIN_DISTANCE = 1f / 3f, MAX_DISTANCE = 30f;
	public static Transform holder;

	private void Awake()
	{
		holder = new GameObject("Audio Holder").transform;
		SetUpPoolReserve();
	}

	private void Update()
	{
		masterMixer.audioMixer.SetFloat("Pitch", Time.timeScale);

		for (int i = 0; i < active.Count; i++)
		{
			SourceManager srcM = active[i];
			if (srcM.timer <= 0f)
			{
				Recycle(srcM);
				i--;
			}
			else
			{
				srcM.SubtractTimer(Time.deltaTime);
			}
		}
	}

	public void PlaySFX(AudioClip clip, Vector3 position, Transform parent = null, float volume = 1f,
		float pitch = 1f)
	{
		if (!clip) return;

		AudioSource src = pool.Dequeue();
		active.Add(new SourceManager(src, clip.length));
		GameObject obj = src.gameObject;
		obj.SetActive(true);
		obj.transform.position = position;
		obj.transform.parent = parent == null ? holder : parent;
		src.clip = clip;
		src.volume = volume;
		src.pitch = pitch;
		src.Play();
	}

	private void Recycle(SourceManager src)
	{
		src.source.gameObject.SetActive(false);
		src.source.transform.parent = holder;
		active.Remove(src);
		pool.Enqueue(src.source);
	}

	private void SetUpPoolReserve()
	{
		for (int i = 0; i < poolReserve; i++)
		{
			GameObject go = new GameObject();
			go.SetActive(false);
			go.transform.parent = holder;
			AudioSource src = go.AddComponent<AudioSource>();
			src.outputAudioMixerGroup = sfxMixer;
			src.spatialBlend = 1f;
			src.minDistance = MIN_DISTANCE;
			src.maxDistance = MAX_DISTANCE;
			pool.Enqueue(src);
		}
	}

	private struct SourceManager
	{
		public AudioSource source;
		public float timer;

		public SourceManager(AudioSource source, float timer)
		{
			this.source = source;
			this.timer = timer;
		}

		public float SubtractTimer(float subtraction)
		{
			return timer -= subtraction;
		}
	}
}