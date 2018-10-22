using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ExpandingCircle : MonoBehaviour
{
	private LineRenderer lr;
	public float maxRadius = 3f;
	public float arcSize = 360f;
	public int arcDetail = 360;
	public float rot = 0f;
	private float currentTimer;
	public float lifeTime = 3f;
	private List<Vector3> points;
	private Vector2 origin;
	public Color startColor, endColor;
	public bool loop = true;
	public bool lerpGrowth = false;
	private float currentRadius = 0f;
	public float growthPower = 1f;
	public float fadePower = 0.8f;
	
	private void Start()
	{
		arcSize = Mathf.Clamp(arcSize, 0f, 360f);
		arcDetail = Mathf.Max(0, arcDetail);
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
		currentRadius = Mathf.Lerp(lerpGrowth ? currentRadius : 0f, maxRadius, Mathf.Pow(delta, growthPower));
		Color c = Color.Lerp(startColor, endColor, Mathf.Pow(delta, fadePower));
		lr.material.color = c;
		UpdateRadius();

		if (currentTimer >= lifeTime)
		{
			Destroy(gameObject);
		}
	}

	private void UpdateRadius()
	{
		for (int i = 0; i < lr.positionCount; i++)
		{
			float angle = (i + rot) * (arcSize / arcDetail);
			//float angle = i + rot - arcSize / 2f;
			angle *= Mathf.Deg2Rad;
			Vector2 pos = origin + new Vector2(Mathf.Sin(angle), Mathf.Cos(angle)) * currentRadius;
			lr.SetPosition(i, pos);
		}
	}

	private void FillPoints()
	{
		points = new List<Vector3>(arcDetail);
		lr.positionCount = arcDetail;
		for (int i = 0; i < arcDetail; i++)
		{
			points.Add(Vector2.zero);
		}
		lr.SetPositions(points.ToArray());
	}
}