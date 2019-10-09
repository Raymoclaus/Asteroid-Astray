using UnityEngine;

[RequireComponent(typeof(EntityMovement))]
[RequireComponent(typeof(PlanetRoomObject))]
public abstract class PlanetTriggerer : Triggerer
{
	private EntityMovement movementBehaviour;
	public EntityMovement MovementBehaviour
		=> movementBehaviour
		?? (movementBehaviour = GetComponent<EntityMovement>());
	private PlanetRoomObject roomObj;
	public PlanetRoomObject RoomObj => roomObj ?? (roomObj = GetComponent<PlanetRoomObject>());

	public virtual void Interacted(PlanetInteractable interactable)
	{

	}

	public Vector3 FacingDirection => MovementBehaviour.FacingDirection;
}
