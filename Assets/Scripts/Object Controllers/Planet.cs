using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlanetInteractablePrompt))]
public class Planet : Entity
{
	private PlanetInteractablePrompt trigger;
	private PlanetInteractablePrompt Trigger
		=> trigger ?? (trigger = GetComponent<PlanetInteractablePrompt>());

	public override void ApplyData(EntityData? data)
	{
		base.ApplyData(data);
	}

	public override void DestroySelf(Entity destroyer)
	{
		base.DestroySelf(destroyer);
	}

	public override EntityType GetEntityType()
	{
		return EntityType.Planet;
	}

	public override void Initialise()
	{
		base.Initialise();
	}

	protected override object CreateDataObject()
	{
		return base.CreateDataObject();
	}

	protected override void OnCollisionEnter2D(Collision2D collision)
	{
		base.OnCollisionEnter2D(collision);
	}

	protected override bool ShouldBeVisible()
	{
		return base.ShouldBeVisible();
	}
}
