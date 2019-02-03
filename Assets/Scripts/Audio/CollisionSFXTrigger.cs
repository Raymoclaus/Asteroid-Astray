using UnityEngine;

public class CollisionSFXTrigger : MonoBehaviour
{
	[SerializeField]
	private float volumeMultiplier = 1f;
	[SerializeField]
	private AudioSO collisionSounds;
	protected static AudioManager audioManager;
	[SerializeField]
	private float ignoreRange = 15f;
	[SerializeField]
	private CameraCtrlTracker camTrackerSO;
	private ContactPoint2D[] contacts = new ContactPoint2D[1];
	[SerializeField]
	private float ignoreCollisionStrength = 0.05f;

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (!collisionSounds) return;
		float collisionStrength = collision.relativeVelocity.magnitude * volumeMultiplier;
		if (collisionStrength < ignoreCollisionStrength) return;


		//collision.GetContacts(contacts);
		//Vector2 contactPoint = contacts[0].point;
		Vector2 contactPoint = (collision.collider.bounds.center
			- collision.otherCollider.bounds.center) / 2f
			+ collision.otherCollider.bounds.center;
		float distance = Vector2.Distance(contactPoint, camTrackerSO.position);
		if (distance > ignoreRange) return;

		audioManager = audioManager ?? FindObjectOfType<AudioManager>();
		if (audioManager)
		{
			audioManager.PlaySFX(collisionSounds.PickRandomClip(), contactPoint, null,
			collisionSounds.PickRandomVolume() * collisionStrength, collisionSounds.PickRandomPitch());
		}
	}
}
