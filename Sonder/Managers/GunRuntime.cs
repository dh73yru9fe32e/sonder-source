using System;
using System.Collections.Generic;
using Sonder.Core;
using Taurus;
using UnityEngine;
using Object = UnityEngine.Object;
using UnityEngine.InputSystem;
using UnityEngine.XR;

namespace Sonder.Managers;

public class GunRuntime : MonoBehaviour
{
	public struct GunData
	{
		public GameObject NewPointer;

		public RaycastHit Ray;

		public VRRig Target;

		public bool HasTarget;
	}

	public bool gunLocked;

	public VRRig lockTarget;

	public int lastLockActor;

	public bool GunLockOn = true;

	public bool GunSmoothPointer = true;

	public bool GunSmallPointer;

	public bool GunHidePointer;

	public bool GunHideLine;

	public bool GunSwapHand;

	public bool GunGripless;

	public bool GunTriggerless;

	public bool GunHardLock;

	public bool GunSounds = true;

	public bool GunVibrations = true;

	public bool GunParticles;

	private const float BodyRadius = 0.35f;

	private GunLib _gun;

	private Material _lineMaterial;

	private Material _ballMaterial;

	private bool _mouseAim;

	private readonly List<GunLib.BodyTarget> _targets = new List<GunLib.BodyTarget>();

	private readonly Dictionary<Transform, VRRig> _targetRigs = new Dictionary<Transform, VRRig>();

