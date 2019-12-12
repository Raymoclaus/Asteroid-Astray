using UnityEngine;

namespace InventorySystem
{
	public class ItemDropper : MonoBehaviour
	{
		[SerializeField] private ChasingItemPickup prefab;

		public void DropItem(ItemObject item, Vector3 location, IInventoryHolder target)
		{
			if (item == ItemObject.Blank) return;

			ChasingItemPickup drop = Instantiate(prefab, location, Quaternion.identity, null);

			drop.SetItem(item);
			drop.SetTarget(target);
		}
	} 
}
