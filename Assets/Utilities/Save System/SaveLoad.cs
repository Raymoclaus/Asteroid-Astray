using System.IO;
using UnityEditor;
using UnityEngine;

namespace SaveSystem
{
	public static class SaveLoad
	{
		private static readonly string path = Application.persistentDataPath + "/saves/",
			extension = ".txt";

		public static void SaveText(string key, string textToSave)
		{
			Directory.CreateDirectory(path);
			File.WriteAllText(KeyPath(key), textToSave);
		}

		public static void SaveText(string key, string[] linesToSave)
		{
			Directory.CreateDirectory(path);
			File.WriteAllLines(KeyPath(key), linesToSave);
		}

		public static string LoadText(string key)
		{
			if (!SaveExists(key)) return default;
			string text = File.ReadAllText(KeyPath(key));
			return text;
		}

		public static string KeyPath(string key) => $"{path}{key}{extension}";

		public static bool SaveExists(string key) => File.Exists(KeyPath(key));

		[MenuItem("Save System/Delete All Save Files")]
		public static void DeleteAllSaveFiles()
		{
			DirectoryInfo directory = new DirectoryInfo(path);
			directory.Delete(true);
			Directory.CreateDirectory(path);
		}

		/// <summary>
		/// Deletes file with name in the "saves" folder.
		/// Note: If the file is in a subfolder, include the subfolder path.
		/// An extension is automatically appended so don't include it.
		/// </summary>
		/// <param name="filename"></param>
		public static void DeleteSaveFile(string filename)
		{
			FileInfo file = new FileInfo($"{path}/{filename}");
		}
	}
}