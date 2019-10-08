using System.Collections;
using UnityEngine;

public class PlanetRoomPushableBlock : PlanetInteractable
{
	private RoomPushableBlock roomBlock;

	public override void Setup(RoomViewer roomViewer, Room room, RoomObject roomObject,
		PlanetVisualData dataSet)
	{
		base.Setup(roomViewer, room, roomObject, dataSet);

		roomBlock = (RoomPushableBlock)roomObject;
		if (roomBlock.activated)
		{
			roomBlock.OnPushed += Move;
			roomBlock.OnDeactivated += Deactivate;
		}
		else
		{
			Deactivate();
		}
	}

	protected override bool VerifyPlanetActor(PlanetTriggerer actor)
	{
		IntPair actorPosition = actor.RoomObj.GetPosition();
		IntPair actorFacingPosition = actorPosition + actor.MovementBehaviour.DirectionValue;
		IntPair currentPosition = GetPosition();
		//if actor is not facing this object, return false
		if (actorFacingPosition != currentPosition) return false;
		return base.VerifyPlanetActor(actor);
	}

	private void OnDisable()
	{
		roomBlock.OnPushed -= Move;
		roomBlock.OnDeactivated -= Deactivate;
	}

	public void Push(IntPair direction) => roomBlock.Push(direction);

	private void Move(IntPair direction, float time)
	{
		Vector2 position = transform.position;
		position += direction;
		StartCoroutine(MoveToPosition(time, position));
	}

	private IEnumerator MoveToPosition(float time, Vector2 position)
	{
		EnableTrigger(false);
		Vector2 originalPos = transform.position;
		System.Action<float> action = (float delta) =>
		{
			Vector2 move = Vector2.Lerp(originalPos, position, Mathf.Pow(delta, 0.7f));
			transform.position = move;
		};
		yield return new ActionOverTime(time, action);
		EnableTrigger(true);
	}

	private void Deactivate() => EnableTrigger(false);
}
