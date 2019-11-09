using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct LineSegment
{
	public readonly Vertex a, b;

	public LineSegment(Vertex a, Vertex b)
	{
		this.a = a;
		this.b = b;
	}

	public float GetSlope()
	{
		return GetSlope(this);
	}

	public static float GetSlope(LineSegment ls)
	{
		if (ls.a.X == ls.b.X)
		{
			return float.PositiveInfinity;
		}
		return (ls.b.Z - ls.a.Z) / (ls.b.X - ls.a.X);
	}

	public float GetLineOffset()
	{
		return GetLineOffset(this);
	}

	public static float GetLineOffset(LineSegment ls)
	{
		float slope = ls.GetSlope();
		if (slope == float.PositiveInfinity)
		{
			return ls.a.X;
		}
		return ls.a.Z - slope * ls.a.X;
	}

	public bool CrossesPoint(Vector2 point, bool includeEndPoints)
	{
		return CrossesPoint(this, point, includeEndPoints);
	}

	public bool CrossesPoint(Vertex v, bool includeEndPoints)
	{
		return CrossesPoint(this, v, includeEndPoints);
	}

	public static bool CrossesPoint(LineSegment ls, Vector2 point, bool includeEndPoints)
	{
		Vector2 a = ls.a.GetXYVector();
		Vector2 b = ls.b.GetXYVector();
		float aToPoint = Vector2.Distance(a, point);
		float bToPoint = Vector2.Distance(b, point);

		if (!includeEndPoints
			&& ((aToPoint < 0.001f && b.y >= point.y)
			|| (bToPoint < 0.001f && a.y >= point.y))) return false;

		float aToB = Vector2.Distance(a, b);

		return Mathf.Abs(aToPoint + bToPoint - aToB) <= 0.001f;
	}

	public static bool CrossesPoint(LineSegment ls, Vertex v, bool includeEndPoints)
	{
		return CrossesPoint(ls, v.GetXYVector(), includeEndPoints);
	}

	public Vector2? GetIntersection(LineSegment other, bool includeEndPoints)
	{
		return GetIntersection(this, other, includeEndPoints);
	}

	public static Vector2? GetIntersection(LineSegment first, LineSegment second, bool includeEndPoints)
	{
		float firstSlope = first.GetSlope();
		float secondSlope = second.GetSlope();

		//if (Mathf.Approximately(firstSlope, secondSlope))
		//{
		//	if (first.CrossesPoint(second.a.GetXYVector())) return second.a.GetXYVector();
		//	if (first.CrossesPoint(second.b.GetXYVector())) return second.b.GetXYVector();
		//	if (second.CrossesPoint(first.a.GetXYVector())) return first.a.GetXYVector();
		//	if (second.CrossesPoint(first.b.GetXYVector())) return first.b.GetXYVector();
		//}

		Vector2 p;
		float firstLineOffset = first.GetLineOffset();
		float secondLineOffset = second.GetLineOffset();

		if (firstSlope == float.PositiveInfinity)
		{
			p.x = firstLineOffset;
			p.y = secondSlope * p.x + secondLineOffset;
		}
		else if (secondSlope == float.PositiveInfinity)
		{
			p.x = secondLineOffset;
			p.y = firstSlope * p.x + firstLineOffset;
		}
		else
		{
			p.x = (secondLineOffset - firstLineOffset) / (firstSlope - secondSlope);
			p.y = firstSlope * p.x + firstLineOffset;
		}

		if (first.CrossesPoint(p, includeEndPoints) && second.CrossesPoint(p, includeEndPoints))
		{
			return p;
		}
		else
		{
			return null;
		}
	}

	public void DrawLine(Color color, float duration)
	{
		Debug.DrawLine(a.GetXZVector(), b.GetXZVector(), color, duration);
	}
}
