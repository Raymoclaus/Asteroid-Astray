using System;
using UnityEngine;

namespace StatisticsTracker
{
	[CreateAssetMenu(fileName = "New Float StatTracker", menuName = "Scriptable Objects/Stat Tracker/Float StatTracker")]
	public class FloatStatTracker : StatTracker
	{
		public event Action<float, float> OnValueUpdated;
		[SerializeField] private float value;
		[SerializeField] private float defaultValue;

		public override Type FieldType => value.GetType();

		public override string ValueString => value.ToString();

		public override bool TryParse(string valueString)
		{
			bool successful = float.TryParse(valueString, out float val);
			if (!successful)
			{
				SteamPunkConsole.WriteLine($"value \"{valueString}\" could not be parsed as a \"{FieldType}\".");
				return false;
			}

			SetValue(val);
			return true;
		}

		public float Value => value;

		public void SetValue(float val)
		{
			float oldVal = value;
			value = val;
			if (oldVal != value)
			{
				OnValueUpdated?.Invoke(oldVal, value);
			}
		}

		public override void ResetToDefault() => SetValue(defaultValue);

		public void AddValue(float amount) => SetValue(value + amount);

		public void SubstractValue(float amount) => AddValue(-amount);
	} 
}
