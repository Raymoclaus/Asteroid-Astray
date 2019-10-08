using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class PlanetData : ITextSaveLoader
{
	public string planetName = "Default Planet Name";
	private List<Room> rooms = new List<Room>();
	IntPair startRoomPosition, finalRoomPosition;
	public Room startRoom, finalRoom;
	public AreaType areaType = AreaType.Cave;

	public PlanetData() { }

	public PlanetData(string[] loadText) => Load(loadText);

	public void AddRoom(Room room) => rooms.Add(room);

	public List<Room> GetRooms() => rooms;

	public int GetRoomCount() => rooms.Count;

	public Room GetRoomAtPosition(IntPair position)
	{
		int index = GetIndexOfRoomAtPosition(position);
		return index == -1 ? null : rooms[index];
	}

	private int GetIndexOfRoomAtPosition(IntPair position)
	{
		for (int i = 0; i < rooms.Count; i++)
		{
			if (rooms[i].position == position) return i;
		}
		return -1;
	}

	public void Save(string path, string key)
	{
		string appendedPath = $"{path}/{key}{planetName}/";
		SaveLoad.SaveText(appendedPath, "planetData", this);
	}

	public void PrepareForSaving() { }

	public void FinishedLoading()
	{
		startRoom = GetRoomAtPosition(startRoomPosition);
		finalRoom = GetRoomAtPosition(finalRoomPosition);
	}

	public ITextSaveLoader[] GetObjectsToSave() => rooms.ToArray();

	public const string SAVE_TAG = "[PlanetData]", SAVE_END_TAG = "[/PlanetData]";
	public string Tag => SAVE_TAG;

	public string EndTag => SAVE_END_TAG;

	public string GetSaveText(int indentLevel)
		=> $"{new string('\t', indentLevel)}{planetNameProp}:{planetName}\n" +
		$"{new string('\t', indentLevel)}{areaTypeProp}:{areaType}\n" +
		$"{new string('\t', indentLevel)}{startRoomPositionProp}:{startRoom.position}\n" +
		$"{new string('\t', indentLevel)}{finalRoomPositionProp}:{finalRoom.position}\n";
	
	private static readonly string planetNameProp = "planetName";
	private static readonly string areaTypeProp = "areaType";
	private static readonly string startRoomPositionProp = "startRoomPosition";
	private static readonly string finalRoomPositionProp = "finalRoomPosition";
	public void Load(string[] lines)
	{
		for (int i = 0; i < lines.Length; i++)
		{
			string line = lines[i];

			if (line == Room.SAVE_TAG)
			{
				int end = lines.IndexOfObjectAfterIndex(i, Room.SAVE_END_TAG);
				rooms.Add(new Room(lines.SubArray(i, end), this));
				i = end;
				continue;
			}

			string[] props = line.Split(':');

			if (props[0] == planetNameProp)
			{
				planetName = props[1];
				continue;
			}
			if (props[0] == areaTypeProp)
			{
				System.Enum.TryParse(props[1], out areaType);
				continue;
			}
			if (props[0] == startRoomPositionProp)
			{
				IntPair.TryParse(props[1], out startRoomPosition);
				continue;
			}
			if (props[0] == finalRoomPositionProp)
			{
				IntPair.TryParse(props[1], out finalRoomPosition);
				continue;
			}
		}

		SaveLoad.SignalFinishedLoading(this);
	}
}
