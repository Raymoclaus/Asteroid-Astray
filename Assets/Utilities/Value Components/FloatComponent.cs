namespace ValueComponents
{
	public class FloatComponent : ValueComponent<float>
	{
		/// <summary>
		/// Adds amount to current value. Returns current value.
		/// </summary>
		/// <param name="amount"></param>
		public float AddValue(float amount)
		{
			return SetValue(currentValue + amount);
		}

		/// <summary>
		/// Subtracts amount from current value. Returns current value.
		/// </summary>
		/// <param name="amount"></param>
		public float SubtractValue(float amount)
		{
			return AddValue(-amount);
		}
	}
}
