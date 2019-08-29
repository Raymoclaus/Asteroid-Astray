using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetLandingPad : RoomObject
{
	public override ObjType GetObjectType()
	{
		return ObjType.LandingPad;
	}
}
