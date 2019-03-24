using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
	[Header("Entity Fields")]
	[SerializeField] protected ChunkCoords _coords;
	public Collider2D[] col;
	public Rigidbody2D rb;
	[SerializeField] protected CameraCtrlTracker camTrackerSO;
	[SerializeField] private EntityPrefabDB prefabs;
	[SerializeField] protected static ParticleGenerator particleGenerator;
	private static LoadingController loadingController;
	[SerializeField] private LoadingController loadingControllerPrefab;
	private static MainCanvas mainCanvas;
	[SerializeField] private MainCanvas mainCanvasPrefab;
	protected static Pause pause;
	protected static AudioManager audioManager;
	[SerializeField] protected ScreenRippleEffectController screenRippleSO;
	protected bool entityReady = false;
	public bool shouldDisablePhysicsOnDistance = true;
	public bool shouldDisableObjectOnDistance = true;
	public bool shouldDisableGameObjectOnShortDistance = true;
	public bool isActive = true;
	public bool disabled = false;
	public bool isInPhysicsRange = false;
	private Vector3 vel;
	private float disableTime;
	protected bool needsInit = true;
	protected bool initialised = false;

	//related layers
	private static bool layersSet;
	protected static int layerDrill, layerProjectile, layerSolid;

	//components to disable/enable
	public List<MonoBehaviour> ScriptComponents;
	public Renderer[] rends;

	private static int entitiesActive;

	public static int GetActive()
	{
		return entitiesActive;
	}

	public virtual void Awake()
	{
		mainCanvas = mainCanvas ?? FindObjectOfType<MainCanvas>() ?? Instantiate(mainCanvasPrefab);
		loadingController = loadingController ?? FindObjectOfType<LoadingController>()
			?? Instantiate(loadingControllerPrefab, mainCanvas.transform);
		if (!EntityNetwork.ready || !loadingController.finishedLoading)
		{
			gameObject.SetActive(false);
			EntityNetwork.postInitActions.Add(() =>
			{
				Initialise();
				gameObject.SetActive(true);
			});
		}
		else
		{
			Initialise();
		}
	}

	public virtual void Initialise()
	{
		entitiesActive++;
		_coords = new ChunkCoords(transform.position);
		EntityNetwork.AddEntity(this, _coords);
		GetLayers();
		entityReady = true;
	}

	public virtual void LateUpdate()
	{
		RepositionInNetwork();
	}

	private void OnDestroy()
	{
		EntityNetwork.RemoveEntity(this);
	}

	public void RepositionInNetwork()
	{
		ChunkCoords newCc = new ChunkCoords(transform.position);
		bool repositioned = false;
		if (newCc != _coords)
		{ 
			EntityNetwork.Reposition(this, newCc);
			repositioned = true;
		}

		if (needsInit && !initialised) return;

		SetAllActivity(IsInView());
		isInPhysicsRange = IsInPhysicsRange();
		if (shouldDisablePhysicsOnDistance)
		{
			if (isInPhysicsRange)
			{
				if (!disabled) return;
				entitiesActive++;
				disabled = false;
				gameObject.SetActive(true);
				if (rb != null)
				{
					rb.simulated = true;
				}
				PhysicsReEnabled();
			}
			else
			{
				if (disabled) return;
				if (repositioned && OnExitPhysicsRange()) return;
				entitiesActive--;
				disabled = true;
				vel = rb == null ? vel : (Vector3)rb.velocity;
				if (rb != null)
				{
					rb.simulated = !shouldDisablePhysicsOnDistance;
				}
				gameObject.SetActive(!shouldDisablePhysicsOnDistance);
			}
		}
	}

	public virtual bool OnExitPhysicsRange()
	{
		return false;
	}

	public void SetCoordinates(ChunkCoords newCc)
	{
		_coords = newCc;
	}

	protected bool IsInView()
	{
		if (!camTrackerSO) return false;
		return camTrackerSO.IsCoordInView(_coords);
	}

	protected bool IsInPhysicsRange()
	{
		if (!camTrackerSO) return false;
		return camTrackerSO.IsCoordInPhysicsRange(_coords);
	}

	public void SetAllActivity(bool active)
	{
		if (active == isActive || !shouldDisableObjectOnDistance) return;
		if (needsInit && !initialised) return;

		isActive = active;

		if (shouldDisableGameObjectOnShortDistance)
		{
			gameObject.SetActive(active);
			return;
		}

		ActivateRenderers(active);

		//enable/disable all relevant components
		for (int i = 0; i < ScriptComponents.Count; i++)
		{
			MonoBehaviour script = ScriptComponents[i];
			if (script != null)
			{
				script.enabled = active;
			}
		}
	}

	protected void ActivateAllColliders(bool activate)
	{
		for (int i = 0; i < col.Length; i++)
		{
			col[i].enabled = activate;
		}
	}

	protected void ActivateRenderers(bool active)
	{
		if (active) active = ShouldBeVisible();

		for (int i = 0; i < rends.Length; i++)
		{
			Renderer r = rends[i];
			if (r != null) r.enabled = active;
		}
	}

	protected virtual bool ShouldBeVisible()
	{
		return true;
	}

	public virtual ICombat GetICombat()
	{
		return null;
	}

	public virtual void CollectResources(Item.Type type, int amount)
	{

	}

	public virtual EntityType GetEntityType()
	{
		return EntityType.Entity;
	}

	public virtual void DestroySelf(Entity destroyer)
	{
		if (destroyer)
		{
			destroyer.DestroyedAnEntity(this);
		}
		if (EntityNetwork.ConfirmLocation(this, _coords))
		{
			EntityNetwork.RemoveEntity(this);
		}
		Destroy(gameObject);
	}

	public ChunkCoords GetCoords()
	{
		return _coords;
	}

	public override string ToString()
	{
		return string.Format("{0} at coordinates {1}.", GetEntityType(), _coords);
	}

	public virtual LaunchTrailController GetLaunchTrailAnimation()
	{
		return null;
	}

	public virtual GameObject GetLaunchImpactAnimation()
	{
		return null;
	}

	public virtual void PhysicsReEnabled()
	{

	}

	private void GetLayers()
	{
		if (layersSet) return;

		layerDrill = LayerMask.NameToLayer("Drill");
		layerProjectile = LayerMask.NameToLayer("Projectile");
		layerSolid = LayerMask.NameToLayer("Solid");

		layersSet = true;
	}

	public virtual Scan ReturnScan()
	{
		return new Scan(GetEntityType(), 1f, 1);
	}

	public virtual void DestroyedAnEntity(Entity target)
	{

	}

	public virtual void Launching()
	{

	}

	public virtual bool CanFireLaser()
	{
		return false;
	}

	public virtual bool CanFireStraightWeapon()
	{
		return false;
	}

	public virtual void AttachLaser(bool attach)
	{

	}

	public virtual void AttachStraightWeapon(bool attach)
	{

	}
}

public enum EntityType
{
	Entity,
	Asteroid,
	Shuttle,
	Nebula,
	BotHive,
	GatherBot
}