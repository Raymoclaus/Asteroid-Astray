using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IActionMessageReceiver
{
	void Interacted(IInteractor interactor, string action);
}
