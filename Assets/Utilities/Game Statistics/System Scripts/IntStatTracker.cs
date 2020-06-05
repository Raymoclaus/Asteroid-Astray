using System;
using UnityEngine;

namespace StatisticsTracker
{
	[CreateAssetMenu(fileName = "New Int StatTracker", menuName = "Scriptable Objects/Stat Tracker/Int StatTracker")]
	public class IntStatTracker : StatTracker
	{
		public event Action<int, int> OnValueUpdated;
		[SerializeField] private int value;
		[SerializeField] private int defaultValue;

		public override Type FieldType => value.GetType();

		public override string ValueString => value.ToString();

		public override bool TryParse(string valueString)
		{
			bool successful = int.TryParse(valueString, out int val);
			if (!successful)
			{
				SteamPunkConsole.WriteLine($"value \"{valueString}\" could not be parsed as a \"{FieldType}\".");
				return false;
			}

			SetValue(val);
			return true;
		}

		public int Value => value;

		public void SetValue(int val)
		{
			int oldVal = value;
			value = val;
			if (oldVal != value)
			{
				OnValueUpdated?.Invoke(oldVal, value);
			}
		}

		public override void ResetToDefault() => SetValue(defaultValue);

		public void AddValue(int amount) => SetValue(value + amount);

		public void SubstractValue(int amount) => AddValue(-amount);

		public void IncrementValue() => AddValue(1);

		public void DecrementValue() => SubstractValue(1);
	} 
}
