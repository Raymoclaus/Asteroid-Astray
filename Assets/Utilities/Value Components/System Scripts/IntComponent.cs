namespace ValueComponents
{
	public class IntComponent : ValueComponent<int>
	{
		public int AddValue(int amount) => SetValue(CurrentValue + amount);

		public int SubtractValue(int amount) => AddValue(-amount);
	}
}
