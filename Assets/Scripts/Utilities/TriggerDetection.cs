using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class TriggerDetection : MonoBehaviour
{
	private List<IReceiveTriggerMessage> receivers
		= new List<IReceiveTriggerMessage>();
	private Collider2D col;
	private Collider2D Col => col ?? (col = GetComponent<Collider2D>());

	private void Awake() => GetReceivers();

	private void GetReceivers()
	{
		Transform t = transform;
		int layer = gameObject.layer;
		while (t != null)
		{
			foreach (IReceiveTriggerMessage rtm in t.GetComponents<IReceiveTriggerMessage>())
			{
				if (rtm.CanReceiveMessagesFromLayer(layer))
				{
					receivers.Add(rtm);
				}
			}
			t = t.parent;
		}
	}

	private void OnTriggerEnter2D(Collider2D otherCol)
	{
		DetectedTrigger(otherCol);
	}

	private void DetectedTrigger(Collider2D otherCol)
	{
		for (int i = 0; i < receivers.Count; i++)
		{
			receivers[i].ReceiveTriggerMessage(Col, otherCol);
		}
	}
}
