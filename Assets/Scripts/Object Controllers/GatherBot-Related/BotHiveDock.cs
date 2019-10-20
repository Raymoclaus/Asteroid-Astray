using UnityEngine;

public class BotHiveDock : MonoBehaviour
{
	[SerializeField]
	private BotHive hive;
	[SerializeField]
	private int ID;

	public void ActivateBot()
	{
		hive.ActivateBot(ID, transform.position);
	}

	public void StartMaintenanceTimer()
	{
		Coroutines.DelayedAction(hive.maintenanceTime, () => hive.BuildBot(ID));
	}
}