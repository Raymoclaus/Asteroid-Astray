using UnityEngine;
using ValueComponents;

public abstract class ValueBasedAnimationController<T> : MonoBehaviour
{
	[SerializeField] protected Animator animator;
	[SerializeField] private string animatorPropertyName;

	protected abstract ValueComponent<T> GetValueComponent { get; }

	private void Awake()
	{
		GetValueComponent.OnValueChanged += UpdateAnimator;
	}

	protected abstract void UpdateAnimator(T oldVal, T newVal);

	protected string AnimatorPropertyName => animatorPropertyName;
}
