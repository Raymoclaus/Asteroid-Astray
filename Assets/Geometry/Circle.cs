using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Circle : Shape {
	private float radius = 1f;
	public Vector2 center = Vector2.zero;
	public Bounds bounds;

	public Circle() {
		CalculateBounds();
	}

	public Circle(float radius) {
		this.radius = radius;
		CalculateBounds();
	}

	public Circle(Vector2 center, float radius) {
		this.center = center;
		this.radius = radius;
		CalculateBounds();
	}

	public bool IsLine() {
		return false;
	}

	public bool IsCircle() {
		return true;
	}

	public bool IsRegularPoly() {
		return false;
	}

	public bool IsPoly() {
		return false;
	}

	public bool Intersects(Shape s) {
		if (s.IsLine()) {
			return Geometry2D.LineSegIntersectsCircle((LineSeg)s, this);
		} else if (s.IsCircle()) {
			return Geometry2D.CircleIntersectsCircle((Circle)s, this);
		} else if (s.IsRegularPoly()) {
			//TODO
		} else if (s.IsPoly()) {
			return Geometry2D.CircleIntersectsPoly(this, (Poly)s);
		}
		return false;
	}

	public List<Vector2> GetVerts() {
		return new List<Vector2>() { center };
	}

	public List<Vector2> GetOffsetVerts() {
		return new List<Vector2>() { center };
	}

	public Bounds GetBounds() {
		return new Bounds((Vector2)bounds.center + center, bounds.size);
	}

	private void CalculateBounds() {
		bounds = new Bounds(center, Vector2.one * 2 * radius);
	}

	public float GetRadius() {
		return radius;
	}

	public void SetRadius(float change) {
		radius = change;
		CalculateBounds();
	}
}
