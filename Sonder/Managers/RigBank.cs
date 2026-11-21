using System;
using System.Collections.Generic;
using System.Reflection;
using GorillaLocomotion;
using HarmonyLib;
using Sonder.Core;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Rendering;
using UnityEngine.XR;

namespace Sonder.Managers;

public static class RigBank
{
	public static bool rigGun;

	public static bool confuseGun;

	public static bool chaseGun;

	public static bool fastChaseGun;

	public static bool orbitGun;

	public static bool grabRig;

	public static bool ghostMonke;

	public static bool invisMonke;

	public static bool spazMonke;

	public static bool wackyMonke;

	public static bool fakeLag;

	public static bool fastFakeLag;

	public static bool fakeGrey;

	public static bool fakeFBT;

	public static bool backwardsHead;

	public static bool backwardsBody;

	public static bool backwardsRig;

	public static bool upsideDownHead;

	public static bool upsideDownBody;

	public static bool upsideDownRig;

	public static bool forwardsBody;

	public static bool forwardsRig;

	public static bool crawlingBody;

	public static bool crawlingRig;

	public static bool spazHead;

	public static bool spazBody;

	public static bool spazRig;

	public static bool upsideDownSpinBot;

	public static bool fullSpinBot;

	public static int headSpinTier;

	public static int spinBotTier;

	public static bool backflipOn;

	public static bool frontflipOn;

	private static readonly System.Random Rng = new System.Random();

	private static VRRig _locked;

	private const float AimRadius = 0.35f;

	private const float RayLength = 512f;

	private static bool _bodyDetached;

	private static Vector3 _bodyHome;

	private static Vector3 _orbitAnchor;

	private static float _lagNextFlip;

	private static bool _lagFrozen;

	private static bool _xPrev;

	private static bool _aPrev;

	private static bool _ghostActive;

	private static bool _invisActive;

	private static bool _rigHeld;

	private static bool _bHeld;

	private static float _flipTime;

	private static float _flipDir;

	private static bool _flipActive;

	private static Harmony _hook;

	private static bool _hookTried;

	private static bool _hookOn;

	private static float _headSpinAngle;

	private static float _spinAngle;

	private static Vector3 _tumble;

	private static bool _rightGripPrev;

	private static bool _leftGripPrev;

	private static bool _hasBodyOverride;

	private static Vector3 _bodyOverride;

	public static string HeadSpinName => (new string[4] { "Off", "V1", "V2", "V3" })[headSpinTier];

	public static string SpinBotName => (new string[6] { "Off", "V1", "V2", "V3", "V4", "V5" })[spinBotTier];

	public static VRRig LockedRig => _locked;

