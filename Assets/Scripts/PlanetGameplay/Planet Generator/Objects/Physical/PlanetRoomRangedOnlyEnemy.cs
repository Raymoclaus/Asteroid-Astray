using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ChaseAndKeepDistanceBehaviour))]
public class PlanetRoomRangedOnlyEnemy : PlanetRoomChasingEnemy
{
	protected new ChaseAndKeepDistanceBehaviour MovementBehaviour
		=> (ChaseAndKeepDistanceBehaviour)base.MovementBehaviour;

	protected override bool ShouldAttack()
		=> base.ShouldAttack() && !MovementBehaviour.IsWithinRunAwayRange();
}