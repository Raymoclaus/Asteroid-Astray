using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Scriptable Objects/Debug Gameplay Manager")]
public class DebugGameplayManager : ScriptableObject
{
	[FormerlySerializedAs("skipIntro")]
	public bool skipRecoveryDialogue = false;
	public bool skipFirstGatheringQuest = false;
	public bool skipMakeARepairKitQuest = false;
	public bool skipRepairTheShuttleQuest = false;
}
