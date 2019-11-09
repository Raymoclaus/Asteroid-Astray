using System;
using UnityEngine;

namespace ValueComponents
{
	public class ValueComponent<T> : MonoBehaviour
	{
		public string valueName;
		[SerializeField] private T currentValue;

		public T CurrentValue
		{
			get => currentValue;
			set => currentValue = value;
		}

		public event Action<T, T> OnValueChanged;

		public virtual T SetValue(T val)
		{
			if (CurrentValue.Equals(val)) return CurrentValue;

			T oldValue = CurrentValue;
			CurrentValue = val;
			OnValueChanged?.Invoke(oldValue, CurrentValue);
			return CurrentValue;
		}

		public virtual string ValueString => CurrentValue.ToString();

		public override string ToString() => $"{valueName}: {ValueString}";
	}
}
