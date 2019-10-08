using TileLightsPuzzle;
using BlockPushPuzzle;

public class RoomExitTrigger : RoomObject
{
	public Direction direction;

	public RoomExitTrigger(Room room, string[] lines) : base(room, lines) { }

	public RoomExitTrigger(Room room, Direction direction) : base(room)
	{
		room.OnChangeExitPosition += AdjustPosition;
		this.direction = direction;
	}

	private void UnlockExit() => CurrentRoom.Unlock(direction);

	private void AdjustPosition(Direction direction, IntPair position)
	{
		if (this.direction != direction) return;
		SetPosition(position);
	}

	public override ObjType ObjectType => ObjType.ExitTrigger;

	public override string ObjectName => "Exit Trigger";

	public const string SAVE_TAG = "[RoomExitTrigger]", SAVE_END_TAG = "[/RoomExitTrigger]";
	public override string Tag => SAVE_TAG;
	public override string EndTag => SAVE_END_TAG;

	public override string GetSaveText(int indentLevel)
		=> $"{base.GetSaveText(indentLevel)}" +
		$"{new string('\t', indentLevel)}{directionProp}:{direction}\n";

	private static readonly string directionProp = "direction";
	public override void Load(string[] lines)
	{
		base.Load(lines);

		for (int i = 0; i < lines.Length; i++)
		{
			string line = lines[i];
			string[] props = line.Split(':');

			if (props[0] == directionProp)
			{
				System.Enum.TryParse(props[1], out direction);
				continue;
			}
		}
	}
}
