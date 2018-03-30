using System.Collections;
using UnityEngine;

public class ParticleGenerator : MonoBehaviour
{
	public static ParticleGenerator singleton;

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

	public static void GenerateParticle(Sprite spr, Vector3 position, Transform parent = null, bool shrink = true, bool fade = true, float lifeTime = 1f, float? speed = null, bool slowDown = false)
	{
		GameObject obj = new GameObject(spr.name);
		obj.transform.position = position;
		obj.transform.parent = parent;
		SpriteRenderer rend = obj.AddComponent<SpriteRenderer>();
		rend.sprite = spr;
		singleton.StartCoroutine(Lifetime(obj, lifeTime, shrink, fade, speed, slowDown));
	}

	private static IEnumerator Lifetime(GameObject obj, float time, bool shrink, bool fade, float? speed, bool slowDown)
	{
		SpriteRenderer rend = fade ? obj.GetComponent<SpriteRenderer>() : null;
		float originalTime = time;
		float randomDir = Random.value * Mathf.PI * 2f;
		float originalSpeed = speed == null ? Random.value : Random.value * (float)speed;
		float spd = originalSpeed;
		Vector3 direction = new Vector3(Mathf.Sin(randomDir), Mathf.Cos(randomDir));
		while (time > 0f)
		{
			float delta = Mathf.Pow(time / originalTime, 0.75f);
			time -= Time.deltaTime;
			if (shrink)
			{
				obj.transform.localScale = Vector3.one * delta;
			}
			if (fade)
			{
				Color c = Color.white;
				c.a = delta;
				rend.color = c;
			}
			if (slowDown)
			{
				spd = originalSpeed * delta;
			}
			obj.transform.position += direction * spd * Time.deltaTime;
			yield return null;
		}
		Destroy(obj);
	}
}