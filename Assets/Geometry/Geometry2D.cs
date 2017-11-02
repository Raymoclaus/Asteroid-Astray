using System.Collections.Generic;
using UnityEngine;

//TODO: separate methods for regular polygons would be more efficient
/// <summary>
/// A collection of functions used for checking intersection between 2D shapes
/// </summary>
public struct Geometry2D {
	/* Point methods */
	#region
	///Returns whether two points are in the same location
	public static bool PointOnPoint(Vector2 p1, Vector2 p2) {
		return p1 == p2;
	}

	///Returns whether a point lies on a line segment
	public static bool PointOnLineSeg(Vector2 p, LineSeg lns) {
		//if the distance between p and the segment ends is equal to the length of the segment, then point is on the segment
		return Mathf.Approximately(Vector2.Distance(p, lns.CenterA) + Vector2.Distance(p, lns.CenterB), lns.Length);
	}

	///Returns whether a point lies in a circle (excludes being on the perimeter)
	public static bool PointInCircle(Vector2 p, Circle c) {
		//if the distance from p and the center of the circle is less than the radius then p is within the circle
		return Vector2.Distance(p, c.WorldCenter) < c.GetRadius();
	}

	///Returns whether a point lies on the perimeter of a circle
	public static bool PointOnCirclePerimeter(Vector2 p, Circle c) {
		//if the distance from p and the center of the circle is equal to the circle's radius then p is on the perimeter
		return Mathf.Approximately(Vector2.Distance(p, c.WorldCenter), c.GetRadius());
	}

	//source: http://geomalgorithms.com/a03-_inclusion.html tested for irregular, concave and self-intersecting polygons
	///Returns whether a point lies within a polygon given its vertices
	public static bool PointInPoly(Vector2 p, List<Vector2> verts) {
		//first check to see if the point is in a bounding box for efficiency reasons
		if (!PointInBounds(p, verts)) {
			return false;
		}

		int wn = 0;
		int j = verts.Count - 1;
		for (int i = 0; i < verts.Count; i++) {
			if (verts[j].y <= p.y) {
				if (verts[i].y > p.y) {
					if (IsLeft(verts[j], verts[i], p) > 0f) {
						wn++;
					}
				}
			} else {
				if (verts[i].y <= p.y) {
					if (IsLeft(verts[j], verts[i], p) < 0f) {
						wn--;
					}
				}
			}
			j = i;
		}

		return wn != 0;
	}

	///Returns whether a point lies within a polygon
	public static bool PointInPoly(Vector2 p, Poly poly) {
		return PointInPoly(p, poly.GetOffsetVerts());
	}

	private static float IsLeft(Vector2 e1, Vector2 e2, Vector2 p) {
		//returns whether point p is to the left of line segment (e1, e2)
		// >0 (is left) ... =0 (on segment) ... <0 (is right)
		return (e2.x - e1.x) * (p.y - e1.y) - (p.x - e1.x) * (e2.y - e1.y);
	}

	public static bool PointInBounds(Vector2 p, List<Vector2> verts) {
		Bounds bds = new Bounds(verts[0], Vector2.zero);
		for (int i = 1; i < verts.Count; i++) {
			bds.Encapsulate(verts[i]);
		}
		return bds.Contains(p);
	}

	///Returns whether a point lies on the perimeter of a polygon given its vertices
	public static bool PointOnPolyPerimeter(Vector2 p, List<Vector2> verts) {
		LineSeg lineCheck = new LineSeg();
		int j = verts.Count - 1;
		//checks every line segment of the polygon to see if the point lies on any segment
		for (int i = 0; i < verts.Count; i++) {
			lineCheck.SetA(verts[i]);
			lineCheck.SetB(verts[j]);
			if (PointOnLineSeg(p, lineCheck)) {
				return true;
			}
			j = i;
		}

		return false;
	}

	///Returns whether a point lies on the perimeter of a polygon
	public static bool PointOnPolyPerimeter(Vector2 p, Poly poly) {
		return PointOnPolyPerimeter(p, poly.GetOffsetVerts());
	}
	#endregion

	/* LineSeg methods */
	#region
	///Returns whether a line segment intersects another line segment
	public static bool LineSegIntersectsLineSeg(LineSeg lns1, LineSeg lns2) {
		//if lines have the same slope (i.e. parallel or collinear)
		if (Mathf.Approximately(lns1.Slope, lns2.Slope)) {
			//returns whether the ends of either segment lies on either segment
			return PointOnLineSeg(lns1.CenterA, lns2) ||
				PointOnLineSeg(lns1.CenterB, lns2) ||
				PointOnLineSeg(lns2.CenterA, lns1) ||
				PointOnLineSeg(lns2.CenterB, lns1);
		}

		Vector2 p;
		//check to see if either line 1 or 2 are vertical (slope would be infinite)
		//slope and offset are needed for slope intercept form calculation (y = mx + b)
		if (lns1.Slope == float.PositiveInfinity) {
			p.x = lns1.Offset;
			p.y = lns2.Slope * p.x + lns2.Offset;
		} else if (lns2.Slope == float.PositiveInfinity) {
			p.x = lns2.Offset;
			p.y = lns1.Slope * p.x + lns1.Offset;
		} else {
			//find point of intersection for line equations
			p.x = (lns2.Offset - lns1.Offset) / (lns1.Slope - lns2.Slope);
			p.y = lns1.Slope * p.x + lns1.Offset;
		}

		//return whether the calculated point is on both line segments
		return PointOnLineSeg(p, lns1) && PointOnLineSeg(p, lns2);
	}

