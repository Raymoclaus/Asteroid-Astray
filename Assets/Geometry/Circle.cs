using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Circle : Shape {
	private float radius = 1f;
	public Vector2 refCenter = Vector2.zero;
	public Vector2 center = Vector2.zero;
	public Vector2 WorldCenter {
		get {
			return GetRefCenter() + center;
		}
		set {
			center = value - GetRefCenter();
		}
	}
	public Bounds bounds;
	private Transform target;

	public Circle(Transform target = null, Vector2? center = null, float radius = 1f) {
		this.target = target;
		this.center = center ?? this.center;
		this.radius = radius;
		CalculateBounds();
	}

	public bool IsLine() {
		return false;
	}

	public bool IsCircle() {
		return true;
	}

	public bool IsPoly() {
		return false;
	}

	public bool Intersects(Shape s) {
		if (s.IsLine()) {
			return Geometry2D.LineSegIntersectsCircle((LineSeg)s, this);
		} else if (s.IsCircle()) {
			return Geometry2D.CircleIntersectsCircle((Circle)s, this);
		} else if (s.IsPoly()) {
			return Geometry2D.CircleIntersectsPoly(this, (Poly)s);
		}
		return false;
	}

	public List<Vector2> GetVerts() {
		return new List<Vector2>() { center };
	}

	public List<Vector2> GetOffsetVerts() {
		return new List<Vector2>() { WorldCenter };
	}

	public Bounds GetBounds() {
		return new Bounds((Vector2)bounds.center + GetRefCenter(), bounds.size);
	}

	private void CalculateBounds() {
		bounds = new Bounds(GetRefCenter(), Vector2.one * 2 * radius);
	}

	public float GetRadius() {
		return radius;
	}

	public void SetRadius(float change) {
		radius = change;
		CalculateBounds();
	}

	public void AttachToTransform(Transform t) {
		target = t;
	}

	public Vector2 GetRefCenter() {
		return target != null ? (Vector2)target.position : refCenter;
	}

	public void Translate(Vector2 move) {
		center += move;
		bounds.center += (Vector3)move;
	}
}
