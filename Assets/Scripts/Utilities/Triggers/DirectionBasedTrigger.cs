using UnityEngine;

public class DirectionBasedTrigger : VicinityTrigger
{
	[SerializeField] private bool suppressDirectionRequirements;
	public bool SuppressDirectionRequirements
	{
		get => suppressDirectionRequirements;
		set => suppressDirectionRequirements = value;
	}

	private RaycastHit2D[] raycastResults = new RaycastHit2D[5];

	public override bool ShouldAddActor(IActor actor)
	{
		if (!base.ShouldAddActor(actor)) return false;

		if (actor is IDirectionalActor dirActor)
		{
			Vector3 actorFacingDirection = dirActor.FacingDirection;
			Vector2 rayOrigin = PivotPosition;
			Vector3 rayDirection = -actorFacingDirection;
			Physics2D.RaycastNonAlloc(rayOrigin,
				rayDirection, raycastResults);
			Debug.DrawRay(rayOrigin, rayDirection, Color.magenta, 3f);
			for (int i = 0; i < raycastResults.Length; i++)
			{
				Transform hit = raycastResults[i].transform;
				if (hit == null) continue;
				Debug.Log(hit.name, hit);
				if (hit.IsChildOf(dirActor.GetTransform)) return true;
			}
		}
		return false;
	}
}
