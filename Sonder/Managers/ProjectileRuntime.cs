using System;
using System.Collections.Generic;
using System.Reflection;
using GorillaLocomotion;
using Sonder.Core;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using UnityEngine.Rendering;

namespace Sonder.Managers;

public class ProjectileRuntime : MonoBehaviour
{
	private sealed class LagState
	{
		public VRRig Rig;

		public float Until;

		public float NextToggle;
	}

	private sealed class BubbleState
	{
		public VRRig Rig;

		public GameObject Bubble;

		public float Until;
	}

	private sealed class PaintState
	{
		public Material Material;

		public Color Original;

		public float Until;
	}

	private sealed class BananaState
	{
		public GameObject Banana;

		public Vector3 Position;

		public float Until;

		public float NextSlip;
	}

	private sealed class Push
	{
		public VRRig Rig;

		public Vector3 Velocity;

		public float Until;
	}

	private static readonly MethodInfo LaunchSnowballMethod = typeof(SnowballThrowable).GetMethod("LaunchSnowballLocal", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

	private readonly List<LagState> _lagged = new List<LagState>();

	private readonly List<BubbleState> _bubbles = new List<BubbleState>();

	private readonly List<BananaState> _bananas = new List<BananaState>();

	private readonly List<Push> _pushes = new List<Push>();

	private readonly Dictionary<int, PaintState> _painted = new Dictionary<int, PaintState>();

	private bool _triggerHeld;

	private float _nextLag;

	private float _nextSnow;

	private float _nextBubble;

	private float _nextBanana;

	private float _nextPaint;

	private bool _snowballErrorLogged;

	private LineRenderer _laser;

	private GameObject _laserDot;

	private Material _laserMaterial;

	private Material _dotMaterial;

	public static ProjectileRuntime Instance { get; private set; }

	public bool LagGun { get; set; }

	public bool SnowballCannon { get; set; }

	public bool BubbleBlaster { get; set; }

	public bool BananaGun { get; set; }

	public bool PaintBall { get; set; }

	public bool SnowVolley { get; set; }

	public bool BombToss { get; set; }

	public bool LaserPointer { get; set; }

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
		RestorePaint();
		RestoreLag();
		ClearBubbles();
		ClearBananas();
		ClearPushes();
		DestroyLaser();
	}

	private void Update()
	{
		UpdateEffects();
		ControllerInputPoller instance = ControllerInputPoller.instance;
		bool flag = (Object)(object)instance != (Object)null && (instance.rightControllerTriggerButton || instance.rightControllerIndexFloat > 0.5f);
		bool pressed = flag && !_triggerHeld;
		_triggerHeld = flag;
		if (MenuManager.IsOpen)
		{
			UpdateLaser(active: false);
			return;
		}
		FireProjectiles(flag, pressed);
		UpdateLaser(LaserPointer);
	}

