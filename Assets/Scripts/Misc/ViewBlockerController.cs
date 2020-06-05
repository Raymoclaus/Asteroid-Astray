using System.Collections;
using UnityEngine;

public class ViewBlockerController : MonoBehaviour
{
	[SerializeField] private CanvasGroup cGroup;
	[SerializeField] private float revealSpeed = 0.3f;

	private void Awake()
	{
		cGroup.alpha = 1f;

		SceneryController sc = FindObjectOfType<SceneryController>();
		if (sc != null)
		{
			sc.OnStarFieldCreated.RunWhenReady(() => StartCoroutine(Reveal()));
		}
	}

	private IEnumerator Reveal()
	{
		while (cGroup.alpha > 0f)
		{
			cGroup.alpha = Mathf.MoveTowards(cGroup.alpha, 0f, Time.deltaTime * revealSpeed);
			yield return null;
		}
	}
}