	// The game re-poses the local rig in VRRig.PostTick, which wipes anything set earlier in
	// the frame. Self rig mods run in a postfix so they land after the game's own pose.
	public static void EnsurePostTickHook()
	{
		if (_hookTried)
		{
			return;
		}
		_hookTried = true;
		try
		{
			MethodInfo methodInfo = AccessTools.Method(typeof(VRRig), "PostTick", (Type[])null, (Type[])null);
			if (methodInfo != null)
			{
				_hook = new Harmony("com.sonder.rigbank");
				_hook.Patch((MethodBase)methodInfo, (HarmonyMethod)null, new HarmonyMethod(typeof(RigBank), "PostTickPostfix", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
				_hookOn = true;
				RuntimeLog.Write("rigbank: PostTick hook on");
			}
		}
		catch (Exception ex)
		{
			RuntimeLog.Write("rigbank: PostTick hook failed: " + ex.Message);
		}
	}

	private static void PostTickPostfix(VRRig __instance)
	{
		try
		{
			GorillaTagger instance = GorillaTagger.Instance;
			if (!((Object)(object)instance == (Object)null) && !((Object)(object)__instance != (Object)(object)instance.offlineVRRig))
			{
				ApplySelf(instance, __instance);
			}
		}
		catch
		{
		}
	}

	public static void CycleHeadSpin()
	{
		headSpinTier = (headSpinTier + 1) % 4;
	}

	public static void CycleSpinBot()
	{
		spinBotTier = (spinBotTier + 1) % 6;
	}

	public static bool AnyGunActive()
	{
		if (!rigGun && !confuseGun && !chaseGun && !fastChaseGun && !orbitGun)
		{
			return grabRig;
		}
		return true;
	}

	public static void ClearLock()
	{
		_locked = null;
	}

	public static void Tick(GorillaTagger tagger, ControllerInputPoller input)
	{
		EnsurePostTickHook();
		if ((Object)(object)tagger == (Object)null)
		{
			return;
		}
		VRRig offlineVRRig = tagger.offlineVRRig;
		_hasBodyOverride = false;
		HandleKeybinds(tagger, input);
		HandleFlipInput(input);
		HandleGhostInvis(tagger);
		HandleFakeLag(tagger);
		if (AnyGunActive())
		{
			bool flag;
			bool flag2;
			if (XRSettings.isDeviceActive && (Object)(object)input != (Object)null)
			{
				flag = input.rightControllerGripFloat > 0.5f;
				flag2 = input.rightControllerTriggerButton;
			}
			else
			{
				flag = Mouse.current != null && Mouse.current.rightButton.isPressed;
				flag2 = Mouse.current != null && Mouse.current.leftButton.isPressed;
			}
			VRRig val = AimTarget(tagger);
			if (flag2 && (Object)(object)val != (Object)null)
			{
				_locked = val;
			}
			if (!flag && !flag2)
			{
				_locked = null;
			}
			if ((Object)(object)_locked != (Object)null && (flag | flag2))
			{
				Act(tagger, _locked, flag2, flag);
			}
		}
		else
		{
			_locked = null;
			RestoreBody(tagger);
		}
		if (!AnyGunActive())
		{
			RestoreBody(tagger);
		}
		if (!_hookOn)
		{
			ApplySelf(tagger, offlineVRRig);
		}
	}

	private static void HandleFlipInput(ControllerInputPoller input)
	{
		bool flag = (Object)(object)input != (Object)null && input.rightControllerGripFloat > 0.5f;
		bool flag2 = (Object)(object)input != (Object)null && input.leftControllerGripFloat > 0.5f;
		if (backflipOn && flag && !_rightGripPrev)
		{
			FireFlip(back: false);
		}
		if (frontflipOn && flag2 && !_leftGripPrev)
		{
			FireFlip(back: true);
		}
		_rightGripPrev = flag;
		_leftGripPrev = flag2;
	}

	private static VRRig AimTarget(GorillaTagger tagger)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		if (!GunRuntime.TryGetAimRay(out var position, out var forward))
		{
			return null;
		}
		int num = (((Object)(object)GTPlayer.Instance != (Object)null) ? (GTPlayer.Instance.locomotionEnabledLayers) : (-5));
		IReadOnlyList<VRRig> activeRigs = VRRigCache.ActiveRigs;
		if (activeRigs == null)
		{
			return null;
		}
		int num2 = 0;
		for (int i = 0; i < activeRigs.Count; i++)
		{
			VRRig val = activeRigs[i];
			if (!((Object)(object)val == (Object)null) && !val.isLocal && !val.isOfflineVRRig)
			{
				int num3 = 1 << ((Component)val).gameObject.layer;
				if ((num & num3) == 0)
				{
					num2 |= num3;
				}
			}
		}
		RaycastHit val2 = default(RaycastHit);
		if (Physics.Raycast(position, forward, out val2, 512f, num | num2))
		{
			VRRig val3 = (((Object)(object)val2.collider != (Object)null) ? ((Component)val2.collider).GetComponentInParent<VRRig>() : null);
			if ((Object)(object)val3 != (Object)null && !val3.isLocal && !val3.isOfflineVRRig)
			{
				return val3;
			}
		}
		float num4 = float.MaxValue;
		VRRig result = null;
		for (int j = 0; j < activeRigs.Count; j++)
		{
			VRRig val4 = activeRigs[j];
			if ((Object)(object)val4 == (Object)null || val4.isLocal || val4.isOfflineVRRig)
			{
				continue;
			}
			Vector3 val5 = (((Object)(object)val4.headMesh != (Object)null) ? val4.headMesh.transform.position : ((Component)val4).transform.position) - position;
			float num5 = Vector3.Dot(val5, forward);
			if (!(num5 <= 0.5f) && !(num5 > 512f))
			{
				Vector3 val6 = val5 - forward * num5;
				if (!(val6.sqrMagnitude > 0.122499995f) && (!((Object)(object)val2.collider != (Object)null) || !(val2.distance < num5 - 0.05f)) && num5 < num4)
				{
					num4 = num5;
					result = val4;
				}
			}
		}
		return result;
	}

	private static void DetachBody(GorillaTagger tagger)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		if (!_bodyDetached && !((Object)(object)tagger == (Object)null) && !((Object)(object)tagger.offlineVRRig == (Object)null))
		{
			_bodyHome = ((Component)tagger.offlineVRRig).transform.position;
			_bodyDetached = true;
		}
	}

