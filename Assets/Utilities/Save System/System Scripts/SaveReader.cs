using System.Collections.Generic;
using System.IO;
using StatisticsTracker;
using UnityEditor;
using UnityEngine;

namespace SaveSystem
{
	public static class SaveReader
	{
		[MenuItem("Temp/Read Files")]
		public static List<SaveFile> GetSaves()
		{
			List<SaveFile> saves = new List<SaveFile>();
			DirectoryInfo directory = new DirectoryInfo(SaveLoad.path);
			string parameterName = "story progression";

			foreach (DirectoryInfo di in directory.EnumerateDirectories())
			{
				string saveFilePath = $"{di.Name}/{UnifiedSaveLoad.SAVE_FILENAME}";
				string saveFileFullPath = $"{di.FullName}/{UnifiedSaveLoad.SAVE_FILENAME}{SaveLoad.extension}";
				bool fileExists = File.Exists(saveFileFullPath);
				if (!fileExists) continue;
				Debug.Log(saveFilePath);
				bool saveExists = SaveLoad.SaveExists(saveFilePath);
				if (!saveExists) continue;
				string progressLine = UnifiedSaveLoad.GetLineOfParameter(
					saveFilePath, StatisticsIO.saveTag, parameterName);
				DataModule module = UnifiedSaveLoad.ConvertLineToModule(progressLine);
				Debug.Log($"Name: {module.parameterName}, Value: {module.data}");
				if (module.parameterName == null) continue;
				saves.Add(new SaveFile(saveFileFullPath, di.Name));
			}

			return saves;
		}

		public static int GetSaveFileCount()
		{
			DirectoryInfo directory = new DirectoryInfo(SaveLoad.path);
			string parameterName = "story progression";

			int count = 0;
			foreach (DirectoryInfo di in directory.EnumerateDirectories())
			{
				string saveFilePath = $"{di.Name}/{UnifiedSaveLoad.SAVE_FILENAME}";
				string saveFileFullPath = $"{di.FullName}/{UnifiedSaveLoad.SAVE_FILENAME}{SaveLoad.extension}";
				bool fileExists = File.Exists(saveFileFullPath);
				if (!fileExists) continue;
				Debug.Log(saveFilePath);
				bool saveExists = SaveLoad.SaveExists(saveFilePath);
				if (!saveExists) continue;
				string progressLine = UnifiedSaveLoad.GetLineOfParameter(
					saveFilePath, StatisticsIO.saveTag, parameterName);
				DataModule module = UnifiedSaveLoad.ConvertLineToModule(progressLine);
				Debug.Log($"Name: {module.parameterName}, Value: {module.data}");
				if (module.parameterName == null) continue;
				count++;
			}

			return count;
		}
	} 
}