	private void FireProjectiles(bool trigger, bool pressed)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		//IL_024b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0250: Unknown result type (might be due to invalid IL or missing references)
		//IL_0255: Unknown result type (might be due to invalid IL or missing references)
		//IL_0263: Unknown result type (might be due to invalid IL or missing references)
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		//IL_026d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0271: Unknown result type (might be due to invalid IL or missing references)
		//IL_0276: Unknown result type (might be due to invalid IL or missing references)
		//IL_0279: Unknown result type (might be due to invalid IL or missing references)
		//IL_027b: Unknown result type (might be due to invalid IL or missing references)
		//IL_028b: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0299: Unknown result type (might be due to invalid IL or missing references)
		float time = Time.time;
		Transform val = ExtraUtil.Hand(left: false);
		Transform val2 = ExtraUtil.Head();
		Transform val3 = (((Object)(object)val != (Object)null) ? val : val2);
		if ((Object)(object)val3 == (Object)null)
		{
			return;
		}
		Vector3 origin = val3.position + val3.forward * 0.28f + Vector3.up * 0.04f;
		Vector3 forward = val3.forward;
		if ((LagGun & trigger) && time >= _nextLag)
		{
			_nextLag = time + 0.45f;
			Spawn(SonderShotType.Lag, origin, forward, 28f, 0.6f, new Color(0.65f, 0.4f, 1f), 0.16f, 0.14f, 6f);
		}
		if ((SnowballCannon & trigger) && time >= _nextSnow)
		{
			_nextSnow = time + 0.18f;
			if (!FireSnowball(origin, forward, 28f))
			{
				Spawn(SonderShotType.Snow, origin, forward, 28f, 5f, Color.white, 0.14f, 0.12f, 5f);
			}
		}
		if ((BubbleBlaster & trigger) && time >= _nextBubble)
		{
			_nextBubble = time + 0.65f;
			Spawn(SonderShotType.Bubble, origin, forward, 17f, 1.4f, new Color(0.35f, 0.85f, 1f, 0.9f), 0.24f, 0.2f, 6f);
		}
		if ((BananaGun & trigger) && time >= _nextBanana)
		{
			_nextBanana = time + 0.75f;
			Spawn(SonderShotType.Banana, origin, forward, 17f, 8f, new Color(1f, 0.83f, 0.2f), 0.18f, 0.16f, 6f);
		}
		if ((PaintBall & trigger) && time >= _nextPaint)
		{
			_nextPaint = time + 0.35f;
			Color color = Color.HSVToRGB(Random.value, 0.85f, 1f);
			Spawn(SonderShotType.Paint, origin, forward, 26f, 0.8f, color, 0.15f, 0.14f, 6f);
		}
		if (SnowVolley & pressed)
		{
			for (int i = -2; i <= 2; i++)
			{
				Vector3 val4 = forward + val3.right * ((float)i * 0.055f) + Vector3.up * ((float)i * 0.02f);
				Vector3 normalized = val4.normalized;
				if (!FireSnowball(origin, normalized, 25f))
				{
					Spawn(SonderShotType.Snow, origin, normalized, 25f, 5f, Color.white, 0.14f, 0.12f, 5f);
				}
			}
		}
		if (BombToss & pressed)
		{
			Spawn(SonderShotType.Bomb, origin, forward, 13f, 9f, new Color(1f, 0.45f, 0.15f), 0.3f, 0.28f, 7f);
		}
	}

	private bool FireSnowball(Vector3 origin, Vector3 direction, float speed)
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			SnowballMaker rightHandInstance = SnowballMaker.rightHandInstance;
			if ((Object)(object)rightHandInstance == (Object)null)
			{
				return false;
			}
			SnowballThrowable val = default(SnowballThrowable);
			if (!rightHandInstance.TryCreateSnowball(0, out val) || (Object)(object)val == (Object)null)
			{
				return false;
			}
			if (LaunchSnowballMethod == null)
			{
				return false;
			}
			LaunchSnowballMethod.Invoke(val, new object[5]
			{
				origin,
				direction.normalized * speed,
				1f,
				false,
				Color.white
			});
			return true;
		}
		catch (Exception ex)
		{
			if (!_snowballErrorLogged)
			{
				_snowballErrorLogged = true;
				RuntimeLog.Write("snowball failed: " + ex.Message);
			}
			return false;
		}
	}

	private void Spawn(SonderShotType type, Vector3 origin, Vector3 direction, float speed, float gravity, Color color, float scale, float radius, float life)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		GameObject obj = GameObject.CreatePrimitive((PrimitiveType)0);
		((Object)obj).name = "SonderShot_" + type;
		Collider component = obj.GetComponent<Collider>();
		if ((Object)(object)component != (Object)null)
		{
			Object.Destroy((Object)(object)component);
		}
		obj.transform.position = origin;
		obj.transform.localScale = Vector3.one * scale;
		Material val = ExtraUtil.Unlit(color, type == SonderShotType.Bubble);
		MeshRenderer component2 = obj.GetComponent<MeshRenderer>();
		if ((Object)(object)component2 != (Object)null)
		{
			((Renderer)component2).sharedMaterial = val;
			((Renderer)component2).shadowCastingMode = (ShadowCastingMode)0;
			((Renderer)component2).receiveShadows = false;
		}
		SonderProjectile sonderProjectile = obj.AddComponent<SonderProjectile>();
		sonderProjectile.Runtime = this;
		sonderProjectile.Type = type;
		sonderProjectile.Velocity = direction.normalized * speed;
		sonderProjectile.Gravity = gravity;
		sonderProjectile.Life = life;
		sonderProjectile.Radius = radius;
		sonderProjectile.PaintColor = color;
		sonderProjectile.SpawnTime = Time.time;
		sonderProjectile.Material = val;
	}

	public void Hit(SonderProjectile projectile, Collider collider, Vector3 point)
	{
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		VRRig val = FindRig(collider);
		if ((Object)(object)val != (Object)null && !val.isLocal)
		{
			switch (projectile.Type)
			{
			case SonderShotType.Lag:
				ApplyLag(val);
				break;
			case SonderShotType.Bubble:
				ApplyBubble(val);
				break;
			case SonderShotType.Banana:
				ApplyBanana(val, point);
				break;
			case SonderShotType.Paint:
				ApplyPaint(val, projectile.PaintColor);
				break;
			case SonderShotType.Bomb:
				ApplyBomb(point);
				break;
			default:
				Pulse(point, projectile.PaintColor);
				break;
			}
		}
		else if (projectile.Type == SonderShotType.Bomb)
		{
			ApplyBomb(point);
		}
		else if (projectile.Type == SonderShotType.Banana)
		{
			SpawnBanana(point);
		}
		else
		{
			Pulse(point, projectile.PaintColor);
		}
	}

	private static VRRig FindRig(Collider collider)
	{
		if ((Object)(object)collider == (Object)null)
		{
			return null;
		}
		RigContainer componentInParent = ((Component)collider).GetComponentInParent<RigContainer>();
		if ((Object)(object)componentInParent != (Object)null && (Object)(object)componentInParent.Rig != (Object)null)
		{
			return componentInParent.Rig;
		}
		return ((Component)collider).GetComponentInParent<VRRig>();
	}

	private void ApplyLag(VRRig rig)
	{
		for (int i = 0; i < _lagged.Count; i++)
		{
			if ((Object)(object)_lagged[i].Rig == (Object)(object)rig)
			{
				_lagged[i].Until = Time.time + 3f;
				return;
			}
		}
		_lagged.Add(new LagState
		{
			Rig = rig,
			Until = Time.time + 3f,
			NextToggle = Time.time
		});
		RuntimeLog.Write("lag gun: " + ExtraUtil.Nickname(rig));
	}

	private void ApplyBubble(VRRig rig)
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < _bubbles.Count; i++)
		{
			if ((Object)(object)_bubbles[i].Rig == (Object)(object)rig)
			{
				_bubbles[i].Until = Time.time + 5f;
				return;
			}
		}
		GameObject val = GameObject.CreatePrimitive((PrimitiveType)0);
		((Object)val).name = "SonderBubble";
		Collider component = val.GetComponent<Collider>();
		if ((Object)(object)component != (Object)null)
		{
			Object.Destroy((Object)(object)component);
		}
		val.transform.SetParent(((Component)rig).transform, false);
		val.transform.localPosition = new Vector3(0f, 1.2f, 0f);
		val.transform.localScale = Vector3.one * 1.65f;
		Material sharedMaterial = ExtraUtil.Unlit(new Color(0.4f, 0.9f, 1f, 0.24f), transparent: true);
		MeshRenderer component2 = val.GetComponent<MeshRenderer>();
		if ((Object)(object)component2 != (Object)null)
		{
			((Renderer)component2).sharedMaterial = sharedMaterial;
			((Renderer)component2).shadowCastingMode = (ShadowCastingMode)0;
			((Renderer)component2).receiveShadows = false;
		}
		_bubbles.Add(new BubbleState
		{
			Rig = rig,
			Bubble = val,
			Until = Time.time + 5f
		});
		RuntimeLog.Write("bubble: " + ExtraUtil.Nickname(rig));
	}

	private void ApplyBanana(VRRig rig, Vector3 point)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = ((Component)rig).transform.position - ExtraUtil.PlayerPosition();
		val = Vector3.ProjectOnPlane(val, Vector3.up);
		if (val.sqrMagnitude < 0.001f)
		{
			val = Vector3.forward;
		}
		AddPush(rig, (val.normalized + Vector3.up * 0.2f) * 15f, 0.6f);
		SpawnBanana(point);
		RuntimeLog.Write("banana: " + ExtraUtil.Nickname(rig));
	}

	private void SpawnBanana(Vector3 point)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = GameObject.CreatePrimitive((PrimitiveType)0);
		((Object)val).name = "SonderBanana";
		Collider component = val.GetComponent<Collider>();
		if ((Object)(object)component != (Object)null)
		{
			Object.Destroy((Object)(object)component);
		}
		Vector3 val2 = ExtraUtil.Grounded(point);
		val.transform.position = new Vector3(val2.x, val2.y + 0.03f, val2.z);
		val.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
		val.transform.localScale = new Vector3(0.32f, 0.05f, 0.18f);
		Material sharedMaterial = ExtraUtil.Unlit(new Color(1f, 0.82f, 0.15f), transparent: false);
		MeshRenderer component2 = val.GetComponent<MeshRenderer>();
		if ((Object)(object)component2 != (Object)null)
		{
			((Renderer)component2).sharedMaterial = sharedMaterial;
			((Renderer)component2).shadowCastingMode = (ShadowCastingMode)0;
			((Renderer)component2).receiveShadows = false;
		}
		_bananas.Add(new BananaState
		{
			Banana = val,
			Position = val.transform.position,
			Until = Time.time + 12f,
			NextSlip = Time.time
		});
	}

	private void ApplyPaint(VRRig rig, Color color)
	{
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)rig.mainSkin == (Object)null)
		{
			return;
		}
		Material material = ((Renderer)rig.mainSkin).material;
		if (!((Object)(object)material == (Object)null) && rig.Creator != null)
		{
			int actorNumber = rig.Creator.ActorNumber;
			if (!_painted.ContainsKey(actorNumber))
			{
				Color original = (material.HasProperty("_Color") ? material.color : material.GetColor("_BaseColor"));
				_painted[actorNumber] = new PaintState
				{
					Material = material,
					Original = original,
					Until = Time.time + 6f
				};
			}
			else
			{
				_painted[actorNumber].Until = Time.time + 6f;
			}
			SetMaterialColor(material, color);
			RuntimeLog.Write("paint: " + ExtraUtil.Nickname(rig));
		}
	}

	private static void SetMaterialColor(Material material, Color color)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		if (material.HasProperty("_Color"))
		{
			material.color = color;
		}
		else if (material.HasProperty("_BaseColor"))
		{
			material.SetColor("_BaseColor", color);
		}
	}

	private void ApplyBomb(Vector3 point)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		GameObject obj = GameObject.CreatePrimitive((PrimitiveType)0);
		((Object)obj).name = "SonderBombPulse";
		Collider component = obj.GetComponent<Collider>();
		if ((Object)(object)component != (Object)null)
		{
			Object.Destroy((Object)(object)component);
		}
		obj.transform.position = point;
		obj.transform.localScale = Vector3.one * 0.3f;
		Material val = ExtraUtil.Unlit(new Color(1f, 0.45f, 0.1f, 0.5f), transparent: true);
		MeshRenderer component2 = obj.GetComponent<MeshRenderer>();
		if ((Object)(object)component2 != (Object)null)
		{
			((Renderer)component2).sharedMaterial = val;
		}
		SonderPulse sonderPulse = obj.AddComponent<SonderPulse>();
		sonderPulse.StartScale = Vector3.one * 0.3f;
		sonderPulse.EndScale = Vector3.one * 5f;
		sonderPulse.Duration = 0.45f;
		sonderPulse.Material = val;
		List<VRRig> list = ExtraUtil.RemoteRigs();
		for (int i = 0; i < list.Count; i++)
		{
			VRRig val2 = list[i];
			if ((Object)(object)val2 == (Object)null)
			{
				continue;
			}
			Vector3 val3 = ((Component)val2).transform.position - point;
			if (!(val3.magnitude > 5.5f))
			{
				if (val3.sqrMagnitude < 0.001f)
				{
					val3 = Vector3.up;
				}
				AddPush(val2, (val3.normalized + Vector3.up * 0.4f) * 20f, 0.6f);
			}
		}
		RuntimeLog.Write("bomb blast");
	}

	private void Pulse(Vector3 point, Color color)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		GameObject obj = GameObject.CreatePrimitive((PrimitiveType)0);
		((Object)obj).name = "SonderImpact";
		Collider component = obj.GetComponent<Collider>();
		if ((Object)(object)component != (Object)null)
		{
			Object.Destroy((Object)(object)component);
		}
		obj.transform.position = point;
		obj.transform.localScale = Vector3.one * 0.12f;
		Material val = ExtraUtil.Unlit(new Color(color.r, color.g, color.b, 0.65f), transparent: true);
		MeshRenderer component2 = obj.GetComponent<MeshRenderer>();
		if ((Object)(object)component2 != (Object)null)
		{
			((Renderer)component2).sharedMaterial = val;
		}
		SonderPulse sonderPulse = obj.AddComponent<SonderPulse>();
		sonderPulse.StartScale = Vector3.one * 0.12f;
		sonderPulse.EndScale = Vector3.one * 0.55f;
		sonderPulse.Duration = 0.3f;
		sonderPulse.Material = val;
	}

	private void UpdateEffects()
	{
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0316: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		//IL_0247: Unknown result type (might be due to invalid IL or missing references)
		//IL_0249: Unknown result type (might be due to invalid IL or missing references)
		//IL_024b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Unknown result type (might be due to invalid IL or missing references)
		//IL_0266: Unknown result type (might be due to invalid IL or missing references)
		//IL_026b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0270: Unknown result type (might be due to invalid IL or missing references)
		//IL_0274: Unknown result type (might be due to invalid IL or missing references)
		float time = Time.time;
		for (int num = _lagged.Count - 1; num >= 0; num--)
		{
			LagState lagState = _lagged[num];
			if ((Object)(object)lagState.Rig == (Object)null)
			{
				_lagged.RemoveAt(num);
			}
			else if (time >= lagState.Until)
			{
				((Behaviour)lagState.Rig).enabled = true;
				_lagged.RemoveAt(num);
			}
			else if (time >= lagState.NextToggle)
			{
				lagState.NextToggle = time + 0.18f;
				((Behaviour)lagState.Rig).enabled = !((Behaviour)lagState.Rig).enabled;
			}
		}
		for (int num2 = _bubbles.Count - 1; num2 >= 0; num2--)
		{
			BubbleState bubbleState = _bubbles[num2];
			if ((Object)(object)bubbleState.Rig == (Object)null || (Object)(object)bubbleState.Bubble == (Object)null || time >= bubbleState.Until)
			{
				if ((Object)(object)bubbleState.Bubble != (Object)null)
				{
					Object.Destroy((Object)(object)bubbleState.Bubble);
				}
				_bubbles.RemoveAt(num2);
			}
			else
			{
				bubbleState.Rig.ApplyLocalTrajectoryOverride(Vector3.zero);
			}
		}
		for (int num3 = _bananas.Count - 1; num3 >= 0; num3--)
		{
			BananaState bananaState = _bananas[num3];
			if ((Object)(object)bananaState.Banana == (Object)null || time >= bananaState.Until)
			{
				if ((Object)(object)bananaState.Banana != (Object)null)
				{
					Object.Destroy((Object)(object)bananaState.Banana);
				}
				_bananas.RemoveAt(num3);
			}
			else
			{
				GTPlayer player = ExtraUtil.Player;
				if (!((Object)(object)player == (Object)null) && player.BodyOnGround && !(time < bananaState.NextSlip))
				{
					Vector3 val = ((Component)player).transform.position - bananaState.Position;
					val.y = 0f;
					if (!(val.magnitude > 0.95f))
					{
						bananaState.NextSlip = time + 1.6f;
						Vector3 val2 = Vector3.Cross(Vector3.up, (val.sqrMagnitude > 0.001f) ? val.normalized : Vector3.forward);
						Rigidbody playerRigidBody = player.playerRigidBody;
						if ((Object)(object)playerRigidBody != (Object)null)
						{
							Vector3 linearVelocity = playerRigidBody.linearVelocity;
							linearVelocity += val2 * 5f + Vector3.up * 1.5f;
							playerRigidBody.linearVelocity = linearVelocity;
						}
					}
				}
			}
		}
		List<int> list = null;
		foreach (KeyValuePair<int, PaintState> item in _painted)
		{
			if (!(time < item.Value.Until))
			{
				if (list == null)
				{
					list = new List<int>();
				}
				list.Add(item.Key);
			}
		}
		if (list != null)
		{
			for (int i = 0; i < list.Count; i++)
			{
				PaintState paintState = _painted[list[i]];
				if ((Object)(object)paintState.Material != (Object)null)
				{
					SetMaterialColor(paintState.Material, paintState.Original);
				}
				_painted.Remove(list[i]);
			}
		}
		ApplyPushes();
	}

	private void AddPush(VRRig rig, Vector3 velocity, float duration)
	{
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < _pushes.Count; i++)
		{
			if ((Object)(object)_pushes[i].Rig == (Object)(object)rig)
			{
				_pushes[i].Velocity = velocity;
				_pushes[i].Until = Time.time + duration;
				return;
			}
		}
		_pushes.Add(new Push
		{
			Rig = rig,
			Velocity = velocity,
			Until = Time.time + duration
		});
	}

	private void ApplyPushes()
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		for (int num = _pushes.Count - 1; num >= 0; num--)
		{
			Push push = _pushes[num];
			if ((Object)(object)push.Rig == (Object)null)
			{
				_pushes.RemoveAt(num);
			}
			else if (Time.time >= push.Until)
			{
				push.Rig.ApplyLocalTrajectoryOverride(Vector3.zero);
				_pushes.RemoveAt(num);
			}
			else
			{
				push.Rig.ApplyLocalTrajectoryOverride(push.Velocity);
			}
		}
	}

	private void ClearPushes()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < _pushes.Count; i++)
		{
			if ((Object)(object)_pushes[i].Rig != (Object)null)
			{
				_pushes[i].Rig.ApplyLocalTrajectoryOverride(Vector3.zero);
			}
		}
		_pushes.Clear();
	}

	private void UpdateLaser(bool active)
	{
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		if (!active)
		{
			if ((Object)(object)_laser != (Object)null)
			{
				((Component)_laser).gameObject.SetActive(false);
			}
			if ((Object)(object)_laserDot != (Object)null)
			{
				_laserDot.SetActive(false);
			}
			return;
		}
		Transform val = ExtraUtil.Hand(left: false);
		if ((Object)(object)val == (Object)null)
		{
			return;
		}
		EnsureLaser();
		if ((Object)(object)_laser == (Object)null)
		{
			return;
		}
		((Component)_laser).gameObject.SetActive(true);
		Vector3 val2 = val.position + val.forward * 0.12f;
		Vector3 forward = val.forward;
		Vector3 val3 = val2 + forward * 40f;
		bool flag = false;
		RaycastHit[] array = Physics.RaycastAll(val2, forward, 40f, -1, (QueryTriggerInteraction)1);
		float num = float.MaxValue;
		Vector3 val4 = Vector3.zero;
		Collider collider = null;
		for (int i = 0; i < array.Length; i++)
		{
			if (!((Object)(object)array[i].collider == (Object)null) && !ExtraUtil.IsLocalCollider(array[i].collider) && !(array[i].distance >= num))
			{
				num = array[i].distance;
				val4 = array[i].point;
				collider = array[i].collider;
				flag = true;
			}
		}
		if (flag)
		{
			val3 = val4;
		}
		_laser.SetPosition(0, val2);
		_laser.SetPosition(1, val3);
		if (!((Object)(object)_laserDot != (Object)null))
		{
			return;
		}
		_laserDot.SetActive(true);
		_laserDot.transform.position = val3;
		if (flag)
		{
			if ((Object)(object)FindRig(collider) != (Object)null)
			{
				_laserDot.transform.localScale = Vector3.one * 0.09f;
			}
			else
			{
				_laserDot.transform.localScale = Vector3.one * 0.06f;
			}
		}
	}

	private void EnsureLaser()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_laser == (Object)null)
		{
			GameObject val = new GameObject("SonderLaser");
			val.transform.SetParent(((Component)this).transform, false);
			_laser = val.AddComponent<LineRenderer>();
			_laser.positionCount = 2;
			_laser.startWidth = 0.012f;
			_laser.endWidth = 0.004f;
			_laser.useWorldSpace = true;
			((Renderer)_laser).shadowCastingMode = (ShadowCastingMode)0;
			((Renderer)_laser).receiveShadows = false;
			_laserMaterial = ExtraUtil.Unlit(new Color(1f, 0.15f, 0.15f, 0.9f), transparent: true);
			((Renderer)_laser).sharedMaterial = _laserMaterial;
		}
		if ((Object)(object)_laserDot == (Object)null)
		{
			_laserDot = GameObject.CreatePrimitive((PrimitiveType)0);
			((Object)_laserDot).name = "SonderLaserDot";
			Collider component = _laserDot.GetComponent<Collider>();
			if ((Object)(object)component != (Object)null)
			{
				Object.Destroy((Object)(object)component);
			}
			_laserDot.transform.SetParent(((Component)this).transform, false);
			_laserDot.transform.localScale = Vector3.one * 0.06f;
			_dotMaterial = ExtraUtil.Unlit(new Color(1f, 0.15f, 0.15f), transparent: false);
			MeshRenderer component2 = _laserDot.GetComponent<MeshRenderer>();
			if ((Object)(object)component2 != (Object)null)
			{
				((Renderer)component2).sharedMaterial = _dotMaterial;
			}
		}
	}

	private void DestroyLaser()
	{
		if ((Object)(object)_laser != (Object)null)
		{
			Object.Destroy((Object)(object)((Component)_laser).gameObject);
		}
		if ((Object)(object)_laserDot != (Object)null)
		{
			Object.Destroy((Object)(object)_laserDot);
		}
		if ((Object)(object)_laserMaterial != (Object)null)
		{
			Object.Destroy((Object)(object)_laserMaterial);
		}
		if ((Object)(object)_dotMaterial != (Object)null)
		{
			Object.Destroy((Object)(object)_dotMaterial);
		}
		_laser = null;
		_laserDot = null;
	}

	private void RestorePaint()
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		foreach (KeyValuePair<int, PaintState> item in _painted)
		{
			if ((Object)(object)item.Value.Material != (Object)null)
			{
				SetMaterialColor(item.Value.Material, item.Value.Original);
			}
		}
		_painted.Clear();
	}

	private void RestoreLag()
	{
		for (int i = 0; i < _lagged.Count; i++)
		{
			if ((Object)(object)_lagged[i].Rig != (Object)null)
			{
				((Behaviour)_lagged[i].Rig).enabled = true;
			}
		}
		_lagged.Clear();
	}

	private void ClearBubbles()
	{
		for (int i = 0; i < _bubbles.Count; i++)
		{
			if ((Object)(object)_bubbles[i].Bubble != (Object)null)
			{
				Object.Destroy((Object)(object)_bubbles[i].Bubble);
			}
		}
		_bubbles.Clear();
	}

	private void ClearBananas()
	{
		for (int i = 0; i < _bananas.Count; i++)
		{
			if ((Object)(object)_bananas[i].Banana != (Object)null)
			{
				Object.Destroy((Object)(object)_bananas[i].Banana);
			}
		}
		_bananas.Clear();
	}
}
