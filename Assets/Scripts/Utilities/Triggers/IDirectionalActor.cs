using UnityEngine;

public interface IDirectionalActor : IActor
{
	Vector3 FacingDirection { get; }
	Transform GetTransform { get; }
}
