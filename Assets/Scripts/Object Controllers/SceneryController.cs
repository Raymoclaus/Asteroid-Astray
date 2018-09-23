using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using CielaSpike;
using System.IO;

public struct CosmicItem
{
	public byte type;
	public Vector3 pos;
	public float size;
	public byte rotation;
	public bool common;

	public CosmicItem(byte type, Vector3 pos, float size, byte rotation, bool common)
	{
		this.type = type;
		this.pos = pos;
		this.size = size;
		this.rotation = rotation;
		this.common = common;
	}
}

public class SceneryController : MonoBehaviour
{
	private string folderPath, lessFrequentImageFolderPath;
	public static SceneryController singleton;
	private bool singletonChecked;

	private List<List<List<List<CosmicItem>>>> items;
	private const int reserveSize = 100;
	private const int largeDistance = 500;
	private const int directions = 4;
	public List<Sprite> types, lessFrequentTypes;
	//less frequent types will appear (1f - commonTypeFrequency  x100)% of the time
	[Range(0f, 1f)]
	public float commonTypeFrequency = 0.9f;
	public Vector2 imageBrightnessRange;
	private int ViewDistance { get { return Mathf.CeilToInt(BgCameraController.bgCam.cam.fieldOfView); } }

	private const int poolSize = 10000;
	private Queue<SpriteRenderer> pool = new Queue<SpriteRenderer>(poolSize);
	private Queue<SpriteRenderer> active = new Queue<SpriteRenderer>(poolSize);
	private Queue<SpriteRenderer> transitionActive = new Queue<SpriteRenderer>(poolSize);

	private ChunkCoords currentCoords = ChunkCoords.Invalid;
	public Vector2Int cosmicDensity = new Vector2Int(10, 100);
	public float perlinStretchModifier = 1f;
	private Vector2 perlinOffset;
	public float starMinDistance = 400f, starDistanceRange = 200f;
	public Vector2 scaleRange = new Vector2(1f, 4f);
	private Transform sceneryHolder;
	private int backgroundLayer;
	public Color nebulaFadeBackground;

	[Header("Texture Variables")]
	private bool texturesGenerated;
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

	private int freeWorkers;

	private void Awake()
	{
		InitialSetup();
	}

	private void InitialSetup()
	{
		if (singletonChecked) return;

		singletonChecked = true;

		if (singleton == null)
		{
			singleton = this;
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			Destroy(gameObject);
			return;
		}

		backgroundLayer = LayerMask.NameToLayer("BackgroundImage");
		sceneryHolder = new GameObject("Scenery Holder").transform;
		sceneryHolder.gameObject.layer = backgroundLayer;
		FillPool();
		ReserveListCapacity();
		perlinOffset = new Vector2(Random.value, Random.value);
		folderPath = Application.dataPath + "/../StarSystemImages";
		lessFrequentImageFolderPath = folderPath + "/LessFrequentImages";
	}

	private void Update()
	{
		CheckCoords();
	}

	private void CheckCoords()
	{
		transform.position = BgCameraController.bgCam.transform.position;
		if (types.Count >= variety)
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
		//send all active objects to the transition pool
		for (int i = active.Count - 1; i >= 0; i--)
		{
			transitionActive.Enqueue(active.Dequeue());
		}

		//activate or create new items to fill in the scenery
		foreach (ChunkCoords c in
			EntityNetwork.GetCoordsInRange(newCoords, ViewDistance, ignoreLackOfExistence: true))
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
		//bool transparent = EntityNetwork.ContainsType(EntityType.Nebula, c, null);

		foreach(CosmicItem item in Chunk(c))
		{
			GameObject obj;
			SpriteRenderer rend;

			if (transitionActive.Count > 0)
			{
				rend = transitionActive.Dequeue();
				obj = rend.gameObject;
			}
			else
			{
				if (pool.Count == 0)
				{
					FillPool();
				}
				rend = pool.Dequeue();
				obj = rend.gameObject;
				obj.SetActive(true);
			}

			obj.transform.position = item.pos;
			rend.sprite = item.common ? types[item.type] : lessFrequentTypes[item.type];
			//Color col = transparent ? nebulaFadeBackground : Color.white;
			Color col = Color.white;
			float delta = (1f - (item.pos.z - starMinDistance) / starDistanceRange) * 0.9f + 0.1f;
			if (item.common)
			{
				col.a *= delta;
			}
			else
			{

				col = Color.Lerp(Color.black, Color.white,
					Mathf.Clamp(delta, imageBrightnessRange.x, imageBrightnessRange.y));
			}
			rend.color = col;
			active.Enqueue(rend);
			obj.transform.localScale = Vector2.one * item.size;
			obj.transform.eulerAngles = Vector3.forward * item.rotation * 45f;
		}

		while (transitionActive.Count > 0)
		{
			SpriteRenderer rend = transitionActive.Dequeue();
			rend.gameObject.SetActive(false);
			pool.Enqueue(rend);
		}
	}

