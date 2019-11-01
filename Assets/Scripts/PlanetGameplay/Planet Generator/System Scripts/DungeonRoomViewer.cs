using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Tilemaps;
using CustomDataTypes;

public class DungeonRoomViewer : MonoBehaviour
{
	private Transform objectParent = null;
	[SerializeField] private Tilemap floorMap, floorDetailMap, wallMap;
	[SerializeField] private List<PlanetVisualData> visualDataSets;
	[SerializeField] private Camera cam;
	[SerializeField] private List<AssetReference> prefabAssetReferences;
	private List<AsyncOperationHandle> loaderhandles = new List<AsyncOperationHandle>();
	private List<DungeonRoomObjectComponent> prefabs = new List<DungeonRoomObjectComponent>();
	private PlanetData planetData;

	public delegate void RoomChangedEventHandler(DungeonRoom newRoom, Direction direction);
	public event RoomChangedEventHandler OnRoomChanged;
	public DungeonRoom ActiveRoom { get; private set; }

	private void Awake()
	{
		planetData = new DungeonGenerator().Generate(1f);
		LoadPrefabs();
	}

	private void Update()
	{
		if (loaderhandles.Exists(t => !t.IsDone)) return;
		enabled = false;
		ShowAllRooms(planetData);
	}

	private void LoadPrefabs()
	{
		for (int i = 0; i < prefabAssetReferences.Count; i++)
		{
			AssetReference ar = prefabAssetReferences[i];
			AsyncOperationHandle<GameObject> handle
				= ar.LoadAssetAsync<GameObject>();
			loaderhandles.Add(handle);
			handle.Completed += LoadedPrefab;
		}
	}

	private void LoadedPrefab(AsyncOperationHandle<GameObject> obj)
	{
		GameObject result = obj.Result;
		DungeonRoomObjectComponent droc = result.GetComponent<DungeonRoomObjectComponent>();
		prefabs.Add(droc);
	}

	public void ShowRoom(string areaType, DungeonRoom room, Vector2 offset, bool destroyExisting = true)
	{
		if (destroyExisting)
		{
			RemoveAllTiles();
			Destroy(objectParent.gameObject);
			objectParent = null;
		}
		objectParent = objectParent ?? new GameObject("Room Object Parent").transform;

		DrawTiles(areaType, room, offset);
		DrawRoomObjects(areaType, room, offset);
		SetCameraPosition(room.Center + offset);

		ActiveRoom = room;
	}

	private void RemoveAllTiles()
	{
		foreach (Tilemap tm in GetComponentsInChildren<Tilemap>())
		{
			tm.ClearAllTiles();
		}
	}

	public void ShowAllRooms(PlanetData data)
	{
		RemoveAllTiles();

		List<DungeonRoom> rooms = data.GetRooms();
		for (int i = 0; i < rooms.Count; i++)
		{
			IntPair dimensions = rooms[i].Dimensions;
			IntPair offset = rooms[i].position;
			offset *= dimensions;
			ShowRoom(data.areaType, rooms[i], offset, false);
		}
		SetCameraPosition(data.startRoom.Center);

		ActiveRoom = data.startRoom;
	}

	private void DrawTiles(string type, DungeonRoom room, Vector2 offset)
	{
		PlanetVisualData dataSet = GetVisualDataSet(type);
		List<DungeonRoomTile> tiles = room.Tiles;
		for (int i = 0; i < tiles.Count; i++)
		{
			CreateTile(tiles[i].Position.x + (int)offset.x,
				tiles[i].Position.y + (int)offset.y,
				tiles[i].type, dataSet);
		}
	}

	private void DrawRoomObjects(string type, DungeonRoom room, Vector3 offset)
	{
		List<DungeonRoomObject> objs = room.roomObjects;
		PlanetVisualData dataSet = GetVisualDataSet(type);

		//GameObject puzzleHolder = new GameObject("Block push puzzle holder");
		//Rigidbody2D rb = puzzleHolder.AddComponent<Rigidbody2D>();
		//rb.gravityScale = 0f;
		//rb.bodyType = RigidbodyType2D.Static;
		//puzzleHolder.AddComponent<CompositeCollider2D>();
		//puzzleHolder.transform.parent = objectParent;

		for (int i = 0; i < objs.Count; i++)
		{
			DungeonRoomObject obj = objs[i];
			if (obj.IsCreated) continue;

			string objName = obj.ObjectName;
			Vector3 pos = obj.RoomSpacePosition + offset;
			DungeonRoomObjectComponent newObj = GetObjectToSpawn(objName);
			if (newObj == null) continue;
			newObj.Setup(this, obj);
			newObj.transform.parent = obj.IsPersistent ? null : objectParent;
			if (obj.IsPersistent) obj.IsCreated = true;
		}
	}

	private DungeonRoomObjectComponent GetObjectToSpawn(string objName)
	{
		DungeonRoomObjectComponent prefab
			= prefabs.FirstOrDefault(t => t.objectName == objName);
		if (prefab == null)
		{
			Debug.Log(objName);
			return null;
		}
		return Instantiate(prefab);
	}

	private void CreateTile(int x, int y, DungeonRoomTileType tileType,
		PlanetVisualData dataSet)
	{
		Vector3Int position = new Vector3Int(x, y, 0);
		switch (tileType)
		{
			case DungeonRoomTileType.Wall:
				wallMap.SetTile(position, dataSet.wallTile);
				break;
			case DungeonRoomTileType.Floor:
				floorMap.SetTile(position, dataSet.floorTile);
				float randomVal = UnityEngine.Random.value;
				if (randomVal <= dataSet.detailChance)
				{
					floorDetailMap.SetTile(position, dataSet.floorDetailTile);
				}
				break;
		}
	}

	private PlanetVisualData GetVisualDataSet(string areaType)
	{
		for (int i = 0; i < visualDataSets.Count; i++)
		{
			if (visualDataSets[i].type == areaType) return visualDataSets[i];
		}
		return null;
	}

	private void SetCameraPosition(Vector2 pos)
	{
		if (cam != null)
		{
			cam.transform.position = new Vector3(pos.x, pos.y, cam.transform.position.z);
		}
	}

	public void Go(DungeonRoom from, Direction direction)
	{
		DungeonRoom nextRoom = from.GetRoom(direction);
		if (nextRoom == null || nextRoom == ActiveRoom) return;
		ActiveRoom = nextRoom;
		IntPair offset = ActiveRoom.position * ActiveRoom.Dimensions;
		ShowRoom(planetData.areaType, ActiveRoom, offset, true);
		OnRoomChanged?.Invoke(ActiveRoom, direction);
	}
}