	private static void RestoreBody(GorillaTagger tagger)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		if (_bodyDetached && !((Object)(object)tagger == (Object)null) && !((Object)(object)tagger.offlineVRRig == (Object)null))
		{
			((Component)tagger.offlineVRRig).transform.position = _bodyHome;
			((Component)tagger.offlineVRRig).transform.rotation = Quaternion.identity;
			_bodyDetached = false;
		}
	}

	private static void Act(GorillaTagger tagger, VRRig locked, bool triggerHeld, bool gripHeld)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		VRRig offlineVRRig = tagger.offlineVRRig;
		if (confuseGun && (Object)(object)locked != (Object)null)
		{
			Transform transform = ((Component)locked).transform;
			transform.position += new Vector3((float)(Rng.NextDouble() - 0.5), (float)(Rng.NextDouble() - 0.5) * 0.6f, (float)(Rng.NextDouble() - 0.5)) * 1.2f;
		}
		bool flag = false;
		if (rigGun & triggerHeld)
		{
			flag = true;
		}
		if ((chaseGun || fastChaseGun || orbitGun) && (Object)(object)locked != (Object)null)
		{
			flag = true;
		}
		if (grabRig & gripHeld)
		{
			flag = true;
		}
		if (!flag)
		{
			return;
		}
		DetachBody(tagger);
		Transform transform2 = ((Component)offlineVRRig).transform;
		if (rigGun & triggerHeld)
		{
			// Fly to where the gun beam ends; with nothing hit, a short way along the aim.
			Vector3 val = GunRuntime.RenderGun().Ray.point;
			if (val == Vector3.zero && GunRuntime.TryGetAimRay(out var aimOrigin, out var aimDirection))
			{
				val = aimOrigin + aimDirection * 2.5f;
			}
			transform2.position = Vector3.Lerp(transform2.position, val, 0.35f);
			_bodyOverride = transform2.position;
			_hasBodyOverride = true;
		}
		else if (chaseGun && (Object)(object)locked != (Object)null)
		{
			Vector3 val2 = ((Component)locked).transform.position - transform2.position;
			if (val2.sqrMagnitude > 0.25f)
			{
				transform2.position += val2.normalized * (7f * Time.deltaTime);
			}
		}
		else if (fastChaseGun && (Object)(object)locked != (Object)null)
		{
			Vector3 val3 = ((Component)locked).transform.position - transform2.position;
			if (val3.sqrMagnitude > 0.25f)
			{
				transform2.position += val3.normalized * (16f * Time.deltaTime);
			}
		}
		else if (orbitGun && (Object)(object)locked != (Object)null)
		{
			float num = Time.time * 3.4f;
			transform2.position = ((Component)locked).transform.position + new Vector3(Mathf.Sin(num) * 1.6f, 1.2f, Mathf.Cos(num) * 1.6f);
		}
		else if (grabRig & gripHeld)
		{
			transform2.position = tagger.rightHandTransform.position;
			_bodyOverride = transform2.position;
			_hasBodyOverride = true;
		}
	}

	private static void ApplySelf(GorillaTagger tagger, VRRig local)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0340: Unknown result type (might be due to invalid IL or missing references)
		//IL_0384: Unknown result type (might be due to invalid IL or missing references)
		//IL_038e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0393: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0403: Unknown result type (might be due to invalid IL or missing references)
		//IL_0408: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0566: Unknown result type (might be due to invalid IL or missing references)
		//IL_0499: Unknown result type (might be due to invalid IL or missing references)
		//IL_041c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_059d: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_05dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_05fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0608: Unknown result type (might be due to invalid IL or missing references)
		//IL_060d: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Unknown result type (might be due to invalid IL or missing references)
		//IL_0265: Unknown result type (might be due to invalid IL or missing references)
		//IL_026f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0291: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02af: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02da: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0303: Unknown result type (might be due to invalid IL or missing references)
		//IL_0317: Unknown result type (might be due to invalid IL or missing references)
		//IL_0321: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)local == (Object)null)
		{
			return;
		}
		Transform headConstraint = local.headConstraint;
		Transform rigTransform = ((Component)local).transform;
		// With the PostTick hook the pose is fresh every tick, so spins apply their running
		// angle; without it the transform keeps last frame's rotation, so apply only the step.
		float dt = Time.deltaTime;
		Quaternion yaw = Quaternion.Euler(0f, rigTransform.eulerAngles.y, 0f);
		if (_hasBodyOverride)
		{
			rigTransform.position = _bodyOverride;
		}
		if ((Object)(object)headConstraint != (Object)null)
		{
			if (headSpinTier > 0)
			{
				float num0 = (new float[4] { 0f, 180f, 420f, 900f })[headSpinTier] * dt;
				_headSpinAngle = Mathf.Repeat(_headSpinAngle + num0, 360f);
				headConstraint.Rotate(Vector3.up, _hookOn ? _headSpinAngle : num0, (Space)1);
			}
			if (spazHead || (spazMonke && _bHeld))
			{
				headConstraint.localRotation = UnityEngine.Random.rotation;
			}
			if (backwardsHead && headSpinTier == 0 && !spazHead)
			{
				headConstraint.localRotation = Quaternion.Euler(0f, 180f, 0f);
			}
			if (upsideDownHead && headSpinTier == 0 && !spazHead && !backwardsHead)
			{
				headConstraint.localRotation = Quaternion.Euler(180f, 0f, 0f);
			}
		}
		if (!_bodyDetached)
		{
			if (spazRig || (spazMonke && _bHeld))
			{
				rigTransform.rotation = UnityEngine.Random.rotation;
				if (spazMonke && _bHeld && _hookOn)
				{
					// The pose is fresh each tick with the hook, so this jitters around you instead of drifting off.
					rigTransform.position += UnityEngine.Random.insideUnitSphere * 0.12f;
				}
			}
			else if (fullSpinBot)
			{
				_tumble += new Vector3(240f, 320f, 180f) * dt;
				_tumble = new Vector3(Mathf.Repeat(_tumble.x, 360f), Mathf.Repeat(_tumble.y, 360f), Mathf.Repeat(_tumble.z, 360f));
				rigTransform.rotation = yaw * Quaternion.Euler(_tumble);
			}
			else if (upsideDownSpinBot)
			{
				rigTransform.rotation = Quaternion.Euler(0f, Time.time * 400f, 0f) * Quaternion.Euler(0f, 0f, 180f);
			}
			else if (spinBotTier > 0)
			{
				float num = (new float[6] { 0f, 120f, 300f, 560f, 820f, 1200f })[spinBotTier];
				_spinAngle = Mathf.Repeat(_spinAngle + num * dt, 360f);
				rigTransform.rotation = Quaternion.Euler(0f, _spinAngle, 0f);
			}
			else if (backwardsRig)
			{
				rigTransform.rotation = yaw * Quaternion.Euler(0f, 180f, 0f);
			}
			else if (upsideDownRig)
			{
				rigTransform.rotation = yaw * Quaternion.Euler(0f, 0f, 180f);
			}
			else if (crawlingRig)
			{
				rigTransform.rotation = yaw * Quaternion.Euler(70f, 0f, 0f);
			}
			else if (backwardsBody)
			{
				rigTransform.rotation = Quaternion.Slerp(rigTransform.rotation, yaw * Quaternion.Euler(0f, 180f, 0f), 0.5f);
			}
			else if (upsideDownBody)
			{
				rigTransform.rotation = Quaternion.Slerp(rigTransform.rotation, yaw * Quaternion.Euler(0f, 0f, 180f), 0.5f);
			}
			else if (forwardsBody || forwardsRig)
			{
				rigTransform.rotation = forwardsRig ? yaw : Quaternion.Slerp(rigTransform.rotation, yaw, 0.5f);
			}
			else if (crawlingBody)
			{
				rigTransform.rotation = Quaternion.Slerp(rigTransform.rotation, yaw * Quaternion.Euler(70f, 0f, 0f), 0.5f);
			}
			else
			{
				rigTransform.rotation = yaw;
			}
		}
		if (spazBody && !_bodyDetached)
		{
			rigTransform.position += new Vector3((float)(Rng.NextDouble() - 0.5), (float)(Rng.NextDouble() - 0.5), (float)(Rng.NextDouble() - 0.5)) * 0.35f;
		}
		if (wackyMonke && _bHeld && !_bodyDetached)
		{
			// Side-to-side wiggle with a hop and a head bob. Rotations are absolute so the speed doesn't
			// depend on frame rate; the offset is only added when the pose is fresh each tick, or it would drift.
			float t = Time.time;
			rigTransform.rotation = yaw * Quaternion.Euler(Mathf.Sin(t * 12f) * 15f, Mathf.Sin(t * 6f) * 20f, Mathf.Sin(t * 10f) * 30f);
			if (_hookOn)
			{
				rigTransform.position += yaw * new Vector3(Mathf.Sin(t * 10f) * 0.18f, Mathf.Abs(Mathf.Sin(t * 20f)) * 0.1f, 0f);
			}
			if ((Object)(object)headConstraint != (Object)null)
			{
				headConstraint.localRotation = Quaternion.Euler(Mathf.Sin(t * 14f) * 35f, Mathf.Sin(t * 7f) * 25f, 0f);
			}
		}
		if (fakeFBT)
		{
			if ((Object)(object)headConstraint != (Object)null)
			{
				headConstraint.localRotation = Quaternion.Euler(Mathf.Sin(Time.time * 1.7f) * 12f, Mathf.Sin(Time.time * 1.1f) * 20f, Mathf.Sin(Time.time * 2.3f) * 8f);
			}
			Transform rightHandTransform = local.rightHandTransform;
			Transform leftHandTransform = local.leftHandTransform;
			float num5 = Mathf.Sin(Time.time * 3f) * 25f;
			float num6 = Mathf.Sin(Time.time * 3.5f + 2f) * 25f;
			if ((Object)(object)rightHandTransform != (Object)null)
			{
				rightHandTransform.Rotate(Vector3.up, _hookOn ? num5 : (num5 * dt), (Space)1);
			}
			if ((Object)(object)leftHandTransform != (Object)null)
			{
				leftHandTransform.Rotate(Vector3.up, _hookOn ? num6 : (num6 * dt), (Space)1);
			}
		}
		if (_flipActive)
		{
			float num2 = Mathf.Clamp01((Time.time - _flipTime) / 0.55f);
			float num3 = Mathf.SmoothStep(0f, 360f, num2) * _flipDir;
			rigTransform.rotation = yaw * Quaternion.Euler(num3, 0f, 0f);
			if (num2 >= 1f)
			{
				_flipActive = false;
			}
		}
		if (fakeGrey)
		{
			Rigidbody rigidbody = tagger.rigidbody;
			if ((Object)(object)rigidbody != (Object)null)
			{
				rigidbody.linearVelocity = Vector3.zero;
			}
			Transform rightHandTransform2 = tagger.rightHandTransform;
			Transform leftHandTransform2 = tagger.leftHandTransform;
			if ((Object)(object)rightHandTransform2 != (Object)null)
			{
				rightHandTransform2.position = ((Component)local).transform.position + Vector3.up * 0.4f;
			}
			if ((Object)(object)leftHandTransform2 != (Object)null)
			{
				leftHandTransform2.position = ((Component)local).transform.position + Vector3.up * 0.4f;
			}
		}
	}

	// A toggles Ghost, X toggles Invis, B is held for Spaz and Wacky. Each only reacts while its mod is on.
	private static void HandleKeybinds(GorillaTagger tagger, ControllerInputPoller input)
	{
		bool a = (Object)(object)input != (Object)null && input.rightControllerPrimaryButton;
		bool x = (Object)(object)input != (Object)null && input.leftControllerPrimaryButton;
		Keyboard current = Keyboard.current;
		_bHeld = ((Object)(object)input != (Object)null && input.rightControllerSecondaryButton) || (current != null && ((ButtonControl)current.bKey).isPressed);
		if (a && !_aPrev && ghostMonke)
		{
			ToggleGhost(tagger);
		}
		if (x && !_xPrev && invisMonke)
		{
			ToggleInvis(tagger);
		}
		_aPrev = a;
		_xPrev = x;
	}

	public static void ToggleGhost(GorillaTagger tagger)
	{
		_ghostActive = !_ghostActive;
		if (_ghostActive)
		{
			_invisActive = false;
		}
		NotificationRuntime.Show(_ghostActive ? "Ghost: ON" : "Ghost: OFF");
	}

	public static void ToggleInvis(GorillaTagger tagger)
	{
		_invisActive = !_invisActive;
		if (_invisActive)
		{
			_ghostActive = false;
		}
		NotificationRuntime.Show(_invisActive ? "Invis: ON" : "Invis: OFF");
	}

	// Other players see the local rig, not you. Disabling it stops it following your body: Ghost leaves it
	// standing where it was, Invis parks it far under you. Renderers are never touched, so nothing the game
	// keeps hidden (hand indicators and the like) gets switched back on afterwards.
	private static void HandleGhostInvis(GorillaTagger tagger)
	{
		if (!ghostMonke)
		{
			_ghostActive = false;
		}
		if (!invisMonke)
		{
			_invisActive = false;
		}
		VRRig local = tagger.offlineVRRig;
		if ((Object)(object)local == (Object)null)
		{
			return;
		}
		if (_ghostActive || _invisActive)
		{
			((Behaviour)local).enabled = false;
			_rigHeld = true;
			if (_invisActive && (Object)(object)tagger.bodyCollider != (Object)null)
			{
				((Component)local).transform.position = ((Component)tagger.bodyCollider).transform.position + Vector3.down * 99f;
			}
		}
		else if (_rigHeld)
		{
			((Behaviour)local).enabled = true;
			_rigHeld = false;
		}
	}

	private static void HandleFakeLag(GorillaTagger tagger)
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		if ((!fakeLag && !fastFakeLag) || (Object)(object)tagger == (Object)null || (Object)(object)tagger.offlineVRRig == (Object)null)
		{
			return;
		}
		float num = (fastFakeLag ? 1f : 2f);
		if (!(Time.time < _lagNextFlip))
		{
			_lagNextFlip = Time.time + num;
			_lagFrozen = !_lagFrozen;
			Rigidbody rigidbody = tagger.rigidbody;
			if ((Object)(object)rigidbody != (Object)null && _lagFrozen)
			{
				rigidbody.linearVelocity = Vector3.zero;
			}
		}
	}

	public static void FireFlip(bool back)
	{
		if (!_flipActive)
		{
			_flipActive = true;
			_flipTime = Time.time;
			_flipDir = (back ? 1f : (-1f));
		}
	}

	public static void FixHead()
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		GorillaTagger instance = GorillaTagger.Instance;
		if (!((Object)(object)instance == (Object)null) && !((Object)(object)instance.offlineVRRig == (Object)null))
		{
			Transform headConstraint = instance.offlineVRRig.headConstraint;
			if ((Object)(object)headConstraint != (Object)null)
			{
				headConstraint.localRotation = Quaternion.identity;
			}
			((Component)instance.offlineVRRig).transform.rotation = Quaternion.identity;
			spazMonke = (spazHead = (spazBody = (spazRig = (wackyMonke = false))));
			headSpinTier = 0;
			backwardsHead = (upsideDownHead = false);
		}
	}
}
