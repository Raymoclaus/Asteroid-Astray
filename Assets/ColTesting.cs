using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColTesting : MonoBehaviour {
	Circle c = new Circle(3f);
	Circle c2 = new Circle(0.3f);
	Poly poly = new Poly(new List<Vector2>() {
		new Vector2(-2f, 0f),
		new Vector2(-3f, 3f),
		new Vector2(0f, 2f),
		new Vector2(2f, 0f),
		new Vector2(0f, -2f),
		new Vector2(-3f, -3f)
	});
	Poly poly2 = new Poly(new List<Vector2>() {
		new Vector2(2f, 0f),
		new Vector2(3f, -3f),
		new Vector2(0f, -2f),
		new Vector2(-2f, 0f),
		new Vector2(0f, 2f),
		new Vector2(3f, 3f)
	});
	Poly poly3 = new Poly(new List<Vector2>() {
		new Vector2(-2f, -2f),
		new Vector2(2f, -2f),
		new Vector2(2f, 2f),
		new Vector2(-1f, 2f),
		new Vector2(-1f, 1f),
		new Vector2(1f, 1f),
		new Vector2(1f, 0f),
		new Vector2(0f, 0f),
		new Vector2(0f, 3f),
		new Vector2(-2f, 3f),
	});
	LineSeg lns = new LineSeg(Vector2.zero, Vector2.one * 5f);
	LineSeg lns2 = new LineSeg(Vector2.zero, new Vector2(1f, -1f) * 5f);
	public LineRenderer lineRend;
	public LineRenderer lineRend2;
	Vector3[] circlePositions;
	Vector3[] circle2Positions;
	Vector3[] polyPositions;
	public int thetaScale = 100;

	void Start() {
//		DrawShape(lineRend, GenerateCirclePositions(c));
//		DrawShape(lineRend, GetPolyPositions(poly2));
		DrawShape(lineRend, GetPolyPositions(poly3));
	}

	void Awake() {
		if (lineRend == null) {
			lineRend = GetComponent<LineRenderer>();
		}
	}

	void Update() {
//		bool isIntersecting = Geometry2D.PointInPoly(Camera.main.ScreenToWorldPoint(Input.mousePosition), poly3.GetVerts());

		c2.center = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		DrawShape(lineRend2, GenerateCirclePositions(c2));
		bool isIntersecting = c2.Intersects(poly3);

//		poly.center = Camera.main.ScreenToWorldPoint(Input.mousePosition);
//		DrawShape(lineRend2, GetPolyPositions(poly));
//		bool isIntersecting = poly.Intersects(poly2);

		if (isIntersecting) {
			lineRend.startColor = Color.green;
			lineRend.endColor = Color.green;
		} else {
			lineRend.startColor = Color.red;
			lineRend.endColor = Color.red;
		}
	}

	private void DrawShape(LineRenderer lr, Vector3[] positions) {
		if (lr != null) {
			lr.numPositions = positions.Length;
			lr.SetPositions(positions);
		}
	}

	private Vector3[] GenerateCirclePositions(Circle circ) {
		Vector3[] positions = new Vector3[thetaScale + 1];

		for (int i = 0; i < positions.Length; i++) {
			positions[i].x = Mathf.Sin(((float)i / thetaScale) * (2f * Mathf.PI));
			positions[i].y = Mathf.Cos(((float)i / thetaScale) * (2f * Mathf.PI));
			positions[i] *= circ.GetRadius();
			positions[i] += (Vector3)circ.center;
		}



		return positions;
	}

	private Vector3[] GetPolyPositions(Poly p) {
		Vector3[] positions = new Vector3[p.GetVerts().Count + 1];

		for (int i = 0; i < p.GetVerts().Count; i++) {
			positions[i] = p.GetVerts()[i] + p.center;
		}

		positions[positions.Length - 1] = positions[0];

		return positions;
	}
}
