using InventorySystem;
using System.Collections.Generic;
using UnityEngine;

public class ParticleGenerator : MonoBehaviour
{
	public static Transform holder;
	private const int poolReserve = 2000;
	private Queue<ParticlePropertyManager> pool = new Queue<ParticlePropertyManager>(poolReserve);
	private List<ParticlePropertyManager> active = new List<ParticlePropertyManager>(poolReserve);

	public ResourceDrop dropPrefab;

	private void Awake()
	{
		holder = new GameObject("Particle Holder").transform;
		SetUpPoolReserve();
	}

	private void Update()
	{
		for (int i = 0; i < active.Count; i++)
		{
			ParticlePropertyManager ppm = active[i];
			if (ppm.done)
			{
				Recycle(ppm);
				i--;
			}
			else
			{
				ppm.Update();
			}
		}
	}

	public void GenerateParticle(
		Sprite spr, Vector3 position, Transform parent = null, bool fadeOut = true,
		float lifeTime = 1f, float speed = 0f, bool slowDown = false, float rotationDeg = 0f,
		float rotationSpeed = 0f, float size = 1f, bool rotationDecay = false, float alpha = 1f,
		Color? tint = null, float fadeIn = 0f, int sortingLayer = 0, int sortingOrder = 0,
		float growthOverLifetime = 1f)
	{
		ParticlePropertyManager ppm = null;
		do
		{
			if (pool.Count == 0)
			{
				SetUpPoolReserve();
			}
			ppm = pool.Dequeue();
		} while (ppm.rend == null);

		SpriteRenderer rend = ppm.rend;
		rend.sprite = spr;
		rend.sortingLayerID = sortingLayer;
		rend.sortingOrder = sortingOrder;
		Color tintFix = tint == null ? Color.white : (Color)tint;
		rend.color = tintFix;
		GameObject obj = rend.gameObject;
		obj.SetActive(true);
		obj.transform.position = position;
		obj.transform.eulerAngles = Vector3.forward * rotationDeg;
		obj.transform.localScale = Vector2.one * size;
		obj.transform.parent = parent == null ? holder : parent;
		ppm.Set(lifeTime, fadeOut, speed, slowDown, rotationSpeed, rotationDecay, alpha,
			tintFix, fadeIn, growthOverLifetime);
		active.Add(ppm);
	}

	private void Recycle(ParticlePropertyManager ppm)
	{
		//disable object and return to pool
		ppm.rend.gameObject.SetActive(false);
		ppm.rend.transform.parent = holder;
		active.Remove(ppm);
		pool.Enqueue(ppm);
	}

	private void SetUpPoolReserve()
	{
		for (int i = 0; i < poolReserve; i++)
		{
			GameObject go = new GameObject();
			go.SetActive(false);
			go.transform.parent = holder;
			SpriteRenderer rend = go.AddComponent<SpriteRenderer>();
			rend.spriteSortPoint = SpriteSortPoint.Pivot;
			pool.Enqueue(new ParticlePropertyManager(rend));
		}
	}

	public void DropResource(IInventoryHolder target, Vector2 pos, Item.Type type)
	{
		if (type == Item.Type.Blank) return;

		ResourceDrop rd = Instantiate(dropPrefab);
		rd.Create(target, pos, type);
	}

	private class ParticlePropertyManager
	{
		public SpriteRenderer rend;
		private float time;
		private bool fadeOut;
		private float originalSpeed;
		private bool slowDown;
		private float originalRotationSpeed;
		private bool rotationDecay;
		private float alpha;
		private Color tint;
		private float fadeIn;
		private float growthOverLifetime;

		private float currentSpeed, currentRotationSpeed, originalTime;
		private Vector3 originalSize, targetSize, direction;

		public bool done;

		public ParticlePropertyManager(SpriteRenderer rend)
		{
			this.rend = rend;
		}

		public void Set(float time, bool fadeOut, float originalSpeed, bool slowDown,
			float originalRotationSpeed, bool rotationDecay, float alpha, Color tint, float fadeIn,
			float growthOverLifetime)
		{
			this.time = time;
			this.fadeOut = fadeOut;
			this.originalSpeed = originalSpeed;
			this.slowDown = slowDown;
			this.originalRotationSpeed = originalRotationSpeed;
			this.rotationDecay = rotationDecay;
			this.alpha = alpha;
			this.tint = tint;
			this.fadeIn = fadeIn;
			this.growthOverLifetime = growthOverLifetime;

			currentSpeed = originalSpeed;
			currentRotationSpeed = originalRotationSpeed;
			originalTime = time;
			originalSize = rend.transform.localScale;
			targetSize = originalSize * growthOverLifetime;
			float randomDirection = Random.value * Mathf.PI * 2f;
			direction = new Vector3(Mathf.Sin(randomDirection), Mathf.Cos(randomDirection));

			done = false;
		}

		public void Update()
		{
			if (done) return;

			//decrease time
			float delta = Mathf.Pow(time / originalTime, 0.75f);
			time -= Time.deltaTime;
			//adjust size
			rend.transform.localScale = Vector3.Lerp(originalSize, targetSize, 1f - delta);
			//adjust colour
			Color c = tint;
			if (originalTime - time < fadeIn)
			{
				c.a = alpha * ((originalTime - time) / fadeIn);
			}
			else if (fadeOut)
			{
				c.a = alpha * delta;
			}
			rend.color = c;
			//adjust speed
			if (slowDown)
			{
				currentSpeed = originalSpeed * delta;
			}

			if (rotationDecay)
			{
				currentRotationSpeed = originalRotationSpeed * delta;
			}
			//adjust rotation
			rend.transform.eulerAngles += Vector3.forward * currentRotationSpeed * Time.deltaTime * 60f;
			//adjust position
			rend.transform.position += direction * currentSpeed * Time.deltaTime;
			rend.sortingOrder = (int)((originalTime - time) * 100f);

			done = time <= 0f;
		}
	}
}