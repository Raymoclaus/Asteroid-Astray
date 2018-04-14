using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ExpandingCircle : MonoBehaviour
{
	private LineRenderer lr;
	private const int edges = 360;
	[SerializeField]
	private float maxRadius = 3f;
	private float currentTimer;
	public float lifeTime = 3f;
	private List<Vector3> points = new List<Vector3>(edges);
	private Vector2 origin;
	public Color startColor, endColor;

	private void Start()
	{
		lr = GetComponent<LineRenderer>();
		for (int i = 0; i < lr.colorGradient.colorKeys.Length; i++)
		{
			lr.colorGradient.colorKeys[i].color = startColor;
		}
		origin = transform.position;
		FillPoints();
	}

	private void Update()
	{
		currentTimer += Time.deltaTime;
		float delta = currentTimer / lifeTime;
		float radius = Mathf.Lerp(0.01f, maxRadius, delta);
		Color c = Color.Lerp(startColor, endColor, Mathf.Pow(delta, 0.8f));
		lr.material.color = c;
		UpdateRadius(radius);

		if (currentTimer >= lifeTime)
		{
			Destroy(gameObject);
		}
	}

	private void UpdateRadius(float r)
	{
		for (int i = 0; i < lr.positionCount; i++)
		{
			float angle = (float)i / edges * Mathf.PI * 2f;
			Vector2 pos = origin + new Vector2(Mathf.Sin(angle), Mathf.Cos(angle)) * r;
			lr.SetPosition(i, pos);
		}
	}

	private void FillPoints()
	{
		lr.positionCount = edges;
		for (int i = 0; i < edges; i++)
		{
			points.Add(Vector2.zero);
		}
		lr.SetPositions(points.ToArray());
	}
}