using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SaveSystem
{
	public static class SaveLoad
	{
		public static readonly string path = Application.persistentDataPath + "/saves/",
			extension = ".txt";

		public static string CurrentSave { get; set; }

		public static string PathToCurrentSave
		{
			get
			{
				if (CurrentSave == null)
				{
					SaveFile firstSave = SaveReader.GetFirstSaveFile();
					if (firstSave == null)
					{
						CurrentSave = GenerateUniqueSaveName();
					}
					else
					{
						CurrentSave = firstSave.Name;
					}
				}
				return $"{path}{CurrentSave}/";
			}
		}

		public static void SaveText(string key, string textToSave)
		{
			Directory.CreateDirectory(PathToCurrentSave);
			File.WriteAllText(RelativeKeyPath(key), textToSave);
		}

		public static void SaveText(string key, string[] linesToSave)
		{
			Directory.CreateDirectory(PathToCurrentSave);
			File.WriteAllLines(RelativeKeyPath(key), linesToSave);
		}

		public static string LoadText(string key)
		{
			if (!RelativeSaveFileExists(key)) return default;
			string text = File.ReadAllText(RelativeKeyPath(key));
			return text;
		}

		public static string RelativeKeyPath(string key) => $"{PathToCurrentSave}{key}{extension}";

		public static bool RelativeSaveFileExists(string key) => File.Exists(RelativeKeyPath(key));

		[MenuItem("Save System/Delete All Save Files")]
		public static void DeleteAllSaveFiles()
		{
			DirectoryInfo directory = new DirectoryInfo(path);
			directory.Delete(true);
			Directory.CreateDirectory(path);
		}

		[MenuItem("Save System/Open Save File Folder")]
		public static void OpenSaveFileFolder()
		{
			if (!Directory.Exists(path)) return;
			Process.Start(path);
		}

		/// <summary>
		/// Deletes file with name in the current save's folder.
		/// Note: If the file is in a subfolder, include the subfolder path.
		/// An extension is automatically appended so don't include it.
		/// </summary>
		/// <param name="filename"></param>
		public static void DeleteSaveFile(string filename)
		{
			//add the path behind the filename
			string fullPath = $"{PathToCurrentSave}{filename}{extension}";
			//check to see if file exists
			if (!File.Exists(fullPath)) return;
			//get the file info
			FileInfo file = new FileInfo(fullPath);
			//delete it
			file.Delete();
		}

		public static string GenerateUniqueSaveName()
		{
			int count = 0;
			while (Directory.Exists($"{path}{count}"))
			{
				count++;
			}

			return count.ToString();
		}
	}
}