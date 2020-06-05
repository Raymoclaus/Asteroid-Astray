using SaveSystem;
using System.Collections.Generic;
using UnityEngine;

namespace InventorySystem
{
	public static class ScriptedDropsIO
	{
		private const string SAVE_FILE_NAME = "Scripted Drops",
			SAVE_TAG_NAME = "Scripted Drops";

		private static Dictionary<string, LimitedScriptedDrops> _scriptedDrops;

		private static Dictionary<string, LimitedScriptedDrops> ScriptedDrops
		{
			get
			{
				if (_scriptedDrops != null) return _scriptedDrops;

				_scriptedDrops = new Dictionary<string, LimitedScriptedDrops>();

				LimitedScriptedDrops[] scriptedDrops = Resources.LoadAll<LimitedScriptedDrops>(string.Empty);
				foreach (LimitedScriptedDrops lsd in scriptedDrops)
				{
					AddToDictionary(lsd);
				}

				return _scriptedDrops;
			}
		}

		public static LimitedScriptedDrops GetScriptedDrops(string scriptedDropsName)
		{
			if (ScriptedDrops.ContainsKey(scriptedDropsName))
			{
				return ScriptedDrops[scriptedDropsName];
			}
			else
			{
				return null;
			}
		}

		private static void AddToDictionary(LimitedScriptedDrops scriptedDrops)
		{
			if (scriptedDrops == null) return;
			if (ScriptedDrops.ContainsKey(scriptedDrops.SaveTagName)) return;
			ScriptedDrops.Add(scriptedDrops.SaveTagName, scriptedDrops);
		}

		public static void Reset()
		{
			if (_scriptedDrops == null) return;

			_scriptedDrops.Clear();
			_scriptedDrops = null;
		}
		
		public static void Save()
		{
			SteamPunkConsole.WriteLine("Scripted Drops: Begin Saving");

			//open file
			UnifiedSaveLoad.OpenFile(SAVE_FILE_NAME, true);
			//create main tag
			SaveTag mainTag = new SaveTag(SAVE_TAG_NAME);
			//iterate over scripted drops
			foreach (LimitedScriptedDrops scriptedDrops in ScriptedDrops.Values)
			{
				scriptedDrops.Save(SAVE_FILE_NAME, mainTag);
			}
			//save file
			UnifiedSaveLoad.SaveOpenedFile(SAVE_FILE_NAME);

			SteamPunkConsole.WriteLine("Scripted Drops: Finished Saving");
		}

		public static void Load()
		{
			SteamPunkConsole.WriteLine("Scripted Drops: Begin Loading");

			//reset defaults
			Reset();
			//create main tag
			SaveTag mainTag = new SaveTag(SAVE_TAG_NAME);
			//iterate contents of file
			UnifiedSaveLoad.IterateTagContents(
				SAVE_FILE_NAME,
				mainTag,
				module => ApplyData(module),
				subtag => CheckSubtag(SAVE_FILE_NAME, subtag));

			SteamPunkConsole.WriteLine("Scripted Drops: Finished Loading");
		}

		private static bool ApplyData(DataModule module)
		{
			switch (module.parameterName)
			{
				default:
					return false;
			}

			return true;
		}

		private static bool CheckSubtag(string filename, SaveTag subtag)
		{
			LimitedScriptedDrops scriptedDrops = GetScriptedDrops(subtag.TagName);
			if (scriptedDrops == null) return false;
			scriptedDrops.Clear();
			UnifiedSaveLoad.IterateTagContents(
				filename,
				subtag,
				module => scriptedDrops.ApplyData(module),
				st => scriptedDrops.CheckSubtag(filename, st));

			return true;
		}
	} 
}
