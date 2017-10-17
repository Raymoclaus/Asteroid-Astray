using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ShakeEffect
{
	public static IEnumerator Shake(Transform obj, float intensity, float time, bool diminishing)
	{
		//init some variables
		float counter = 0f, angle = 0f, distance = 0f;
		Vector2 shakenPos = Vector2.zero, centerPos = obj.position;

		//loop until counter reaches time. If time is less than or equal to 0 then do this infinitely
		while (time > 0f ? counter < time : true)
		{
			//increment counter
			counter += Time.deltaTime;
			//pick a random angle and distance based on intensity
			angle = Random.Range(0f, 360f);
			distance = Random.Range(0f, diminishing ? intensity * (1 - (counter / time)) : intensity);
			//calculate position to shake to
			shakenPos.x = Mathf.Sin(Mathf.Deg2Rad * angle);
			shakenPos.y = Mathf.Cos(Mathf.Deg2Rad * angle);
			shakenPos *= distance;
			//apply position to object
			obj.position = centerPos + shakenPos;
			//wait until end of frame
			yield return 0;
		}

		//re-center object to original position
		obj.position = centerPos;
	}
}
