using System;
using UnityEngine;

namespace StatisticsTracker
{
	[CreateAssetMenu(fileName = "New Bool StatTracker", menuName = "Scriptable Objects/Stat Tracker/Bool StatTracker")]
	public class BoolStatTracker : StatTracker
	{
		public event Action<bool, bool> OnValueUpdated;
		[SerializeField] private bool value;
		[SerializeField] private bool defaultValue;

		public override Type FieldType => value.GetType();

		public override string ValueString => value.ToString();

		public override bool TryParse(string valueString)
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

		public bool Value => value;

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
