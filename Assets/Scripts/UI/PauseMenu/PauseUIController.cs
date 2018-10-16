using System.Collections;
using UnityEngine;

public class PauseUIController : MonoBehaviour
{
	[SerializeField]
	private RectTransform[] tabs;
	[SerializeField]
	private CanvasGroup[] panels;
	[SerializeField]
	private float tabShiftDuration = 0.6f;
	[SerializeField]
	private CanvasGroup pauseUIGroup;
	[SerializeField]
	private CustomScreenEffect uiCam;
	[SerializeField]
	private CustomScreenEffect mainCam;
	[SerializeField]
	private Material uiRenderEffect;

	public void Activate(bool active, System.Action a = null, bool instant = false)
	{
		if (gameObject.activeSelf == active) return;

		if (active)
		{
			Coro(OpenUI(instant));
		}
		else
		{
			Coro(CloseUI(a, instant));
		}
	}

	public void ClickedTab(int tabID)
	{
		StartCoroutine(TabShift(tabID));
		bool organised = false;
		int increment = 0;
		while (!organised)
		{
			int check = tabID + increment;
			bool reachedLeftEdge = check >= tabs.Length;
			if (!reachedLeftEdge)
			{
				tabs[check].SetAsFirstSibling();
			}
			check = tabID - increment;
			bool reachedRightEdge = check < 0;
			if (!reachedRightEdge)
			{
				tabs[check].SetAsFirstSibling();
			}
			if (reachedLeftEdge && reachedRightEdge)
			{
				organised = true;
			}
			increment++;
		}
	}

	private IEnumerator TabShift(int tabID)
	{
		float timer = 0f;
		while (timer < tabShiftDuration)
		{
			timer += GameController.UnscaledDeltaTime;

			for (int i = 0; i < panels.Length; i++)
			{
				float moveTo = i == tabID ? 1f : 0f;
				panels[i].alpha = Mathf.Lerp(-i + 1f, moveTo, timer / tabShiftDuration);
			}
			yield return null;
		}
	}

	private IEnumerator OpenUI(bool instant = false)
	{
		ActivateRender(true);
		if (!instant)
		{
			float timer = 0f;
			while (timer < 1f)
			{
				timer += GameController.UnscaledDeltaTime * 2f;
				pauseUIGroup.alpha = Mathf.Lerp(0f, 1f, timer);
				yield return null;
			}
		}
		pauseUIGroup.alpha = 1f;
	}

	private IEnumerator CloseUI(System.Action a, bool instant = false)
	{
		if (!instant)
		{
			float timer = 0f;
			while (timer < 1f)
			{
				timer += GameController.UnscaledDeltaTime * 2f;
				pauseUIGroup.alpha = Mathf.Lerp(1f, 0f, timer);
				yield return null;
			}
		}
		pauseUIGroup.alpha = 0f;
		if (a != null) a();
		ActivateRender(false);
		gameObject.SetActive(false);
	}

	private void ActivateRender(bool activate)
	{
		uiCam.gameObject.SetActive(activate);
		int i = 0;
		for (; i < mainCam.effects.Length; i++)
		{
			if (mainCam.effects[i] == uiRenderEffect)
			{
				mainCam.SetNoBlit(i, !activate);
				break;
			}
		}
	}

	private void Coro(IEnumerator c)
	{
		gameObject.SetActive(true);
		StartCoroutine(c);
	}
}