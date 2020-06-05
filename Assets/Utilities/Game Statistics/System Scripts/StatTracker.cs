using SaveSystem;
using System;
using UnityEngine;

namespace StatisticsTracker
{
	public abstract class StatTracker : ScriptableObject
	{
		public abstract Type FieldType { get; }

		/// <summary>
		/// Parses the string and sets the value if parsing was successful.
		/// </summary>
		/// <param name="valueString"></param>
		/// <returns>Returns whether parsing was successful</returns>
		public abstract bool TryParse(string valueString);

		/// <summary>
		/// Returns a string representation of the stat's data.
		/// </summary>
		public abstract string ValueString { get; }

		public abstract void ResetToDefault();

		public string SaveTagName => StatisticsIO.Sanitise(name);

		public virtual void Save(string filename, SaveTag parentTag)
		{
			//save value
			DataModule module = new DataModule(SaveTagName, FieldType.ToString(), ValueString);
			UnifiedSaveLoad.UpdateOpenedFile(filename, parentTag, module);
		}

		public virtual bool ApplyData(DataModule module)
		{
			if (module.parameterName == SaveTagName)
			{
				bool foundVal = TryParse(module.data);
				if (!foundVal)
				{
					Debug.Log("Value data could not be parsed.");
				}
			}
			else
			{
				return false;
			}

			return true;
		}

		public virtual bool CheckSubtag(string filename, SaveTag subtag)
		{
			return false;
		}
	}
}
