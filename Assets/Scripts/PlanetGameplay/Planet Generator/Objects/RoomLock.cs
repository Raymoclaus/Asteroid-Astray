public class RoomLock : RoomObject
{
	public RoomKey.KeyColour colour;
	public Direction direction;
	private ItemStack unlockRequirement;

	public RoomLock(Room room, string[] lines) : base(room, lines) { }

	public RoomLock(Room room, RoomKey.KeyColour colour,
		Direction direction, ItemStack unlockRequirement) : base(room)
	{
		room.OnChangeExitPosition += AdjustPosition;
		this.colour = colour;
		this.direction = direction;
		this.unlockRequirement = unlockRequirement;
	}

	private void AdjustPosition(Direction direction, IntPair position)
	{
		if (this.direction != direction) return;
		SetPosition(position);
	}

	public void SetDirection(Direction direction) => this.direction = direction;

	public void AttemptUnlock(Triggerer actor)
	{
		if (actor.RequestObject(unlockRequirement))
		{
			CurrentRoom.Unlock(direction);
		}
	}

	public override ObjType ObjectType => ObjType.Lock;

	public override string ObjectName => "Lock";

	public const string SAVE_TAG = "[RoomLock]", SAVE_END_TAG = "[/RoomLock]";
	public override string Tag => SAVE_TAG;
	public override string EndTag => SAVE_END_TAG;

	public override string GetSaveText(int indentLevel)
		=> $"{base.GetSaveText(indentLevel)}" +
		$"{new string('\t', indentLevel)}{colourProp}:{colour}\n" +
		$"{new string('\t', indentLevel)}{directionProp}:{direction}\n" +
		$"{new string('\t', indentLevel)}{unlockRequirementProp}:{unlockRequirement}\n";

	private static readonly string colourProp = "colour";
	private static readonly string directionProp = "direction";
	private static readonly string unlockRequirementProp = "unlockRequirement";
	public override void Load(string[] lines)
	{
		base.Load(lines);

		for (int i = 0; i < lines.Length; i++)
		{
			string line = lines[i];
			string[] props = line.Split(':');

			if (props[0] == colourProp)
			{
				System.Enum.TryParse(props[1], out colour);
				continue;
			}
			if (props[0] == directionProp)
			{
				System.Enum.TryParse(props[1], out direction);
				continue;
			}
			if (props[0] == unlockRequirementProp)
			{
				ItemStack.TryParse(props[1], out unlockRequirement);
				continue;
			}
		}
	}
}
