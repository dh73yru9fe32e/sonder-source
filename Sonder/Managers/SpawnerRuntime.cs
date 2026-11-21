using System.Collections.Generic;
using Photon.Pun;
using Sonder.Core;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Sonder.Managers;

// Master-only spawn guns for Ghost Reactor and Critters.
public class SpawnerRuntime : MonoBehaviour
{
	private const float SpawnDelay = 0.25f;

	private static readonly CrittersActor.CrittersActorType[] CritterItemTypes = new CrittersActor.CrittersActorType[6]
	{
		CrittersActor.CrittersActorType.StunBomb,
		CrittersActor.CrittersActorType.StickyTrap,
		CrittersActor.CrittersActorType.NoiseMaker,
		CrittersActor.CrittersActorType.Cage,
		CrittersActor.CrittersActorType.Bag,
		CrittersActor.CrittersActorType.Grabber
	};

	private readonly List<GameEntity> _grItems = new List<GameEntity>();

	private readonly List<string> _foodNames = new List<string>();

	private int _grIndex;

	private int _critterIndex;

	private int _foodIndex;

	private int _critterItemIndex;

	private float _nextSpawn;

	private bool _warned;

	public static SpawnerRuntime Instance { get; private set; }

	public bool GRItemGun { get; set; }

	public bool GRDestroyGun { get; set; }

	public bool CritterGun { get; set; }

	public bool CritterFoodGun { get; set; }

	public bool CritterItemGun { get; set; }

	private void Awake()
	{
		Instance = this;
	}

	private void OnDestroy()
	{
		if ((Object)(object)Instance == (Object)(object)this)
		{
			Instance = null;
		}
	}

	private void Update()
	{
		if ((!GRItemGun && !GRDestroyGun && !CritterGun && !CritterFoodGun && !CritterItemGun) || (Object)(object)GunRuntime.Instance == (Object)null)
		{
			return;
		}
		if (!GunRuntime.GetGunInput(trigger: true))
		{
			_warned = false;
			return;
		}
		if (Time.time < _nextSpawn)
		{
			return;
		}
		_nextSpawn = Time.time + SpawnDelay;
		Vector3 point = GunRuntime.RenderGun().Ray.point;
		if (point == Vector3.zero)
		{
			return;
		}
		Vector3 position = point + Vector3.up * 0.3f;
		if (GRItemGun && LoadGRItems())
		{
			SpawnGRItem(_grItems[_grIndex], position);
		}
		if (GRDestroyGun)
		{
			DestroyGRNear(point);
		}
		if (CritterGun)
		{
			SpawnCritter(_critterIndex, position);
		}
		if (CritterFoodGun)
		{
			SpawnCritterActor(CrittersActor.CrittersActorType.Food, _foodIndex, position);
		}
		if (CritterItemGun)
		{
			SpawnCritterActor(CritterItemTypes[_critterItemIndex], -1, position);
		}
	}

	// ---------- Ghost Reactor ----------

	public static string SelectedGRItemName
	{
		get
		{
			if ((Object)(object)Instance == (Object)null || !Instance.LoadGRItems())
			{
				return "NONE";
			}
			return CleanName(((Object)Instance._grItems[Instance._grIndex].gameObject).name);
		}
	}

	public static void NextGRItem()
	{
		if (!((Object)(object)Instance == (Object)null) && Instance.LoadGRItems())
		{
			Instance._grIndex = (Instance._grIndex + 1) % Instance._grItems.Count;
			NotificationRuntime.Show("Item: " + SelectedGRItemName);
		}
	}

	public static void SpawnAllGRItemsAtHand()
	{
		if ((Object)(object)Instance == (Object)null || (Object)(object)GorillaTagger.Instance == (Object)null)
		{
			return;
		}
		Instance._warned = false;
		if (!Instance.LoadGRItems())
		{
			Instance.Warn("Not in Ghost Reactor");
			return;
		}
		Vector3 hand = GorillaTagger.Instance.rightHandTransform.position;
		for (int i = 0; i < Instance._grItems.Count; i++)
		{
			Vector3 offset = Quaternion.Euler(0f, 360f * i / Instance._grItems.Count, 0f) * Vector3.forward * 0.7f;
			Instance.SpawnGRItem(Instance._grItems[i], hand + offset);
		}
	}

	private static GameEntityManager GRManager()
	{
		// Prefer the manager for the zone you are standing in (reactor, tunnel or drill).
		if ((Object)(object)VRRig.LocalRig != (Object)null && VRRig.LocalRig.zoneEntity != null)
		{
			GTZone zone = VRRig.LocalRig.zoneEntity.currentZone;
			if (zone == GTZone.ghostReactor || zone == GTZone.ghostReactorTunnel || zone == GTZone.ghostReactorDrill)
			{
				GameEntityManager local = GameEntityManager.GetManagerForZone(zone);
				if ((Object)(object)local != (Object)null)
				{
					return local;
				}
			}
		}
		GhostReactor reactor = GhostReactor.instance;
		if ((Object)(object)reactor == (Object)null || (Object)(object)reactor.grManager == (Object)null)
		{
			return null;
		}
		return reactor.grManager.gameEntityManager;
	}

