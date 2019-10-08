using System;

public static class GameEvents
{
	public static Action<EntityType> OnEntityDestroyed;
	public static void EntityDestroyed(EntityType type) => OnEntityDestroyed?.Invoke(type);

	public static Action<Item.Type, int> OnItemCollected;
	public static void ItemCollected(Item.Type type, int amount) => OnItemCollected?.Invoke(type, amount);
	
	public static Action<Item.Type, int> OnItemCrafted;
	public static void ItemCrafted(Item.Type type, int amount) => OnItemCrafted?.Invoke(type, amount);
	
	public static Action<Item.Type> OnItemUsed;
	public static void ItemUsed(Item.Type type) => OnItemUsed?.Invoke(type);
	
	public static Action<Waypoint> OnWaypointReached;
	public static void WaypointReached(Waypoint waypoint) => OnWaypointReached?.Invoke(waypoint);
	
	public static Action OnSave;
	public static void Save() => OnSave?.Invoke();
}
