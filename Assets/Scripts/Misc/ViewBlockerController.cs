using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewBlockerController : MonoBehaviour
{
	[SerializeField] private CanvasGroup cGroup;
	[SerializeField] private float revealSpeed = 0.3f;

	private void Awake()
	{
		cGroup.alpha = 1f;
		SceneryController.AddListener(() =>
		{
			StartCoroutine(Reveal());
		});
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