	private void FillChunk(ChunkCoords c)
	{
		ChunkCoords signedCoords = c;
		signedCoords.ConvertToSignedCoords();
		float amount = Mathf.PerlinNoise(c.X * perlinStretchModifier + perlinOffset.x,
			c.Y * perlinStretchModifier + perlinOffset.y);

		float min = Mathf.Min(cosmicDensity.x, cosmicDensity.y);
		float max = Mathf.Max(cosmicDensity.x, cosmicDensity.y);
		amount = amount * (max - min) + min;
		for (int i = 0; i < (int)amount; i++)
		{
			Vector2Pair area = ChunkCoords.GetCellArea(c);
			Vector3 spawnPos = new Vector3(Random.Range(area.A.x, area.B.x), Random.Range(area.A.y, area.B.y),
				(1f - Mathf.Pow(Random.value, 7f * (max / amount))) * starDistanceRange + starMinDistance);
			bool common = Random.value <= commonTypeFrequency;
			List<Sprite> listToChooseFrom = common ? types : lessFrequentTypes;
			CosmicItem newItem = new CosmicItem((byte)Random.Range(0, listToChooseFrom.Count), spawnPos,
				Random.Range(scaleRange.x, scaleRange.y), (byte)Random.Range(0, 8), common);
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
		items = CosmicItemFileReader.Load(items, largeDistance, reserveSize);
	}

	public static void Save()
	{
		CosmicItemFileReader.Save(singleton.items);
	}

	private void FillPool()
	{
		for (int i = 0; i < poolSize; i++)
		{
			GameObject obj = new GameObject();
			obj.transform.parent = sceneryHolder;
			obj.layer = backgroundLayer;
			obj.SetActive(false);

			SpriteRenderer rend = obj.AddComponent<SpriteRenderer>();
			pool.Enqueue(rend);
		}
	}

	public static IEnumerator CreateStarSystems(System.Action a)
	{
		yield return null;

		//ensure this class is setup properly first
		SceneryController sc = singleton;
		sc.InitialSetup();

		//if textures have already been generated then don't worry about making more
		if (sc.texturesGenerated) yield break;
		sc.texturesGenerated = true;

		//check for existing star system textures
		if (!sc.CheckForExistingStars())
		{
			//determine min/max amount of stars per texture
			Vector2Int starNumRange = new Vector2Int((int)Mathf.Pow(2f, sc.starPowerRange.x),
				(int)Mathf.Pow(2f, sc.starPowerRange.y));

			//prepare worker threads
			int expected;
			ThreadPool.GetMinThreads(out sc.freeWorkers, out expected);
			expected = 0;
			sc.freeWorkers = Mathf.Max(1, sc.freeWorkers - 2);
			int maxFreeWorkers = sc.freeWorkers;

			//prepare colour arrays
			Color[][] tex = new Color[sc.variety][];
			for (int i = 0; i < tex.Length; i++)
			{
				tex[i] = new Color[sc.textureSize.x * sc.textureSize.y];
			}
			int completedLines = 0;
			int linesPerJob = 256;

			//prepare star systems
			Star[][] systems = new Star[sc.variety][];
			for (int i = 0; i < systems.Length; i++)
			{
				System.Random rnd = new System.Random();
				float power = (float)rnd.NextDouble() * (sc.starPowerRange.y - sc.starPowerRange.x)
					+ sc.starPowerRange.x;
				int numStars = (int)((float)rnd.NextDouble() * (starNumRange.y - starNumRange.x) + starNumRange.x);
				float padding = (numStars - starNumRange.x) / (starNumRange.y - starNumRange.x)
					* (sc.texturePaddingRange.y - sc.texturePaddingRange.x) + sc.texturePaddingRange.x;
				float[] biasDirections = new float[(int)Mathf.Floor((float)rnd.NextDouble() * 4)];

				for (int j = 0; j < biasDirections.Length; j++)
				{
					biasDirections[j] = (float)rnd.NextDouble() * Mathf.PI * 2f;
				}

				systems[i] = new Star[numStars];
				for (int j = 0; j < systems[i].Length; j++)
				{
					systems[i][j] = new Star(sc.textureSize, padding, sc.starSizeRange, power, sc.colorMin,
						sc.colorMax, rnd, biasDirections);
				}
				yield return null;
			}

			//wait until texture generation is done
			while (true)
			{
				bool allWorkDone = false;

				//use available workers to generate textures
				for (; sc.freeWorkers > 0; sc.freeWorkers--)
				{
					//which texture are we up to?
					int textureCounter = completedLines / sc.textureSize.y;
					//have we made enough?
					if (textureCounter >= sc.variety)
					{
						allWorkDone = true;
						break;
					}
					//where does the worker start in the texture?
					int start = completedLines % sc.textureSize.y;
					//start working
					sc.StartCoroutineAsync(sc.GenerateTexture(systems[textureCounter], tex[textureCounter],
						start, linesPerJob));
					completedLines += linesPerJob;
				}
				//if all textures are complete and all workers are free
				if (allWorkDone && sc.freeWorkers == maxFreeWorkers)
				{
					break;
				}
				yield return null;
			}

			//apply all textures and turn them into sprites
			Directory.CreateDirectory(sc.folderPath);
			for (int i = 0; i < tex.Length; i++)
			{
				Texture2D t = new Texture2D(sc.textureSize.x, sc.textureSize.y, TextureFormat.RGBA32,
					false);
				t.SetPixels(tex[i]);
				t.Apply();
				byte[] bytes = t.EncodeToPNG();
				File.WriteAllBytes(sc.folderPath + "/starSystem_" + i + ".jpg", bytes);
				sc.types.Add(Sprite.Create(t,
					new Rect(Vector2.zero, new Vector2(sc.textureSize.x, sc.textureSize.y)),
					Vector2.one / 2f));
			}
		}

		//fill background with new textures
		sc.CheckCoords(); 

		//run mandatory action, probably to signal that it's finished
		a();
	}

	private IEnumerator GenerateTexture(Star[] stars, Color[] tex, int start,
		int amount)
	{
		for (int i = 0; i < textureSize.x; i++)
		{
			for (int j = start; j < start + amount; j++)
			{
				tex[i * textureSize.y + j] = GetColorOfPixel(i, j, stars);
			}
		}
		freeWorkers++;
		yield return null;
	}

	private Color GetColorOfPixel(int x, int y, Star[] stars)
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
				float delta = Mathf.Pow(1f - (distance / stars[i].size), 4f);
				alpha += delta;
				r += stars[i].color.r / 255f * delta;
				b += stars[i].color.b / 255f * delta;
			}

