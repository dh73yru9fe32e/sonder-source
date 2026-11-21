using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using GorillaExtensions;
using GorillaLocomotion;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using Sonder.Core;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Sonder.Managers;

public class OverpoweredRuntime : MonoBehaviour
{
	private sealed class Push
	{
		public VRRig Rig;

		public Vector3 Velocity;

		public float Until;
	}

	public static float grabTimer;

	public static bool GrabPatchEnabled;

	private float _logSpamNext;

	private float _minesNext;

	private readonly List<Push> _pushes = new List<Push>();

	private readonly Dictionary<int, float> _flingCooldowns = new Dictionary<int, float>();

	private bool _sizeShift;

	private bool _sizeCaptured;

	private int _sizeStep = -1;

	private float _sizeOriginal = 1f;

	private bool _primaryHeld;

	private float _nextRealitySkip;

	private float _nextForcePush;

	private float _nextKickGunGR;
	private float _breakMoveNext;

	private static int _grKickCmdId;
	private static readonly Dictionary<int, float> _grDelayDd = new Dictionary<int, float>();

	public bool KickGunGR { get; set; }

	public bool AutoZoneAuth { get; set; }

	public bool DeafenGunEnabled { get; set; }

	public bool DestroyGunEnabled { get; set; }

	public bool ForceGrabGunEnabled { get; set; }

	public static OverpoweredRuntime Instance { get; private set; }

	public bool FlingTouch { get; set; }

	public bool TimeSlow { get; set; }

	public bool RealitySkip { get; set; }

	public bool MagnetHands { get; set; }

	public bool ForcePush { get; set; }

	public bool LogSpam { get; set; }

	public bool CrashGun { get; set; }

	public bool CrashAll { get; set; }

	private float _nextCrashGun;
	private static bool _crashRunning;

	public bool BreakMovement { get; set; }
	public bool BreakMovementGun { get; set; }
	public bool UnbreakMovementGun { get; set; }
	private float _breakMoveGunNext;
	private float _unbreakMoveGunNext;

	public bool MinesLucy { get; set; }

	public bool SizeShift
	{
		get
		{
			return _sizeShift;
		}
		set
		{
			if (_sizeShift == value)
			{
				return;
			}
			_sizeShift = value;
			GTPlayer player = ExtraUtil.Player;
			if ((Object)(object)player == (Object)null)
			{
				return;
			}
			if (value)
			{
				if (!_sizeCaptured)
				{
					_sizeOriginal = player.ScaleMultiplier;
					_sizeCaptured = true;
				}
				_sizeStep = (_sizeStep + 1) % 3;
				float scaleMultiplier = ((_sizeStep == 0) ? 2.2f : ((_sizeStep == 1) ? 0.5f : 1f));
				player.SetScaleMultiplier(scaleMultiplier);
			}
			else if (_sizeCaptured)
			{
				player.SetScaleMultiplier(_sizeOriginal);
			}
		}
	}

	public static void RunAntiBan(string param)
	{
		RuntimeLog.Write("antiban " + param);
	}

	public static void DeafenPlayer(int actorNumber)
	{
		IReadOnlyList<VRRig> activeRigs = VRRigCache.ActiveRigs;
		if (activeRigs == null)
		{
			return;
		}
		for (int i = 0; i < activeRigs.Count; i++)
		{
			VRRig val = activeRigs[i];
			if (!((Object)(object)val == (Object)null) && !val.isLocal && val.Creator != null)
			{
				_ = val.Creator.ActorNumber;
			}
		}
	}

	public static void DestroyPlayer(Player player)
	{
		DestroyPlayer((player != null) ? player.ActorNumber : 0);
	}

	public static void DestroyPlayer(NetPlayer player)
	{
		DestroyPlayer((player != null) ? player.ActorNumber : 0);
	}

