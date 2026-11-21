using System.Collections.Generic;
using Sonder.Core;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Sonder.Managers;

public class SuperInfectionRuntime : MonoBehaviour
{
	private const float SpawnDelay = 0.15f;

	private const float DestroyRadius = 0.4f;

	private readonly HashSet<SIResource.ResourceType> _guns = new HashSet<SIResource.ResourceType>();

	private readonly Dictionary<SIResource.ResourceType, int> _typeIds = new Dictionary<SIResource.ResourceType, int>();

	private float _nextSpawn;

	private bool _warned;

	public static SuperInfectionRuntime Instance { get; private set; }

	public bool AllResourcesGun { get; set; }

	public bool DestroyResourceGun { get; set; }

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

	public void SetGun(SIResource.ResourceType type, bool on)
	{
		if (on)
		{
			_guns.Add(type);
		}
		else
		{
			_guns.Remove(type);
		}
	}

	private void Update()
	{
		if ((_guns.Count == 0 && !AllResourcesGun && !DestroyResourceGun) || (Object)(object)GunRuntime.Instance == (Object)null)
		{
			return;
		}
		if (!GunRuntime.GetGunInput(trigger: true))
		{
			_warned = false;
			return;
		}
		Vector3 point = GunRuntime.RenderGun().Ray.point;
		if (point == Vector3.zero)
		{
			return;
		}
		if (Time.time < _nextSpawn)
		{
			return;
		}
		_nextSpawn = Time.time + SpawnDelay;
		if (DestroyResourceGun)
		{
			DestroyNear(point);
		}
		if (_guns.Count == 0 && !AllResourcesGun)
		{
			return;
		}
		Vector3 position = point + Vector3.up * 0.15f;
		if (AllResourcesGun)
		{
			for (int i = 0; i < (int)SIResource.ResourceType.Count; i++)
			{
				Spawn((SIResource.ResourceType)i, position);
			}
			return;
		}
		foreach (SIResource.ResourceType gun in _guns)
		{
			Spawn(gun, position);
		}
	}

	public static void SpawnAllAtHand()
	{
		if (!((Object)(object)Instance == (Object)null) && !((Object)(object)GorillaTagger.Instance == (Object)null))
		{
			Instance._warned = false;
			Vector3 position = GorillaTagger.Instance.rightHandTransform.position;
			for (int i = 0; i < (int)SIResource.ResourceType.Count; i++)
			{
				Instance.Spawn((SIResource.ResourceType)i, position);
			}
		}
	}

	private bool Spawn(SIResource.ResourceType type, Vector3 position)
	{
		SuperInfectionManager manager = SuperInfectionManager.activeSuperInfectionManager;
		if ((Object)(object)manager == (Object)null || (Object)(object)manager.gameEntityManager == (Object)null)
		{
			Warn("Not in Super Infection");
			return false;
		}
		GameEntityManager entityManager = manager.gameEntityManager;
		if (!entityManager.IsZoneAuthority())
		{
			Warn("Need master");
			return false;
		}
		if (!TryGetTypeId(manager, type, out var typeId))
		{
			Warn(type.ToString() + " not found");
			return false;
		}
		Quaternion rotation = Quaternion.Euler(0f, Random.Range(0, 360), 0f);
		return entityManager.RequestCreateItem(typeId, position, rotation, 0L) != GameEntityId.Invalid;
	}

	private void DestroyNear(Vector3 point)
	{
		SuperInfectionManager manager = SuperInfectionManager.activeSuperInfectionManager;
		if ((Object)(object)manager == (Object)null || (Object)(object)manager.gameEntityManager == (Object)null)
		{
			Warn("Not in Super Infection");
			return;
		}
		GameEntityManager entityManager = manager.gameEntityManager;
		if (!entityManager.IsZoneAuthority())
		{
			Warn("Need master");
			return;
		}
		foreach (Collider hit in Physics.OverlapSphere(point, DestroyRadius, -1, QueryTriggerInteraction.Collide))
		{
			SIResource resource = ((Component)hit).GetComponentInParent<SIResource>();
			if ((Object)(object)resource != (Object)null && (Object)(object)resource.myGameEntity != (Object)null)
			{
				entityManager.RequestDestroyItem(resource.myGameEntity.id);
			}
		}
	}

	private bool TryGetTypeId(SuperInfectionManager manager, SIResource.ResourceType type, out int typeId)
	{
		if (_typeIds.TryGetValue(type, out typeId))
		{
			return true;
		}
		SuperInfection zone = manager.zoneSuperInfection;
		if ((Object)(object)zone != (Object)null && zone.resourceRegions != null)
		{
			foreach (SIResourceRegion region in zone.resourceRegions)
			{
				if ((Object)(object)region != (Object)null && (Object)(object)region.resourcePrefab != (Object)null && region.resourcePrefab.type == type)
				{
					typeId = ((Object)region.resourcePrefab.gameObject).name.GetStaticHash();
					_typeIds[type] = typeId;
					return true;
				}
			}
		}
		foreach (SIResource resource in Resources.FindObjectsOfTypeAll<SIResource>())
		{
			if ((Object)(object)resource != (Object)null && resource.type == type)
			{
				typeId = ((Object)resource.gameObject).name.Replace("(Clone)", "").Trim().GetStaticHash();
				_typeIds[type] = typeId;
				return true;
			}
		}
		return false;
	}

	private void Warn(string message)
	{
		if (!_warned)
		{
			_warned = true;
			RuntimeLog.Write("super infection: " + message);
			NotificationRuntime.Show(message);
		}
	}
}
