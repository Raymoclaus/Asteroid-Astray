using SceneControllers;
using UnityEngine;

public class ExitPlanetPrompt : MonoBehaviour
{
	[SerializeField] private GameObject promptObject;

	public delegate void ActivatePromptEventHandler(bool activate);
	public static ActivatePromptEventHandler OnActivatePrompt;
	public static void ActivatePrompt() => OnActivatePrompt?.Invoke(true);

	private void OnEnable() => OnActivatePrompt += Activate;

	private void OnDisable() => OnActivatePrompt -= Activate;

	public void Yes()
	{
		Activate(false);
		SceneLoader.LoadScene("SpaceScene");
	}

	public void No() => Activate(false);

	public void Activate(bool activate)
	{
		promptObject.SetActive(activate);
		TimeController.SetTimeScale(this, activate ? 0f : 1f);
	}
}
