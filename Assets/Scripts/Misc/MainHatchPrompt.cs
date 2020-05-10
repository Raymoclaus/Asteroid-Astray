using System;
using System.Collections.Generic;
using InputHandlerSystem;
using InventorySystem;
using QuestSystem;
using StatisticsTracker;
using TriggerSystem;
using UnityEngine;

public class MainHatchPrompt : MonoBehaviour, IActionMessageReceiver, IWaypointable, IDeliveryReceiver
{
	[SerializeField] private Animator anim;
	[SerializeField] private VicinityWaypoint _waypoint;
	[SerializeField] private BoolStatTracker _shipIsPoweredUpStat;

	private NarrativeManager _narrativeManager;
	private Dictionary<IDeliverer, List<IDelivery>> expectedDeliveries = new Dictionary<IDeliverer, List<IDelivery>>();

	public event Action<IInteractor> OnInteracted;
	public event Action<IDeliverer, IDelivery> OnDeliveryReceived;

	public string UniqueID { get; set; }

	public Vector3 Position
	{
		get => transform.position;
		set => Debug.Log("The position of this object cannot be changed in this way.", gameObject);
	}

	public VicinityWaypoint GetWaypoint() => _waypoint;

	public void Interacted(IInteractor interactor, GameAction action)
	{
		if (!IsPoweredDown)
		{
			if (interactor is IHatchEnterer hatchEnterer)
			{
				BringObjectThroughHatch(hatchEnterer);
			}
		}

		OnInteracted?.Invoke(interactor);

		interactor.Interact(this);
	}

	public void BringObjectThroughHatch(IHatchEnterer obj)
	{
		Open();
		obj.EnterHatch(Position);
	}

	public bool IsPoweredDown => !_shipIsPoweredUpStat.value;

	public bool SetPower(bool active) => _shipIsPoweredUpStat.value = active;

	private NarrativeManager NarrativeManager => _narrativeManager != null
		? _narrativeManager
		: (_narrativeManager = FindObjectOfType<NarrativeManager>());

	private void Open() => anim.SetTrigger("Open");

	public bool IsExpectingDelivery(IDeliverer deliverer, IDelivery delivery)
	{
		if (!expectedDeliveries.ContainsKey(deliverer)) return false;

		foreach (IDelivery d in expectedDeliveries[deliverer])
		{
			if (d.Matches(delivery)) return true;
		}
		return false;
	}

	public void ExpectDelivery(IDeliverer deliverer, IDelivery delivery)
	{
		if (!expectedDeliveries.ContainsKey(deliverer))
		{
			expectedDeliveries.Add(deliverer, new List<IDelivery>());
		}
		
		expectedDeliveries[deliverer].Add(delivery);
	}

	public bool ReceiveDelivery(IDeliverer deliverer, IDelivery delivery)
	{
		if (!IsExpectingDelivery(deliverer, delivery)) return false;
		bool deliveryReceived = RemoveDelivery(deliverer, delivery);
		if (!deliveryReceived) return false;
		OnDeliveryReceived?.Invoke(deliverer, delivery);
		return true;
	}

	private bool RemoveDelivery(IDeliverer deliverer, IDelivery delivery)
	{
		if (!expectedDeliveries.ContainsKey(deliverer)) return false;
		IDelivery toRemove = null;
		foreach (IDelivery d in expectedDeliveries[deliverer])
		{
			if (!d.Matches(delivery)) continue;
			toRemove = d;
			break;
		}
		if (toRemove == null) return false;
		return expectedDeliveries[deliverer].Remove(toRemove);
	}
}