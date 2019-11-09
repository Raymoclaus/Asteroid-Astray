using System;
using UnityEngine;

namespace ValueComponents
{
	public class RangedFloatComponent : FloatComponent, IRangedValue<float>
	{
		[SerializeField] private float upperLimit = 100f, lowerLimit = 0f;

		public event Action OnValueReachedUpperLimit;
		public event Action OnValueReachedLowerLimit;
		public event Action<float, float> OnRatioChanged;

		private void Awake()
		{
			OnValueChanged += ValueChanged;
		}

		private void ValueChanged(float oldVal, float newVal)
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

		public float CurrentRatio
			=> GetRatio(CurrentValue);

		public float GetRatio(float value)
			=> (value - LowerLimit) / (UpperLimit - LowerLimit);

		public float UpperLimit
		{
			get => upperLimit;
			private set => upperLimit = value;
		}

		public float LowerLimit
		{
			get => lowerLimit;
			private set => lowerLimit = value;
		}

		public float DifferenceBetweenUpperAndLowerLimit
			=> UpperLimit - LowerLimit;

		public override float SetValue(float amount)
		{
			if (amount == CurrentValue) return CurrentValue;

			if (!IsAboveLowerLimit(amount))
			{
				base.SetValue(LowerLimit);
				OnValueReachedLowerLimit?.Invoke();
				return CurrentValue;
			}
			if (!IsBelowUpperLimit(amount))
			{
				base.SetValue(UpperLimit);
				OnValueReachedUpperLimit?.Invoke();
				return CurrentValue;
			}
			base.SetValue(amount);
			return CurrentValue;
		}

		public void SetToUpperLimit() => SetValue(UpperLimit);

		public void SetToLowerLimit() => SetValue(LowerLimit);

		public void SetRatio(float ratio)
		{
			float difference = DifferenceBetweenUpperAndLowerLimit;
			float amountAboveLower = difference * ratio;
			float amountToSet = LowerLimit + difference;
			SetValue(amountToSet);
		}

		public void AddRatio(float ratio) => SetRatio(CurrentRatio + ratio);

		public void SubtractRatio(float ratio) => AddRatio(-ratio);

		public void MultiplyRatio(float multiplier)
			=> SetRatio(CurrentRatio * multiplier);

		public bool IsAboveLowerLimit(float value) => value > LowerLimit;

		public bool IsBelowUpperLimit(float value) => value < UpperLimit;

		public bool IsBetweenLimits(float value)
			=> IsAboveOrEqualToLowerLimit(value)
			   && IsBelowOrEqualToUpperLimit(value);

		public bool IsAboveOrEqualToLowerLimit(float value) => value >= LowerLimit;

		public bool IsBelowOrEqualToUpperLimit(float value) => value <= UpperLimit;

		public void SetUpperLimit(float value, bool keepCurrentRatio)
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
			else if(!IsBelowOrEqualToUpperLimit(CurrentValue))
			{
				SetToUpperLimit();
			}
		}

		public void SetLowerLimit(float value, bool keepCurrentRatio)
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

		public override string ValueString
			=> $"{LowerLimit}<{base.ValueString}<{UpperLimit}";
	}
}
