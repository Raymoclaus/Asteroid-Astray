using UnityEngine;

public class MainHatchPrompt : MonoBehaviour, IPromptRespone
{
	public string interactString;
	private KeyCode interactKey = KeyCode.E;
	[SerializeField]
	private ShipEntryPanel shipEntryUI;

	public bool CheckResponse()
	{
		return Input.GetKey(interactKey);
	}

	public void Enter()
	{

	}

	public void Execute()
	{
		Pause.InstantPause(true);
		shipEntryUI = shipEntryUI ?? FindObjectOfType<ShipEntryPanel>();
		shipEntryUI.OpenPanel();
	}

	public void Exit()
	{
		shipEntryUI = shipEntryUI ?? FindObjectOfType<ShipEntryPanel>();
		shipEntryUI.gameObject.SetActive(false);
	}

	public string InteractString()
	{
		return string.Format(interactString, interactKey.ToString());
	}
}