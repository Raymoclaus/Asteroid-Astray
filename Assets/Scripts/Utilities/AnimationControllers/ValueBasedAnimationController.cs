using UnityEngine;
using ValueComponents;

public abstract class ValueBasedAnimationController<T> : MonoBehaviour
{
	[SerializeField] private ValueComponent<T> valueComponent;
	[SerializeField] protected Animator animator;
	[SerializeField] private string animatorValueName;

	private void Awake()
	{
		valueComponent.OnValueChanged += UpdateAnimator;
	}

	protected abstract void UpdateAnimator(T oldVal, T newVal);

	protected string AnimatorValueName => animatorValueName;
}
