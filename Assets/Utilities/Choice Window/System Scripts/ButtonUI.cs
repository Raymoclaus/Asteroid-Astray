using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ButtonUI : MonoBehaviour
{
	[SerializeField] private Button button;

	public void AddListener(UnityAction action)
	{
		button.onClick.AddListener(action);
	}

	public void Select()
	{
		button.Select();
	}
}
