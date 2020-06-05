using UnityEngine;
using ValueComponents;

public class FloatRatioBasedAnimationController : ValueBasedAnimationController<float>
{
	[SerializeField] private RangedFloatComponent rangedFloatComponent;

	protected override ValueComponent<float> GetValueComponent
		=> rangedFloatComponent;

	protected override void UpdateAnimator(float oldVal, float newVal)
	{
		animator.SetFloat(AnimatorPropertyName, rangedFloatComponent.CurrentRatio);
	}
}
