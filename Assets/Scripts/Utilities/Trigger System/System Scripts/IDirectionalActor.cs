using UnityEngine;

namespace TriggerSystem
{
	public interface IDirectionalActor : IActor
	{
		Vector3 FacingDirection { get; }
		Transform GetTransform { get; }
	}

}