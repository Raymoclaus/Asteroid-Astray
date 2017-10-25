using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Poly : Shape {
	private List<Vector2> verts;
	public Vector2 center = Vector2.zero;
	public Bounds bounds;
	public int VertsCount {
		get {
			return verts.Count;
		}
	}

	public Poly() {
		verts = new List<Vector2>(3);
		for (int i = 0; i < 3; i++) {
			verts.Add(Vector2.zero);
		}
		CalculateBounds();
	}

	public Poly(int size) {
		verts = new List<Vector2>(size);
		for (int i = 0; i < size; i++) {
			verts.Add(Vector2.zero);
		}
		CalculateBounds();
	}

	public Poly(Vector2[] vertArr) {
		verts = new List<Vector2>(vertArr);
		for (int i = 0; i < vertArr.Length; i++) {
			verts.Add(vertArr[i]);
		}
		while (verts.Count < 3) {
			verts.Add(Vector2.zero);
		}
		CalculateBounds();
	}

	public Poly(List<Vector2> vertArr) {
		verts = vertArr;
		while (verts.Count < 3) {
			verts.Add(Vector2.zero);
		}
		CalculateBounds();
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
			return Geometry2D.PolyIntersectsPoly(this, (Poly)s);
		}
		return false;
	}

	/// Returns the list of vertices of the polygon relative to its center.
	/// Changes to this list will affect the polygon because it is not a copy.
	public List<Vector2> GetVerts() {
		return verts;
	}

	/// Returns a copy of the list of vertices of the polygon offset by its center.
	public List<Vector2> GetOffsetVerts() {
		List<Vector2> offsetVerts = new List<Vector2>(verts);
		for (int i = 0; i < offsetVerts.Count; i++) {
			offsetVerts[i] += center;
		}
		return offsetVerts;
	}

	/// Returns a copy of the list of vertices of the polygon offset by its center and the given position.
	public List<Vector2> GetOffsetVerts(Vector2 otherOffset) {
		List<Vector2> offsetVerts = new List<Vector2>(verts);
		for (int i = 0; i < verts.Count; i++) {
			offsetVerts[i] += center + otherOffset;
		}
		return offsetVerts;
	}

	/// Returns the bounds of the polygon in world space
	public Bounds GetBounds() {
		return new Bounds((Vector2)bounds.center + center, bounds.size);
	}

	private void CalculateBounds() {
		bounds = new Bounds(verts[0], Vector2.zero);
		for (int i = 1; i < verts.Count; i++) {
			bounds.Encapsulate(verts[i]);
		}
	}

	public void AddVertex(Vector2 vert) {
		verts.Add(vert);
		bounds.Encapsulate(vert);
	}

	public void SetVertexPosition(int vertID, Vector2 pos) {
		verts[vertID] = pos;
		CalculateBounds();
	}

	public void SetVertexPosition(int vertID, float x, float y) {
		verts[vertID] = new Vector2(x, y);
		CalculateBounds();
	}
}
