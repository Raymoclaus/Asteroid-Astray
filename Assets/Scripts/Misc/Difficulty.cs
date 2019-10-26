using UnityEngine;

public static class Difficulty
{
	//https://www.desmos.com/calculator/waol9m1mjy
	public static int DistanceBasedDifficulty(float distance)
	{
		//solve formula for difficulty rounded to nearest integer
		distance /= EntityNetwork.CHUNK_SIZE;
		float difficultyCurve = Mathf.Sqrt(distance);
		return (int)difficultyCurve;
	}
}