using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class VicinityTrigger : MonoBehaviour
{
	public delegate void VicinityTriggerEventHandler();
	public event VicinityTriggerEventHandler OnEnterTrigger, OnExitTrigger;
	private bool triggerActive = false;
	private CircleCollider2D col;

	protected virtual void Awake()
	{
		OnEnterTrigger += EnterTrigger;
		OnExitTrigger += ExitTrigger;

		col = GetComponent<CircleCollider2D>();
		col.isTrigger = true;
		gameObject.layer = LayerMask.NameToLayer("VicinityTrigger");
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (IsTriggerActive() || !collision.attachedRigidbody.GetComponent<Shuttle>()) return;
		triggerActive = true;
		OnEnterTrigger?.Invoke();
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		if (!IsTriggerActive() || !collision.attachedRigidbody.GetComponent<Shuttle>()) return;
		triggerActive = false;
		OnExitTrigger?.Invoke();
	}

	protected bool IsTriggerActive() => triggerActive;

	protected virtual void EnterTrigger() { }

	protected virtual void ExitTrigger() { }
}
