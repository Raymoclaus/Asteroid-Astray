using BlockPushPuzzle;

public class RoomPushableBlock : RoomObject
{
	private PushPuzzle blockPushPuzzle;
	private IntPair puzzlePos;

	public delegate void PushedEventHandler(IntPair direction, float time);
	public event PushedEventHandler OnPushed;
	public delegate void DeactivatedEventHandler();
	public event DeactivatedEventHandler OnDeactivated;

	public bool activated = true;

	public RoomPushableBlock(Room room, string[] lines, PushPuzzle blockPushPuzzle) : base(room, lines)
	{
		this.blockPushPuzzle = blockPushPuzzle;
		blockPushPuzzle.SetBlock(puzzlePos, true);
		blockPushPuzzle.OnPuzzleCompleted += Deactivate;
		blockPushPuzzle.OnBlockMoved += Move;
	}

	public RoomPushableBlock(Room room, IntPair position, PushPuzzle blockPushPuzzle,
		IntPair puzzlePos) : base(room, position)
	{
		this.blockPushPuzzle = blockPushPuzzle;
		blockPushPuzzle.OnPuzzleCompleted += Deactivate;
		blockPushPuzzle.OnBlockMoved += Move;
		this.puzzlePos = puzzlePos;
	}

	public override ObjType ObjectType => ObjType.PushableBlock;

	public void Push(IntPair direction)
		=> blockPushPuzzle.PushBlock(puzzlePos, direction);

	private void Move(IntPair pos, IntPair dir, float time)
	{
		if (pos != puzzlePos) return;
		puzzlePos += dir;
		OnPushed?.Invoke(dir, time);
		SetPosition(Position + dir);
	}

	private void Deactivate()
	{
		activated = false;
		OnDeactivated?.Invoke();
	}

	public override string ObjectName => "Pushable Block";

	public override void PrepareForSaving() => blockPushPuzzle.LoadInitialState();

	public const string SAVE_TAG = "[RoomPushableBlock]", SAVE_END_TAG = "[/RoomPushableBlock]";
	public override string Tag => SAVE_TAG;
	public override string EndTag => SAVE_END_TAG;

	public override string GetSaveText(int indentLevel)
		=> $"{base.GetSaveText(indentLevel)}" +
		$"{new string('\t', indentLevel)}{puzzlePosProp}:{puzzlePos}\n" +
		$"{new string('\t', indentLevel)}{activatedProp}:{activated}\n";

	private static readonly string puzzlePosProp = "puzzlePos";
	private static readonly string activatedProp = "activated";
	public override void Load(string[] lines)
	{
		base.Load(lines);

		for (int i = 0; i < lines.Length; i++)
		{
			string line = lines[i];
			string[] props = line.Split(':');

			if (props[0] == puzzlePosProp)
			{
				IntPair.TryParse(props[1], out puzzlePos);
				continue;
			}
			if (props[0] == activatedProp)
			{
				bool.TryParse(props[1], out activated);
				continue;
			}
		}
	}
}
