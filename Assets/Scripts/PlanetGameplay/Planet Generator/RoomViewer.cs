using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RoomViewer : MonoBehaviour
{
	private Transform objectParent = null;
	[SerializeField] private Tilemap floorMap, wallMap;
	[SerializeField] private List<PlanetVisualData> visualDataSets;
	[SerializeField] private PlanetRoomExitTrigger exitTriggerPrefab;
	[SerializeField] private PlanetRoomKey keyPrefab;
	[SerializeField] private PlanetRoomLock lockPrefab;
	[SerializeField] private PlanetRoomLandingPad landingPadPrefab;
	[SerializeField] private PlanetRoomTileLight tileLightPrefab;

	[SerializeField] private Camera cam;

	public void ShowRoom(PlanetData data, Room room, Vector2 offset, bool destroyExisting = true)
	{
		if (destroyExisting)
		{
			RemoveAllTiles();
			Destroy(objectParent.gameObject);
			objectParent = null;
		}
		objectParent = objectParent ?? new GameObject("Room Object Parent").transform;

		DrawTiles(data.areaType, room, offset);
		DrawRoomObjects(data.areaType, room, offset);

		SetCameraPosition(room.GetCenter());
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

		List<Room> rooms = data.GetRooms();
		for (int i = 0; i < rooms.Count; i++)
		{
			Vector2Int dimensions = rooms[i].GetDimensions();
			Vector2Int offset = rooms[i].position;
			offset.Scale(dimensions);
			ShowRoom(data, rooms[i], offset, false);
			//yield return new WaitForSeconds(1f);
		}
		SetCameraPosition(rooms[0].GetCenter());
	}

	private void DrawTiles(AreaType type, Room room, Vector2 offset)
	{
		PlanetVisualData dataSet = GetVisualDataSet(type);
		List<RoomTile> tiles = room.GetTiles();
		for (int i = 0; i < tiles.Count; i++)
		{
			CreateTile(tiles[i].position.x + (int)offset.x,
				tiles[i].position.y + (int)offset.y,
				tiles[i].type, dataSet);
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
					roomObj = CreateObject(lockPrefab, room, objs[i], dataSet);
					break;
				case RoomObject.ObjType.Key:
					roomObj = CreateObject(keyPrefab, room, objs[i], dataSet);
					break;
				case RoomObject.ObjType.ExitTrigger:
					roomObj = CreateObject(exitTriggerPrefab, room, objs[i], dataSet);
					break;
				case RoomObject.ObjType.LandingPad:
					roomObj = CreateObject(landingPadPrefab, room, objs[i], dataSet);
					break;
				case RoomObject.ObjType.TileLight:
					roomObj = CreateObject(tileLightPrefab, room, objs[i], dataSet);
					break;
			}

			roomObj.transform.position = objs[i].position + offset;
		}
	}

	private void CreateTile(int x, int y, RoomTile.TileType tileType,
		PlanetVisualData dataSet)
	{
		Vector3Int position = new Vector3Int(x, y, 0);
		switch (tileType)
		{
			case RoomTile.TileType.Wall:
				wallMap.SetTile(position, dataSet.wallTile);
				break;
			case RoomTile.TileType.Floor:
				floorMap.SetTile(position, dataSet.floorTile);
				break;
		}
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
		newObj.transform.parent = objectParent;
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
}
