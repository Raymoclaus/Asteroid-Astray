using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public struct CosmicItem
{
	public byte type;
	public Vector3 pos;
	public double size;
	public byte rotation;

	public CosmicItem(byte type, Vector3 pos, double size, byte rotation)
	{
		this.type = type;
		this.pos = pos;
		this.size = size;
		this.rotation = rotation;
	}
}

public class SceneryController : MonoBehaviour
{
	private List<List<List<List<CosmicItem>>>> items;
	private const int reserveSize = 100;
	private const int largeDistance = 500;
	private const int directions = 4;
	private List<Sprite> types = new List<Sprite>();
	private int ViewDistance { get { return CameraCtrl.camCtrl.TotalViewRange; } }

	private const int poolSize = 900;
	private Queue<GameObject> pool = new Queue<GameObject>(poolSize);
	private Queue<GameObject> active = new Queue<GameObject>(poolSize);
	private Queue<SpriteRenderer> rendPool = new Queue<SpriteRenderer>(poolSize);
	private Queue<SpriteRenderer> rendActive = new Queue<SpriteRenderer>(poolSize);

	private ChunkCoords currentCoords = ChunkCoords.Invalid;
	private List<ChunkCoords> oldCoords = new List<ChunkCoords>();
	public Vector2Int cosmicDensity = new Vector2Int(10, 100);
	public Vector2 scaleRange = new Vector2(1f, 4f);
	private Transform sceneryHolder;
	private int backgroundLayer;

	[Header("Texture Variables")]
	public int variety = 10;
	public Vector2Int textureSize = new Vector2Int(256, 256);
	[Tooltip("Size of each individual star.")]
	public Vector2 starSizeRange = new Vector2(2, 30);
	[Tooltip("Larger numbers are slower to compute. This adds more stars Eg. 2^Number")]
	public Vector2 starPowerRange = new Vector2(5f, 8f);
	[Tooltip("Use this to make sure the sprite isn't cut off at the edges of a texture.")]
	public Vector2 texturePaddingRange = new Vector2(0.3f, 0.65f);
	[Range(0, 255)]
	public int colorMin = 0;
	[Range(0, 255)]
	public int colorMax = 255;
	public float hazeRange = 150f;
	public float hazePower = 2f;
	public float hazeOpacity = 3f;


	private void Awake()
	{
		ReserveListCapacity();
		sceneryHolder = new GameObject("Scenery Holder").transform;
		sceneryHolder.gameObject.layer = backgroundLayer;
		backgroundLayer = LayerMask.NameToLayer("BackgroundImage");
		FillPool();
		StartCoroutine(CreateStarSystems());
	}

	private void Update()
	{
		if (types.Count >= 10)
		{
			ChunkCoords newCoords = new ChunkCoords(transform.position);
			if (newCoords != currentCoords)
			{
				CoordsChanged(newCoords);
				currentCoords = newCoords;
			}
		}
	}

	private void CoordsChanged(ChunkCoords newCoords)
	{
		//return all active objects to the pool
		for (int i = active.Count - 1; i >= 0; i--)
		{
			GameObject obj = active.Dequeue();
			obj.SetActive(false);
			pool.Enqueue(obj);
			rendPool.Enqueue(rendActive.Dequeue());
		}

		//activate or create new items to fill in the scenery
		foreach (ChunkCoords c in EntityNetwork.GetCoordsInRange(newCoords, ViewDistance))
		{
			//create new cosmic items
			if (Chunk(c).Count == 0)
			{
				FillChunk(c);
			}

			SetUpScenery(c);
		}
	}

	private void SetUpScenery(ChunkCoords c)
	{
		foreach(CosmicItem item in Chunk(c))
		{
			GameObject obj = pool.Dequeue();
			active.Enqueue(obj);
			obj.transform.position = item.pos;
			SpriteRenderer rend = rendPool.Dequeue();
			rend.sprite = types[item.type];
			rendActive.Enqueue(rend);
			obj.transform.localScale = Vector2.one * (float)item.size;
			obj.transform.eulerAngles = Vector3.forward * item.rotation * 45f;
			obj.SetActive(true);
		}
	}

