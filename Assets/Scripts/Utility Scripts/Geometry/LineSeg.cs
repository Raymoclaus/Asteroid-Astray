using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineSeg : Shape {
	public Vector2 a, b, center = Vector2.zero;

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
	}

	public LineSeg(Vector2 a, Vector2 b) {
		this.a = a;
		this.b = b;
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

	public Bounds GetBounds() {
		Bounds bds = new Bounds();
		foreach (Vector2 vert in GetVerts()) {
			bds.Encapsulate(vert);
		}
		return bds;
	}
}
