using InventorySystem;
using System.Collections.Generic;
using GenericExtensions;
using UnityEngine;

[CreateAssetMenu(fileName = "New Scripted Drops", menuName = "Scriptable Objects/Inventory System/Scripted Drops")]
public class LimitedScriptedDrops : ScriptableObject
{
	public bool scriptedDropsActive = false;
	[SerializeField] private List<LootGroup> items = new List<LootGroup>();

	public LootGroup GetScriptedDrop(IInventoryHolder target)
	{
		if (target.IsNotA<Shuttle>()) return new LootGroup();
		if (items.Count == 0) scriptedDropsActive = false;
		if (!scriptedDropsActive) return new LootGroup();

		int random = Random.Range(1, items.Count);
		if (random >= items.Count)
		{
			random = 0;
		}
		LootGroup stacks = items[random];
		items.RemoveAt(random);
		scriptedDropsActive = items.Count != 0;
		return stacks;
	}
}
