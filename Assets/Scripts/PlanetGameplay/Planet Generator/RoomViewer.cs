using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomViewer : MonoBehaviour
{
	private GameObject parentObject = null;
	[SerializeField] private List<PlanetVisualData> visualDataSets;
	[SerializeField] private PlanetFloorTile floorTilePrefab;
	[SerializeField] private PlanetWallTile wallTilePrefab;
	[SerializeField] private PlanetRoomExitTrigger exitTriggerPrefab;
	[SerializeField] private PlanetRoomKey keyPrefab;
	[SerializeField] private PlanetRoomLock lockPrefab;

	[SerializeField] private Camera cam;

	public void ShowRoom(PlanetData data, Room room, Vector2 offset, bool destroyExisting = true)
	{
		if (parentObject != null && destroyExisting)
		{
			Destroy(parentObject);
		}
		parentObject = parentObject ?? new GameObject("Room");

		DrawTiles(data.areaType, room, offset);
		DrawRoomObjects(data.areaType, room, offset);

		SetCameraPosition(room.GetCenter());
	}

	public IEnumerator ShowAllRooms(PlanetData data)
	{
		List<Room> rooms = data.GetRooms();
		for (int i = 0; i < rooms.Count; i++)
		{
			Vector2Int dimensions = rooms[i].GetDimensions();
			Vector2Int offset = rooms[i].position;
			offset.Scale(dimensions);
			ShowRoom(data, rooms[i], offset, false);
			yield return new WaitForSeconds(1f);
		}
		SetCameraPosition(rooms[0].GetCenter());
	}

	private void DrawTiles(AreaType type, Room room, Vector2 offset)
	{
		PlanetVisualData dataSet = GetVisualDataSet(type);
		List<RoomTile> tiles = room.GetTiles();
		for (int i = 0; i < tiles.Count; i++)
		{
			List<Sprite> spriteSet = null;
			PlanetTile tilePrefab = null;
			switch (tiles[i].type)
			{
				case RoomTile.TileType.Floor:
					spriteSet = dataSet.floorTiles;
					tilePrefab = floorTilePrefab;
					break;
				case RoomTile.TileType.UpWall:
					spriteSet = dataSet.topWallTiles;
					tilePrefab = wallTilePrefab;
					break;
				case RoomTile.TileType.UpRightInnerWall:
					spriteSet = dataSet.topRightInnerWallTiles;
					tilePrefab = wallTilePrefab;
					break;
				case RoomTile.TileType.UpRightOuterWall:
					spriteSet = dataSet.topRightOuterWallTiles;
					tilePrefab = wallTilePrefab;
					break;
				case RoomTile.TileType.RightWall:
					spriteSet = dataSet.rightWallTiles;
					tilePrefab = wallTilePrefab;
					break;
				case RoomTile.TileType.DownRightInnerWall:
					spriteSet = dataSet.bottomRightInnerWallTiles;
					tilePrefab = wallTilePrefab;
					break;
				case RoomTile.TileType.DownRightOuterWall:
					spriteSet = dataSet.bottomRightOuterWallTiles;
					tilePrefab = wallTilePrefab;
					break;
				case RoomTile.TileType.DownWall:
					spriteSet = dataSet.bottomWallTiles;
					tilePrefab = wallTilePrefab;
					break;
				case RoomTile.TileType.DownLeftInnerWall:
					spriteSet = dataSet.bottomLeftInnerWallTiles;
					tilePrefab = wallTilePrefab;
					break;
				case RoomTile.TileType.DownLeftOuterWall:
					spriteSet = dataSet.bottomLeftOuterWallTiles;
					tilePrefab = wallTilePrefab;
					break;
				case RoomTile.TileType.LeftWall:
					spriteSet = dataSet.leftWallTiles;
					tilePrefab = wallTilePrefab;
					break;
				case RoomTile.TileType.UpLeftInnerWall:
					spriteSet = dataSet.topLeftInnerWallTiles;
					tilePrefab = wallTilePrefab;
					break;
				case RoomTile.TileType.UpLeftOuterWall:
					spriteSet = dataSet.topLeftOuterWallTiles;
					tilePrefab = wallTilePrefab;
					break;
			}
			CreateTile((float)tiles[i].position.x + offset.x, (float)tiles[i].position.y + offset.y, spriteSet, tilePrefab);
		}
	}

	private void DrawRoomObjects(AreaType type, Room room, Vector2 offset)
	{
		List<RoomObject> objs = room.roomObjects;
		PlanetVisualData dataSet = GetVisualDataSet(type);

		for (int i = 0; i < objs.Count; i++)
		{
			RoomObject.ObjType objType = objs[i].GetObjectType();
			//Debug.Log(objType);

			PlanetRoomObject roomObj = null;

			switch (objType)
			{
				default: continue;
				case RoomObject.ObjType.Lock:
					PlanetRoomLock lockObj =
						(PlanetRoomLock)CreateObject(lockPrefab, room, objs[i], dataSet);
					roomObj = lockObj;
					break;
				case RoomObject.ObjType.Key:
					roomObj = CreateObject(keyPrefab, room, objs[i], dataSet);
					break;
				case RoomObject.ObjType.ExitTrigger:
					PlanetRoomExitTrigger triggerObj =
						(PlanetRoomExitTrigger)CreateObject(exitTriggerPrefab, room, objs[i], dataSet);
					roomObj = triggerObj;
					break;
			}

			roomObj.transform.position = objs[i].position + offset;
		}
	}

	private void CreateTile(float x, float y, List<Sprite> spriteSet, PlanetTile tilePrefab)
	{
		if (spriteSet == null) return;

		//pick randomly from tile set given
		int setLength = spriteSet.Count;
		int randomIndex = Random.Range(0, setLength);
		Sprite randomSprite = spriteSet[randomIndex];

		PlanetTile tile = (PlanetTile)CreateObject(tilePrefab, null, null, null);
		tile.SprRend.sprite = randomSprite;
		Vector2 position = new Vector2(x, y);
		tile.transform.position = position;
	}

	private void CreateExitTrigger(Direction direction)
	{
		PlanetRoomExitTrigger exitTrigger = 
			(PlanetRoomExitTrigger)CreateObject(exitTriggerPrefab, null, null, null);
		exitTrigger.direction = direction;
	}

	private PlanetRoomObject CreateObject(PlanetRoomObject obj, Room room, RoomObject roomObj, PlanetVisualData dataSet)
	{
		PlanetRoomObject newObj = Instantiate(obj);
		newObj.Setup(room, roomObj, dataSet);
		newObj.transform.parent = parentObject.transform;
		return newObj;
	}

	private PlanetVisualData GetVisualDataSet(AreaType type)
	{
		for (int i = 0; i < visualDataSets.Count; i++)
		{
			if (visualDataSets[i].type == type) return visualDataSets[i];
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

	private void AddToParent(GameObject obj) => obj.transform.parent = parentObject.transform;
}