	private bool LoadGRItems()
	{
		if (_grItems.Count > 0)
		{
			return true;
		}
		GameEntityManager manager = GRManager();
		if ((Object)(object)manager == (Object)null || manager.tempFactoryItems == null)
		{
			return false;
		}
		foreach (GameEntity entity in manager.tempFactoryItems)
		{
			if ((Object)(object)entity != (Object)null && IsGRItem(entity) && !_grItems.Contains(entity))
			{
				_grItems.Add(entity);
			}
		}
		_grIndex = 0;
		return _grItems.Count > 0;
	}

	// Ghost Reactor items carry a tool, collectible, core or breakable component. The factory list also holds
	// other modes' entities, and Custom Maps agents carry GRAttributes, so a plain "GR" prefix is not enough.
	private static bool IsGRItem(GameEntity entity)
	{
		if (((Object)entity.gameObject).name.StartsWith("CustomMaps") || IsEnemy(entity))
		{
			return false;
		}
		foreach (MonoBehaviour behaviour in ((Component)entity).GetComponentsInChildren<MonoBehaviour>(true))
		{
			if ((Object)(object)behaviour == (Object)null)
			{
				continue;
			}
			string type = ((object)behaviour).GetType().Name;
			if (type.StartsWith("GRTool") || type == "GRCollectible" || type == "GRSentientCore" || type == "GRBreakable")
			{
				return true;
			}
		}
		return false;
	}

	// Enemies are left out so this can't be used to swarm other players.
	private static bool IsEnemy(GameEntity entity)
	{
		foreach (MonoBehaviour behaviour in ((Component)entity).GetComponentsInChildren<MonoBehaviour>(true))
		{
			if ((Object)(object)behaviour != (Object)null && ((object)behaviour).GetType().Name.StartsWith("GREnemy"))
			{
				return true;
			}
		}
		return false;
	}

	private void SpawnGRItem(GameEntity prefab, Vector3 position)
	{
		GameEntityManager manager = GRManager();
		if ((Object)(object)manager == (Object)null)
		{
			Warn("Not in Ghost Reactor");
			return;
		}
		if (!manager.IsZoneAuthority())
		{
			Warn("Need master of the reactor");
			return;
		}
		// The game only opens the reactor once you are in a room and standing inside its zone.
		if (!PhotonNetwork.InRoom)
		{
			Warn("Join a private room first");
			return;
		}
		if (PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.IsVisible)
		{
			Warn("Private rooms only");
			return;
		}
		if (!manager.IsZoneActive())
		{
			Warn("Walk into the reactor first");
			return;
		}
		string name = ((Object)prefab.gameObject).name;
		Quaternion rotation = Quaternion.Euler(0f, Random.Range(0, 360), 0f);
		position = PullInsideBounds(manager, position);
		if (manager.RequestCreateItem(name.GetStaticHash(), position, rotation, 0L) == GameEntityId.Invalid)
		{
			string reason = (!manager.IsPositionInManagerBounds(position)) ? "aim inside the reactor" : "blocked by the game";
			if (!_warned)
			{
				BoxCollider box = manager.boundsBoxCollider;
				RuntimeLog.Write("spawner: GR create failed at " + position + ", bounds=" + (((Object)(object)box == (Object)null) ? "none" : (box.bounds.ToString() + " enabled=" + box.enabled)));
			}
			Warn(CleanName(name) + ": " + reason);
		}
	}

	// The game rejects creates outside the reactor's bounds box, which a ray hitting the outer wall easily lands
	// past. Nudge near misses back inside rather than failing.
	private static Vector3 PullInsideBounds(GameEntityManager manager, Vector3 position)
	{
		BoxCollider box = manager.boundsBoxCollider;
		if ((Object)(object)box == (Object)null || manager.IsPositionInManagerBounds(position))
		{
			return position;
		}
		Bounds bounds = box.bounds;
		if (bounds.size == Vector3.zero)
		{
			return position;
		}
		bounds.Expand(-0.4f);
		Vector3 inside = bounds.ClosestPoint(position);
		if (Vector3.Distance(inside, position) > 1.5f)
		{
			return position;
		}
		return inside;
	}

	private void DestroyGRNear(Vector3 point)
	{
		GameEntityManager manager = GRManager();
		if ((Object)(object)manager == (Object)null)
		{
			Warn("Not in Ghost Reactor");
			return;
		}
		if (!manager.IsZoneAuthority())
		{
			Warn("Need master");
			return;
		}
		foreach (Collider hit in Physics.OverlapSphere(point, 0.4f, -1, QueryTriggerInteraction.Collide))
		{
			GameEntity entity = ((Component)hit).GetComponentInParent<GameEntity>();
			if ((Object)(object)entity != (Object)null && (Object)(object)entity.manager == (Object)(object)manager && !IsEnemy(entity))
			{
				manager.RequestDestroyItem(entity.id);
			}
		}
	}

