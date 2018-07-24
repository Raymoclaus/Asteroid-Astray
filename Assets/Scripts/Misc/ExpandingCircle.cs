using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ExpandingCircle : MonoBehaviour
{
	private LineRenderer lr;
	public float maxRadius = 3f;
	public float arcSize = 360f;
	public float rot = 0f;
	private float currentTimer;
	public float lifeTime = 3f;
	private List<Vector3> points;
	private Vector2 origin;
	public Color startColor, endColor;
	public bool loop = true;

	private void Start()
	{
		arcSize = Mathf.Clamp(arcSize, 0f, 360f);
		lr = GetComponent<LineRenderer>();
		lr.loop = loop;
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
			float angle = (i / 360f) * Mathf.PI * 2f + rot - arcSize / 2f;
			Vector2 pos = origin + new Vector2(Mathf.Sin(angle), Mathf.Cos(angle)) * r;
			lr.SetPosition(i, pos);
		}
	}

	private void FillPoints()
	{
		points = new List<Vector3>((int)arcSize);
		lr.positionCount = (int)arcSize;
		for (int i = 0; i <= (int)arcSize; i++)
		{
			points.Add(Vector2.zero);
		}
		lr.SetPositions(points.ToArray());
	}
}