using UnityEngine;

public interface IInventoryHolder
{
	void GiveItem(Item.Type itemType);
	Transform GetTransform { get; }
}