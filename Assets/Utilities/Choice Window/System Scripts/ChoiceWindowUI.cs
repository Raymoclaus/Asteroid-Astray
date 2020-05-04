using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class ChoiceWindowUI : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI message;
	[SerializeField] private TextButtonUI textButtonPrefab;
	[SerializeField] private IconButtonUI iconButtonPrefab;
	[SerializeField] private Transform buttonHolder;
	[SerializeField] private CanvasGroup canvasGroup;
	[SerializeField] private float fadeInDuration = 0.5f;

	private void Start()
	{
		canvasGroup.alpha = 0f;
	}

	private void Update()
	{
		canvasGroup.alpha += Time.unscaledDeltaTime / Mathf.Min(0.001f, fadeInDuration);
	}

	public TextButtonUI AddTextButton(string label, UnityAction onClickListener)
	{
		TextButtonUI newButton = Instantiate(textButtonPrefab, buttonHolder);
		newButton.SetLabelText(label);
		newButton.AddListener(onClickListener);
		return newButton;
	}

	public IconButtonUI AddIconButton(Sprite icon, UnityAction onClickListener)
	{
		IconButtonUI newButton = Instantiate(iconButtonPrefab, buttonHolder);
		newButton.SetIcon(icon);
		newButton.AddListener(onClickListener);
		return newButton;
	}

	public void SetMessage(string text)
	{
		message.text = text;
	}

	public void Close()
	{
		Destroy(gameObject);
	}
}
