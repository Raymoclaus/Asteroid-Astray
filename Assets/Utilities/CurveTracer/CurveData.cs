using UnityEngine;

namespace CurveTracerSystem
{
	[System.Serializable]
	public struct CurveData
	{
		public Vector3 positionA, positionB;
		public AnimationCurve curve;
	}

}