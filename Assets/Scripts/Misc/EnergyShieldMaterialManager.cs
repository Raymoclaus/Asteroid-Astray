﻿using EquipmentSystem;
using System.Collections;
using UnityEngine;
using ValueComponents;

[RequireComponent(typeof(SpriteRenderer))]
public class EnergyShieldMaterialManager : MonoBehaviour, IEquipment, IShieldMaterial
{
	[SerializeField] private RangedFloatComponent shieldComponent;
	private Material mat;
	private SpriteRenderer sprRend;
	[SerializeField] private Animator anim;
	private const string FORCE_RADIUS = "_ForceRadius", DISTORTION_AMPLITUDE = "_DistortionAmplitude",
		FORCE_LOCATION_X = "_ForceLocationX", FORCE_LOCATION_Y = "_ForceLocationY",
		RIPPLE_PROGRESS = "_RippleProgress", RIPPLE_ANGLE = "_RippleAngle";
	[SerializeField] private float dist = Mathf.Sqrt(2f), duration = 0.3f, rippleDuration = 0.3f,
		targetForceRadius = 1f, targetDistortionAmplitude = 2f, startingRippleProgress = 0.4f,
		targetRippleProgress = 0.7f;
	[SerializeField] private Vector3 defaultScale = Vector3.one * 1.2f;

	private void Awake()
	{
		sprRend = GetComponent<SpriteRenderer>();
		mat = sprRend.material;
		UpdateShield(shieldComponent.CurrentRatio, 1f);
		shieldComponent.OnValueChanged += UpdateShield;
		SetDefaultScale();
	}

	private void UpdateShield(float oldVal, float newVal)
	{
		if (newVal <= 0f)
		{
			Break();
		}
		else if (oldVal <= 0f)
		{
			Restore();
		}
	}

	private void LateUpdate()
	{
		transform.eulerAngles = Vector3.zero;
	}

	private void Restore()
	{
		sprRend.enabled = true;
		anim.SetTrigger("Idle");
	}

	private void Break() => anim.SetTrigger("Break");

	private void Hide() => sprRend.enabled = false;

	public void TakeHit(Vector3 direction)
	{
		StopAllCoroutines();
		Vector2 pos = direction.normalized * dist;
		SetForcePosition(pos);
		SetRippleAngle(Vector2.SignedAngle(Vector2.up, -direction) * Mathf.Deg2Rad);
		StartCoroutine(Bend());
	}

	private IEnumerator Bend()
	{
		float timer = 0f;
		while (timer < duration)
		{
			timer += Time.deltaTime;

			if (timer >= duration)
			{
				timer = duration * 1.1f;
			}
			
			float halfPoint = duration / 2f;
			float delta = 1f - Mathf.Abs(timer - halfPoint) / halfPoint;
			SetForceRadius(targetForceRadius * delta);
			SetDistortionAmplitude(targetDistortionAmplitude * delta);
			float rippleProgress = Mathf.Lerp(startingRippleProgress, targetRippleProgress,
				timer / rippleDuration);
			SetRippleProgress(rippleProgress);

			yield return null;
		}

		while (timer < rippleDuration)
		{
			timer += Time.deltaTime;

			float rippleProgress = Mathf.Lerp(startingRippleProgress, targetRippleProgress,
				timer / rippleDuration);
			SetRippleProgress(rippleProgress);

			yield return null;
		}

		SetForceRadius(0f);
		SetDistortionAmplitude(0f);
	}

	private void SetForceRadius(float radius) => mat?.SetFloat(FORCE_RADIUS, radius);

	private void SetDistortionAmplitude(float amplitude)
		=> mat?.SetFloat(DISTORTION_AMPLITUDE, amplitude);

	private void SetForcePosition(Vector2 pos)
	{
		mat?.SetFloat(FORCE_LOCATION_X, pos.x);
		mat?.SetFloat(FORCE_LOCATION_Y, pos.y);
	}

	private void SetRippleProgress(float progress)
		=> mat?.SetFloat(RIPPLE_PROGRESS, progress);

	private void SetRippleAngle(float angle) => mat?.SetFloat(RIPPLE_ANGLE, angle);

	public void Shrink()
	{
		Coroutines.TimedAction(1f,
			delta => SetScaleMod(1f - delta),
			null,
			false);
	}

	public void SetDefaultScale() => SetScaleMod(1f);

	public void SetScaleMod(float mod) => transform.localScale = defaultScale * mod;
}
