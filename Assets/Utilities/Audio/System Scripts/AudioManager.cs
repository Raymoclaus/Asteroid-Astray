using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace AudioUtilities
{
	public class AudioManager : MonoBehaviour
	{
		public AudioMixerGroup masterGroup;

		private const int poolReserve = 400;
		private Queue<AudioSource> pool = new Queue<AudioSource>(poolReserve);
		private List<SourceManager> active = new List<SourceManager>(poolReserve);
		private const float MIN_DISTANCE = 1f / 3f, MAX_DISTANCE = 30f;
		public static Transform holder;

		private void Awake()
		{
			holder = new GameObject("Audio Holder").transform;
		}

		private void Update()
		{
			masterGroup.audioMixer.SetFloat("MusicPitch", Time.timeScale);
			masterGroup.audioMixer.SetFloat("SfxPitch", Time.timeScale);

			for (int i = 0; i < active.Count; i++)
			{
				SourceManager srcM = active[i];
				srcM.SubtractTimer(Time.deltaTime);
				if (srcM.timer <= 0f)
				{
					Recycle(srcM);
					i--;
				}
			}
		}

		public void PlaySFX(AudioClip clip, Vector3 position, Transform parent = null, float volume = 1f,
			float pitch = 1f)
		{
			if (clip == null) return;

			AudioSource src = GetSourceFromPool();
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

		private AudioSource GetSourceFromPool()
		{
			if (pool.Count == 0)
			{
				AddNewSource();
			}
			return pool.Dequeue();
		}

		private void Recycle(SourceManager src)
		{
			src.source.gameObject.SetActive(false);
			src.source.transform.parent = holder;
			active.Remove(src);
			pool.Enqueue(src.source);
		}

		private void AddNewSource()
		{
			GameObject go = new GameObject();
			go.SetActive(false);
			go.transform.parent = holder;
			AudioSource src = go.AddComponent<AudioSource>();
			src.outputAudioMixerGroup = masterGroup;
			src.spatialBlend = 1f;
			src.minDistance = MIN_DISTANCE;
			src.maxDistance = MAX_DISTANCE;
			pool.Enqueue(src);
		}

		private class SourceManager
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
}