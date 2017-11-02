using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineSeg : Shape {
	private Vector2 a, b;
	public Vector2 refCenter = Vector2.zero;
	public Bounds bounds;
	private Transform target;

	public Vector2 CenterA {
		get { return a + GetRefCenter(); }
	}

	public Vector2 CenterB {
		get { return b + GetRefCenter(); }
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
		this.refCenter = center;
		this.a = a;
		this.b = b;
		CalculateBounds();
	}

	public LineSeg(Transform target, Vector2 a, Vector2 b) {
		this.target = target;
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

	public bool IsPoly() {
		return false;
	}

	public bool Intersects(Shape s) {
		if (s.IsLine()) {
			return Geometry2D.LineSegIntersectsLineSeg(this, (LineSeg)s);
		} else if (s.IsCircle()) {
			return Geometry2D.LineSegIntersectsCircle(this, (Circle)s);
		} else if (s.IsPoly()) {
			return Geometry2D.LineSegIntersectsPoly(this, (Poly)s);
		}
		return false;
	}

	public List<Vector2> GetVerts() {
		return new List<Vector2>() { a, b };
	}

	public List<Vector2> GetOffsetVerts() {
		return new List<Vector2>() { a + GetRefCenter(), b + GetRefCenter() };
	}

	public Bounds GetBounds() {
		return new Bounds((Vector2)bounds.center + GetRefCenter(), bounds.size);
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

	public void AttachToTransform(Transform t) {
		target = t;
	}

	public Vector2 GetRefCenter() {
		return target != null ? (Vector2)target.position : refCenter;
	}

	public void Translate(Vector2 move) {
		a += move;
		b += move;
		bounds.center += (Vector3)move;
	}

	public float GetRotation() {
		return target != null ? target.eulerAngles.z : 0f;
	}
}
