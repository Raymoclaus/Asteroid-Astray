using System;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace StatisticsTracker
{
	public static class StatisticsIO
	{
		private const string SAVE_KEY = "statistics";

		[SteamPunkConsoleCommand(command = "loadStats", info = "Loads the game data")]
		public static void Load()
		{
			//get saved text from file with key, default null if no file exists
			string text = SaveLoad.LoadText(SAVE_KEY);
			if (text == null) return;

			//split text up into lines
			string[] lines = text.Split('\n');

			//iterate over lines
			for (int i = 0; i < lines.Length; i++)
			{
				string line = lines[i];
				//split line up into separate strings
				string[] entry = line.Split('|');
				//ignore line if not correct format
				if (entry.Length != 3) continue;

				//get information about the data entry
				string parameterName = entry[0].ToLower();
				string parameterTypeName = entry[1];
				Type parameterType = Type.GetType(parameterTypeName);
				string valueString = entry[2];

				//check and make sure a parameter with that name exists
				StatTracker stat = GetTracker(parameterName);
				if (stat == null)
				{
					SteamPunkConsole.WriteLine($"Error on line {i + 1} \"{line}\": no parameter with name \"{parameterName}\" found.");
					continue;
				}

				//check and make sure field type matches given type
				if (stat.FieldType != parameterType)
				{
					SteamPunkConsole.WriteLine($"Error on line {i + 1} \"{line}\": parameter type \"{parameterTypeName}\" does not match the expected type \"{stat.FieldType}\".");
					continue;
				}

				//set value of matching field
				bool parsedSuccessfully = stat.SetValue(valueString);

				if (!parsedSuccessfully)
				{
					SteamPunkConsole.WriteLine($"Error on line {i + 1} \"{line}\": value \"{valueString}\" could not be parsed as a \"{parameterType}\".");
					continue;
				}
			}

			SteamPunkConsole.WriteLine("Load Successful");
		}

		[SteamPunkConsoleCommand(command = "saveStats", info = "Saves the game data")]
		public static void SaveAll()
		{
			StatTracker[] stats = Resources.LoadAll<StatTracker>("StatTrackers");
			StringBuilder toSave = new StringBuilder();
			for (int i = 0; i < stats.Length; i++)
			{
				StatTracker stat = stats[i];
				string entry = $"{stat.name.ToLower()}|{stat.FieldType}|{stat.ValueString}";
				SteamPunkConsole.WriteLine(entry);
				toSave.AppendLine(entry);
			}

			string result = toSave.ToString();
			SaveLoad.SaveText(SAVE_KEY, result);
			SteamPunkConsole.WriteLine("Save Successful");
		}

		[SteamPunkConsoleCommand(command = "resetStats", info = "Resets all stats to default values and saves.")]
		[MenuItem("Game Statistics/Reset All Stats")]
		public static void ResetAllStats()
		{
			StatTracker[] stats = Resources.LoadAll<StatTracker>("StatTrackers");
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
				SteamPunkConsole.WriteLine(stats[i].name.ToLower().Replace(' ', '_'));
			}
		}

		[SteamPunkConsoleCommand(command = "setStat", info = "Sets the value of the stat with the given name to the given value.")]
		public static void SetStat(string statName, string value)
		{
			StatTracker stat = GetTracker(statName);
			if (stat == null) return;
			bool successful = stat.SetValue(value);
			if (successful)
			{
				SteamPunkConsole.WriteLine($"{stat.name} set to {value}.");
			}
		}

		private static StatTracker GetTracker(string parameterName)
		{
			StatTracker stat = Resources.Load<StatTracker>($"StatTrackers/{parameterName.ToLower().Replace('_', ' ')}");
			if (stat == null)
			{
				SteamPunkConsole.WriteLine($"No stat with name \"{parameterName}\" found. Use statList to get a list of available stat names.");
			}

			return stat;
		}
	}
}