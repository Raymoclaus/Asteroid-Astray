using UnityEngine;

[RequireComponent(typeof(IPhysicsController))]
public class MovementBehaviour : MonoBehaviour
{
	private IPhysicsController physicsController;
	protected IPhysicsController PhysicsController
		=> physicsController ?? (physicsController = GetComponent<IPhysicsController>());

	public Vector3 GetDirection() => PhysicsController.GetDirection();
}
