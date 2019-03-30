using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Debug Gameplay Manager")]
public class DebugGameplayManager : ScriptableObject
{
	public bool skipRecoveryDialogue = false;
	public bool skipFirstGatheringQuest = false;
	public bool skipMakeARepairKitQuest = false;
	public bool skipRepairTheShuttleQuest = false;
	public bool skipReturnToTheShipQuest = false;
	public bool skipAquireAnEnergySourceQuest = false;
}
