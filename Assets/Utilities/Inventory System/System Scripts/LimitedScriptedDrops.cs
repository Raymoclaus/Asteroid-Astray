using SaveSystem;
using StatisticsTracker;
using System.Collections.Generic;
using UnityEngine;

namespace InventorySystem
{
	[CreateAssetMenu(fileName = "New Scripted Drops", menuName = "Scriptable Objects/Inventory System/Scripted Drops")]
	public class LimitedScriptedDrops : ScriptableObject
	{
		[SerializeField] private BoolStatTracker _dropsActiveTracker;
		[SerializeField] private List<LootGroup> itemTable = new List<LootGroup>();
		private List<LootGroup> itemsLeft = new List<LootGroup>();
		private string _intendedInventoryHolderID;

		public LootGroup GetScriptedDrop(IInventoryHolder target)
		{
			if (target.UniqueID != _intendedInventoryHolderID) return default;
			if (itemsLeft.Count == 0) ScriptedDropsIsActive = false;
			if (!ScriptedDropsIsActive) return default;

			int random = Random.Range(1, itemsLeft.Count);
			if (random >= itemsLeft.Count)
			{
				random = 0;
			}
			LootGroup stacks = itemsLeft[random];
			itemsLeft.RemoveAt(random);
			ScriptedDropsIsActive = itemsLeft.Count != 0;
			return stacks;
		}

		public bool ScriptedDropsIsActive
		{
			get => _dropsActiveTracker.Value;
			private set => _dropsActiveTracker.SetValue(value);
		}

		public void ActivateScriptedDrops(string inventoryHolderID)
		{
			_intendedInventoryHolderID = inventoryHolderID;
			ScriptedDropsIsActive = true;
			ResetLeftoverItems();
		}

		public void DeactivateScriptedDrops()
		{
			ScriptedDropsIsActive = false;
		}

		public void Clear()
		{
			itemsLeft.Clear();
		}

		private void ResetLeftoverItems()
		{
			itemsLeft = new List<LootGroup>(itemTable);
		}

		public string SaveTagName => name;

		public void Save(string filename, SaveTag parentTag)
		{
			//create main tag
			SaveTag mainTag = new SaveTag(SaveTagName, parentTag);
			//iterate over loot groups
			for (int i = 0; i < itemsLeft.Count; i++)
			{
				itemsLeft[i].Save(filename, mainTag, i.ToString());
			}
		}

		public bool ApplyData(DataModule module)
		{
			switch (module.parameterName)
			{
				default:
					return false;
			}

			return true;
		}

		public bool CheckSubtag(string filename, SaveTag subtag)
		{
			if (LootGroup.RecogniseTag(subtag))
			{
				LootGroup group = new LootGroup();

				UnifiedSaveLoad.IterateTagContents(
					filename,
					subtag,
					module => group.ApplyData(module),
					st => group.CheckSubtag(filename, st));

				itemsLeft.Add(group);
			}
			else
			{
				return false;
			}

			return true;
		}
	}

}