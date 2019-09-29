using UnityEngine;
using BlockPushPuzzle;

[System.Serializable]
public class RoomPushableBlock : RoomObject
{
	private PushPuzzle blockPushPuzzle;
	private IntPair puzzlePos;

	public delegate void PushedEventHandler(IntPair direction, float time);
	public event PushedEventHandler OnPushed;
	public delegate void DeactivatedEventHandler();
	public event DeactivatedEventHandler OnDeactivated;

	public bool activated = true;

	public RoomPushableBlock(PushPuzzle blockPushPuzzle, IntPair puzzlePos)
	{
		this.blockPushPuzzle = blockPushPuzzle;
		blockPushPuzzle.OnPuzzleCompleted += Deactivate;
		blockPushPuzzle.OnBlockMoved += Move;
		this.puzzlePos = puzzlePos;
	}

	public override ObjType GetObjectType() => ObjType.PushableBlock;

	public void Push(IntPair direction)
		=> blockPushPuzzle.PushBlock(puzzlePos, direction);

	private void Move(IntPair pos, IntPair dir, float time)
	{
		if (pos != puzzlePos) return;
		puzzlePos += dir;
		OnPushed?.Invoke(dir, time);
		SetPosition(GetPosition() + dir);
	}

	private void Deactivate()
	{
		activated = false;
		OnDeactivated?.Invoke();
	}


}
