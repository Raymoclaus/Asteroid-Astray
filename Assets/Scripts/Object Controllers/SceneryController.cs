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
	private bool ready;

	private List<List<List<List<CosmicItem>>>> items;
	private const int reserveSize = 100;
	private const int largeDistance = 500;
	private const int directions = 4;
	public List<Sprite> types, lessFrequentTypes;
	//less frequent types will appear (1f - commonTypeFrequency  x100)% of the time
	[Range(0f, 1f)]
	public float commonTypeFrequency = 0.9f;
	public Vector2 imageBrightnessRange;
	[SerializeField]
	private BgCamTracker bgCamTrackerSO;
	private int ViewDistance { get { return Mathf.CeilToInt(bgCamTrackerSO ? bgCamTrackerSO.fieldOfView : 1f); } }

	private const int poolSize = 10000;
	private Queue<StarFieldMaterialPropertyManager> pool = new Queue<StarFieldMaterialPropertyManager>(poolSize);
	private Queue<StarFieldMaterialPropertyManager> active = new Queue<StarFieldMaterialPropertyManager>(poolSize);
	private Queue<StarFieldMaterialPropertyManager> transitionActive = new Queue<StarFieldMaterialPropertyManager>(poolSize);

	private ChunkCoords currentCoords = ChunkCoords.Invalid;
	public Vector2Int cosmicDensity = new Vector2Int(10, 100);
	public float perlinStretchModifier = 1f;
	private Vector2 perlinOffset;
	public float starMinDistance = 400f, starDistanceRange = 200f;
	public Vector2 scaleRange = new Vector2(1f, 4f);
	private Transform sceneryHolder;
	private int backgroundLayer;
	public Color nebulaFadeBackground;
	[SerializeField] private Material customSpriteMaterial;

	[Header("Texture Variables")]
	private bool texturesGenerated;
	private bool found, done, searchCompleted;
	private int searchAmount;
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
		if (!LoadingController.IsLoading)
		{
			StartCoroutine(CreateStarSystems(null));
		}
	}

	private void InitialSetup()
	{
		if (ready) return;

		backgroundLayer = LayerMask.NameToLayer("BackgroundImage");
		sceneryHolder = new GameObject("Scenery Holder").transform;
		sceneryHolder.gameObject.layer = backgroundLayer;
		FillPool();
		ReserveListCapacity();
		perlinOffset = new Vector2(Random.value, Random.value);
		folderPath = Application.dataPath + "/../StarSystemImages";
		lessFrequentImageFolderPath = folderPath + "/LessFrequentImages";
		ready = true;
	}

	private void Update()
	{
		CheckCoords();
	}

	private void OnDestroy()
	{
		if (sceneryHolder) Destroy(sceneryHolder.gameObject);
	}

	private void CheckCoords()
	{
		if (!texturesGenerated) return;

		if (bgCamTrackerSO)
		{
			transform.position = bgCamTrackerSO.position;
		}
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
		List<ChunkCoords> coords = EntityNetwork.GetCoordsInRange(
			newCoords, ViewDistance, ignoreLackOfExistenceInGrid: true);
		for (int i = 0; i < coords.Count; i++)
		{
			ChunkCoords c = coords[i];
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
		for (int i = 0; i < Chunk(c).Count; i++)
		{
			CosmicItem item = Chunk(c)[i];
			StarFieldMaterialPropertyManager sfmpm;
			SpriteRenderer rend;
			Transform tr;
			GameObject obj;

			if (transitionActive.Count > 0)
			{
				sfmpm = transitionActive.Dequeue();
				rend = sfmpm.rend;
				tr = sfmpm.transform;
				obj = sfmpm.obj;
			}
			else
			{
				if (pool.Count == 0)
				{
					FillPool();
				}
				sfmpm = pool.Dequeue();
				rend = sfmpm.rend;
				tr = sfmpm.transform;
				obj = sfmpm.obj;
				obj.SetActive(true);
			}

			tr.position = item.pos;
			List<Sprite> listToUse = item.common ? types : lessFrequentTypes;
			if (item.type >= listToUse.Count)
			{
				Debug.Log($"Is common? {item.common}. List size: {listToUse.Count}. Item value: {item.type}.", obj);
			}
			else
			{
				rend.sprite = item.common ? types[item.type] : lessFrequentTypes[item.type];
			}
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
			sfmpm.SetColor(col);
			active.Enqueue(sfmpm);
			tr.localScale = Vector2.one * item.size;
			tr.eulerAngles = Vector3.forward * item.rotation * 45f;
		}

		while (transitionActive.Count > 0)
		{
			StarFieldMaterialPropertyManager sfmpm = transitionActive.Dequeue();
			sfmpm.obj.SetActive(false);
			pool.Enqueue(sfmpm);
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
			bool common = lessFrequentTypes.Count == 0 || Random.value <= commonTypeFrequency;
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

	public void Save()
	{
		CosmicItemFileReader.Save(items);
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
			rend.material = customSpriteMaterial;

			StarFieldMaterialPropertyManager sfmpm = new StarFieldMaterialPropertyManager(rend, obj.transform, obj);
			pool.Enqueue(sfmpm);
		}
	}

	public IEnumerator CreateStarSystems(System.Action a)
	{
		yield return null;

		//ensure this class is setup properly first
		InitialSetup();

		//if textures have already been generated then don't worry about making more
		if (texturesGenerated) yield break;
		

		StartCoroutine(CheckForExistingStars());

		while (!done)
		{
			yield return null;
		}

		//check for existing star system textures
		if (!found)
		{
			Debug.Log("Generating new Star Systems");
			//determine min/max amount of stars per texture
			Vector2Int starNumRange = new Vector2Int((int)Mathf.Pow(2f, starPowerRange.x),
				(int)Mathf.Pow(2f, starPowerRange.y));

			//prepare worker threads
			int expected;
			ThreadPool.GetMinThreads(out freeWorkers, out expected);
			expected = 0;
			freeWorkers = Mathf.Max(1, freeWorkers - 2);
			int maxFreeWorkers = freeWorkers;

			//prepare colour arrays
			Color[][] tex = new Color[variety][];
			for (int i = 0; i < tex.Length; i++)
			{
				tex[i] = new Color[textureSize.x * textureSize.y];
			}
			int completedLines = 0;
			int linesPerJob = 256;

			//prepare star systems
			Star[][] systems = new Star[variety][];
			for (int i = 0; i < systems.Length; i++)
			{
				System.Random rnd = new System.Random();
				float power = (float)rnd.NextDouble() * (starPowerRange.y - starPowerRange.x)
					+ starPowerRange.x;
				int numStars = (int)((float)rnd.NextDouble() * (starNumRange.y - starNumRange.x) + starNumRange.x);
				float padding = (numStars - starNumRange.x) / (starNumRange.y - starNumRange.x)
					* (texturePaddingRange.y - texturePaddingRange.x) + texturePaddingRange.x;
				float[] biasDirections = new float[(int)Mathf.Floor((float)rnd.NextDouble() * 4)];

				for (int j = 0; j < biasDirections.Length; j++)
				{
					biasDirections[j] = (float)rnd.NextDouble() * Mathf.PI * 2f;
				}

				systems[i] = new Star[numStars];
				for (int j = 0; j < systems[i].Length; j++)
				{
					systems[i][j] = new Star(textureSize, padding, starSizeRange, power, colorMin,
						colorMax, rnd, biasDirections);
				}
				yield return null;
			}

			//wait until texture generation is done
			Debug.Log($"Beginning thread work. {maxFreeWorkers} workers available.");
			while (true)
			{
				bool allWorkDone = false;

				//use available workers to generate textures
				for (; freeWorkers > 0; freeWorkers--)
				{
					//which texture are we up to?
					int textureCounter = completedLines / textureSize.y;
					//have we made enough?
					if (textureCounter >= variety)
					{
						allWorkDone = true;
						break;
					}
					//where does the worker start in the texture?
					int start = completedLines % textureSize.y;
					//start working
					Debug.Log($"Starting new work. Texture {textureCounter}, line {start}");
					this.StartCoroutineAsync(GenerateTexture(systems[textureCounter], tex[textureCounter], start, linesPerJob));
					//EZThread.ExecuteInBackground(() => GenerateTexture(systems[textureCounter], tex[textureCounter],
					//	start, linesPerJob));
					completedLines += linesPerJob;
				}
				//if all textures are complete and all workers are free
				if (allWorkDone && freeWorkers == maxFreeWorkers)
				{
					break;
				}
				yield return null;
			}
			Debug.Log("Finished thread work");

			//apply all textures and turn them into sprites
			Directory.CreateDirectory(folderPath);
			Directory.CreateDirectory(lessFrequentImageFolderPath);
			for (int i = 0; i < tex.Length; i++)
			{
				Texture2D t = new Texture2D(textureSize.x, textureSize.y, TextureFormat.RGBA32,
					false);
				t.SetPixels(tex[i]);
				t.Apply();
				byte[] bytes = t.EncodeToPNG();
				File.WriteAllBytes(folderPath + "/starSystem_" + i + ".jpg", bytes);
				types.Add(Sprite.Create(t,
					new Rect(Vector2.zero, new Vector2(textureSize.x, textureSize.y)),
					Vector2.one / 2f));

				if (i % 3 == 0) yield return null;
			}
		}
		else
		{
			Debug.Log("Adding existing Star Systems");
		}
		texturesGenerated = true;

		//run mandatory action, probably to signal that it's finished
		a?.Invoke();
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

	private class Star
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

	private IEnumerator CheckForExistingStars()
	{
		Debug.Log("Checking for existing Star Systems");
		done = false;
		found = false;
		if (!Directory.Exists(folderPath))
		{
			Debug.Log("No existing Star Systems found");
			done = true;
			yield break;
		}
		//get common type images
		StartCoroutine(SearchForImages(folderPath, "*.png", types));
		while (!searchCompleted) yield return null;
		variety -= searchAmount;
		StartCoroutine(SearchForImages(folderPath, "*.jpg", types));
		while (!searchCompleted) yield return null;
		variety -= searchAmount;
		yield return null;
		//get images from folder marked as less frequent
		StartCoroutine(SearchForImages(lessFrequentImageFolderPath, "*.png", lessFrequentTypes));
		while (!searchCompleted) yield return null;
		StartCoroutine(SearchForImages(lessFrequentImageFolderPath, "*.jpg", lessFrequentTypes));
		while (!searchCompleted) yield return null;
		found = variety <= 0;
		done = true;
	}

	private IEnumerator SearchForImages(string path, string filename, List<Sprite> spriteList)
	{
		searchAmount = 0;
		if (!Directory.Exists(path)) yield break;

		searchCompleted = false;
		string[] images = Directory.GetFiles(path, filename);
		for (int i = 0; i < images.Length; i++)
		{
			byte[] bytes = File.ReadAllBytes(images[i]);
			Texture2D t = new Texture2D(1, 1);
			t.LoadImage(bytes);
			t.Apply();
			spriteList.Add(Sprite.Create(t, new Rect(Vector2.zero, new Vector2(t.width, t.height)),
				Vector2.one / 2f));

			if (i % 5 == 0) yield return null;
		}
		searchAmount = images.Length;
		searchCompleted = true;
	}

	private class StarFieldMaterialPropertyManager
	{
		public MaterialPropertyBlock mpb = new MaterialPropertyBlock();
		public SpriteRenderer rend;
		public Transform transform;
		public GameObject obj;

		public StarFieldMaterialPropertyManager(SpriteRenderer rend, Transform transform, GameObject obj)
		{
			this.rend = rend;
			this.transform = transform;
			this.obj = obj;
		}

		public void SetColor(Color col)
		{
			rend.GetPropertyBlock(mpb);
			mpb.SetColor("_Color", col);
			rend.SetPropertyBlock(mpb);
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