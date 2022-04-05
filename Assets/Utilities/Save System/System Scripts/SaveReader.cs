using System;
using System.Collections.Generic;
using System.IO;

namespace SaveSystem
{
	public static class SaveReader
	{
		private const string SAVE_FILE_NAME = "file data",
			SAVE_TAG_NAME = "SaveCard",
			FILE_ID_VAR_NAME = "File ID";

		public static List<SaveFile> GetSaves()
		{
			List<SaveFile> saves = new List<SaveFile>();
			IterateValidSaveFileDirectories(di =>
			{
				saves.Add(new SaveFile(di));
				return false;
			});
			return saves;
		}

		/// <summary>
		/// Iterates over each folder in the save file directory.
		/// Folders that don't contain the appropriate save files are skipped.
		/// Invokes the function on every valid save folder until the given function specifies to stop iteration.
		/// </summary>
		/// <param name="function">Method exits if function is null. Return true to stop iteration, or false to continue.</param>
		private static void IterateValidSaveFileDirectories(Func<DirectoryInfo, bool> function)
		{
			if (function == null
				|| !Directory.Exists(SaveLoad.path)) return;

			DirectoryInfo directory = new DirectoryInfo(SaveLoad.path);
			string originalSaveName = SaveLoad.CurrentSave;
			SaveLoad.CurrentSave = null;

			foreach (DirectoryInfo di in directory.EnumerateDirectories())
			{
				SaveLoad.CurrentSave = di.Name;
				bool currentSaveIsValid = VerifyCurrentSave();
				if (!currentSaveIsValid) continue;
				bool endIteration = function.Invoke(di);
				if (endIteration) break;
			}

			SaveLoad.CurrentSave = originalSaveName;
		}

		private static bool VerifyCurrentSave()
		{
			bool saveExists = SaveLoad.RelativeSaveFileExists(SAVE_FILE_NAME);
			if (!saveExists) return false;

			SaveTag saveTag = new SaveTag(SAVE_TAG_NAME);
			DataModule module = UnifiedSaveLoad.GetModuleOfParameter(SAVE_FILE_NAME, saveTag, FILE_ID_VAR_NAME);
			if (module == DataModule.INVALID_DATA_MODULE) return false;

			return true;
		}
		
		public static int GetSaveFileCount()
		{
			int count = 0;

			IterateValidSaveFileDirectories(di =>
			{
				count++;
				return false;
			});

			return count;
		}

		public static bool DirectoryWithNameExists(string dirName)
		{
			bool found = false;

			IterateValidSaveFileDirectories(di =>
			{
				if (di.Name == dirName)
				{
					found = true;
					return true;
				}
				else
				{
					return false;
				}
			});

			return found;
		}

		public static void Save(string fileID)
		{
			//open file
			UnifiedSaveLoad.OpenFile(SAVE_FILE_NAME, true);
			//create main tag
			SaveTag mainTag = new SaveTag(SAVE_TAG_NAME);
			//save file id
			DataModule module = new DataModule(FILE_ID_VAR_NAME, fileID);
			UnifiedSaveLoad.UpdateOpenedFile(SAVE_FILE_NAME, mainTag, module);
			UnifiedSaveLoad.SaveOpenedFile(SAVE_FILE_NAME);
		}
	} 
}
