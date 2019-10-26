using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ValueComponents;

public class FloatBasedAnimationController : ValueBasedAnimationController<float>
{
	[SerializeField] private FloatComponent floatComponent;

	protected override ValueComponent<float> GetValueComponent => floatComponent;

	protected override void UpdateAnimator(float oldVal, float newVal)
	{
		animator.SetFloat(AnimatorPropertyName, newVal);
	}
}