	public static void DestroyPlayer(int actorNumber)
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		IReadOnlyList<VRRig> activeRigs = VRRigCache.ActiveRigs;
		if (activeRigs == null)
		{
			return;
		}
		for (int i = 0; i < activeRigs.Count; i++)
		{
			VRRig val = activeRigs[i];
			if (!((Object)(object)val == (Object)null) && !val.isLocal && val.Creator != null && val.Creator.ActorNumber == actorNumber)
			{
				((Component)val).transform.position = new Vector3(99999f, 99999f, 99999f);
				if ((Object)(object)((Component)val).gameObject != (Object)null)
				{
					((Component)val).gameObject.SetActive(false);
				}
			}
		}
	}

	public static void ForceGrabTarget(VRRig target, Vector3 position)
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)target == (Object)null || target.isLocal || (Object)(object)VRRig.LocalRig == (Object)null)
		{
			return;
		}
		bool flag = target.leftHandLink.CanBeGrabbed();
		bool flag2 = target.rightHandLink.CanBeGrabbed();
		if (!flag && !flag2)
		{
			return;
		}
		GrabPatch.enabled = true;
		((Behaviour)VRRig.LocalRig).enabled = false;
		((Component)VRRig.LocalRig).transform.position = position;
		TakeMyHand_HandLink val = (flag ? target.leftHandLink : target.rightHandLink);
		TakeMyHand_HandLink val2 = (flag ? VRRig.LocalRig.leftHandLink : VRRig.LocalRig.rightHandLink);
		if (val.grabbedPlayer != NetworkSystem.Instance.LocalPlayer)
		{
			if (grabTimer <= 0f)
			{
				grabTimer = Time.time + 0.2f;
			}
			if (!(Time.time <= grabTimer))
			{
				((Component)VRRig.LocalRig).transform.position = target.syncPos;
				val2.TentacleTryCreateLink(val);
				grabTimer = Time.time + 0.2f;
			}
		}
	}

	public static Vector3 ShakePos(VRRig rig)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)rig != (Object)null))
		{
			return Vector3.zero;
		}
		return ((Component)rig).transform.position + Random.insideUnitSphere;
	}

	public static void RunGrabAll(Func<VRRig, Vector3> selector)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		IReadOnlyList<VRRig> activeRigs = VRRigCache.ActiveRigs;
		if (activeRigs == null || selector == null)
		{
			return;
		}
		for (int i = 0; i < activeRigs.Count; i++)
		{
			VRRig val = activeRigs[i];
			if (!((Object)(object)val == (Object)null) && !val.isLocal)
			{
				Vector3 position = selector(val);
				((Component)val).transform.position = position;
			}
		}
	}

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
		RestoreSize();
		ExtraUtil.SetTimeScale("time-slow", enabled: false, 0.35f);
		ClearPushes();
	}

	private void Update()
	{
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		ApplyFlingTouch();
		ApplySizeShiftSanity();
		ApplyTimeSlow();
		ApplyRealitySkip();
		ApplyMagnetHands();
		ApplyForcePush();
		ApplyLogSpam();
		ApplyBreakMovement();
		ApplyBreakMovementGun();
		ApplyUnbreakMovementGun();
		ApplyMinesLucy();
		ApplyKickGunGR();
		ApplyCrashGun();
		if ((Object)(object)GunRuntime.Instance != (Object)null)
		{
			if (ForceGrabGunEnabled && GunRuntime.Instance.gunLocked && (Object)(object)GunRuntime.Instance.lockTarget != (Object)null)
			{
				ForceGrabTarget(GunRuntime.Instance.lockTarget, ((Component)GunRuntime.Instance.lockTarget).transform.position);
			}
		}
		ApplyPushes();
	}

	private void ApplyFlingTouch()
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		if (!FlingTouch)
		{
			return;
		}
		GorillaTagger tagger = ExtraUtil.Tagger;
		if ((Object)(object)tagger == (Object)null || (Object)(object)tagger.leftHandTransform == (Object)null || (Object)(object)tagger.rightHandTransform == (Object)null)
		{
			return;
		}
		List<VRRig> list = ExtraUtil.RemoteRigs();
		for (int i = 0; i < list.Count; i++)
		{
			VRRig val = list[i];
			if ((Object)(object)val == (Object)null || val.Creator == null)
			{
				continue;
			}
			float num = Vector3.Distance(tagger.leftHandTransform.position, ((Component)val).transform.position);
			float num2 = Vector3.Distance(tagger.rightHandTransform.position, ((Component)val).transform.position);
			if (num > 1.15f && num2 > 1.15f)
			{
				continue;
			}
			int actorNumber = val.Creator.ActorNumber;
			if (!_flingCooldowns.TryGetValue(actorNumber, out var value) || !(Time.time < value))
			{
				_flingCooldowns[actorNumber] = Time.time + 0.6f;
				Vector3 val2 = ((Component)val).transform.position - ExtraUtil.PlayerPosition();
				val2 = Vector3.ProjectOnPlane(val2, Vector3.up);
				if (val2.sqrMagnitude < 0.001f)
				{
					val2 = Vector3.forward;
				}
				AddPush(val, (val2.normalized + Vector3.up * 0.35f) * 18f, 0.55f);
			}
		}
	}

	private void ApplySizeShiftSanity()
	{
		if (SizeShift && !_sizeCaptured)
		{
			GTPlayer player = ExtraUtil.Player;
			if ((Object)(object)player != (Object)null)
			{
				_sizeOriginal = player.ScaleMultiplier;
				_sizeCaptured = true;
			}
		}
	}

	private void RestoreSize()
	{
		GTPlayer player = ExtraUtil.Player;
		if ((Object)(object)player != (Object)null && _sizeCaptured)
		{
			player.SetScaleMultiplier(_sizeOriginal);
		}
		_sizeCaptured = false;
		_sizeStep = -1;
	}

	private void ApplyTimeSlow()
	{
		ExtraUtil.SetTimeScale("time-slow", TimeSlow, 0.35f);
	}

	private void ApplyRealitySkip()
	{
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		ControllerInputPoller instance = ControllerInputPoller.instance;
		bool flag = (Object)(object)instance != (Object)null && instance.rightControllerPrimaryButton;
		bool flag2 = flag && !_primaryHeld;
		_primaryHeld = flag;
		if (!RealitySkip || !flag2 || Time.time < _nextRealitySkip)
		{
			return;
		}
		Transform val = ExtraUtil.Head();
		GTPlayer player = ExtraUtil.Player;
		if (!((Object)(object)val == (Object)null) && !((Object)(object)player == (Object)null))
		{
			_nextRealitySkip = Time.time + 0.55f;
			ExtraUtil.Teleport(((Component)player).transform.position + val.forward * 4.5f + Vector3.up * 0.2f);
			Rigidbody playerRigidBody = player.playerRigidBody;
			if ((Object)(object)playerRigidBody != (Object)null)
			{
				playerRigidBody.linearVelocity = Vector3.zero;
			}
		}
	}

	private void ApplyMagnetHands()
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		if (!MagnetHands)
		{
			return;
		}
		Vector3 val = ExtraUtil.PlayerPosition();
		List<VRRig> list = ExtraUtil.RemoteRigs();
		for (int i = 0; i < list.Count; i++)
		{
			VRRig val2 = list[i];
			if (!((Object)(object)val2 == (Object)null))
			{
				Vector3 val3 = val + Vector3.up * 1.2f - (((Component)val2).transform.position + Vector3.up * 1.2f);
				if (!(val3.magnitude > 16f))
				{
					AddPush(val2, val3.normalized * 13f, 0.12f);
				}
			}
		}
	}

	private void ApplyForcePush()
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		if (!ForcePush || Time.time < _nextForcePush)
		{
			return;
		}
		_nextForcePush = Time.time + 0.85f;
		Transform val = ExtraUtil.Head();
		if ((Object)(object)val == (Object)null)
		{
			return;
		}
		Vector3 position = val.position;
		Vector3 forward = val.forward;
		List<VRRig> list = ExtraUtil.RemoteRigs();
		for (int i = 0; i < list.Count; i++)
		{
			VRRig val2 = list[i];
			if (!((Object)(object)val2 == (Object)null))
			{
				Vector3 val3 = ((Component)val2).transform.position - position;
				if (!(val3.magnitude > 9f) && !(Vector3.Dot(forward, val3.normalized) < 0.55f))
				{
					AddPush(val2, (val3.normalized + Vector3.up * 0.25f) * 20f, 0.55f);
				}
			}
		}
	}

	private void ApplyLogSpam()
	{
		if (!LogSpam)
		{
			_logSpamNext = 0f;
			return;
		}
		if (Time.time < _logSpamNext)
		{
			return;
		}
		_logSpamNext = Time.time + 0.35f;
		try
		{
			VRRig own = GROwnRig();
			if ((Object)(object)own == (Object)null) return;
			Vector3 pos = ((Component)own).transform.position - new Vector3(0f, 10f, 0f);
			GRCrr(pos, Vector3.zero, new Quaternion[1] { Quaternion.identity }, l: true, ss: new int[1] { -70 }, h: null, t: (object)(RpcTarget)0);
		}
		catch { }
	}

	private void ApplyBreakMovement()
	{
		// Malachi Break Movement All: networked GR break (Breakm, type 1517967647) on GR players
		if (!BreakMovement)
		{
			return;
		}
		if (Time.time < _breakMoveNext)
		{
			return;
		}
		_breakMoveNext = Time.time + 3f;
		try
		{
			int[] actors = (from r in GRAllRigs(includeSelf: false)
				where GRInZone(r)
				let p = GRPlayerFromRig(r)
				where p != null
				select p.ActorNumber).Distinct().ToArray();
			if (actors.Length == 0) return;
			StartCoroutine(GRBreakMovementRoutine(actors));
		}
		catch { }
	}

	private IEnumerator GRBreakMovementRoutine(int[] actors)
	{
		// Aggressively acquire GR zone/map ownership before firing Breakm payloads
		yield return GREnsureZoneOwnershipRoutine(2.5f, 0.1f);

		try { GRBreakmActors(actors, b: false); }
		catch { }

		// follow-up waves to improve reliability once/if authority lands
		yield return new WaitForSeconds(0.2f);
		try { GREnsureZoneOwnershipBurst(); } catch { }
		try { GRBreakmActors(actors, b: false); } catch { }
	}

	private void ApplyBreakMovementGun()
	{
		if (!BreakMovementGun)
		{
			_breakMoveGunNext = 0f;
			return;
		}
		GunRuntime gun = GunRuntime.Instance;
		if ((Object)(object)gun == (Object)null)
		{
			return;
		}
		VRRig target = (gun.gunLocked ? gun.lockTarget : null);
		if ((Object)(object)target == (Object)null)
		{
			try
			{
				if (gun.lastLockActor > 0)
				{
					Player p = GRPlayerFromActor(gun.lastLockActor);
					if (p != null) target = GRRigFromPlayer(p);
				}
			}
			catch { }
		}
		if (!GunRuntime.GetGunInput(trigger: true) || (Object)(object)target == (Object)null)
		{
			return;
		}
		if (Time.time < _breakMoveGunNext)
		{
			return;
		}
		_breakMoveGunNext = Time.time + 0.25f;
		Player tp = GRPlayerFromRig(target);
		if (tp == null || !GRInZone(target))
		{
			NotificationRuntime.Show("Target must be in Ghost Reactor!");
			return;
		}
		StartCoroutine(GRBreakMovementRoutine(new int[1] { tp.ActorNumber }));
		NotificationRuntime.Show("Break Movement: " + ExtraUtil.Nickname(target));
	}

	private void ApplyUnbreakMovementGun()
	{
		if (!UnbreakMovementGun)
		{
			_unbreakMoveGunNext = 0f;
			return;
		}
		GunRuntime gun = GunRuntime.Instance;
		if ((Object)(object)gun == (Object)null)
		{
			return;
		}
		VRRig target = (gun.gunLocked ? gun.lockTarget : null);
		if ((Object)(object)target == (Object)null)
		{
			try
			{
				if (gun.lastLockActor > 0)
				{
					Player p = GRPlayerFromActor(gun.lastLockActor);
					if (p != null) target = GRRigFromPlayer(p);
				}
			}
			catch { }
		}
		if (!GunRuntime.GetGunInput(trigger: true) || (Object)(object)target == (Object)null)
		{
			return;
		}
		if (Time.time < _unbreakMoveGunNext)
		{
			return;
		}
		_unbreakMoveGunNext = Time.time + 0.25f;
		Player tp = GRPlayerFromRig(target);
		if (tp == null || !GRInZone(target))
		{
			NotificationRuntime.Show("Target must be in Ghost Reactor!");
			return;
		}
		StartCoroutine(GRUnbreakMovementRoutine(new int[1] { tp.ActorNumber }));
		NotificationRuntime.Show("Unbreak Movement: " + ExtraUtil.Nickname(target));
	}

	public static void UnbreakMovementAll()
	{
		if ((Object)(object)Instance == (Object)null) return;
		Instance.StartCoroutine(Instance.GRUnbreakMovementAllRoutine());
	}

	private IEnumerator GRUnbreakMovementAllRoutine()
	{
		VRRig own = GROwnRig();
		if (!PhotonNetwork.InRoom || (Object)(object)own == (Object)null || !GRInZone(own))
		{
			NotificationRuntime.Show("You Must Be In GhostReactor!");
			yield break;
		}
		int[] actors = (from r in GRAllRigs(includeSelf: false)
			where GRInZone(r)
			let p = GRPlayerFromRig(r)
			where p != null
			select p.ActorNumber).Distinct().ToArray();
		if (actors.Length == 0)
		{
			NotificationRuntime.Show("No One Is In GhostReactor!");
			yield break;
		}
		yield return GRUnbreakMovementRoutine(actors);
		NotificationRuntime.Show("Unbreak Movement: ALL");
	}

	private IEnumerator GRUnbreakMovementRoutine(int[] actors)
	{
		if (actors == null || actors.Length == 0) yield break;
		yield return GREnsureZoneOwnershipRoutine(2.5f, 0.1f);
		try { GRUnbreakActors(actors); } catch { }
		yield return new WaitForSeconds(0.2f);
		try { GREnsureZoneOwnershipBurst(); } catch { }
		try { GRUnbreakActors(actors); } catch { }
	}

	private static void GRUnbreakActors(int[] actorNumbers)
	{
		if (actorNumbers == null || actorNumbers.Length == 0) return;
		int localActor = (PhotonNetwork.LocalPlayer != null) ? PhotonNetwork.LocalPlayer.ActorNumber : -1;
		int[] targets = actorNumbers.Where(a => a > 0 && a != localActor).Distinct().ToArray();
		if (targets.Length == 0) return;
		// Restore/fix attempt: send neutral Breakm payload with identity rotation and zero offsets.
		int m = 1;
		int typeId = 1517967647;
		Vector3[] pos = Enumerable.Repeat(Vector3.zero, m).ToArray();
		Quaternion[] rot = Enumerable.Repeat(Quaternion.identity, m).ToArray();
		int[] types = Enumerable.Repeat(typeId, m).ToArray();
		GRCrr(pos, Vector3.zero, rot, l: true, ss: types, h: null, t: targets);
	}

	private void ApplyMinesLucy()
	{
		if (!MinesLucy)
		{
			_minesNext = 0f;
			return;
		}
		if (Time.time < _minesNext)
		{
			return;
		}
		_minesNext = Time.time + 3f;
		TrySpawnMinesLucy(blue: true);
	}

	private static void TrySpawnMinesLucy(bool blue)
	{
		try
		{
			if (!PhotonNetwork.InRoom || PhotonNetwork.LocalPlayer == null || !PhotonNetwork.LocalPlayer.IsMasterClient)
			{
				return;
			}
			GameObject go = GameObject.Find("Environment Objects/05Maze_PersistentObjects/Ghosts/Halloween Ghost/FloatingChaseSkeleton");
			if ((Object)(object)go == (Object)null) return;
			HalloweenGhostChaser h = go.GetComponent<HalloweenGhostChaser>();
			if ((Object)(object)h == (Object)null) return;
			try
			{
				PhotonView view = go.GetComponent<PhotonView>();
				if ((Object)(object)view == (Object)null) view = ((Component)h).GetComponent<PhotonView>();
				if ((Object)(object)view != (Object)null && !view.IsMine)
				{
					view.RequestOwnership();
				}
			}
			catch { }
			// Malachi: bool[]{true,false} spawn blue => isSummoned false, bool[]{true,true} spawn red => isSummoned true
			try
			{
				h.timeGongStarted = Time.time;
				h.currentState = (HalloweenGhostChaser.ChaseState)4;
				h.isSummoned = !blue;
			}
			catch { }
		}
		catch { }
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

	// ---------- Kick [GR] (Malachi exact method, optimized orchestration) ----------

	private void ApplyKickGunGR()
	{
		if (!KickGunGR)
		{
			_nextKickGunGR = 0f;
			return;
		}
		GunRuntime gun = GunRuntime.Instance;
		if ((Object)(object)gun == (Object)null)
		{
			return;
		}

		VRRig target = (gun.gunLocked ? gun.lockTarget : null);
		if ((Object)(object)target == (Object)null)
		{
			// fallback: use last locked actor if lock target ref dropped
			try
			{
				if (gun.lastLockActor > 0)
				{
					Player p = GRPlayerFromActor(gun.lastLockActor);
					if (p != null) target = GRRigFromPlayer(p);
				}
			}
			catch { }
		}

		bool fire = GunRuntime.GetGunInput(trigger: true);
		if (!fire || (Object)(object)target == (Object)null)
		{
			return;
		}
		if (Time.time < _nextKickGunGR)
		{
			return;
		}
		_nextKickGunGR = Time.time + 0.12f;

		// Ensure GR zone auth is requested before firing (best-effort)
		GREnsureZoneOwnership();
		GRKick(target);
	}

	public static void KickAllGR()
	{
		GREnsureZoneOwnership();
		VRRig own = GROwnRig();
		if (!PhotonNetwork.InRoom || (Object)(object)own == (Object)null || !GRInZone(own))
		{
			NotificationRuntime.Show("You Must Be In GhostReactor!");
			return;
		}
		List<VRRig> targets = (from e in GRAllRigs(includeSelf: false) where GRInZone(e) select e).ToList();
		if (targets.Count == 0)
		{
			NotificationRuntime.Show("No One Is In GhostReactor!");
			return;
		}
		for (int i = 0; i < targets.Count; i++)
		{
			VRRig r = targets[i];
			if ((Object)(object)r == (Object)null || r.Creator == null) continue;
			GRKick(r);
		}
	}

	// ---------- Crash [GR] (Malachi method: q(88)/Breakm/Crr -71) ----------

	private void ApplyCrashGun()
	{
		if (!CrashGun)
		{
			_nextCrashGun = 0f;
			return;
		}
		GunRuntime gun = GunRuntime.Instance;
		if ((Object)(object)gun == (Object)null) return;

		VRRig target = (gun.gunLocked ? gun.lockTarget : null);
		if ((Object)(object)target == (Object)null)
		{
			try
			{
				if (gun.lastLockActor > 0)
				{
					Player p = GRPlayerFromActor(gun.lastLockActor);
					if (p != null) target = GRRigFromPlayer(p);
				}
			}
			catch { }
		}

		if (!GunRuntime.GetGunInput(trigger: true) || (Object)(object)target == (Object)null)
			return;
		if (_crashRunning || Time.time < _nextCrashGun)
			return;

		_nextCrashGun = Time.time + 0.5f;
		StartCrashGR(target, force: true);
	}

	public static void CrashAllGR()
	{
		if (_crashRunning) return;
		StartCrashGR(null, force: true);
	}

	private static void StartCrashGR(VRRig targetRig, bool force = false)
	{
		if (!force && _crashRunning) return;
		if ((Object)(object)Instance == (Object)null) return;
		Instance.StartCoroutine(CrashGRRoutine(targetRig));
	}

	private static IEnumerator CrashGRRoutine(VRRig targetRig)
	{
		_crashRunning = true;
		try
		{
			VRRig own = GROwnRig();
			if (!PhotonNetwork.InRoom || (Object)(object)own == (Object)null || own.zoneEntity == null)
				yield break;
			if (!GRInZone(own))
			{
				NotificationRuntime.Show("You Must Be In GhostReactor!");
				yield break;
			}

			GameEntityManager g = GameEntityManager.GetManagerForZone(own.zoneEntity.currentZone);
			if ((Object)(object)g == (Object)null)
				yield break;

			bool isAll = (Object)(object)targetRig == (Object)null;
			if (!isAll)
			{
				if ((Object)(object)targetRig == (Object)null || !GRInZone(targetRig))
					yield break;
			}
			else
			{
				int othersInGR = GRAllRigs(includeSelf: false).Count((VRRig r) => GRInZone(r));
				if (othersInGR == 0)
					yield break;
			}

			if (!GRHasZoneAuthority(g))
			{
				GROw(g);
				yield return GREnsureZoneOwnershipRoutine(2.5f, 0.1f);
			}

			// Malachi Breakm/Crr -71, tuned for strong remote effect without local lag
			object targetObj = isAll ? (object)own : (object)targetRig;
			for (int wave = 0; wave < 3; wave++)
			{
				if (isAll)
				{
					int[] actors = (from r in GRAllRigs(includeSelf: false)
						where GRInZone(r)
						let p = GRPlayerFromRig(r)
						where p != null
						select p.ActorNumber).Distinct().ToArray();
					if (actors.Length > 0)
					{
						GRBreakmActors(actors, b: true);
					}
				}
				else
				{
					GRBreakmTargetRig(targetRig, b: true);
				}

				// Two -71 payloads per wave (Malachi core crash vector)
				GRCrr(Vector3.zero, Vector3.zero, Quaternion.identity, l: true, ss: new int[1] { -71 }, h: g, t: targetObj);
				GRCrr(Vector3.zero, Vector3.zero, Quaternion.identity, l: true, ss: new int[1] { -71 }, h: g, t: targetObj);
				yield return new WaitForSeconds(0.2f);
			}

			if (!isAll)
				NotificationRuntime.Show("Crashed: " + ExtraUtil.Nickname(targetRig));
		}
		finally
		{
			_crashRunning = false;
		}
	}

	private static void GRBreakmActors(int[] actorNumbers, bool b = false)
	{
		if (actorNumbers == null || actorNumbers.Length == 0) return;
		int localActor = (PhotonNetwork.LocalPlayer != null) ? PhotonNetwork.LocalPlayer.ActorNumber : -1;
		int[] targets = actorNumbers.Where(a => a > 0 && a != localActor).Distinct().ToArray();
		if (targets.Length == 0) return;
		int m = b ? 102 : 1;
		int typeId = b ? 1551901997 : 1517967647;
		Vector3[] pos = Enumerable.Repeat(Vector3.zero, m).ToArray();
		Quaternion[] rot = Enumerable.Repeat(Quaternion.Euler(90f, 0f, 0f), m).ToArray();
		int[] types = Enumerable.Repeat(typeId, m).ToArray();
		GRCrr(pos, Vector3.zero, rot, l: true, ss: types, h: null, t: targets);
	}

	private static void GRBreakmTargetRig(VRRig rig, bool b = false)
	{
		if ((Object)(object)rig == (Object)null) return;
		Player p = GRPlayerFromRig(rig);
		if (p == null || PhotonNetwork.LocalPlayer == null) return;
		int[] a2 = new int[2] { p.ActorNumber, PhotonNetwork.LocalPlayer.ActorNumber };
		int m = b ? 102 : 1;
		int typeId = b ? 1551901997 : 1517967647;
		Vector3[] pos = Enumerable.Repeat(Vector3.zero, m).ToArray();
		Quaternion[] rot = Enumerable.Repeat(Quaternion.Euler(90f, 0f, 0f), m).ToArray();
		int[] types = Enumerable.Repeat(typeId, m).ToArray();
		GRCrr(pos, Vector3.zero, rot, l: true, ss: types, h: null, t: a2);
	}

	private static bool _grZoneControlNotified;
	private static int _grZoneControlNotifiedZone = int.MinValue;

	private static bool GRHasZoneAuthority(GameEntityManager g)
	{
		try
		{
			if ((Object)(object)g == (Object)null) return false;
			// Master client treated as having effective zone/map control for GR ops
			if (PhotonNetwork.InRoom && PhotonNetwork.LocalPlayer != null && PhotonNetwork.LocalPlayer.IsMasterClient)
				return true;
			if (g.guard != null && g.guard.isTrulyMine) return true;
			if (g.photonView != null && g.photonView.IsMine) return true;
		}
		catch { }
		return false;
	}

	private static bool GRHasTrueZoneOwnership(GameEntityManager g)
	{
		try
		{
			if ((Object)(object)g == (Object)null) return false;
			if (g.guard != null && g.guard.isTrulyMine) return true;
			if (g.photonView != null && g.photonView.IsMine) return true;
		}
		catch { }
		return false;
	}

	private static void GRNotifyZoneControlIfGained(GameEntityManager g)
	{
		try
		{
			if ((Object)(object)g == (Object)null) return;
			if (!GRHasTrueZoneOwnership(g))
			{
				// allow future notification if we lose/reacquire in same zone context
				return;
			}
			int zone = 0;
			try { zone = (int)g.zone; } catch { zone = 0; }
			if (_grZoneControlNotified && _grZoneControlNotifiedZone == zone) return;
			_grZoneControlNotified = true;
			_grZoneControlNotifiedZone = zone;
			NotificationRuntime.Show("Zone control acquired (GR zone " + zone + ")");
		}
		catch { }
	}

	private static void GREnsureZoneOwnershipBurst()
	{
		try
		{
			VRRig own = GROwnRig();
			if ((Object)(object)own == (Object)null || own.zoneEntity == null) return;
			GameEntityManager g = GameEntityManager.GetManagerForZone(own.zoneEntity.currentZone);
			if ((Object)(object)g == (Object)null) return;
			if (GRHasTrueZoneOwnership(g))
			{
				GRNotifyZoneControlIfGained(g);
				return;
			}

			// Master-first aggressive acquisition (Malachi-style guard authority path)
			if (PhotonNetwork.LocalPlayer != null && PhotonNetwork.LocalPlayer.IsMasterClient)
			{
				try { if (g.guard != null) g.guard.RequestOwnershipImmediatelyWithGuaranteedAuthority(); } catch { }
				try { if (g.photonView != null) g.photonView.RequestOwnership(); } catch { }
				try { if (g.guard != null) g.guard.RequestOwnership((Action)null, (Action)null); } catch { }
			}
			else
			{
				try { if (g.guard != null) g.guard.RequestOwnership((Action)null, (Action)null); } catch { }
				try { if (g.photonView != null) g.photonView.RequestOwnership(); } catch { }
			}
			if (GRHasTrueZoneOwnership(g)) GRNotifyZoneControlIfGained(g);
		}
		catch { }
	}

	private static void GREnsureZoneOwnership()
	{
		GREnsureZoneOwnershipBurst();
	}

	private static IEnumerator GREnsureZoneOwnershipRoutine(float timeoutSeconds, float intervalSeconds)
	{
		float end = Time.time + Mathf.Max(0.1f, timeoutSeconds);
		float interval = Mathf.Max(0.03f, intervalSeconds);
		while (Time.time < end)
		{
			try
			{
				VRRig own = GROwnRig();
				if ((Object)(object)own != (Object)null && own.zoneEntity != null)
				{
					GameEntityManager g = GameEntityManager.GetManagerForZone(own.zoneEntity.currentZone);
					if ((Object)(object)g != (Object)null)
					{
						if (GRHasTrueZoneOwnership(g))
						{
							GRNotifyZoneControlIfGained(g);
							yield break;
						}
						GREnsureZoneOwnershipBurst();
						if (GRHasTrueZoneOwnership(g))
						{
							GRNotifyZoneControlIfGained(g);
							yield break;
						}
						// Master treated as effective control: can proceed even if guard not yet mine
						if (GRHasZoneAuthority(g)) yield break;
					}
				}
			}
			catch { }
			yield return new WaitForSeconds(interval);
		}
	}

	private static void GRDelay(float t, Action s)
	{
		if (s == null) return;
		// Use a stable key per method+target to avoid closure-token stalls
		string keyStr = s.Method.MetadataToken.ToString() + ":" + (s.Target != null ? s.Target.GetHashCode().ToString() : "0");
		int key = keyStr.GetHashCode();
		if (!_grDelayDd.ContainsKey(key) || Time.time > _grDelayDd[key])
		{
			s();
			_grDelayDd[key] = Time.time + t;
		}
	}

	private static VRRig GROwnRig() => VRRig.LocalRig;

	private static bool GRInZone(VRRig r)
	{
		if ((Object)(object)r == (Object)null || r.zoneEntity == null) return false;
		int zone = (int)r.zoneEntity.currentZone;
		return zone == 24 || zone == 28;
	}

	private static List<VRRig> GRAllRigs(bool includeSelf)
	{
		List<VRRig> list = new List<VRRig>();
		IReadOnlyList<VRRig> activeRigs = VRRigCache.ActiveRigs;
		if (activeRigs == null) return list;
		for (int i = 0; i < activeRigs.Count; i++)
		{
			VRRig r = activeRigs[i];
			if ((Object)(object)r == (Object)null) continue;
			if (!includeSelf && (r.isOfflineVRRig || r.isLocal)) continue;
			list.Add(r);
		}
		return list;
	}

	private static Player GRPlayerFromRig(VRRig r)
	{
		try { if ((Object)(object)r != (Object)null && r.Creator != null) return r.Creator.GetPlayerRef(); } catch { }
		return null;
	}

	private static Player GRPlayerFromActor(int actor)
	{
		Player[] players = PhotonNetwork.PlayerList;
		if (players == null) return null;
		for (int i = 0; i < players.Length; i++)
		{
			if (players[i] != null && players[i].ActorNumber == actor) return players[i];
		}
		return null;
	}

	private static VRRig GRRigFromPlayer(Player p)
	{
		if (p == null) return null;
		try
		{
			if (GorillaGameManager.instance != null)
			{
				NetPlayer np = p;
				return GorillaGameManager.instance.FindPlayerVRRig(np);
			}
		}
		catch { }
		List<VRRig> rigs = GRAllRigs(includeSelf: true);
		for (int i = 0; i < rigs.Count; i++)
		{
			Player rp = GRPlayerFromRig(rigs[i]);
			if (rp != null && rp.ActorNumber == p.ActorNumber) return rigs[i];
		}
		return null;
	}

	private static VRRig GRRigFromNet(NetPlayer np)
	{
		try { return (np != null) ? GRRigFromPlayer(np.GetPlayerRef()) : null; } catch { return null; }
	}

	private static object[] GRGetCol(int i)
	{
		// Malachi: check collectibles, then itemPrefabFactory (Field 84) on current zone manager
		try
		{
			VRRig own = GROwnRig();
			if ((Object)(object)own != (Object)null && own.zoneEntity != null)
			{
				GameEntityManager g = GameEntityManager.GetManagerForZone(own.zoneEntity.currentZone);
				if ((Object)(object)g != (Object)null)
				{
					Dictionary<int, GameObject> dict = Traverse.Create(g).Field("itemPrefabFactory").GetValue<Dictionary<int, GameObject>>();
					if (dict != null && dict.TryGetValue(i, out GameObject prefab) && (Object)(object)prefab != (Object)null)
					{
						string name = ((Object)prefab).name;
						string disp = name.Replace("GhostReactor", "GhostReactor ").Replace("_", " ").Replace("Collectible", "").Replace("Monkeye01", "Monkeye");
						return new object[4] { name, disp, i, 0L };
					}
				}
			}
		}
		catch { }
		return new object[0];
	}

	private static void GRKick(object t)
	{
		VRRig own = GROwnRig();
		if (!PhotonNetwork.InRoom || (Object)(object)own == (Object)null || own.zoneEntity == null)
		{
			return;
		}
		GameEntityManager managerForZone = GameEntityManager.GetManagerForZone(own.zoneEntity.currentZone);
		if ((Object)(object)managerForZone == (Object)null)
		{
			return;
		}
		RequestableOwnershipGuard guard = managerForZone.guard;
		NetPlayer val = null;

		if (t is RpcTarget)
		{
			List<VRRig> list = (from e in GRAllRigs(includeSelf: false) where GRInZone(e) select e).ToList();
			if (list.Count == 0)
			{
				NotificationRuntime.Show("No One Is In GhostReactor!");
				return;
			}
			// Kick all GR rigs when invoked with RpcTarget
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i] != null && list[i].Creator != null)
				{
					GRKick(list[i]);
				}
			}
			return;
		}

		VRRig val3 = t as VRRig;
		if (val3 != null)
		{
			val = val3.Creator;
		}

		Player val4 = t as Player;
		if (val4 != null)
		{
			val = val4;
		}

		if (t is int[] array && array.Length > 0)
		{
			Player p = GRPlayerFromActor(array[0]);
			if (p != null) val = p;
		}

		if (val == null || !GRInZone(GRRigFromNet(val)))
		{
			NotificationRuntime.Show("Selected Player Must Be In GhostReactor!");
			return;
		}
		if (!GRInZone(own))
		{
			NotificationRuntime.Show("You Must Be In GhostReactor!");
			return;
		}

		// Best-effort: ensure/refresh ownership before firing Malachi payload
		GREnsureZoneOwnership();

		GRCrr(
			Enumerable.Repeat<Vector3>(((Component)own).transform.position - new Vector3(0f, 2f, 0f), 101).ToArray(),
			Vector3.zero,
			Enumerable.Repeat<Quaternion>(Quaternion.identity, 101).ToArray(),
			l: true,
			Enumerable.Repeat(-20887423, 101).ToArray(),
			null,
			t);

		// Malachi: immediate transfer if already mine; also schedule retries
		if (guard != null && guard.isTrulyMine)
		{
			try { guard.TransferOwnership(val, ""); } catch { }
		}
		if ((Object)(object)Instance != (Object)null)
		{
			Instance.StartCoroutine(GRTransferRetryRoutine(managerForZone, val, 0.15f, 8));
		}
	}

	private static IEnumerator GRTransferRetryRoutine(GameEntityManager manager, NetPlayer target, float interval, int attempts)
	{
		for (int i = 0; i < attempts; i++)
		{
			yield return new WaitForSeconds(interval);
			try
			{
				if ((Object)(object)manager == (Object)null || manager.guard == null || target == null) yield break;
				if (manager.guard.isTrulyMine)
				{
					manager.guard.TransferOwnership(target, "");
				}
				else
				{
					if (PhotonNetwork.LocalPlayer != null && PhotonNetwork.LocalPlayer.IsMasterClient)
						manager.guard.RequestOwnershipImmediatelyWithGuaranteedAuthority();
					else
						manager.guard.RequestOwnership((Action)null, (Action)null);
				}
			}
			catch { }
		}
	}

	private static void GRCrr(object pp, Vector3 v, object rr, bool l = false, int[] ss = null, GameEntityManager h = null, object t = null)
	{
		if (!PhotonNetwork.InRoom || !l)
		{
			return;
		}
		VRRig own = GROwnRig();
		GameEntityManager g = h ?? ((own != null && own.zoneEntity != null) ? GameEntityManager.GetManagerForZone(own.zoneEntity.currentZone) : null);
		if ((Object)(object)g == (Object)null)
		{
			return;
		}

		int s = -1;
		if (ss != null && (ss.Length == 1 || ss.All((int e) => e == ss[0])))
		{
			s = ss[0];
		}
		object p = Vector3.zero;
		if (pp is Vector3 val) p = val;
		if (pp is Vector3[] array) p = array;

		int i;
		if (s == -70) i = 618507822;
		else if (s == -69) i = 46876848;
		else if (s == -71) i = 618507822;
		else i = ((s == -1) ? 1618940484 : s);

		object[] o = GRGetCol(i);

		bool hasAuth = GRHasZoneAuthority(g);
		if (!hasAuth)
		{
			GROw(g);
			GREnsureZoneOwnershipBurst();
			hasAuth = GRHasZoneAuthority(g);
		}
		// For Break Movement / GR ops, try to fire even when authority is still settling;
		// ownership bursts + retries run in background. Master has effective zone control.
		if (!hasAuth)
		{
			return;
		}

		object i2 = (o != null && o.Length > 2) ? o[2] : (object)i;
		bool b2 = v == Vector3.zero;
		if (ss != null && ss.Length > 1)
		{
			i2 = ss;
		}
		if (s == -71)
		{
			i2 = Enumerable.Repeat(618507822, 102).ToArray();
			rr = Enumerable.Repeat<Quaternion>(Quaternion.identity, 102).ToArray();
			p = Enumerable.Repeat<Vector3>(((Component)own).transform.position, 102).ToArray();
		}

		float delay = b2 ? 0.095f : 0.137f;
		GRDelay(delay, () =>
		{
			try
			{
				object pp2 = b2 ? p : ((object)(((Component)own).transform.position + new Vector3(0f, 0.4f, 0f)));
				GRCr(pp2, rr, i2, t ?? ((object)(RpcTarget)0), g);

				if (s == -69 || s == -70 || !b2)
				{
					try
					{
						GameEntity gameEntityFromNetId = g.GetGameEntityFromNetId(_grKickCmdId);
						if ((Object)(object)gameEntityFromNetId != (Object)null && PhotonNetwork.LocalPlayer != null && gameEntityFromNetId.heldByActorNumber != PhotonNetwork.LocalPlayer.ActorNumber)
						{
							if (s != -69 && s != -70)
							{
								Vector3 throwPos = (p is Vector3 pv) ? pv : ((Component)own).transform.position;
								g.RequestGrabEntity(gameEntityFromNetId.id, false, Vector3.zero, Quaternion.identity);
								g.RequestThrowEntity(gameEntityFromNetId.id, false, throwPos, v, v);
							}
							else
							{
								g.photonView.RPC("DestroyItemRPC", (RpcTarget)0, new object[1] { new int[1] { _grKickCmdId } });
							}
						}
					}
					catch { }
				}
			}
			catch { }
		});
	}

	private static void GRCr(object pp, object rr, object id, object q = null, GameEntityManager s = null)
	{
		VRRig own = GROwnRig();
		GameEntityManager m = s ?? ((own != null && own.zoneEntity != null) ? GameEntityManager.GetManagerForZone(own.zoneEntity.currentZone) : null);
		if ((Object)(object)m == (Object)null)
		{
			return;
		}
		object t = q ?? ((object)(RpcTarget)0);
		int iv = 0;
		try { iv = m.GetNetIdFromEntityId(GameEntityId.Invalid); } catch { }

		Quaternion r = Quaternion.identity;
		Quaternion[] qw = rr as Quaternion[] ?? new Quaternion[0];
		if (rr is Quaternion qt) r = qt;

		Vector3 p = Vector3.zero;
		Vector3[] vw = pp as Vector3[] ?? new Vector3[0];
		if (pp is Vector3 vt) p = vt;

		object[] w = new object[0];
		if (id is int ii)
		{
			long cd = 0L;
			try
			{
				object[] col = GRGetCol(ii);
				if (col != null && col.Length > 3) cd = Convert.ToInt64(col[3]);
			}
			catch { }
			w = new object[6]
			{
				new int[1] { GRCreateNetId(1 + m.FactoryGetBuiltInEntityCountById(ii), m) },
				new int[1] { ii },
				new long[1] { BitPackUtils.PackWorldPosForNetwork(p) },
				new int[1] { BitPackUtils.PackQuaternionForNetwork(r) },
				new long[1] { cd },
				new int[1] { iv }
			};
		}
		else if (id is int[] ig)
		{
			int n = ig.Length;
			int[] ns = new int[n];
			int[] ids = new int[n];
			long[] po = new long[n];
			int[] ro = new int[n];
			long[] cds = new long[n];
			int[] ivs = new int[n];
			for (int io = 0; io < n; io++)
			{
				ids[io] = ig[io];
				ns[io] = GRCreateNetId(io + m.FactoryGetBuiltInEntityCountById(ig[io]), m);
				long cd = 0L;
				try
				{
					object[] col = GRGetCol(ig[io]);
					if (col != null && col.Length > 3) cd = Convert.ToInt64(col[3]);
				}
				catch { }
				cds[io] = cd;
				ro[io] = BitPackUtils.PackQuaternionForNetwork((qw.Length > io) ? qw[io] : Quaternion.identity);
				po[io] = BitPackUtils.PackWorldPosForNetwork((vw.Length > io) ? vw[io] : p);
				ivs[io] = iv;
			}
			w = new object[6] { ns, ids, po, ro, cds, ivs };
		}
		if (w.Length < 6)
		{
			return;
		}
		try { _grKickCmdId = ((int[])w[0])[0]; } catch { }

		if (t is RpcTarget e)
		{
			GRSendCr(m, e, w);
		}
		Player k = t as Player;
		if (k != null)
		{
			GRSendCr(m, k, w);
		}
		VRRig vr = t as VRRig;
		if (vr != null)
		{
			long lp = BitPackUtils.PackWorldPosForNetwork(((Component)vr).transform.position);
			if (id is int[] le) w[2] = Enumerable.Repeat(lp, le.Length).ToArray();
			else w[2] = new long[1] { lp };
			GRSendCr(m, vr, w);
		}
		if (t is int[] y && y.Length > 0)
		{
			int firstType = 0;
			try { firstType = ((int[])w[1])[0]; } catch { }
			bool je = firstType == 1551901997;
			Player targetPlayer = GRPlayerFromActor(y[0]);
			VRRig targetRig = (targetPlayer != null) ? GRRigFromPlayer(targetPlayer) : null;
			Vector3 targetPos = ((Object)(object)targetRig != (Object)null) ? ((Component)targetRig).transform.position : Vector3.zero;
			long lp2 = BitPackUtils.PackWorldPosForNetwork(targetPos - new Vector3(0f, je ? 4f : 0f, 0f));
			if (id is int[] le2) w[2] = Enumerable.Repeat(lp2, le2.Length).ToArray();
			else w[2] = new long[1] { lp2 };
			GRSendCr(m, y, w);

			// Malachi async follow-up (best-effort main-thread via coroutine)
			if ((Object)(object)Instance != (Object)null)
			{
				Instance.StartCoroutine(GRCrPostRoutine(id));
			}
		}

		// Throttle flush to reduce local overhead under spam
		if ((Object)(object)Instance != (Object)null && !_crashRunning)
		{
			Instance.StartCoroutine(GRRPCFlushRoutine(1.3f));
		}
	}

	private static IEnumerator GRCrPostRoutine(object id)
	{
		// Skip expensive full-scene scan during rapid crash fire to avoid local lag
		if (_crashRunning) yield break;
		yield return new WaitForSeconds(0.3f);
		try
		{
			// lightweight: no FindObjectsOfType scan
		}
		catch { }
	}

	private static object[] GRHalf(object[] o, bool f = true)
	{
		if (o == null || o.Length == 0) return o;
		int[] first = o[0] as int[];
		if (first == null || first.Length < 45) return o;
		return o.Select((object x) =>
		{
			Array array = (Array)x;
			Array array2 = Array.CreateInstance(array.GetType().GetElementType(), array.Length / 2);
			if (f) Array.Copy(array, array2, array2.Length);
			else Array.Copy(array, array.Length - array2.Length, array2, 0, array2.Length);
			return (object)array2;
		}).ToArray();
	}

	private static void GRSpeci(PhotonView pv, string m, int[] a, object[] p)
	{
		try
		{
			ExitGames.Client.Photon.Hashtable val = new ExitGames.Client.Photon.Hashtable();
			val.Add((byte)0, (object)pv.ViewID);
			val.Add((byte)2, (object)PhotonNetwork.ServerTimestamp);
			val.Add((byte)3, (object)m);
			val.Add((byte)4, (object)p);
			try { val.Add((byte)5, (object)(byte)PhotonNetwork.PhotonServerSettings.RpcList.IndexOf(m)); } catch { }
			RaiseEventOptions val2 = new RaiseEventOptions { TargetActors = a };
			SendOptions val3 = new SendOptions { Reliability = true, Encrypt = false };
			try { val3.DeliveryMode = (ExitGames.Client.Photon.DeliveryMode)3; } catch { }
			PhotonNetwork.NetworkingClient.LoadBalancingPeer.OpRaiseEvent((byte)200, (object)val, val2, val3);
		}
		catch { }
	}

	private static void GRSendCr(GameEntityManager g, object t, object[] o)
	{
		if ((Object)(object)g == (Object)null || o == null || o.Length == 0) return;
		PhotonView p = g.photonView;
		if ((Object)(object)p == (Object)null) return;

		int[] ids = o[0] as int[];
		bool b2 = ids != null && ids.Length > 45;
		const string c2 = "CreateItemRPC";
		object[] f = GRHalf(o);
		object[] s = GRHalf(o, f: false);

		if (t is RpcTarget e)
		{
			p.RPC(c2, e, f);
			if (b2) p.RPC(c2, e, s);
		}
		Player k = t as Player;
		if (k != null)
		{
			p.RPC(c2, k, f);
			if (b2) p.RPC(c2, k, s);
		}
		if (t is int[] y && y.Length > 0)
		{
			GRSpeci(p, c2, y, f);
			if (b2) GRSpeci(p, c2, y, s);
		}
		VRRig r = t as VRRig;
		if (r != null)
		{
			VRRig own = GROwnRig();
			if ((Object)(object)r == (Object)(object)own)
			{
				p.RPC(c2, (RpcTarget)1, f);
				if (b2) p.RPC(c2, (RpcTarget)1, s);
			}
			else if (r.Creator != null)
			{
				Player rp = r.Creator.GetPlayerRef();
				if (rp != null)
				{
					p.RPC(c2, rp, f);
					if (b2) p.RPC(c2, rp, s);
				}
			}
		}
	}

	private static int GRCreateNetId(int n, GameEntityManager g, bool s = true)
	{
		try
		{
			Traverse tr = Traverse.Create(g).Field("nextNetId");
			int num = tr.GetValue<int>();
			if (s)
			{
				num += n;
				tr.SetValue(num);
			}
			return num;
		}
		catch { return 0; }
	}

	private static void GROw(GameEntityManager g)
	{
		try
		{
			if ((Object)(object)g == (Object)null) return;
			if (GRHasZoneAuthority(g)) return;

			if (PhotonNetwork.LocalPlayer != null && PhotonNetwork.LocalPlayer.IsMasterClient)
			{
				// Immediate + retries; master path should seize map/entity authority fast
				try { if (g.guard != null) g.guard.RequestOwnershipImmediatelyWithGuaranteedAuthority(); } catch { }
				try { if (g.photonView != null) g.photonView.RequestOwnership(); } catch { }
				if ((Object)(object)Instance != (Object)null)
				{
					Instance.StartCoroutine(GROwRetryRoutine(g, 0.08f, 20, masterFast: true));
				}
			}
			else
			{
				try { if (g.guard != null) g.guard.RequestOwnership((Action)null, (Action)null); } catch { }
				try { if (g.photonView != null) g.photonView.RequestOwnership(); } catch { }
				if ((Object)(object)Instance != (Object)null)
				{
					Instance.StartCoroutine(GROwRetryRoutine(g, 0.25f, 12, masterFast: false));
				}
			}
		}
		catch { }
	}

	private static IEnumerator GROwRetryRoutine(GameEntityManager g, float interval, int attempts, bool masterFast)
	{
		for (int i = 0; i < attempts; i++)
		{
			try
			{
				if ((Object)(object)g == (Object)null) yield break;
				if (GRHasTrueZoneOwnership(g))
				{
					GRNotifyZoneControlIfGained(g);
					yield break;
				}
				if (masterFast && PhotonNetwork.LocalPlayer != null && PhotonNetwork.LocalPlayer.IsMasterClient)
				{
					try { if (g.guard != null) g.guard.RequestOwnershipImmediatelyWithGuaranteedAuthority(); } catch { }
				}
				else
				{
					try { if (g.guard != null) g.guard.RequestOwnership((Action)null, (Action)null); } catch { }
				}
				try { if (g.photonView != null && !g.photonView.IsMine) g.photonView.RequestOwnership(); } catch { }
				if (GRHasTrueZoneOwnership(g))
				{
					GRNotifyZoneControlIfGained(g);
					yield break;
				}
			}
			catch { }
			yield return new WaitForSeconds(interval);
		}
	}

	private static IEnumerator GRRPCFlushRoutine(float delay)
	{
		yield return new WaitForSeconds(delay);
		GRRPCFlush();
	}

	private static void GRRPCFlush()
	{
		try
		{
			if (!PhotonNetwork.InRoom) return;
			try
			{
				if (MonkeAgent.instance != null)
				{
					int i = int.MaxValue;
					MonkeAgent.instance.rpcCallLimit = i;
					MonkeAgent.instance.rpcErrorMax = i;
					MonkeAgent.instance.logErrorMax = i;
				}
			}
			catch { }
			try { if (PhotonNetwork.LocalPlayer != null) PhotonNetwork.OpCleanActorRpcBuffer(PhotonNetwork.LocalPlayer.ActorNumber); } catch { }
			try
			{
				GorillaTagger tagger = GorillaTagger.Instance;
				if ((Object)(object)tagger != (Object)null && (Object)(object)tagger.myVRRig != (Object)null)
				{
					PhotonNetwork.RemoveBufferedRPCs(tagger.myVRRig.ViewID, null, null);
				}
			}
			catch { }
			try { PhotonNetwork.SendAllOutgoingCommands(); } catch { }
		}
		catch { }
	}
}