	public static GunRuntime Instance { get; private set; }

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
		if (_gun != null)
		{
			_gun.Dispose();
		}
	}

	private void Update()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_032b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0330: Unknown result type (might be due to invalid IL or missing references)
		//IL_0338: Unknown result type (might be due to invalid IL or missing references)
		//IL_033a: Unknown result type (might be due to invalid IL or missing references)
		//IL_033c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0346: Unknown result type (might be due to invalid IL or missing references)
		//IL_0294: Unknown result type (might be due to invalid IL or missing references)
		//IL_0299: Unknown result type (might be due to invalid IL or missing references)
		//IL_029d: Unknown result type (might be due to invalid IL or missing references)
		//IL_029f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ba: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)GorillaTagger.Instance == (Object)null || (Object)(object)VRRig.LocalRig == (Object)null)
		{
			return;
		}
		if (_gun == null)
		{
			if ((Object)(object)_lineMaterial == (Object)null)
			{
				_lineMaterial = ExtraUtil.Unlit(Color.white, transparent: true);
			}
			if ((Object)(object)_ballMaterial == (Object)null)
			{
				_ballMaterial = ExtraUtil.Unlit(Color.white, transparent: true);
			}
			_gun = new GunLib(_lineMaterial, _ballMaterial);
			_gun.Triggered += OnTaurusLock;
		}
		bool num = AnyLockGunEnabled();
		bool docked = MenuManager.Docked;
		bool flag = !XRSettings.isDeviceActive;
		bool wasdFly = ModRuntime.Instance != null && ModRuntime.Instance.WasdFly;
		bool flag2 = docked | flag | MenuManager.ScreenMode | wasdFly;
		bool flag3 = GunGripless || GripHeld();
		bool aiming = num & flag3;
		bool trigger = GunLockOn && (GunTriggerless || GetGunInput(trigger: true));
		if (GunHardLock && gunLocked)
		{
			trigger = true;
		}
		_targets.Clear();
		_targetRigs.Clear();
		IReadOnlyList<VRRig> activeRigs = VRRigCache.ActiveRigs;
		if (activeRigs != null)
		{
			for (int i = 0; i < activeRigs.Count; i++)
			{
				VRRig val = activeRigs[i];
				if (!((Object)(object)val == (Object)null) && !val.isLocal && !val.isOfflineVRRig)
				{
					Transform val2 = (((Object)(object)val.headMesh != (Object)null) ? val.headMesh.transform : ((Component)val).transform);
					_targets.Add(new GunLib.BodyTarget(val2, 0.35f, ((Component)val).transform));
					_targetRigs[val2] = val;
				}
			}
		}
		Transform transform = ((Component)GorillaTagger.Instance).transform;
		FingerAim.Sample(GunSwapHand);
		_gun.BeamStartWidth = SonderSettings.GunBeamWidth;
		_gun.BeamEndWidth = SonderSettings.GunBeamWidth * 0.5f;
		float num2 = SonderSettings.GunBallSize;
		if (GunSmallPointer)
		{
			num2 *= 0.6f;
		}
		_gun.BallScale = num2;
		_gun.LineVisible = !GunHideLine;
		_gun.BallVisible = !GunHidePointer;
		Camera val3 = (flag ? MenuManager.EyeCamera : MenuManager.ActiveCamera);
		_mouseAim = flag2 && (Object)(object)val3 != (Object)null && (Mouse.current != null || wasdFly);
		if (_mouseAim)
		{
			// While WASD fly has the mouse locked, aim with the crosshair in the middle of the screen.
			Vector2 val4 = (ModRuntime.WasdLooking ? new Vector2((float)Screen.width * 0.5f, (float)Screen.height * 0.5f) : ((InputControl<Vector2>)(object)((Pointer)Mouse.current).position).ReadValue());
			Ray val5 = val3.ScreenPointToRay((val4));
			Vector3 origin = val5.origin + Vector3.down * 0.35f;
			_gun.Update(origin, val5.direction, aiming, trigger, _targets, ((Component)VRRig.LocalRig).transform, null, transform);
		}
		else if (TryGetAimRay(out var origin, out var direction))
		{
			_gun.Update(origin, direction, aiming, trigger, _targets, ((Component)VRRig.LocalRig).transform, origin, transform);
		}
		else
		{
			_gun.Hide();
		}
		Color gunColor = SonderSettings.GunColor;
		_gun.SetColors(gunColor, Color.Lerp(gunColor, Color.white, 0.4f));
		if ((Object)(object)_gun.LockedBody != (Object)null && _targetRigs.TryGetValue(_gun.LockedBody, out var value))
		{
			gunLocked = true;
			lockTarget = value;
		}
		else
		{
			gunLocked = false;
			lockTarget = null;
		}
	}

	// The rig poses the hand after Update, so re-attach the beam start once the hand is where you see it.
	private void LateUpdate()
	{
		if (_gun != null && !_mouseAim && _gun.IsVisible && TryGetAimRay(out var origin, out var _))
		{
			_gun.SetVisualOrigin(origin);
		}
	}

	public static bool TryGetAimRay(out Vector3 origin, out Vector3 direction)
	{
		GunRuntime instance = Instance;
		GorillaTagger tagger = GorillaTagger.Instance;
		bool left = (Object)(object)instance != (Object)null && instance.GunSwapHand && (Object)(object)tagger != (Object)null && (Object)(object)tagger.leftHandTransform != (Object)null;
		return FingerAim.TryGetRay(left, out origin, out direction);
	}

	private void OnTaurusLock(Transform body)
	{
		if (GunVibrations && (Object)(object)GorillaTagger.Instance != (Object)null)
		{
			GorillaTagger.Instance.StartVibration(true, 0.7f, 0.25f);
		}
		if (_targetRigs.TryGetValue(body, out var value))
		{
			lastLockActor = ((value.Creator != null) ? value.Creator.ActorNumber : 0);
			if (GunSounds)
			{
				NotificationRuntime.Show("Locked: " + ExtraUtil.Nickname(value));
			}
			RuntimeLog.Write("gun lock: " + ExtraUtil.Nickname(value));
		}
	}

	public static bool AnyGunEnabled => AnyLockGunEnabled();

	private static bool AnyLockGunEnabled()
	{
		OverpoweredRuntime instance = OverpoweredRuntime.Instance;
		if ((Object)(object)instance != (Object)null && instance.ForceGrabGunEnabled)
		{
			return true;
		}
		RigGunRuntime instance2 = RigGunRuntime.Instance;
		if ((Object)(object)instance2 != (Object)null && (instance2.RigSpazGun || instance2.ConfusePlayerGun || instance2.ChasePlayerGun || instance2.OrbitPlayerGun))
		{
			return true;
		}
		try
		{
			foreach (string item in ModManager.ActiveTitles())
			{
				if (item != null && item.IndexOf("gun", StringComparison.OrdinalIgnoreCase) >= 0)
				{
					return true;
				}
			}
		}
		catch
		{
		}
		return false;
	}

	private static bool GripHeld()
	{
		ControllerInputPoller instance = ControllerInputPoller.instance;
		if ((Object)(object)instance != (Object)null && instance.rightControllerGripFloat > 0.5f)
		{
			return true;
		}
		if (Mouse.current != null && !MenuManager.PointerOverMenu)
		{
			return Mouse.current.rightButton.isPressed;
		}
		return false;
	}

	public static bool GetGunInput(bool trigger)
	{
		// Clicking a menu button should not also fire the gun.
		if (Mouse.current != null && !MenuManager.PointerOverMenu && (trigger ? Mouse.current.leftButton.isPressed : Mouse.current.rightButton.isPressed))
		{
			return true;
		}
		ControllerInputPoller instance = ControllerInputPoller.instance;
		if ((Object)(object)instance == (Object)null)
		{
			return false;
		}
		if (!trigger)
		{
			return instance.rightControllerPrimaryButton;
		}
		return instance.rightControllerIndexFloat > 0.5f;
	}

	public static GunData RenderGun()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		GunRuntime instance = Instance;
		GunData result = default(GunData);
		if ((Object)(object)instance == (Object)null || instance._gun == null)
		{
			return result;
		}
		result.NewPointer = ((!instance._gun.IsVisible) ? null : ((instance._gun.VisualOrigin != Vector3.zero) ? null : null));
		RaycastHit ray = default(RaycastHit);
		ray.point = instance._gun.TargetPoint;
		ray.normal = instance._gun.TargetNormal;
		Transform val = (((Object)(object)GorillaTagger.Instance != (Object)null) ? GorillaTagger.Instance.rightHandTransform : null);
		if ((Object)(object)val != (Object)null)
		{
			ray.distance = Vector3.Distance(val.position, instance._gun.TargetPoint);
		}
		result.Ray = ray;
		result.Target = instance.lockTarget;
		result.HasTarget = instance.gunLocked && (Object)(object)instance.lockTarget != (Object)null;
		return result;
	}

	public static void Unlock()
	{
		GunRuntime instance = Instance;
		if ((Object)(object)instance != (Object)null)
		{
			instance.gunLocked = false;
			instance.lockTarget = null;
			if (instance._gun != null)
			{
				instance._gun.Hide();
			}
		}
	}

	public static void SetPointerVisible(bool visible)
	{
		GunRuntime instance = Instance;
		if (!((Object)(object)instance == (Object)null) && instance._gun != null && !visible)
		{
			instance._gun.Hide();
		}
	}
}
