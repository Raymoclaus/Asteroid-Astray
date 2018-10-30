using UnityEngine;

public class ShipEntryPanel : MonoBehaviour
{
	[SerializeField]
	private Shuttle shuttle;
	[SerializeField]
	private GameObject holder;

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			ClosePanel();
		}
	}

	public void ClosePanel()
	{
		Pause.InstantPause(false);
		holder.SetActive(false);
	}

	public void OpenPanel()
	{
		Pause.InstantPause(true);
		holder.SetActive(true);
	}

	public void Store()
	{
		shuttle = shuttle ?? FindObjectOfType<Shuttle>();
		shuttle.StoreInShip();
	}
}