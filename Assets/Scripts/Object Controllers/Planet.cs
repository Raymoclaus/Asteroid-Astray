using UnityEngine;

public class Planet : Entity
{
	[SerializeField] private PlanetInteractablePrompt trigger;

	public override EntityType GetEntityType() => EntityType.Planet;

	public string planetName = "Default Planet Name";
	private int exploredCount = 0;
	public float difficultyModifier = 1f;

	protected override void Awake()
	{
		base.Awake();

		Vector2 originalPos = transform.position;
		float distance = originalPos.magnitude;
		float modifiedDistance = distance / BgCameraController.SCROLL_SPEED - distance;
		Vector2 modifiedPos = originalPos.normalized * modifiedDistance;
		trigger.SetColliderOffset(modifiedPos);

		trigger.OnInteraction += OnInteracted;
	}

	private void OnInteracted(Triggerer actor)
	{
		if (!(actor is ShuttleTriggerer)) return;
		GoToPlanet();
	}

	private void GoToPlanet()
	{
		SaveLoad.Save("Last visited planet", planetName);
		PlanetGenerator generator = new PlanetGenerator();
		PlanetData data = generator.Generate((exploredCount + 1) * Difficulty.DistanceBasedDifficulty(DistanceFromCenter) * difficultyModifier);
		SceneLoader.LoadSceneStatic("PlanetScene");
	}
}
