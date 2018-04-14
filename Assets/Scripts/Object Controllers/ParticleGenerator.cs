using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleGenerator : MonoBehaviour
{
	public static ParticleGenerator singleton;

	private Queue<SpriteRenderer> pool = new Queue<SpriteRenderer>(poolReserve);
	private List<SpriteRenderer> active = new List<SpriteRenderer>(poolReserve);
	private const int poolReserve = 1000;

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

	public static void GenerateParticle(
		Sprite spr, Vector3 position, Transform parent = null, bool shrink = true, bool fadeOut = true,
		float lifeTime = 1f, float speed = 0f, bool slowDown = false, float rotationDeg = 0f,
		float rotationSpeed = 0f, float size = 1f, bool rotationDecay = false, float alpha = 1f, Color? tint = null,
		float fadeIn = 0f, int sortingLayer = 0)
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
		obj.transform.parent = parent == null ? singleton.transform : parent;
		singleton.StartCoroutine(Lifetime(rend, lifeTime, shrink, fadeOut, speed, slowDown, rotationSpeed, rotationDecay, alpha, tintFix, fadeIn));
	}

	private static IEnumerator Lifetime(
		SpriteRenderer rend, float time, bool shrink, bool fadeOut, float originalSpeed, bool slowDown,
		float originalRotationSpeed, bool rotationDecay, float alpha, Color tint, float fadeIn)
	{
		float spd = originalSpeed;
		float rotSpd = originalRotationSpeed;
		float originalTime = time;

		float randomDir = Random.value * Mathf.PI * 2f;
		Vector3 direction = new Vector3(Mathf.Sin(randomDir), Mathf.Cos(randomDir));

		while (time > 0f)
		{
			float delta = Mathf.Pow(time / originalTime, 0.75f);
			time -= Time.deltaTime;

			if (shrink)
			{
				rend.transform.localScale = Vector3.one * delta;
			}

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

			if (slowDown)
			{
				spd = originalSpeed * delta;
			}

			if (rotationDecay)
			{
				rotSpd = originalRotationSpeed * delta;
			}

			rend.transform.eulerAngles += Vector3.forward * rotSpd;
			rend.transform.position += direction * spd * Time.deltaTime;
			yield return null;
		}

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