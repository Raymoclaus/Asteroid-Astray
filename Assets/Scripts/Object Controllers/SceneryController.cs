using System.Collections.Generic;
using UnityEngine;

public struct CosmicItem
{
	public int type;
	public double x;
	public double y;
	public double size;

	public CosmicItem(int type, double x, double y, double size)
	{
		this.type = type;
		this.x = x;
		this.y = y;
		this.size = size;
	}
}

public class SceneryController : MonoBehaviour
{
	private List<List<List<List<CosmicItem>>>> items;
	private const int reserveSize = 20;
	private const int largeDistance = 500;
	private const int directions = 4;
	public Sprite[] types;
	private int ViewDistance { get { return CameraCtrl.camCtrl.TotalViewRange; } }
	private const int poolSize = 2000;
	private Queue<GameObject> pool = new Queue<GameObject>(poolSize);
	private Queue<GameObject> active = new Queue<GameObject>(300);
	private ChunkCoords currentCoords = ChunkCoords.Invalid;
	private List<ChunkCoords> oldCoords = new List<ChunkCoords>();
	public int cosmicDensity = 10;
	private Transform sceneryHolder;

	private void Awake()
	{
		//set camera settings so that the background remains a solid color
		Camera.main.clearFlags = CameraClearFlags.SolidColor;
		ReserveListCapacity();
		sceneryHolder = new GameObject("Scenery Holder").transform;
		FillPool();
	}

	private void Update()
	{
		ChunkCoords newCoords = new ChunkCoords(transform.position);
		if (newCoords != currentCoords)
		{
			CoordsChanged(newCoords);
			currentCoords = newCoords;
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
		}

		//get all coords now in view
		List<ChunkCoords> coordsToCheck = EntityNetwork.GetCoordsInRange(newCoords, ViewDistance);

		//activate or create new items to fill in the scenery
		foreach (ChunkCoords c in coordsToCheck)
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
			obj.transform.position = new Vector2((float)item.x, (float)item.y);
			obj.GetComponent<SpriteRenderer>().sprite = types[item.type];
			obj.transform.localScale = Vector2.one * (float)item.size;
			obj.SetActive(true);
		}
	}

	private void FillChunk(ChunkCoords c)
	{
		for (int i = 0; i < Random.Range(0, cosmicDensity); i++)
		{
			Vector2Pair area = ChunkCoords.GetCellArea(c);
			Vector2 spawnPos = new Vector2(Random.Range(area.A.x, area.B.x), Random.Range(area.A.y, area.B.y));
			CosmicItem newItem = new CosmicItem(Random.Range(0, types.Length), spawnPos.x, spawnPos.y, Random.Range(0.5f, 1.5f));
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
			SpriteRenderer rend = obj.AddComponent<SpriteRenderer>();
			rend.sortingLayerName = "Background";
			obj.transform.parent = sceneryHolder;
			obj.SetActive(false);
			pool.Enqueue(obj);
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