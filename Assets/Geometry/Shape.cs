using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Shape {
	bool IsLine();

	bool IsCircle();

	bool IsRegularPoly();

	bool IsPoly();

	bool Intersects(Shape s);

	List<Vector2> GetVerts();

	List<Vector2> GetOffsetVerts();

	Bounds GetBounds();
}
