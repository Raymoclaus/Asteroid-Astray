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
		public abstract bool SetValue(string valueString);

		/// <summary>
		/// Returns a string representation of the stat's data.
		/// </summary>
		public abstract string ValueString { get; }

		public abstract void ResetToDefault();
	} 
}
