using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public abstract class PauseTab : MonoBehaviour
{
	private CanvasGroup cg;

	public virtual void OnResume() { }

	public virtual void OnOpen() { }

	public CanvasGroup GetCanvasGroup()
	{
		return cg ?? (cg = GetComponent<CanvasGroup>());
	}
}