	///Returns whether a line segment intersects a circle
	public static bool LineSegIntersectsCircle(LineSeg lns, Circle c) {
		//for efficiency, check if either end of the line is in the circle first
		if (PointInCircle(lns.CenterA, c) || PointInCircle(lns.CenterB, c)) {
			return true;
		}

		Vector2 point = Vector2.zero;
		//do checks to see whether the slope is 0 or infinite
		if (Mathf.Approximately(lns.Slope, 0f)) {
			point = new Vector2(c.WorldCenter.x, lns.CenterA.y);
		} else if (lns.Slope == float.PositiveInfinity) {
			point = new Vector2(lns.CenterA.x, c.WorldCenter.y);
		} else {
			//get negative reciprocal of slope of line segment to find a new perpendicular line
			float slope = -1f / lns.Slope;
			//find the offset that the new line needs to also contain the center of the circle
			float offset = -slope * c.WorldCenter.x + c.WorldCenter.y;
			//calculate point of intersection between the perpendicular lines
			point.x = (lns.Offset - offset) / (slope - lns.Slope);
			point.y = slope * point.x + offset;
		}

		//check if the calculated point is on the line segment and in the circle
		return PointInCircle(point, c) && PointOnLineSeg(point, lns);
	}

	///Returns whether a line segment intersects a polygon (given a list of vertices)
	public static bool LineSegIntersectsPoly(LineSeg lns, List<Vector2> verts) {
		//Check to see if either end of the segment is in the polygon
		if (PointInPoly(lns.CenterA, verts) || PointInPoly(lns.CenterB, verts)) {
			return true;
		}

		int j = verts.Count - 1;
		LineSeg lnsCheck = new LineSeg();
		//checks every line segment in the polygon until an intersection is found
		for (int i = 0; i < verts.Count; i++) {
			lnsCheck.SetA(verts[i]);
			lnsCheck.SetB(verts[j]);
			if (LineSegIntersectsLineSeg(lns, lnsCheck)) {
				return true;
			}
			j = i;
		}

		return false;
	}

	///Returns whether a line segment intersects a polygon
	public static bool LineSegIntersectsPoly(LineSeg lns, Poly p) {
		return LineSegIntersectsPoly(lns, p.GetOffsetVerts());
	}
	#endregion

	/* Circle methods */
	#region
	///Returns whether two circles intersect
	public static bool CircleIntersectsCircle(Circle c1, Circle c2) {
		//if the distance between the center of each circle adds up to less than the sum of the radii then they intersect
		return Vector2.Distance(c1.WorldCenter, c2.WorldCenter) < c1.GetRadius() + c2.GetRadius();
	}

	///Returns whether a circle intersects a polygon (given a list of vertices)
	public static bool CircleIntersectsPoly(Circle c, List<Vector2> verts) {
		//check to see if the center of the circle is in the poly
		if (PointInPoly(c.WorldCenter, verts)) {
			return true;
		}

		//check to see if any points are in the circle
		foreach (Vector2 vert in verts) {
			if (PointInCircle(vert, c)) {
				return true;
			}
		}

		LineSeg lnsCheck = new LineSeg();
		int j = verts.Count - 1;
		//check each line segment of the polygon until an intersection is found
		for (int i = 0; i < verts.Count; i++) {
			lnsCheck.SetA(verts[i]);
			lnsCheck.SetB(verts[j]);
			if (LineSegIntersectsCircle(lnsCheck, c)) {
				return true;
			}
			j = i;
		}

		return false;
	}

	///Returns whether a circle intersects a polygon
	public static bool CircleIntersectsPoly(Circle c, Poly p) {
		return CircleIntersectsPoly(c, p.GetOffsetVerts());
	}
	#endregion

	/* Poly methods */
	#region
	///Returns whether a polygon intersects another polygon (given a list of vertices)
	public static bool PolyIntersectsPoly(List<Vector2> verts1, List<Vector2> verts2) {
		//check to see if any vertices exist inside either polygon (more efficient and catches most cases)
		foreach (Vector2 vert in verts1) {
			if (PointInPoly(vert, verts2)) {
				return true;
			}
		}
		foreach (Vector2 vert in verts2) {
			if (PointInPoly(vert, new List<Vector2>(verts1))) {
				return true;
			}
		}

		//check to see if any of the edges of a polygon intersect with the edges of the other
		//this code ensures concave shapes work too but it's potentially slow
		LineSeg lnsCheck = new LineSeg();
		int j = verts1.Count - 1;
		for (int i = 0; i < verts1.Count; i++) {
			lnsCheck.SetA(verts1[i]);
			lnsCheck.SetB(verts1[j]);
			if (LineSegIntersectsPoly(lnsCheck, verts2)) {
				return true;
			}
			j = i;
		}

		return false;
	}

	///Returns whether a polygon intersects another polygon
	public static bool PolyIntersectsPoly(Poly p1, Poly p2) {
		return PolyIntersectsPoly(p1.GetOffsetVerts(), p2.GetOffsetVerts());
	}
	#endregion
}