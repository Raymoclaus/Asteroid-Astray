using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class VicinityTrigger : MonoBehaviour
{
	[SerializeField] private Collider2D col;

	protected List<Triggerer> nearbyActors = new List<Triggerer>();

	public delegate void VicinityTriggerEventHandler(Triggerer actor);
	public VicinityTriggerEventHandler OnEnterTrigger;

	protected virtual void Awake()
	{
		col.isTrigger = true;
		gameObject.layer = LayerMask.NameToLayer("VicinityTrigger");
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		AddActor(collision.attachedRigidbody.GetComponent<Triggerer>());
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		RemoveActor(collision.attachedRigidbody.GetComponent<Triggerer>());
	}

	private void AddActor(Triggerer actor)
	{
		if (actor != null && !nearbyActors.Contains(actor))
		{
			nearbyActors.Add(actor);
			EnterTrigger(actor);
			actor.EnteredTrigger(this);
		}
	}

	private void RemoveActor(Triggerer actor)
	{
		if (actor != null && nearbyActors.Contains(actor))
		{
			nearbyActors.Remove(actor);
			ExitTrigger(actor);
			actor.ExitedTrigger(this);
		}
	}

	protected bool IsTriggerActive() => nearbyActors.Count > 0;

	protected virtual void EnterTrigger(Triggerer actor) { }
	protected virtual void ExitTrigger(Triggerer actor) { }

	public void EnableTrigger(bool enable) => col.enabled = enable;
}
