using System;
using UnityEngine;

namespace ValueComponents
{
	public class RangedIntComponent : IntComponent, IRangedValue<int>
	{
		[SerializeField] private int upperLimit = 100, lowerLimit = 0;

		private void Awake()
		{
			OnValueChanged += ValueChanged;
		}

		private void ValueChanged(int oldVal, int newVal)
		{
			if (newVal == UpperLimit)
			{
				OnValueReachedUpperLimit?.Invoke();
			}

			if (newVal == LowerLimit)
			{
				OnValueReachedLowerLimit?.Invoke();
			}

			float oldRatio = GetRatio(oldVal);
			float newRatio = CurrentRatio;
			if (oldRatio != newRatio)
			{
				OnRatioChanged?.Invoke(oldRatio, newRatio);
			}
		}

		public int UpperLimit
		{
			get => upperLimit;
			private set => upperLimit = value;
		}

		public int LowerLimit
		{
			get => lowerLimit;
			private set => lowerLimit = value;
		}

		public float CurrentRatio
			=> GetRatio(CurrentValue);

		public int DifferenceBetweenUpperAndLowerLimit
			=> UpperLimit - LowerLimit;

		public event Action OnValueReachedUpperLimit;
		public event Action OnValueReachedLowerLimit;
		public event Action<float, float> OnRatioChanged;

		public void AddRatio(float ratio) => SetRatio(CurrentRatio + ratio);

		public float GetRatio(int value)
			=> (float)(value - LowerLimit) / (UpperLimit - LowerLimit);

		public bool IsAboveLowerLimit(int value) => value > LowerLimit;

		public bool IsAboveOrEqualToLowerLimit(int value) => value >= LowerLimit;

		public bool IsBelowOrEqualToUpperLimit(int value) => value <= UpperLimit;

		public bool IsBelowUpperLimit(int value) => value < UpperLimit;

		public bool IsBetweenLimits(int value)
			=> IsAboveOrEqualToLowerLimit(value)
			   && IsBelowOrEqualToUpperLimit(value);

		public void MultiplyRatio(float multiplier)
			=> SetRatio(CurrentRatio * multiplier);

		public void SetLowerLimit(int value, bool keepCurrentRatio)
		{
			if (!IsBelowOrEqualToUpperLimit(value))
			{
				throw new ArgumentOutOfRangeException(
					"value",
					value,
					"Given value must be below or equal to the upper limit.");
			}

			float oldRatio = CurrentRatio;
			LowerLimit = value;
			if (keepCurrentRatio)
			{
				SetRatio(oldRatio);
			}
			else if (!IsAboveOrEqualToLowerLimit(CurrentValue))
			{
				SetToLowerLimit();
			}
		}

		public void SetRatio(float ratio)
		{
			float difference = DifferenceBetweenUpperAndLowerLimit;
			float amountAboveLower = difference * ratio;
			float amountToSet = LowerLimit + difference;
			SetValue((int)amountToSet);
		}

		public void SetToLowerLimit() => SetValue(LowerLimit);

		public void SetToUpperLimit() => SetValue(UpperLimit);

		public void SetUpperLimit(int value, bool keepCurrentRatio)
		{
			if (!IsAboveOrEqualToLowerLimit(value))
			{
				throw new ArgumentOutOfRangeException(
					"value",
					value,
					"Given value must be above or equal to the lower limit.");
			}

			float oldRatio = CurrentRatio;
			UpperLimit = value;
			if (keepCurrentRatio)
			{
				SetRatio(oldRatio);
			}
			else if (!IsBelowOrEqualToUpperLimit(CurrentValue))
			{
				SetToUpperLimit();
			}
		}

		public void SubtractRatio(float ratio) => AddRatio(-ratio);

		public override string ValueString
			=> $"{LowerLimit}<{base.ValueString}<{UpperLimit}";
	}
}
