using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleGenerator : MonoBehaviour
{
	public static ParticleGenerator singleton;
	public static Transform holder;

	private const int poolReserve = 2000;
	private Queue<SpriteRenderer> pool = new Queue<SpriteRenderer>(poolReserve);
	private List<SpriteRenderer> active = new List<SpriteRenderer>(poolReserve);

	private void Awake()
	{
		if (singleton == null)
		{
			singleton = this;
			DontDestroyOnLoad(gameObject);
			holder = new GameObject("Particle Holder").transform;
		}
		else
		{
			Destroy(gameObject);
		}

		SetUpPoolReserve();
	}

	public static void GenerateParticle(
		Sprite spr, Vector3 position, Transform parent = null, bool fadeOut = true, float lifeTime = 1f,
		float speed = 0f, bool slowDown = false, float rotationDeg = 0f, float rotationSpeed = 0f, float size = 1f,
		bool rotationDecay = false, float alpha = 1f, Color? tint = null, float fadeIn = 0f, int sortingLayer = 0,
		float growthOverLifetime = 1f)
	{
		SpriteRenderer rend = singleton.pool.Dequeue();
		singleton.active.Add(rend);
		rend.sprite = spr;
		rend.sortingLayerID = sortingLayer;
		Color tintFix = tint == null ? Color.white : (Color)tint;
		rend.color = tintFix;
		GameObject obj = rend.gameObject;
		obj.SetActive(true);
		obj.transform.position = position;
		obj.transform.eulerAngles = Vector3.forward * rotationDeg;
		obj.transform.localScale = Vector2.one * size;
		obj.transform.parent = parent == null ? holder : parent;
		singleton.StartCoroutine(Lifetime(rend, lifeTime, fadeOut, speed, slowDown, rotationSpeed,
			rotationDecay, alpha, tintFix, fadeIn, growthOverLifetime));
	}

	private static IEnumerator Lifetime(
		SpriteRenderer rend, float time, bool fadeOut, float originalSpeed, bool slowDown,
		float originalRotationSpeed, bool rotationDecay, float alpha, Color tint, float fadeIn,
		float growthOverLifetime)
	{
		//record original values
		float spd = originalSpeed;
		float rotSpd = originalRotationSpeed;
		float originalTime = time;
		Vector3 originalSize = rend.transform.localScale;
		Vector3 targetSize = originalSize * growthOverLifetime;
		//initialise random values
		float randomDir = Random.value * Mathf.PI * 2f;
		Vector3 direction = new Vector3(Mathf.Sin(randomDir), Mathf.Cos(randomDir));

		//loop for particle's lifetime
		while (time > 0f)
		{
			//decrease time
			float delta = Mathf.Pow(time / originalTime, 0.75f);
			time -= Time.deltaTime;
			//adjust size
			rend.transform.localScale = Vector3.Lerp(originalSize, targetSize, 1f - delta);
			//adjust colour
			Color c = tint;
			if (originalTime - time < fadeIn)
			{
				c.a = alpha * (originalTime - time) / fadeIn;
			}
			else if (fadeOut)
			{
				c.a = alpha * delta;
			}
			rend.color = c;
			//adjust speed
			if (slowDown)
			{
				spd = originalSpeed * delta;
			}

			if (rotationDecay)
			{
				rotSpd = originalRotationSpeed * delta;
			}
			//adjust rotation
			rend.transform.eulerAngles += Vector3.forward * rotSpd * Time.deltaTime * 60f;
			//adjust position
			rend.transform.position += direction * spd * Time.deltaTime;
			yield return null;
		}

		//disable object and return to pool
		rend.gameObject.SetActive(false);
		rend.transform.parent = singleton.transform;
		singleton.active.Remove(rend);
		singleton.pool.Enqueue(rend);
	}

	private void SetUpPoolReserve()
	{
		for (int i = 0; i < poolReserve; i++)
		{
			GameObject go = new GameObject();
			go.SetActive(false);
			go.transform.parent = transform;
			SpriteRenderer rend = go.AddComponent<SpriteRenderer>();
			pool.Enqueue(rend);
		}
	}
}