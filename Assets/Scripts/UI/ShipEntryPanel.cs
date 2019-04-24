using UnityEngine;

public class ShipEntryPanel : MonoBehaviour
{
	private Shuttle shuttle;
	private Shuttle Shuttle
	{
		get { return shuttle ?? (shuttle = FindObjectOfType<Shuttle>()); }
	}
	[SerializeField] private GameObject holder;

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			ClosePanel();
		}
	}

	public void ClosePanel()
	{
		if (holder.activeSelf)
		{
			Pause.InstantPause(false);
			holder.SetActive(false);
		}
	}

	public void OpenPanel()
	{
		if (!holder.activeSelf)
		{
			Pause.InstantPause(true);
			holder.SetActive(true);
		}
	}

	public void Store() => Shuttle.StoreInShip();
}