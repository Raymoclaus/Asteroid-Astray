using System;
using UnityEngine;

namespace ValueComponents
{
	public class ValueComponent<T> : MonoBehaviour
	{
		public string valueName;
		public T currentValue;

		public event Action<T, T> OnValueChanged;

		public virtual T SetValue(T val)
		{
			if (currentValue.Equals(val)) return currentValue;

			T oldValue = currentValue;
			currentValue = val;
			OnValueChanged?.Invoke(oldValue, currentValue);
			return currentValue;
		}
	}
}
