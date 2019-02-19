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
}
