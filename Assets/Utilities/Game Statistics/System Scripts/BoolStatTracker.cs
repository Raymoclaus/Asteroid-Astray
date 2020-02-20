using System;
using UnityEngine;

namespace StatisticsTracker
{
	[CreateAssetMenu(fileName = "New Bool StatTracker", menuName = "Scriptable Objects/Stat Tracker/Bool StatTracker")]
	public class BoolStatTracker : StatTracker
	{
		public event Action<bool, bool> OnValueUpdated;
		public bool value;
		public bool defaultValue;

		public override Type FieldType => value.GetType();

		public override string ValueString => value.ToString();

		public override bool Parse(string valueString)
		{
			bool successful = bool.TryParse(valueString, out bool val);
			if (!successful)
			{
				SteamPunkConsole.WriteLine($"value \"{valueString}\" could not be parsed as a \"{FieldType}\".");
				return false;
			}

			SetValue(val);
			return true;
		}

		public void SetValue(bool val)
		{
			bool oldVal = value;
			value = val;
			if (oldVal != value)
			{
				OnValueUpdated?.Invoke(oldVal, value);
			}
		}

		public override void ResetToDefault() => SetValue(defaultValue);

		public void ToggleValue() => SetValue(!value);

		public bool IsTrue => value;

		public bool IsFalse => !value;
	}
}
