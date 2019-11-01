using System;
using CustomDataTypes;
using TriggerSystem;
using TriggerSystem.MessageReceivers;
using UnityEngine;

[RequireComponent(typeof(DungeonRoomObjectComponent))]
public class PlanetExitTrigger : InteractableObject
{
	private DungeonRoomObjectComponent droc;
	public DungeonRoomObjectComponent Droc => droc != null ? droc
		: (droc = GetComponent<DungeonRoomObjectComponent>());
	[SerializeField] private GameObject invisibleWallPrefab;
	[SerializeField] private Collider2D solidCollider;
	[SerializeField] private SpriteRenderer lockRenderer;
	[SerializeField] private Sprite[] lockSprites;

	private void Start()
	{
		Direction direction = GetDirection;
		Vector3 pos = transform.position + IntPair.GetDirection(direction);
		Instantiate(invisibleWallPrefab, pos, Quaternion.identity, transform);

		if (IsLocked)
		{
			Lock(GetDirection, LockID);
			Room.OnExitUnlocked += Unlock;
		}
		else
		{
			Unlock(GetDirection);
			Room.OnExitLocked += Lock;
		}
	}

	private void Lock(Direction direction, int lockID)
	{
		if (direction != GetDirection) return;

		//set lock image
		lockRenderer.sprite = GetLockSprite(lockID);

		//enable lock image
		lockRenderer.enabled = true;

		//enable lock collider
		solidCollider.enabled = true;
	}

	private void Unlock(Direction direction)
	{
		if (direction != GetDirection) return;

		//disable lock image
		lockRenderer.enabled = false;

		//disable lock collider
		solidCollider.enabled = false;
	}

	protected override void PerformAction(IInteractor interactor)
	{
		interactor.Interact(this);
	}

	private Sprite GetLockSprite(int lockID)
	{
		if (lockID < 0 || lockID >= lockSprites.Length) return null;
		return lockSprites[lockID];
	}

	private Direction GetDirection
		=> (Direction)Droc.Data;

	private DungeonRoom Room
		=> Droc.RoomObject.CurrentRoom;

	private bool IsLocked
		=> Room.IsLocked(GetDirection);

	private int LockID
		=> Room.LockID(GetDirection);
}
