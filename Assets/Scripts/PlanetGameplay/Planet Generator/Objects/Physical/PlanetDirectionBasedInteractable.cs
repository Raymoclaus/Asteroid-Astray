using UnityEngine;

public class PlanetDirectionBasedInteractable : PlanetInteractable
{
	private RaycastHit2D[] raycastResults = new RaycastHit2D[1];

	protected override bool VerifyPlanetActor(PlanetTriggerer actor)
	{
		Vector3 actorFacingDirection = actor.FacingDirection;
		Physics2D.RaycastNonAlloc(GetPosition(),
			-actorFacingDirection, raycastResults);
		for (int i = 0; i < raycastResults.Length; i++)
		{
			Transform hit = raycastResults[i].transform;
			Debug.Log(hit.name, hit);
			if (hit.IsChildOf(transform)) return true;
		}
		return false;
	}
}
