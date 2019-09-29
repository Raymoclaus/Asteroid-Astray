using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class SaveLoad
{
	private static readonly string path = Application.persistentDataPath + "/saves/",
		extension = ".txt";

	public static void Save(string key, object objectToSave)
	{
		Directory.CreateDirectory(path);
		BinaryFormatter formatter = new BinaryFormatter();
		using (FileStream stream = new FileStream(path + key + extension, FileMode.Create))
		{
			formatter.Serialize(stream, objectToSave);
		}
	}

	public static T Load<T>(string key)
	{
		if (!SaveExists(key)) return default;
		BinaryFormatter formatter = new BinaryFormatter();
		T loadedObject = default;
		using (FileStream stream = new FileStream(path + key + extension, FileMode.Open))
		{
			loadedObject = (T)formatter.Deserialize(stream);
		}
		return loadedObject;
	}

	public static bool SaveExists(string key)
	{
		string filePath = path + key + extension;
		return File.Exists(filePath);
	}

	public static void DeleteAllSaveFiles()
	{
		DirectoryInfo directory = new DirectoryInfo(path);
		directory.Delete(true);
		Directory.CreateDirectory(path);
	}
}
