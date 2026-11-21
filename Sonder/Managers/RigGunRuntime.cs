using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Sonder.Managers;

public class RigGunRuntime : MonoBehaviour
{
	private const float SpazStrength = 0.45f;

	private const float ChaseSpeed = 14f;

	private const float OrbitRadius = 1.6f;

	private const float OrbitSpeed = 3.4f;

	private const float ConfuseMinDistance = 2.5f;

	private const float ConfuseMaxDistance = 7f;

	private float _orbitAngle;

	private float _confuseNextTime;

	public static RigGunRuntime Instance { get; private set; }

	public bool RigSpazGun { get; set; }

	public bool ConfusePlayerGun { get; set; }

	public bool ChasePlayerGun { get; set; }

	public bool OrbitPlayerGun { get; set; }

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
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		GunRuntime instance = GunRuntime.Instance;
		if ((Object)(object)instance == (Object)null || !instance.gunLocked || (Object)(object)instance.lockTarget == (Object)null)
		{
			return;
		}
		VRRig lockTarget = instance.lockTarget;
		if (lockTarget.isLocal || lockTarget.isOfflineVRRig)
		{
			return;
		}
		if (RigSpazGun)
		{
			Transform transform = ((Component)lockTarget).transform;
			transform.position += Random.insideUnitSphere * (0.45f * Time.deltaTime * 60f * 0.1f);
		}
		if (ConfusePlayerGun && Time.time > _confuseNextTime)
		{
			_confuseNextTime = Time.time + 0.75f;
			Vector3 val = ExtraUtil.PlayerPosition();
			Vector2 insideUnitCircle = Random.insideUnitCircle;
			Vector2 normalized = insideUnitCircle.normalized;
			float num = Random.Range(2.5f, 7f);
			((Component)lockTarget).transform.position = val + new Vector3(normalized.x, 0.2f, normalized.y) * num;
		}
		if (ChasePlayerGun)
		{
			Vector3 val2 = ExtraUtil.PlayerPosition() - ((Component)lockTarget).transform.position;
			val2.y = 0f;
			if (val2.sqrMagnitude > 1f)
			{
				Transform transform2 = ((Component)lockTarget).transform;
				transform2.position += val2.normalized * (14f * Time.deltaTime);
			}
		}
		if (OrbitPlayerGun)
		{
			_orbitAngle += 3.4f * Time.deltaTime;
			if (_orbitAngle > 360f)
			{
				_orbitAngle -= 360f;
			}
			Vector3 val3 = ExtraUtil.PlayerPosition();
			Vector3 val4 = default(Vector3);
			val4 = new Vector3(Mathf.Sin(_orbitAngle) * 1.6f, 1.2f, Mathf.Cos(_orbitAngle) * 1.6f);
			((Component)lockTarget).transform.position = val3 + val4;
		}
	}
}
