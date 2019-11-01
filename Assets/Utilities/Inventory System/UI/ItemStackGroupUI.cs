using System.Collections.Generic;
using UnityEngine;

namespace InventorySystem.UI
{
	public class ItemStackGroupUI : MonoBehaviour
	{
		[SerializeField] private ItemStackUI stackPrefab;

		public void SetStackGroup(List<ItemStack> stacks)
		{
			foreach (Transform child in transform)
			{
				Destroy(child.gameObject);
			}

			for (int i = 0; i < stacks.Count; i++)
			{
				ItemStackUI stackobj = Instantiate(stackPrefab, transform);
				stackobj.SetStack(stacks[i]);
			}
		}
	}
}