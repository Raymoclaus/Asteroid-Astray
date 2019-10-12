namespace ValueComponents
{
	public class FloatComponent : ValueComponent<float>
	{
		public void AddValue(float amount)
		{
			SetValue(currentValue + amount);
		}

		public void SubtractValue(float amount)
		{
			AddValue(-amount);
		}
	}
}
