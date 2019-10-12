using System;

namespace ValueComponents
{
	public class RangedFloatComponent : FloatComponent
	{
		public float upperLimit = 100f, lowerLimit = 0f;

		public event Action OnValueReachedUpperLimit;
		public event Action OnValueReachedLowerLimit;

		public override void SetValue(float amount)
		{
			if (amount <= lowerLimit)
			{
				base.SetValue(lowerLimit);
				OnValueReachedLowerLimit?.Invoke();
				return;
			}
			if (amount >= upperLimit)
			{
				base.SetValue(upperLimit);
				OnValueReachedUpperLimit?.Invoke();
				return;
			}
			base.SetValue(amount);
		}

		public void SetToUpperLimit() => SetValue(upperLimit);

		public void SetToLowerLimit() => SetValue(lowerLimit);
	}
}
