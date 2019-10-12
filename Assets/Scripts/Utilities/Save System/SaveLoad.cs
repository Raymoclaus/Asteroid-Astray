using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class SaveLoad
{
	private static readonly string path = Application.persistentDataPath + "/saves/",
		extension = ".txt";

	public static void SerializeSave(string key, object objectToSave)
	{
		Directory.CreateDirectory(path);
		BinaryFormatter formatter = new BinaryFormatter();
		using (FileStream stream = new FileStream(path + key + extension, FileMode.Create))
		{
			formatter.Serialize(stream, objectToSave);
		}
	}

	public static T LoadSerialized<T>(string key)
	{
		if (!SaveExists(key)) return default;
		BinaryFormatter formatter = new BinaryFormatter();
		T loadedObject = default;
		using (FileStream stream = new FileStream(KeyPath(key), FileMode.Open))
		{
			loadedObject = (T)formatter.Deserialize(stream);
		}
		return loadedObject;
	}

	public static void SaveText(string appendedPath, string key, string textToSave)
	{
		Directory.CreateDirectory($"{path}{appendedPath}");
		File.WriteAllText(KeyPath(appendedPath, key), textToSave);
	}

	public static string LoadText(string key)
	{
		if (!SaveExists(key)) return default;
		string text = File.ReadAllText(KeyPath(key));
		text = text.Replace("\t", string.Empty);
		return text;
	}

	private static string KeyPath(string key) => KeyPath(string.Empty, key);

	private static string KeyPath(string appendedPath, string key) => $"{path}{appendedPath}{key}{extension}";

	public static bool SaveExists(string key) => File.Exists(KeyPath(key));

	public static void DeleteAllSaveFiles()
	{
		DirectoryInfo directory = new DirectoryInfo(path);
		directory.Delete(true);
		Directory.CreateDirectory(path);
	}
}
