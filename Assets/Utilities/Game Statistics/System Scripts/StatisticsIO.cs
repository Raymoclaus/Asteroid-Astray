using SaveSystem;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace StatisticsTracker
{
	public static class StatisticsIO
	{
		public const string SAVE_FILE_NAME = "Statistics",
			SAVE_TAG_NAME = "Statistics";
		private static Dictionary<string, StatTracker> _trackers;

		private static Dictionary<string, StatTracker> Trackers
		{
			get
			{
				if (_trackers != null) return _trackers;

				_trackers = new Dictionary<string, StatTracker>();

				StatTracker[] stats = Resources.LoadAll<StatTracker>(string.Empty);
				foreach (StatTracker stat in stats)
				{
					AddToDictionary(stat);
				}

				return _trackers;
			}
		}

		[SteamPunkConsoleCommand(command = "resetStats", info = "Resets all stats to default values and saves.")]
		public static void ResetAllStats()
		{
			foreach (StatTracker stat in Trackers.Values)
			{
				stat.ResetToDefault();
			}
		}

		[SteamPunkConsoleCommand(command = "resetStat", info = "Resets a specific stat.")]
		public static void ResetStat(string statName)
		{
			StatTracker stat = GetTracker(statName);
			if (stat == null) return;
			stat.ResetToDefault();
		}

		[SteamPunkConsoleCommand(command = "statList", info = "Prints a list of available stat names.")]
		public static void StatList()
		{
			SteamPunkConsole.WriteLine($"List of available stat names");
			SteamPunkConsole.WriteLine($"============================");
			foreach (StatTracker stat in Trackers.Values)
			{
				SteamPunkConsole.WriteLine(stat.SaveTagName);
			}
		}

		public static string Sanitise(string input) => input.ToLower().Replace(" ", "_");

		[SteamPunkConsoleCommand(command = "setStat", info = "Sets the value of the stat with the given name to the given value.")]
		public static bool SetStat(string statName, string value)
		{
			StatTracker stat = GetTracker(statName);
			if (stat == null) return false;
			bool successful = stat.TryParse(value);
			if (successful)
			{
				SteamPunkConsole.WriteLine($"{stat.SaveTagName} set to {value}.");
				return true;
			}
			else
			{
				SteamPunkConsole.WriteLine($"{stat.SaveTagName} could not be set to {value}");
				return false;
			}
		}

		public static StatTracker GetTracker(string parameterName)
		{
			parameterName = Sanitise(parameterName);

			if (Trackers.ContainsKey(parameterName))
			{
				return Trackers[parameterName];
			}
			else
			{
				return null;
			}
		}

		private static void AddToDictionary(StatTracker tracker)
		{
			if (tracker == null) return;
			if (Trackers.ContainsKey(tracker.SaveTagName)) return;
			Trackers.Add(tracker.SaveTagName, tracker);
		}

#if UNITY_EDITOR
		[MenuItem("Game Statistics/Save")]
#endif
		public static void Save()
		{
			SteamPunkConsole.WriteLine("StatTrackers: Begin Saving");

			//open file
			UnifiedSaveLoad.OpenFile(SAVE_FILE_NAME, true);
			//create main tag
			SaveTag mainTag = new SaveTag(SAVE_TAG_NAME);
			//iterate over the statistics
			foreach (StatTracker stat in Trackers.Values)
			{
				stat.Save(SAVE_FILE_NAME, mainTag);
			}
			//save file
			UnifiedSaveLoad.SaveOpenedFile(SAVE_FILE_NAME);
			
			SteamPunkConsole.WriteLine("StatTrackers: Finished Saving");
		}

#if UNITY_EDITOR
		[MenuItem("Game Statistics/Load")]
#endif
		public static void Load()
		{
			SteamPunkConsole.WriteLine("StatTrackers: Begin Loading");

			//reset defaults
			ResetAllStats();
			//create main tag
			SaveTag mainTag = new SaveTag(SAVE_TAG_NAME);
			//iterate contents of file
			UnifiedSaveLoad.IterateTagContents(
				SAVE_FILE_NAME,
				mainTag,
				module => ApplyData(module),
				subtag => CheckSubtag(SAVE_FILE_NAME, subtag));

			SteamPunkConsole.WriteLine("StatTrackers: Loading Successful");
		}

		private static bool ApplyData(DataModule module)
		{
			if (!Trackers.ContainsKey(module.parameterName)) return false;
			StatTracker stat = GetTracker(module.parameterName);
			stat.ApplyData(module);
			return true;
		}

		private static bool CheckSubtag(string filename, SaveTag subtag)
		{
			return false;
		}
	}
}