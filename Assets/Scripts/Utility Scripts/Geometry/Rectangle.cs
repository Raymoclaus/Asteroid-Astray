using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rectangle : Shape {
	private List<Vector2> verts;
	public Vector2 center = Vector2.zero;

	public Rectangle() {
		verts = new List<Vector2>(4);
		for (int i = 0; i < 4; i++) {
			verts.Add(Vector2.zero);
		}
	}

	public Rectangle(Vector2[] vertArr) {
		verts = new List<Vector2>(4);
		for (int i = 0; i < vertArr.Length && i < verts.Count; i++) {
			verts[i] = vertArr[i];
		}
	}

	public Rectangle(List<Vector2> vertArr) {
		verts = new List<Vector2>(4);
		for (int i = 0; i < vertArr.Count && i < verts.Count; i++) {
			verts[i] = vertArr[i];
		}
	}

	public bool IsLine() {
		return false;
	}

	public bool IsCircle() {
		return false;
	}

	public bool IsRegularPoly() {
		return false;
	}

	public bool IsPoly() {
		return true;
	}

	public bool Intersects(Shape s) {
		if (s.IsLine()) {
			return Geometry2D.LineSegIntersectsPoly((LineSeg)s, GetVerts());
		} else if (s.IsCircle()) {
			return Geometry2D.CircleIntersectsPoly((Circle)s, GetVerts());
		} else if (s.IsRegularPoly()) {
			//TODO
		} else if (s.IsPoly()) {
			return Geometry2D.PolyIntersectsPoly(s.GetVerts(), GetVerts());
		}
		return false;
	}

	public List<Vector2> GetVerts() {
		return verts;
	}

	public List<Vector2> GetOffsetVerts() {
		List<Vector2> offsetVerts = new List<Vector2>(verts);
		for (int i = 0; i < offsetVerts.Count; i++) {
			offsetVerts[i] += center;
		}
		return offsetVerts;
	}

	public Bounds GetBounds() {
		Bounds bds = new Bounds();
		foreach (Vector2 vert in GetVerts()) {
			bds.Encapsulate(vert);
		}
		return bds;
	}
}
