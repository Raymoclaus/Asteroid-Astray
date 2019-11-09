using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Vertex
{
	public float X { get; private set; }
	public float Z { get; private set; }

	public Vertex(float x, float z)
	{
		X = x;
		Z = z;
	}

	public Vertex(Vector2 vector)
	{
		X = vector.x;
		Z = vector.y;
	}

	public Vector2 GetXYVector()
	{
		return new Vector2(X, Z);
	}

	public Vector3 GetXZVector()
	{
		return new Vector3(X, 0f, Z);
	}

	public float Distance(Vertex other)
	{
		return Distance(this, other);
	}

	public static float Distance(Vertex a, Vertex b)
	{
		float rise = a.Z - b.Z;
		float run = a.X - b.X;
		return Mathf.Sqrt(rise * rise + run * run);
	}
}
