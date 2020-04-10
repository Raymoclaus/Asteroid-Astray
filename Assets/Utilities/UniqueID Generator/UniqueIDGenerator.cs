using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SaveSystem;

public static class UniqueIDGenerator
{
	private static Dictionary<string, IUnique> uniqueIDs = new Dictionary<string, IUnique>();
	private static Random r = new Random();
	private const int MIN_LENGTH = 8;

	/// <summary>
	/// Adds an ID to the list that is not attached to an object.
	/// </summary>
	/// <param name="ID"></param>
	/// <returns></returns>
	private static bool AddID(string ID)
	{
		if (IDExists(ID)) return false;
		uniqueIDs.Add(ID, null);
		return true;
	}

	/// <summary>
	/// Adds an object and its ID to the uniqueIDs list.
	/// </summary>
	/// <param name="obj"></param>
	/// <returns>Returns false if obj is null or the ID already exists.
	/// Returns true if the object and its ID already exists,
	/// or if the object was successfully added to the list.</returns>
	public static bool AddObject(IUnique obj)
	{
		if (obj == null) return false;
		if (obj.UniqueID == null)
		{
			obj.UniqueID = GenerateUniqueID();
		}
		else if (IDExists(obj.UniqueID))
		{
			return GetObjectByID(obj.UniqueID) == obj;
		}
		
		uniqueIDs.Add(obj.UniqueID, obj);
		return true;
	}
	
	public static IUnique GetObjectByID(string ID)
	{
		if (!IDExists(ID)) return null;
		return uniqueIDs[ID];
	}

	public static bool IDHasObject(string ID)
	{
		return GetObjectByID(ID) != null;
	}

	/// <summary>
	/// Sets an ID to be paired with the given object.
	/// The ID must already exist in the list.
	/// The object must already have a matching UniqueID property.
	/// If an object is already paired with an ID, then the new object will replace it, and the old object's ID will be set to null.
	/// </summary>
	/// <param name="ID"></param>
	/// <param name="obj"></param>
	/// <returns>Returns false if conditions not met and nothing is changed.</returns>
	public static bool SetObjectToID(string ID, IUnique obj)
	{
		if (!IDExists(ID)) return false;
		if (obj.UniqueID != ID) return false;

		IUnique oldObj = uniqueIDs[ID];
		if (oldObj != null)
		{
			oldObj.UniqueID = null;
		}

		uniqueIDs[ID] = obj;
		return true;
	}

	/// <summary>
	/// Removes an object from the uniqueIDs list.
	/// </summary>
	/// <param name="obj"></param>
	/// <returns>Returns false if obj is null,
	/// obj's ID is already taken by another object
	/// or the ID doesn't exist in the list.</returns>
	public static bool RemoveObj(IUnique obj)
	{
		if (obj == null) return false;
		if (GetObjectByID(obj.UniqueID) != obj) return false;
		return uniqueIDs.Remove(obj.UniqueID);
	}

	/// <summary>
	/// Removes an ID from the list.
	/// If an object is paired with the ID then it's UniqueID property is set to null.
	/// </summary>
	/// <param name="ID"></param>
	/// <returns>Returns false if ID doesn't exist.</returns>
	public static bool RemoveID(string ID)
	{
		if (!IDExists(ID)) return false;
		IUnique obj = uniqueIDs[ID];
		if (obj != null)
		{
			obj.UniqueID = null;
		}
		return uniqueIDs.Remove(ID);
	}

	public static bool IDExists(string ID)
	{
		return uniqueIDs.ContainsKey(ID);
	}

	/// <summary>
	/// Creates a unique ID that meets certain requirements.
	/// </summary>
	/// <returns>Returns an ID as a string.</returns>
	private static string GenerateUniqueID()
	{
		StringBuilder builder = new StringBuilder();

		do
		{
			builder.Append(RandomAsciiGenerator.GetRandomChar(true, true, true, false, false, false));
		} while (!MeetsMinimumRequirements(builder.ToString()));

		return builder.ToString();
	}

	public static bool MeetsMinimumRequirements(string ID)
		=> !IDExists(ID)
		   && ID.Length >= MIN_LENGTH;

	private const string FILE_NAME = "Generated IDs.txt";

	public static void Save()
	{
		string path = $"{SaveLoad.PathToCurrentSave}{FILE_NAME}";
		string[] IDs = uniqueIDs.Keys.ToArray();
		File.WriteAllLines(path, uniqueIDs.Keys);
	}

	public static void Load()
	{
		string path = $"{SaveLoad.PathToCurrentSave}{FILE_NAME}";
		uniqueIDs.Clear();

		foreach (string line in File.ReadLines(path))
		{
			AddID(line);
		}
	}
}
