using UnityEngine;

public class CollisionSFXTrigger : MonoBehaviour
{
	[SerializeField] private float volumeMultiplier = 1f;
	[SerializeField] private AudioSO collisionSounds;
	protected static AudioManager audioManager;
	protected static AudioManager AudioMngr => audioManager != null ? audioManager
		: (audioManager = FindObjectOfType<AudioManager>());
	[SerializeField] private float ignoreRange = 15f;
	private ContactPoint2D[] contacts = new ContactPoint2D[1];
	[SerializeField] private float ignoreCollisionStrength = 0.05f;
	private static AudioListener listener;
	private static AudioListener Listener => listener != null ? listener
		: (listener = FindObjectOfType<AudioListener>());

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (collisionSounds == null) return;
		float collisionStrength = collision.relativeVelocity.magnitude * volumeMultiplier;
		if (collisionStrength < ignoreCollisionStrength) return;

		Vector2 contactPoint = collision.otherCollider.bounds.center;
		float distance = Vector2.SqrMagnitude(contactPoint - (Vector2)ListenerPosition);
		if (distance >= ignoreRange) return;

		AudioMngr.PlaySFX(collisionSounds.PickRandomClip(), contactPoint, null,
			collisionSounds.PickRandomVolume() * collisionStrength, collisionSounds.PickRandomPitch());
	}

	private Vector3 ListenerPosition => Listener.transform.position;
}
