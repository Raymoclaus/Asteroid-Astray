﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RoomViewer : MonoBehaviour
{
	private Transform objectParent = null;
	[SerializeField] private Tilemap floorMap, floorDetailMap, wallMap;
	[SerializeField] private List<PlanetVisualData> visualDataSets;
	[SerializeField] private PlanetPlayer playerPrefab;
	[SerializeField] private PlanetRoomExitTrigger exitTriggerPrefab;
	[SerializeField] private PlanetRoomKey keyPrefab;
	[SerializeField] private PlanetRoomLock lockPrefab;
	[SerializeField] private PlanetRoomLandingPad landingPadPrefab;
	[SerializeField] private PlanetRoomTileLight tileLightPrefab;
	[SerializeField] private PlanetRoomPushableBlock pushableBlockPrefab;
	[SerializeField] private PlanetRoomGroundButton greenGroundButtonPrefab, redGroundButtonPrefab;
	[SerializeField] private PlanetRoomEnemy spooderPrefab;
	[SerializeField] private PlanetRoomTreasureChest treasureChestPrefab;

	[SerializeField] private Camera cam;

	private PlanetData planetData;

	public delegate void RoomChangedEventHandler(Room newRoom, Direction direction);
	public event RoomChangedEventHandler OnRoomChanged;
	public Room ActiveRoom { get; private set; }

	private void Awake()
	{
		planetData = FindExistingPlanetData() ?? new PlanetGenerator().Generate(1);
		ShowAllRooms(planetData);
		PlanetRoomObject player = CreateObject(playerPrefab, ActiveRoom, null, GetVisualDataSet(planetData.areaType));
		player.transform.position = (Vector2)ActiveRoom.GetWorldSpacePosition() + ActiveRoom.GetCenter() + Vector2.down * 4f;
		player.transform.parent = null;
	}

	private PlanetData FindExistingPlanetData()
	{
		string planetName = SaveLoad.Load<string>("Last visited planet");
		if (planetName == null) return null;
		return SaveLoad.Load<PlanetData>(planetName);
	}

	public void ShowRoom(AreaType areaType, Room room, Vector2 offset, bool destroyExisting = true)
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
		SetCameraPosition(room.GetCenter() + offset);

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

		List<Room> rooms = data.GetRooms();
		for (int i = 0; i < rooms.Count; i++)
		{
			IntPair dimensions = rooms[i].GetDimensions();
			IntPair offset = rooms[i].position;
			offset *= dimensions;
			ShowRoom(data.areaType, rooms[i], offset.ConvertToVector2, false);
		}
		SetCameraPosition(data.startRoom.GetCenter());

		ActiveRoom = data.startRoom;
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

		GameObject puzzleHolder = new GameObject("Block push puzzle holder");
		Rigidbody2D rb = puzzleHolder.AddComponent<Rigidbody2D>();
		rb.gravityScale = 0f;
		rb.bodyType = RigidbodyType2D.Static;
		puzzleHolder.AddComponent<CompositeCollider2D>();
		puzzleHolder.transform.parent = objectParent;

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
				case RoomObject.ObjType.PushableBlock:
					roomObj = CreateObject(pushableBlockPrefab, room, objs[i], dataSet);
					roomObj.transform.parent = puzzleHolder.transform;
					break;
				case RoomObject.ObjType.GreenGroundButton:
					roomObj = CreateObject(greenGroundButtonPrefab, room, objs[i], dataSet);
					break;
				case RoomObject.ObjType.RedGroundButton:
					roomObj = CreateObject(redGroundButtonPrefab, room, objs[i], dataSet);
					break;
				case RoomObject.ObjType.Spooder:
					roomObj = CreateObject(spooderPrefab, room, objs[i], dataSet);
					break;
				case RoomObject.ObjType.TreasureChest:
					roomObj = CreateObject(treasureChestPrefab, room, objs[i], dataSet);
					break;
			}

			roomObj.transform.position =
				objs[i].GetPosition().ConvertToVector2 + offset;
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
				float randomVal = Random.value;
				if (randomVal <= dataSet.detailChance)
				{
					floorDetailMap.SetTile(position, dataSet.floorDetailTile);
				}
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
		newObj.Setup(this, room, roomObj, dataSet);
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

	public void Go(Direction direction)
	{
		Room nextRoom = ActiveRoom.GetRoom(direction);
		if (nextRoom == null) return;
		ActiveRoom = nextRoom;
		IntPair offset = ActiveRoom.position * ActiveRoom.GetDimensions();
		ShowRoom(planetData.areaType, ActiveRoom, offset.ConvertToVector2, true);
		OnRoomChanged?.Invoke(ActiveRoom, direction);
	}
}
