using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineSeg : Shape {
	private Vector2 a, b;
	public Vector2 center = Vector2.zero;
	public Bounds bounds;

	public Vector2 CenterA {
		get { return a + center; }
	}

	public Vector2 CenterB {
		get { return b + center; }
	}

	public float Length {
		get { return Vector2.Distance(a, b); }
	}

	public float Slope {
		get {
			if (a.x == b.x) {
				return float.PositiveInfinity;
			} else {
				return (b.y - a.y) / (b.x - a.x);
			}
		}
	}

	public float Offset {
		get {
			if (Slope == float.PositiveInfinity) {
				return a.x;
			} else {
				return a.y - Slope * a.x;
			}
		}
	}

	public LineSeg() {
		a = b = Vector2.zero;
		CalculateBounds();
	}

	public LineSeg(Vector2 a, Vector2 b) {
		this.a = a;
		this.b = b;
		CalculateBounds();
	}

	public LineSeg(Vector2 center, Vector2 a, Vector2 b) {
		this.center = center;
		this.a = a;
		this.b = b;
		CalculateBounds();
	}

	public bool IsLine() {
		return true;
	}

	public bool IsCircle() {
		return false;
	}

	public bool IsRegularPoly() {
		return false;
	}

	public bool IsPoly() {
		return false;
	}

	public bool Intersects(Shape s) {
		if (s.IsLine()) {
			return Geometry2D.LineSegIntersectsLineSeg(this, (LineSeg)s);
		} else if (s.IsCircle()) {
			return Geometry2D.LineSegIntersectsCircle(this, (Circle)s);
		} else if (s.IsRegularPoly()) {
			//TODO
		} else if (s.IsPoly()) {
			return Geometry2D.LineSegIntersectsPoly(this, (Poly)s);
		}
		return false;
	}

	public List<Vector2> GetVerts() {
		return new List<Vector2>() { a + center, b + center };
	}

	public List<Vector2> GetOffsetVerts() {
		return new List<Vector2>() { a + center, b + center };
	}

	public List<Vector2> GetOffsetVerts(Vector2 offset) {
		return new List<Vector2>() { a + center + offset, b + center + offset };
	}

	public Bounds GetBounds() {
		return new Bounds((Vector2)bounds.center + center, bounds.size);
	}

	private void CalculateBounds() {
		bounds = new Bounds(a, Vector2.zero);
		bounds.Encapsulate(b);
	}

	public Vector2 GetA() {
		return a;
	}

	public Vector2 GetB() {
		return b;
	}

	public void SetA(Vector2 change) {
		a = change;
		CalculateBounds();
	}

	public void SetB(Vector2 change) {
		b = change;
		CalculateBounds();
	}
}
