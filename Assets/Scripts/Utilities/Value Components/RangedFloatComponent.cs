using System;

namespace ValueComponents
{
	public class RangedFloatComponent : FloatComponent
	{
		public float upperLimit = 100f, lowerLimit = 0f;

		public event Action OnValueReachedUpperLimit;
		public event Action OnValueReachedLowerLimit;

		public float Ratio
			=> (currentValue - lowerLimit) / (upperLimit - lowerLimit);

		public override float SetValue(float amount)
		{
			if (amount <= lowerLimit)
			{
				base.SetValue(lowerLimit);
				OnValueReachedLowerLimit?.Invoke();
				return currentValue;
			}
			if (amount >= upperLimit)
			{
				base.SetValue(upperLimit);
				OnValueReachedUpperLimit?.Invoke();
				return currentValue;
			}
			base.SetValue(amount);
			return currentValue;
		}

		public void SetToUpperLimit() => SetValue(upperLimit);

		public void SetToLowerLimit() => SetValue(lowerLimit);
	}
}
