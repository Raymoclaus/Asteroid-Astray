using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameEvents
{
	public delegate void EntityDestroyedEventHandler(EntityType type);
	public static event EntityDestroyedEventHandler OnEntityDestroyed;
	public static void EntityDestroyed(EntityType type) => OnEntityDestroyed?.Invoke(type);

	public delegate void ItemCollectedEventHandler(Item.Type type, int amount);
	public static event ItemCollectedEventHandler OnItemCollected;
	public static void ItemCollected(Item.Type type, int amount) => OnItemCollected?.Invoke(type, amount);

	public delegate void ItemCraftedEventHandler(Item.Type type, int amount);
	public static event ItemCraftedEventHandler OnItemCrafted;
	public static void ItemCrafted(Item.Type type, int amount) => OnItemCrafted?.Invoke(type, amount);

	public delegate void ItemUsedEventHandler(Item.Type type);
	public static event ItemUsedEventHandler OnItemUsed;
	public static void ItemUsed(Item.Type type) => OnItemUsed?.Invoke(type);

	public delegate void WaypointReachedEventHandler(Vector3 location);
	public static event WaypointReachedEventHandler OnWaypointReached;
	public static void WaypointReached(Vector3 location) => OnWaypointReached?.Invoke(location);

	public static void ClearEvent()
	{
		OnEntityDestroyed = null;
		OnItemCollected = null;
		OnItemCrafted = null;
		OnItemUsed = null;
		OnWaypointReached = null;
	}
}
