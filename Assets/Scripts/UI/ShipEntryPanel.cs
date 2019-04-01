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

	public void Store()
	{
		shuttle = shuttle ?? FindObjectOfType<Shuttle>();
		shuttle.StoreInShip();
	}
}