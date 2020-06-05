using SaveSystem;
using System;
using System.Collections.Generic;

namespace InventorySystem
{
	[Serializable]
	public struct LootGroup
	{
		public List<Loot> group;

		public int Count => group?.Count ?? 0;

		public bool IsEmpty => Count == 0;

		public List<ItemStack> GetStacks
		{
			get
			{
				List<ItemStack> stacks = new List<ItemStack>();

				for (int i = 0; i < Count; i++)
				{
					ItemStack stack = group[i].GetStack();
					stacks.Add(stack);
				}

				return stacks;
			}
		}

		public void AddLootToGroup(Loot loot)
		{
			if (group == null)
			{
				group = new List<Loot>();
			}

			group.Add(loot);
		}

		private const string SAVE_TAG_NAME = "Loot Group",
			COUNT_VAR_NAME = "Count";

		public void Save(string filename, SaveTag parentTag, string modifier)
		{
			//create main tag
			SaveTag mainTag = new SaveTag($"{SAVE_TAG_NAME}:{modifier}", parentTag);
			//save group size
			DataModule module = new DataModule(COUNT_VAR_NAME, Count);
			UnifiedSaveLoad.UpdateOpenedFile(filename, mainTag, module);
			//iterate over loot groups
			for (int i = 0; i < Count; i++)
			{
				group[i].Save(filename, mainTag, i.ToString());
			}
		}

		public static bool RecogniseTag(SaveTag tag) => tag.TagName.StartsWith(SAVE_TAG_NAME);

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
			if (Loot.RecogniseTag(subtag))
			{
				Loot loot = new Loot();

				UnifiedSaveLoad.IterateTagContents(
					filename,
					subtag,
					module => loot.ApplyData(module),
					st => loot.CheckSubtag(filename, st));

				AddLootToGroup(loot);
			}
			else
			{
				return false;
			}

			return true;
		}
	}
}