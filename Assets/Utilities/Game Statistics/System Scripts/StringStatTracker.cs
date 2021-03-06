﻿using System;
using UnityEngine;

namespace StatisticsTracker
{
	[CreateAssetMenu(fileName = "New String StatTracker", menuName = "Scriptable Objects/Stat Tracker/String StatTracker")]
	public class StringStatTracker : StatTracker
	{
		public event Action<string, string> OnValueUpdated;
		[SerializeField] private string value;
		[SerializeField] private string defaultValue;

		public override Type FieldType => value.GetType();

		public override string ValueString => value;

		public override bool TryParse(string valueString)
		{
			SetValue(valueString);
			return true;
		}

		public override void ResetToDefault() => SetValue(defaultValue);

		public string Value => value;

		public void SetValue(string val)
		{
			string oldVal = value;
			value = val;
			if (oldVal != val)
			{
				OnValueUpdated?.Invoke(oldVal, value);
			}
		}
	}

}