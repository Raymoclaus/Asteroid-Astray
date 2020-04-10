using System.Collections.Generic;
using System.Linq;
using SaveSystem;
using UnityEditor;
using UnityEngine;

namespace StatisticsTracker
{
	public static class StatisticsIO
	{
		public static readonly SaveTag saveTag = new SaveTag("Statistics");

		private static Dictionary<string, StatTracker> trackers = new Dictionary<string, StatTracker>();

		[SteamPunkConsoleCommand(command = "loadStats", info = "Loads the game data")]
		public static void Load()
		{
			SteamPunkConsole.WriteLine("Loading StatTrackers");

			//get file name
			string fileName = UnifiedSaveLoad.SAVE_FILENAME;
			//Get all StatTrackers
			StatTracker[] stats = GetAllStatTrackers();
			//Query UnifiedSaveLoad for each stat tracker name
			for (int i = 0; i < stats.Length; i++)
			{
				//get parameter name
				string pName = stats[i].name;
				Debug.Log(pName);
				//query UnifiedSaveLoad
				string line = UnifiedSaveLoad.GetLineOfParameter(fileName, saveTag, pName);
				if (line == null)
				{
					stats[i].ResetToDefault();
				}
				else
				{
					//convert line to a data module
					DataModule module = UnifiedSaveLoad.ConvertParameterLineToModule(line);
					//send the data to the stat tracker
					bool successful = stats[i].Parse(module.data);
					if (!successful)
					{
						stats[i].ResetToDefault();
					}
				}
			}

			SteamPunkConsole.WriteLine("StatTracker Loading Successful");
		}

		[SteamPunkConsoleCommand(command = "saveStats", info = "Saves the game data")]
		[MenuItem("Game Statistics/Save All")]
		public static void SaveAll()
		{
			//get all statistics scriptable objects
			StatTracker[] stats = GetAllStatTrackers();

			//iterate over the statistics
			for (int i = 0; i < stats.Length; i++)
			{
				StatTracker stat = stats[i];
				DataModule module = new DataModule(
					stat.name,
					stat.FieldType.ToString(),
					stat.ValueString);
				UnifiedSaveLoad.UpdateUnifiedSaveFile(saveTag, module);
				string entry = module.ToString();
				SteamPunkConsole.WriteLine(entry);
			}
			UnifiedSaveLoad.SaveUnifiedSaveFile();
			
			SteamPunkConsole.WriteLine("Save Successful");
		}

		[SteamPunkConsoleCommand(command = "resetStats", info = "Resets all stats to default values and saves.")]
		public static void ResetAllStats()
		{
			StatTracker[] stats = GetAllStatTrackers();
			for (int i = 0; i < stats.Length; i++)
			{
				stats[i].ResetToDefault();
			}

			if (!Application.isEditor)
			{
				SteamPunkConsole.WriteLine("Stats have been reset to default values.");
				SaveAll();
			}
		}

		private static StatTracker[] GetAllStatTrackers()
		{
			StatTracker[] t = Resources.LoadAll<StatTracker>("StatTrackers");
			for (int i = 0; i < t.Length; i++)
			{
				AddToDictionary(t[i]);
			}
			return t;
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
			StatTracker[] stats = Resources.LoadAll<StatTracker>("StatTrackers");
			SteamPunkConsole.WriteLine($"List of available stat names");
			SteamPunkConsole.WriteLine($"============================");
			for (int i = 0; i < stats.Length; i++)
			{
				SteamPunkConsole.WriteLine(stats[i].name.Replace(' ', '_'));
			}
		}

		[SteamPunkConsoleCommand(command = "setStat", info = "Sets the value of the stat with the given name to the given value.")]
		public static void SetStat(string statName, string value)
		{
			StatTracker stat = GetTracker(statName);
			if (stat == null) return;
			bool successful = stat.Parse(value);
			if (successful)
			{
				SteamPunkConsole.WriteLine($"{stat.name} set to {value}.");
			}
		}

		public static StatTracker GetTracker(string parameterName)
		{
			parameterName = parameterName.Replace('_', ' ');
			if (trackers.ContainsKey(parameterName))
			{
				return trackers[parameterName];
			}
			StatTracker stat = Resources.LoadAll<StatTracker>(string.Empty).FirstOrDefault(t => t.name == parameterName);
			if (stat == null)
			{
				SteamPunkConsole.WriteLine($"No tracker with the name, \"{parameterName}\" exists.");
			}
			else
			{
				AddToDictionary(stat);
			}

			return stat;
		}

		private static void AddToDictionary(StatTracker tracker)
		{
			if (tracker == null) return;
			if (trackers.ContainsKey(tracker.name)) return;
			trackers.Add(tracker.name, tracker);
		}
	}
}