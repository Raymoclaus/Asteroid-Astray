using UnityEngine;

public static class Difficulty
{
	//1 = easy, 2 = normal, 3 = hard, 4 = expert
	public static int DIFFICULTY_MODIFIER = 2;
	private static int rangeBasedDifficultyModifier { get { return 9 - 2 * DIFFICULTY_MODIFIER; } }

	public static int DistanceBasedDifficulty(float distance)
	{
		//simplified quadratic formula
		//formula: difficulty = (sqrt(modifier^2 + 4 * distance) - modifier) / 2
		//solve formula for difficulty rounded to nearest integer
		return (int)(((Mathf.Sqrt(Mathf.Pow(rangeBasedDifficultyModifier, 2f)) + 4 * distance / Cnsts.CHUNK_SIZE)
			- rangeBasedDifficultyModifier) / 2f);
	}
}