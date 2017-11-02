using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Poly : Shape {
	private List<Vector2> verts;
	public Vector2 refCenter = Vector2.zero;
	public Bounds bounds;
	private Transform target;
	public int VertsCount {
		get {
			return verts.Count;
		}
	}

	public Poly(List<Vector2> vertArr = null, Transform target = null) {
		verts = vertArr != null ? vertArr : new List<Vector2>(3);
		while (verts.Count < 3) {
			verts.Add(Vector2.zero);
		}
		this.target = target;
		CalculateBounds();
	}

	public bool IsLine() {
		return false;
	}

	public bool IsCircle() {
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

	/// Returns a copy of the list of vertices of the polygon offset by its center and the given offset.
	public List<Vector2> GetOffsetVerts() {
		List<Vector2> offsetVerts = new List<Vector2>(verts);
		for (int i = 0; i < verts.Count; i++) {
			offsetVerts[i] += GetRefCenter();
			if (target != null && !Mathf.Approximately(target.eulerAngles.z, 0f)) {
				offsetVerts[i] = RotatePointAroundPivot(offsetVerts[i], target.eulerAngles.z, target.position);
			}
		}
		return offsetVerts;
	}

	private Vector2 RotatePointAroundPivot(Vector2 p, float angle, Vector2? pivot = null) {
		Vector2 pvot = pivot != null ? (Vector2)pivot : Vector2.zero;
		angle *= Mathf.Deg2Rad;
		return new Vector2(Mathf.Cos(angle) * (p.x - pvot.x) - Mathf.Sin(angle) * (p.y - pvot.y) + pvot.x,
			Mathf.Sin(angle) * (p.x - pvot.x) + Mathf.Cos(angle) * (p.y - pvot.y) + pvot.y);
	}

	/// Returns the bounds of the polygon in world space
	public Bounds GetBounds() {
		return new Bounds((Vector2)bounds.center + GetRefCenter(), bounds.size);
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

	public void AttachToTransform(Transform t) {
		target = t;
	}

	public Vector2 GetRefCenter() {
		return target != null ? (Vector2)target.position : refCenter;
	}

	public void Translate(Vector2 move) {
		for (int i = 0; i < verts.Count; i++) {
			verts[i] += move;
		}
		bounds.center += (Vector3)move;
	}
}