	private void FillChunk(ChunkCoords c)
	{
		for (int i = 0; i < Random.Range(cosmicDensity.x, cosmicDensity.y); i++)
		{
			Vector2Pair area = ChunkCoords.GetCellArea(c);
			Vector3 spawnPos = new Vector3(Random.Range(area.A.x, area.B.x), Random.Range(area.A.y, area.B.y), (1f - Mathf.Pow(Random.value, 6f)) * 100f + 500f);
			CosmicItem newItem = new CosmicItem((byte)Random.Range(0, types.Count), spawnPos, Random.Range(scaleRange.x, scaleRange.y), (byte)Random.Range(0, 8));
			Chunk(c).Add(newItem);
		}
	}

	private void ReserveListCapacity()
	{
		items = new List<List<List<List<CosmicItem>>>>(directions);
		for (int dir = 0; dir < directions; dir++)
		{
			items.Add(new List<List<List<CosmicItem>>>(largeDistance));
			for (int x = 0; x < largeDistance; x++)
			{
				items[dir].Add(new List<List<CosmicItem>>(largeDistance));
				for (int y = 0; y < largeDistance; y++)
				{
					items[dir][x].Add(new List<CosmicItem>(reserveSize));
				}
			}
		}
	}

	private void FillPool()
	{
		for (int i = 0; i < poolSize; i++)
		{
			GameObject obj = new GameObject();
			obj.transform.parent = sceneryHolder;
			obj.layer = backgroundLayer;
			obj.SetActive(false);
			pool.Enqueue(obj);

			SpriteRenderer rend = obj.AddComponent<SpriteRenderer>();
			rendPool.Enqueue(rend);
		}
	}

	private IEnumerator CreateStarSystems()
	{
		Color32[][] textures = new Color32[variety][];
		Vector2Int starNumRange = new Vector2Int((int)Mathf.Pow(2, starPowerRange.x), (int)Mathf.Pow(2, starPowerRange.y));
		Thread[] threads = new Thread[variety];
		System.Random rnd = new System.Random();
		//create textures
		for (int i = 0; i < variety; i++)
		{
			threads[i] = new Thread(() => GenerateTexture(starNumRange, out textures[i], rnd));
			threads[i].Start();
		}

		while(CheckThreadsRunning(threads))
		{
			yield return null;
		}

		for (int i = 0; i < variety; i++)
		{
			if (textures[i] == null) continue;
			Texture2D tex = new Texture2D(textureSize.x, textureSize.y, TextureFormat.RGBA32, false);
			tex.SetPixels32(textures[i]);
			tex.Apply();
			types.Add(Sprite.Create(tex, new Rect(Vector2.zero, new Vector2(tex.width, tex.height)), Vector2.one / 2f));
		}
	}

	private bool CheckThreadsRunning(Thread[] threads)
	{
		foreach (Thread t in threads)
		{
			if (t.ThreadState == ThreadState.Running)
			{
				return true;
			}
		}
		return false;
	}

	private void GenerateTexture(Vector2Int starNumRange, out Color32[] tex, System.Random rnd)
	{
		tex = new Color32[textureSize.x * textureSize.y];
		float power = (float)rnd.NextDouble() * (starPowerRange.y - starPowerRange.x) + starPowerRange.x;
		int numStars = (int)((float)rnd.NextDouble() * (starNumRange.y - starNumRange.x) + starNumRange.x);
		float padding = (numStars - starNumRange.x) / (starNumRange.y - starNumRange.x) * (texturePaddingRange.y - texturePaddingRange.x) + texturePaddingRange.x;
		float[] biasDirections = new float[(int)Mathf.Floor((float)rnd.NextDouble() * 4)];

		for (int i = 0; i < biasDirections.Length; i++)
		{
			biasDirections[i] = (float)rnd.NextDouble() * Mathf.PI * 2f;
		}

		Star[] stars = new Star[numStars];
		for (int i = 0; i < stars.Length; i++)
		{
			stars[i] = new Star(textureSize, padding, starSizeRange, power, colorMin, colorMax, rnd, biasDirections);
		}

		for (int i = 0; i < textureSize.x; i++)
		{
			for (int j = 0; j < textureSize.y; j++)
			{
				tex[i * textureSize.y + j] = GetColorOfPixel(i, j, stars);
			}
		}
	}

