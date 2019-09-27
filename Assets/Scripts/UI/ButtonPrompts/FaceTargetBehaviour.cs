using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceTargetBehaviour : TargetBasedBehaviour
{
	private void Update()
	{
		FaceDirection(TargetDirection);
	}
}
