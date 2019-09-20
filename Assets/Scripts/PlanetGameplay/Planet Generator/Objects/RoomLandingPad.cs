using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomLandingPad : RoomObject
{
	public override ObjType GetObjectType()
	{
		return ObjType.LandingPad;
	}
}
