using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IReceiveTriggerMessage
{
	void ReceiveTriggerMessage(Collider2D col, Collider2D otherCol);
	bool CanReceiveMessagesFromLayer(int layer);
}
