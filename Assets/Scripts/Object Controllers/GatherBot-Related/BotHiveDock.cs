using UnityEngine;

public class BotHiveDock : MonoBehaviour
{
	[SerializeField]
	private BotHive hive;
	[SerializeField]
	private int ID;

	public void ActivateBot()
	{
		StartCoroutine(hive.ActivateBot(ID, transform.position));
	}

	public void StartMaintenanceTimer()
	{
		StartCoroutine(DelayedAction.Go(() => hive.BuildBot(ID), hive.maintenanceTime));
	}
}