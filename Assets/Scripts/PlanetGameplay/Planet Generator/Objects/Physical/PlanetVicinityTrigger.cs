using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(VicinityTrigger))]
public abstract class PlanetVicinityTrigger : PlanetRoomObject
{
	private VicinityTrigger trigger;
	private VicinityTrigger Trigger => trigger ?? (trigger = GetComponent<VicinityTrigger>());

	protected List<PlanetTriggerer> nearbyActors = new List<PlanetTriggerer>();

	private void OnEnable()
	{
		Trigger.OnEnterTrigger += Triggered;
		Trigger.OnExitedTrigger += UnTriggered;
	}

	private void OnDisable()
	{
		Trigger.OnEnterTrigger -= Triggered;
		Trigger.OnExitedTrigger -= UnTriggered;
	}

	protected virtual bool IsTriggered() => nearbyActors.Count > 0;

	private void Triggered(Triggerer actor)
	{
		if (actor is PlanetTriggerer)
		{
			PlanetTriggerer planetActor = (PlanetTriggerer)actor;
			nearbyActors.Add(planetActor);
			PlanetActorTriggered(planetActor);
		}
	}

	protected virtual void PlanetActorTriggered(PlanetTriggerer actor) { }

	private void UnTriggered(Triggerer actor)
	{
		if (actor is PlanetTriggerer)
		{
			PlanetTriggerer planetActor = (PlanetTriggerer)actor;
			nearbyActors.Remove(planetActor);
			PlanetActorUntriggered(planetActor);
		}
	}

	protected virtual void PlanetActorUntriggered(PlanetTriggerer actor) { }
}
