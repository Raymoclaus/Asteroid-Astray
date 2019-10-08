using UnityEngine;

[System.Serializable]
public struct Vector2Pair
{
	public Vector2 a, b;

	public Vector2Pair(Vector2 a, Vector2 b)
	{
		this.a = a;
		this.b = b;
	}
}