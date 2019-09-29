using System.Collections.Generic;
using UnityEngine;

public abstract class PlanetVicinityTrigger : PlanetRoomObject
{
	[SerializeField] private VicinityTrigger trigger;

	protected List<PlanetTriggerer> nearbyActors = new List<PlanetTriggerer>();

	private void OnEnable()
	{
		trigger.OnEnterTrigger += Triggered;
		trigger.OnExitedTrigger += UnTriggered;
	}

	private void OnDisable()
	{
		trigger.OnEnterTrigger -= Triggered;
		trigger.OnExitedTrigger -= UnTriggered;
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