	// ---------- Critters ----------

	public static string SelectedCritterName
	{
		get
		{
			CritterIndex index = CritterTypes();
			if ((Object)(object)index == (Object)null)
			{
				return "NONE";
			}
			CritterConfiguration config = index[Instance._critterIndex];
			return (config != null) ? config.critterName.ToUpperInvariant() : "NONE";
		}
	}

	public static string SelectedFoodName
	{
		get
		{
			if ((Object)(object)Instance == (Object)null || !Instance.LoadFoodNames())
			{
				return "NONE";
			}
			return CleanName(Instance._foodNames[Instance._foodIndex]);
		}
	}

	public static string SelectedCritterItemName
	{
		get
		{
			if ((Object)(object)Instance == (Object)null)
			{
				return "NONE";
			}
			return CritterItemTypes[Instance._critterItemIndex].ToString().ToUpperInvariant();
		}
	}

	public static void NextCritter()
	{
		CritterIndex index = CritterTypes();
		if (!((Object)(object)index == (Object)null) && index.critterTypes.Count != 0)
		{
			Instance._critterIndex = (Instance._critterIndex + 1) % index.critterTypes.Count;
			NotificationRuntime.Show("Critter: " + SelectedCritterName);
		}
	}

	public static void NextFood()
	{
		if (!((Object)(object)Instance == (Object)null) && Instance.LoadFoodNames())
		{
			Instance._foodIndex = (Instance._foodIndex + 1) % Instance._foodNames.Count;
			NotificationRuntime.Show("Food: " + SelectedFoodName);
		}
	}

	public static void NextCritterItem()
	{
		if (!((Object)(object)Instance == (Object)null))
		{
			Instance._critterItemIndex = (Instance._critterItemIndex + 1) % CritterItemTypes.Length;
			NotificationRuntime.Show("Item: " + SelectedCritterItemName);
		}
	}

	private static CritterIndex CritterTypes()
	{
		CrittersManager manager = CrittersManager.instance;
		if ((Object)(object)Instance == (Object)null || (Object)(object)manager == (Object)null || (Object)(object)manager.creatureIndex == (Object)null || manager.creatureIndex.critterTypes == null)
		{
			return null;
		}
		return manager.creatureIndex;
	}

	private bool LoadFoodNames()
	{
		if (_foodNames.Count > 0)
		{
			return true;
		}
		foreach (CrittersFood food in Resources.FindObjectsOfTypeAll<CrittersFood>())
		{
			if ((Object)(object)food == (Object)null || food.subObjects == null || food.subObjects.Length == 0)
			{
				continue;
			}
			foreach (GameObject sub in food.subObjects)
			{
				_foodNames.Add(((Object)(object)sub != (Object)null) ? ((Object)sub).name : "FOOD");
			}
			break;
		}
		_foodIndex = 0;
		return _foodNames.Count > 0;
	}

	private CrittersManager CritterAuthority()
	{
		CrittersManager manager = CrittersManager.instance;
		if ((Object)(object)manager == (Object)null)
		{
			Warn("Not in Critters");
			return null;
		}
		if (!manager.LocalAuthority())
		{
			Warn("Need master");
			return null;
		}
		return manager;
	}

	private void SpawnCritter(int critterType, Vector3 position)
	{
		CrittersManager manager = CritterAuthority();
		if (!((Object)(object)manager == (Object)null))
		{
			// The game's own SpawnCritter throws when every critter slot is in use.
			try
			{
				if ((Object)(object)manager.SpawnCritter(critterType, position, Quaternion.identity) == (Object)null)
				{
					Warn("Critter limit reached");
				}
			}
			catch (System.NullReferenceException)
			{
				Warn("Critter limit reached");
			}
		}
	}

	private void SpawnCritterActor(CrittersActor.CrittersActorType type, int subIndex, Vector3 position)
	{
		CrittersManager manager = CritterAuthority();
		if ((Object)(object)manager == (Object)null)
		{
			return;
		}
		CrittersActor actor = manager.SpawnActor(type, subIndex);
		if ((Object)(object)actor == (Object)null)
		{
			Warn(type.ToString() + " limit reached");
			return;
		}
		actor.MoveActor(position, Quaternion.Euler(0f, Random.Range(0, 360), 0f));
		actor.SetImpulseVelocity(Vector3.zero, Vector3.zero);
		if (actor is CrittersFood food)
		{
			food.SpawnData(food.maxFood, food.maxFood, food.startingSize);
		}
	}

	// ---------- Shared ----------

	private static string CleanName(string name)
	{
		return name.Replace("(Clone)", "").Replace("_", " ").Trim().ToUpperInvariant();
	}

	private void Warn(string message)
	{
		if (!_warned)
		{
			_warned = true;
			RuntimeLog.Write("spawner: " + message);
			NotificationRuntime.Show(message);
		}
	}
}