	private Color32 GetColorOfPixel(int x, int y, Star[] stars)
	{
		float r = 0;
		float b = 0;
		float alpha = 0;

		for (int i = 0; i < stars.Length; i++)
		{
			Vector2 sp = stars[i].pos;
			Vector2 riseRun = new Vector2(Mathf.Abs(sp.y - y), Mathf.Abs(sp.x - x));

			float distance = riseRun.x * riseRun.y + Mathf.Max(riseRun.x, riseRun.y);
			if (distance < stars[i].size)
			{
				float delta = Mathf.Pow(1 - (distance / stars[i].size), 4f);
				alpha += delta;
				r += stars[i].color.r * delta;
				b += stars[i].color.b * delta;
			}

			float hazeDist = Mathf.Sqrt(riseRun.x * riseRun.x + riseRun.y * riseRun.y);
			if (hazeDist < hazeRange)
			{
				float delta = Mathf.Pow(1f - (hazeDist / hazeRange), hazePower);
				alpha += hazeOpacity / 255f * delta;
				r += hazeOpacity * delta;
				b += hazeOpacity * delta;
			}
		}

		r = Mathf.Clamp(r, 0f, 255f);
		b = Mathf.Clamp(b, 0f, 255f);
		alpha = Mathf.Clamp01(alpha) * 255f;
		float g = Mathf.Min(r, b) * 0.9f;

		return new Color32((byte)r, (byte)g, (byte)b, (byte)alpha);
	}

	class Star
	{
		public Vector2 pos;
		public Color color;
		public float size;

		public Star(Vector2Int textureSize, float padding, Vector2 sizeRange, float power, int colorMin, int colorMax, System.Random rnd, float[] biasDirections)
		{
			Vector2 boundsSize = new Vector2(textureSize.x / 2 * padding, textureSize.y / 2 * padding);
			size = Mathf.Pow((float)rnd.NextDouble(), power) * (sizeRange.y - sizeRange.x) + sizeRange.x;
			color = new Color((float)rnd.NextDouble() * (colorMax - colorMin) + colorMin, 0f, (float)rnd.NextDouble() * (colorMax - colorMin) + colorMin, 255f);

			float randomDirection;
			int count = 0;
			do
			{
				randomDirection = (float)rnd.NextDouble() * Mathf.PI * 2f;
				count++;
			} while (!NearBiasDirections(biasDirections, randomDirection) && count < 3);

			Vector2 direction = new Vector2(Mathf.Sin(randomDirection), Mathf.Cos(randomDirection));
			float distance = (float)rnd.NextDouble();
			if (NearBiasDirections(biasDirections, randomDirection))
			{
				distance = Mathf.Pow(distance, 0.8f);
			}
			distance *= Mathf.Min(boundsSize.x, boundsSize.y);
			pos = direction * distance + new Vector2(textureSize.x / 2, textureSize.y / 2);
		}

		private float AngleDifference(float a, float b)
		{
			while (a < 0f || b < 0f)
			{
				a += Mathf.PI * 2f;
				b += Mathf.PI * 2f;
			}
			a = a % (Mathf.PI * 2f);
			b = b % (Mathf.PI * 2f);
			float delta = Mathf.Abs(a - b);
			if (delta > Mathf.PI)
			{
				var temp = Mathf.Max(a, b) - Mathf.PI * 2f;
				return Mathf.Abs(temp - Mathf.Min(a, b));
			}
			else
			{
				return delta;
			}
		}

		private bool NearBiasDirections(float[] biasDirections, float value)
		{
			for (int i = 0; i < biasDirections.Length; i++)
			{
				if (AngleDifference(biasDirections[i], value) < Mathf.PI / 12f)
				{
					return true;
				}
			}
			return false;
		}
	}

	#region Convenient short-hand methods for accessing the grid
	private List<CosmicItem> Chunk(ChunkCoords cc)
	{
		return Column(cc)[cc.Y];
	}

	private List<List<CosmicItem>> Column(ChunkCoords cc)
	{
		return Direction(cc)[cc.X];
	}

	private List<List<List<CosmicItem>>> Direction(ChunkCoords cc)
	{
		return items[(int)cc.Direction];
	}
	#endregion
}