using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
	private const int reserveSize = 200;
	private const int largeDistance = 500;
	private const int directions = 4;
	private List<Sprite> types = new List<Sprite>();
	private int ViewDistance { get { return CameraCtrl.camCtrl.TotalViewRange; } }

	private const int poolSize = 1800;
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

	private void Awake()
	{
		ReserveListCapacity();
		sceneryHolder = new GameObject("Scenery Holder").transform;
		sceneryHolder.gameObject.layer = backgroundLayer;
		backgroundLayer = LayerMask.NameToLayer("BackgroundImage");
		FillPool();
		CreateStarSystems();
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

	private void CreateStarSystems()
	{
		Vector2Int starNumRange = new Vector2Int((int)Mathf.Pow(2, starPowerRange.x), (int)Mathf.Pow(2, starPowerRange.y));
		//create textures
		for (int i = 0; i < variety; i++)
		{
			StartCoroutine(GenerateTexture(starNumRange));
		}
	}

	private IEnumerator GenerateTexture(Vector2Int starNumRange)
	{
		float power = Random.Range(starPowerRange.x, starPowerRange.y);
		int numStars = Random.Range(starNumRange.x, starNumRange.y);
		float padding = (numStars - starNumRange.x) / (starNumRange.y - starNumRange.x) * (texturePaddingRange.y - texturePaddingRange.x) + texturePaddingRange.x;

		Star[] stars = new Star[numStars];
		for (int i = 0; i < stars.Length; i++)
		{
			stars[i] = new Star(textureSize, padding, starSizeRange, power, colorMin, colorMax);
		}

		Texture2D tex = new Texture2D(textureSize.x, textureSize.y, TextureFormat.RGBA32, false);
		for (int i = 0; i < tex.width; i++)
		{
			for (int j = 0; j < tex.height; j++)
			{
				tex.SetPixel(i, j, GetColorOfPixel(i, j, stars));
			}

			//if (i % textureSize.y / 60 == 0)
			//{
			//	yield return null;
			//}
		}
		tex.Apply();
		types.Add(Sprite.Create(tex, new Rect(new Vector2(0f, 0f), new Vector2(tex.width, tex.height)), Vector2.one / 2f));

		yield return null;
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
				alpha += Mathf.Pow(1 - (distance / stars[i].size), 4f);
				r += stars[i].color.r * alpha;
				b += stars[i].color.b * alpha;
			}
		}
		r = Mathf.Clamp(r, 0f, 255f);
		b = Mathf.Clamp(b, 0f, 255f);
		alpha = Mathf.Clamp01(alpha) * 255f;

		return new Color32((byte)r, (byte)(Mathf.Min(r, b)), (byte)b, (byte)alpha);
	}

	class Star
	{
		public Vector2 pos;
		public Color color;
		public float size;

		public Star(Vector2Int textureSize, float padding, Vector2 sizeRange, float power, int colorMin, int colorMax)
		{
			Vector2 boundsSize = new Vector2(textureSize.x / 2 * padding, textureSize.y / 2 * padding);
			size = Mathf.Pow(Random.value, power) * (sizeRange.y - sizeRange.x) + sizeRange.x;
			color = new Color(Random.Range(colorMin, colorMax), 0f, Random.Range(colorMin, colorMax), 255f);

			float randomDirection = Random.Range(0, Mathf.PI * 2f);
			Vector2 direction = new Vector2(Mathf.Sin(randomDirection), Mathf.Cos(randomDirection));
			float distance = Random.value * Mathf.Sqrt(boundsSize.x * boundsSize.x + boundsSize.y * boundsSize.y);
			pos = direction * distance + new Vector2(textureSize.x / 2, textureSize.y / 2);
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