			float hazeDist = Mathf.Sqrt(riseRun.x * riseRun.x + riseRun.y * riseRun.y);
			if (hazeDist < hazeRange)
			{
				float delta = Mathf.Pow(1f - (hazeDist / hazeRange), hazePower);
				alpha += hazeOpacity / 255f * delta;
				r += hazeOpacity / 255f * delta;
				b += hazeOpacity / 255f * delta;
			}
		}

		r = Mathf.Clamp01(r);
		b = Mathf.Clamp01(b);
		alpha = Mathf.Clamp01(alpha);
		float g = Mathf.Min(r, b) * 0.9f;
		return new Color(r, g, b, alpha);
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
				distance = Mathf.Pow(distance, 0.5f);
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
				if (AngleDifference(biasDirections[i], value) < Mathf.PI / 24f)
				{
					return true;
				}
			}
			return false;
		}
	}

	private bool CheckForExistingStars()
	{
		if (!Directory.Exists(folderPath)) return false;
		//get common type images
		variety -= SearchForImages(folderPath, "*.png", types) + SearchForImages(folderPath, "*.jpg", types);
		//get images from folder marked as less frequent
		SearchForImages(lessFrequentImageFolderPath, "*.png", lessFrequentTypes);
		SearchForImages(lessFrequentImageFolderPath, "*.jpg", lessFrequentTypes);
		return variety <= 0;
	}

	private int SearchForImages(string path, string filename, List<Sprite> spriteList)
	{
		string[] images = Directory.GetFiles(path, filename);
		for (int i = 0; i < images.Length; i++)
		{
			byte[] bytes = File.ReadAllBytes(images[i]);
			Texture2D t = new Texture2D(1, 1);
			t.LoadImage(bytes);
			t.Apply();
			spriteList.Add(Sprite.Create(t, new Rect(Vector2.zero, new Vector2(t.width, t.height)),
				Vector2.one / 2f));
		}
		return images.Length;
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