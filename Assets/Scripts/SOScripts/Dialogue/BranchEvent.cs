using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Dialogue/Branch Event")]
public class BranchEvent : ScriptableObject
{
	public ConversationWithActions[] nextConversations;
	[SerializeField]
	protected DialogueChoiceDirection[] options;
	private int choice;

	public ConversationWithActions GetConversation()
	{
		return nextConversations[choice];
	}

	public void ChoiceOptionChosen(DialogueChoiceDirection dcd)
	{
		if (dcd.direction < 0)
		{
			Debug.Log("choice direction value cannot be below 0, defaulting to 0");
			choice = 0;
		}
		if (dcd.direction >= nextConversations.Length)
		{
			Debug.Log("choice direction value cannot exceed or equal the length of nextConversations array, defaulting to 0");
			dcd.direction = 0;
		}

		choice = dcd.direction;
	}

	public virtual DialogueChoiceDirection[] GetOptions()
	{
		return options;
	}
}