using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatBasedAnimationController : ValueBasedAnimationController<float>
{
	protected override void UpdateAnimator(float oldVal, float newVal)
	{
		animator.SetFloat(AnimatorValueName, newVal);
	}
}
