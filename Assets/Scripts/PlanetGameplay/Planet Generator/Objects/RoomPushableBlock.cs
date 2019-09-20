using UnityEngine;
using BlockPushPuzzle;

public class RoomPushableBlock : RoomObject
{
	private PushPuzzle blockPushPuzzle;
	private Vector2Int puzzlePos;

	public delegate void PushedEventHandler(Vector2Int direction, float time);
	public event PushedEventHandler OnPushed;
	public delegate void DeactivatedEventHandler();
	public event DeactivatedEventHandler OnDeactivated;

	public bool activated = true;

	public RoomPushableBlock(PushPuzzle blockPushPuzzle, Vector2Int puzzlePos)
	{
		this.blockPushPuzzle = blockPushPuzzle;
		blockPushPuzzle.OnPuzzleCompleted += Deactivate;
		blockPushPuzzle.OnBlockMoved += Move;
		this.puzzlePos = puzzlePos;
	}

	public override ObjType GetObjectType() => ObjType.PushableBlock;

	public void Push(Vector2Int direction)
		=> blockPushPuzzle.PushBlock(puzzlePos, direction);

	private void Move(Vector2Int pos, Vector2Int dir, float time)
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
