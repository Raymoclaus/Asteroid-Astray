using UnityEditor;

namespace StatisticsTracker.CustomisedEditor
{
	public static class StatisticsIOEditor
	{
		[MenuItem("Game Statistics/Reset All Stats")]
		public static void ResetAllStats() => StatisticsIO.ResetAllStats();
	}
}