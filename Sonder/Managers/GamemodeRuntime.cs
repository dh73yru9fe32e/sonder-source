using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GorillaGameModes;
using GorillaLocomotion;
using GorillaTagScripts;
using Photon.Pun;
using Photon.Realtime;
using Sonder.Core;
using Sonder.Menu;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Sonder.Managers;

public class GamemodeRuntime : MonoBehaviour
{
	public static GamemodeRuntime Instance { get; private set; }

	public bool TagGun { get; set; }
	public bool TagAllExceptGun { get; set; }
	public bool TagAura { get; set; }
	public bool UnTagGunM { get; set; }
	public bool UnTagAuraM { get; set; }
	public bool UnTagOthersM { get; set; }
	public bool MatGunM { get; set; }
	public bool MatAuraM { get; set; }
	public bool FlickTagGun { get; set; }
	public bool GuardianFlingGun { get; set; }
	public bool GuardianSpazGun { get; set; }
	public bool GuardianOrbitGun { get; set; }
	public bool GuardianToPointAll { get; set; }
	public bool PaintBrawlHardAimbot { get; set; }
	public bool PaintBrawlSoftAimbot { get; set; }
	public bool NoPaintBrawlCooldownM { get; set; }
	public bool SetOrangeTeamGunM { get; set; }
	public bool SetBlueTeamGunM { get; set; }
	public bool SetNoTeamGunM { get; set; }
	public bool SetStunnedGunM { get; set; }
	public bool SetHitGunM { get; set; }
	public bool GodModeGunM { get; set; }
	public bool UnGodModeGunM { get; set; }

	private float _matTimer;
	private int _matState;
	private float _nextAction;
	private bool _guardianNeedRestore;

	private void Awake() { Instance = this; }

	private void OnDestroy()
	{
		if ((Object)(object)Instance == (Object)(object)this) Instance = null;
		RestoreGuardianRig();
		RestorePaintCooldown();
	}

	private void Update()
	{
		try
		{
			TickGunsAndAuras();
			TickPaintBrawl();
			TickGuardianRestore();
		}
		catch { }
	}

	private static bool InRoom() => PhotonNetwork.InRoom && NetworkSystem.Instance != null && NetworkSystem.Instance.InRoom;
	private static NetPlayer LocalPlayer() => NetworkSystem.Instance != null ? NetworkSystem.Instance.LocalPlayer : null;
	private static bool IsMaster() => InRoom() && PhotonNetwork.LocalPlayer != null && PhotonNetwork.LocalPlayer.IsMasterClient;
	private static GorillaGameManager GM() => GorillaGameManager.instance;

	private static bool IsGameMode(GameModeType t)
	{
		if (!InRoom() || (Object)(object)GM() == (Object)null) return false;
		try { return GM().GameType() == t; } catch { return false; }
	}

	private static IEnumerable<VRRig> AllRigs(bool includeSelf)
	{
		var rigs = VRRigCache.ActiveRigs;
		if (rigs == null) yield break;
		for (int i = 0; i < rigs.Count; i++)
		{
			VRRig r = rigs[i];
			if ((Object)(object)r == (Object)null || r.isOfflineVRRig) continue;
			if (!includeSelf && r.isLocal) continue;
			yield return r;
		}
	}

	private static IEnumerable<VRRig> OtherRigs() => AllRigs(false);
	private static NetPlayer NetOf(VRRig r) { try { return r != null ? r.Creator : null; } catch { return null; } }

	private static bool ActualTagged(VRRig rig)
	{
		try
		{
			if ((Object)(object)rig == (Object)null || (Object)(object)GM() == (Object)null) return false;
			NetPlayer p = NetOf(rig);
			if (p == null) return false;
			var gm = GM();
			if (gm is GorillaFreezeTagManager freeze && freeze.currentInfected != null) return freeze.currentInfected.Contains(p);
			if (gm is GorillaPaintbrawlManager paint) { try { return paint.playerLives[p.ActorNumber] <= 0; } catch { return false; } }
			if (gm is GorillaHuntManager hunt && hunt.currentHunted != null) return hunt.currentHunted.Contains(p);
			if (gm is GorillaTagManager tag && tag.currentInfected != null) return tag.currentInfected.Contains(p);
		}
		catch { }
		return false;
	}

	private static void Tagged(NetPlayer p)
	{
		if (p == null || !InRoom() || (Object)(object)GM() == (Object)null) return;
		try
		{
			var gm = GM();
			if (gm is GorillaFreezeTagManager freeze) { if (freeze.currentInfected == null || !freeze.currentInfected.Contains(p)) freeze.AddInfectedPlayer(p, true); return; }
			if (gm is GorillaPaintbrawlManager paint) { try { paint.playerLives[p.ActorNumber] = 0; } catch { } return; }
			if (gm is GorillaHuntManager hunt) { if (hunt.currentHunted == null || !hunt.currentHunted.Contains(p)) hunt.currentHunted.Add(p); return; }
			if (gm is GorillaTagManager tag) { if (tag.currentInfected == null || !tag.currentInfected.Contains(p)) tag.AddInfectedPlayer(p, true); return; }
		}
		catch { }
	}

	private static void UnTag(NetPlayer p)
	{
		if (p == null || !InRoom() || (Object)(object)GM() == (Object)null) return;
		try
		{
			var gm = GM();
			if (gm is GorillaFreezeTagManager freeze) { freeze.currentInfected?.Remove(p); return; }
			if (gm is GorillaPaintbrawlManager paint) { try { paint.playerLives[p.ActorNumber] = 3; } catch { } return; }
			if (gm is GorillaHuntManager hunt) { hunt.currentHunted?.Remove(p); return; }
			if (gm is GorillaTagManager tag) { tag.currentInfected?.Remove(p); return; }
		}
		catch { }
	}

	private static bool IsPaintbrawl() => IsGameMode((GameModeType)3);
	private static bool IsGuardianMode() => IsGameMode((GameModeType)8);
	private static bool IsHunt() => IsGameMode((GameModeType)2);

	private static bool CanHit(VRRig r)
	{
		try
		{
			if ((Object)(object)r == (Object)null || (Object)(object)GM() == (Object)null) return false;
			if (ActualTagged(r)) return false;
			if (GM() is GorillaPaintbrawlManager pb)
			{
				try { if (pb.teamBattle && r.Creator != null && LocalPlayer() != null && pb.OnSameTeam(r.Creator, LocalPlayer())) return false; } catch { }
			}
			return true;
		}
		catch { return false; }
	}

	private static bool PaintAction(Action s)
	{
		if (!IsMaster()) { NotificationRuntime.Show("MASTER ONLY"); return false; }
		if (!IsPaintbrawl()) { NotificationRuntime.Show("PAINTBRAWL ONLY"); return false; }
		try { s(); return true; } catch { return false; }
	}

	private static void ChangeTeam(VRRig r, int s)
	{
		if ((Object)(object)r == (Object)null) return;
		if (!IsMaster()) { NotificationRuntime.Show("MASTER ONLY"); return; }
		if (!IsPaintbrawl()) { NotificationRuntime.Show("PAINTBRAWL ONLY"); return; }
		try
		{
			var val = GM() as GorillaPaintbrawlManager;
			if (val == null || r.Creator == null) return;
			int actor = r.Creator.ActorNumber;
			if (s == 3 || s == 999) { try { val.playerLives[actor] = s; } catch { } }
			else
			{
				float now = Time.time;
				if (s == 67) { try { val.playerStunTimes[actor] = now + 0.3f; } catch { } }
				else if (s == 69) { try { val.playerHitTimes[actor] = now - 2.78f; } catch { } }
				else
				{
					try
					{
						var dict = val.playerStatusDict as IDictionary;
						if (dict != null)
						{
							Type valType = val.playerStatusDict.GetType().GetGenericArguments()[1];
							dict[actor] = Enum.ToObject(valType, s);
						}
					}
					catch { }
				}
			}
		}
		catch { }
	}

	private static void ReportTag(VRRig rig)
	{
		try { if (rig != null && rig.Creator != null) GorillaGameModes.GameMode.ReportTag(rig.Creator); } catch { }
	}

	private static Vector3 RandomV(float d) => UnityEngine.Random.insideUnitSphere * d;

	public static void TagSelf()
	{
		if (!InRoom()) return;
		VRRig me = VRRig.LocalRig;
		if ((Object)(object)me == (Object)null) return;
		if (IsMaster()) { Tagged(LocalPlayer()); return; }
		if (IsGuardianMode()) { Instance?.TryGetGuardian(); return; }
		try
		{
			VRRig target = null;
			if (IsHunt() && GM() is GorillaHuntManager hm)
			{
				var t = hm.GetTargetOf(LocalPlayer());
				if (t != null) target = RigFromNet(t);
			}
			if ((Object)(object)target == (Object)null)
			{
				foreach (var r in OtherRigs()) { if (ActualTagged(r)) { target = r; break; } }
			}
			if ((Object)(object)target != (Object)null)
				((Component)me).transform.position = target.rightHandTransform != null ? target.rightHandTransform.position : ((Component)target).transform.position;
		}
		catch { }
	}

	public static void TagAll()
	{
		if (!InRoom()) return;
		if (IsGuardianMode()) { NotificationRuntime.Show("GUARDIAN MODE"); return; }
		if (IsHunt() && GM() is GorillaHuntManager hm)
		{
			var t = hm.GetTargetOf(LocalPlayer());
			if (t == null) return;
			if (IsMaster()) Tagged(t); else { var rig = RigFromNet(t); if (rig != null) ReportTag(rig); }
			return;
		}
		if (IsMaster())
		{
			foreach (var r in OtherRigs()) { var p = NetOf(r); if (p != null) Tagged(p); }
			return;
		}
		VRRig me = VRRig.LocalRig; if ((Object)(object)me == (Object)null) return;
		foreach (var r in OtherRigs().Where(CanHit).OrderBy(r => Vector3.Distance(((Component)r).transform.position, ((Component)me).transform.position)))
		{
			((Component)me).transform.position = ((Component)r).transform.position + RandomV(0.2f);
			ReportTag(r);
			break;
		}
	}

	public static void TagAllExceptGunAction(VRRig except)
	{
		if (!InRoom() || (Object)(object)except == (Object)null) return;
		if (IsGuardianMode() || IsHunt()) { NotificationRuntime.Show("UNSUPPORTED"); return; }
		if (IsMaster())
		{
			foreach (var r in OtherRigs()) { if ((Object)(object)r == (Object)(object)except) continue; var p = NetOf(r); if (p != null) Tagged(p); }
			return;
		}
		VRRig me = VRRig.LocalRig; if ((Object)(object)me == (Object)null) return;
		foreach (var r in OtherRigs().Where(x => (Object)(object)x != (Object)(object)except && CanHit(x)).OrderBy(r => Vector3.Distance(((Component)r).transform.position, ((Component)me).transform.position)))
		{
			((Component)me).transform.position = ((Component)r).transform.position + RandomV(0.2f);
			ReportTag(r);
			break;
		}
	}

	public static void TagOthersM()
	{
		if (!IsMaster()) { NotificationRuntime.Show("MASTER ONLY"); return; }
		foreach (var r in OtherRigs()) { var p = NetOf(r); if (p != null) Tagged(p); }
	}

	public static void UnTagSelfM() { if (!IsMaster()) { NotificationRuntime.Show("MASTER ONLY"); return; } UnTag(LocalPlayer()); }
	public static void UnTagAllM() { if (!IsMaster()) { NotificationRuntime.Show("MASTER ONLY"); return; } foreach (var r in AllRigs(true)) { var p = NetOf(r); if (p != null) UnTag(p); } }
	public static void UnTagOthersMAction() { if (!IsMaster()) { NotificationRuntime.Show("MASTER ONLY"); return; } foreach (var r in OtherRigs()) { var p = NetOf(r); if (p != null) UnTag(p); } }

	private void DoMat(NetPlayer p)
	{
		if (p == null) return;
		_matTimer -= Time.deltaTime;
		if (_matTimer > 0f) return;
		_matTimer = 0.08f;
		_matState++;
		if ((_matState & 1) == 1) Tagged(p); else UnTag(p);
	}

	public static void MatSelfM() { if (!IsMaster()) { NotificationRuntime.Show("MASTER ONLY"); return; } Instance?.DoMat(LocalPlayer()); }
	public static void MatAllV1M() { if (!IsMaster()) { NotificationRuntime.Show("MASTER ONLY"); return; } foreach (var r in OtherRigs()) Instance?.DoMat(NetOf(r)); }
	public static void MatAllV2M() { if (!IsMaster()) { NotificationRuntime.Show("MASTER ONLY"); return; } foreach (var r in OtherRigs()) Instance?.DoMat(NetOf(r)); }
	public static void GiveMatAllM() { if (!IsMaster()) { NotificationRuntime.Show("MASTER ONLY"); return; } foreach (var r in OtherRigs()) { var p = NetOf(r); if (p != null) { Tagged(p); UnTag(p); } } }

	private static VRRig RigFromNet(NetPlayer p)
	{
		if (p == null) return null;
		foreach (var r in AllRigs(true)) { try { if (r.Creator != null && r.Creator.ActorNumber == p.ActorNumber) return r; } catch { } }
		return null;
	}

	private void TryGetGuardian()
	{
		if (!IsGuardianMode()) { NotificationRuntime.Show("GUARDIAN MODE"); return; }
		try
		{
			var idols = Object.FindObjectsOfType<TappableGuardianIdol>();
			if (idols == null || idols.Length == 0) return;
			TappableGuardianIdol best = null; float bd = float.MaxValue;
			VRRig me = VRRig.LocalRig; if ((Object)(object)me == (Object)null) return;
			foreach (var t in idols)
			{
				if (t == null || !((Component)t).gameObject.activeSelf) continue;
				float d = Vector3.Distance(((Component)t).transform.position, ((Component)me).transform.position);
				if (d < bd) { bd = d; best = t; }
			}
			if (best == null) return;
			if (bd > 1.2f)
			{
				_guardianNeedRestore = true;
				((Behaviour)me).enabled = false;
				((Component)me).transform.position = ((Component)best).transform.position;
				return;
			}
			try { var pv = ((Component)best).GetComponent<PhotonView>(); if (pv != null) pv.RPC("OnTapRPC", RpcTarget.All, 0f); } catch { }
		}
		catch { }
	}

	public static void GetGuardian() { Instance?.TryGetGuardian(); }
	public static void UndoGuardian() { Instance?.RestoreGuardianRig(); }
	private void RestoreGuardianRig()
	{
		try { VRRig me = VRRig.LocalRig; if ((Object)(object)me != (Object)null) ((Behaviour)me).enabled = true; } catch { }
		_guardianNeedRestore = false;
	}
	private void TickGuardianRestore()
	{
		if (_guardianNeedRestore && IsGuardianMode() && ActualTagged(VRRig.LocalRig)) RestoreGuardianRig();
	}

	private static void GuardMeth(Action s)
	{
		if (!IsGuardianMode()) { NotificationRuntime.Show("GUARDIAN MODE"); return; }
		if (!ActualTagged(VRRig.LocalRig)) { NotificationRuntime.Show("BE GUARDIAN"); return; }
		try { s(); } catch { }
	}

	private static void Throw(VRRig rig, Vector3 vel)
	{
		if ((Object)(object)rig == (Object)null || rig.Creator == null) return;
		try
		{
			var pv = ((Component)rig).GetComponent<PhotonView>();
			if (pv == null) pv = PhotonView.Get(((Component)rig).gameObject);
			if (pv != null)
			{
				Player target = null;
				try { target = rig.Creator.GetPlayerRef(); } catch { }
				if (target == null) try { target = PhotonNetwork.CurrentRoom != null ? PhotonNetwork.CurrentRoom.GetPlayer(rig.Creator.ActorNumber) : null; } catch { }
				if (target != null)
				{
					pv.RPC("GrabbedByPlayer", target, true, false, false);
					pv.RPC("DroppedByPlayer", target, vel);
				}
			}
		}
		catch { }
	}

	public static void GuardianFlingAll() => GuardMeth(() => { foreach (var r in OtherRigs()) Throw(r, new Vector3(0f, 5555f, 0f)); });
	public static void GuardianSpazAll() => GuardMeth(() => { foreach (var r in OtherRigs()) Throw(r, RandomV(1000f)); });
	public static void GuardianOrbitAll() => GuardMeth(() => { int i=0; foreach (var r in OtherRigs()) { float a = Time.time*3f + i*0.7f; Throw(r, new Vector3(Mathf.Cos(a), 0.2f, Mathf.Sin(a))*50f); i++; } });
	public static void GiveGuardianFlingAll() => GuardMeth(() => { foreach (var r in OtherRigs()) if (!ActualTagged(r)) Throw(r, new Vector3(0f, 5555f, 0f)); });
	public static void GiveGuardianSpazAll() => GuardMeth(() => { foreach (var r in OtherRigs()) if (!ActualTagged(r)) Throw(r, RandomV(1000f)); });
	public static void GiveGuardianOrbitAll() => GuardMeth(() => { int i=0; foreach (var r in OtherRigs()) if (!ActualTagged(r)) { float a = Time.time*3f + i*0.7f; Throw(r, new Vector3(Mathf.Cos(a), 0.2f, Mathf.Sin(a))*50f); i++; } });

	public static void GuardianToPointAllFire(Vector3 point)
	{
		GuardMeth(() => { foreach (var r in OtherRigs()) Throw(r, (point - ((Component)r).transform.position) * 8f); });
	}

	public static void NoPaintBrawlCooldownToggle()
	{
		PaintAction(() =>
		{
			var val = GM() as GorillaPaintbrawlManager; if (val == null) return;
			bool on = !(Mathf.Approximately(val.hitCooldown, 0f));
			val.hitCooldown = on ? 0f : 3f;
			val.stunGracePeriod = on ? 0f : 2f;
			NotificationRuntime.Show(on ? "COOLDOWN OFF" : "COOLDOWN ON");
		});
	}

	public static void PaintBrawlTeamsM() => PaintAction(() => { NotificationRuntime.Show("TEAM MODE"); });
	public static void RandomizeTeamsM()
	{
		PaintAction(() =>
		{
			var rng = new System.Random();
			foreach (var r in AllRigs(true)) { try { ChangeTeam(r, rng.Next(0,2)); } catch { } }
		});
	}

	public static void SetTeamAllM(int team) { foreach (var r in AllRigs(true)) ChangeTeam(r, team); }
	public static void SetTeamSelfM(int team) { ChangeTeam(VRRig.LocalRig, team); }

	public static void GodModeAllM(bool on) { foreach (var r in AllRigs(true)) ChangeTeam(r, on ? 999 : 3); }
	public static void GodModeSelfM(bool on) { ChangeTeam(VRRig.LocalRig, on ? 999 : 3); }

	private void TickGunsAndAuras()
	{
		var gr = GunRuntime.Instance;
		VRRig locked = ((Object)(object)gr != (Object)null && gr.gunLocked) ? gr.lockTarget : null;

		if (locked != null)
		{
			if (TagGun) { var p = NetOf(locked); if (p != null) { if (IsMaster()) Tagged(p); else ReportTag(locked); } }
			if (TagAllExceptGun) { TagAllExceptGunAction(locked); }
			if (FlickTagGun) { var p = NetOf(locked); if (p != null) { if (IsMaster()) Tagged(p); else ReportTag(locked); } }
			if (UnTagGunM && IsMaster()) { var p = NetOf(locked); if (p != null) UnTag(p); }
			if (MatGunM && IsMaster()) { DoMat(NetOf(locked)); }
			if (GuardianFlingGun) GuardMeth(() => Throw(locked, new Vector3(0f,5555f,0f)));
			if (GuardianSpazGun) GuardMeth(() => Throw(locked, RandomV(1000f)));
			if (GuardianOrbitGun) GuardMeth(() => { float a=Time.time*3f; Throw(locked, new Vector3(Mathf.Cos(a),0.2f,Mathf.Sin(a))*50f); });
			if (SetOrangeTeamGunM) ChangeTeam(locked, 1);
			if (SetBlueTeamGunM) ChangeTeam(locked, 0);
			if (SetNoTeamGunM) ChangeTeam(locked, 2);
			if (SetStunnedGunM) ChangeTeam(locked, 67);
			if (SetHitGunM) ChangeTeam(locked, 69);
			if (GodModeGunM) ChangeTeam(locked, 999);
			if (UnGodModeGunM) ChangeTeam(locked, 3);
		}

		if (TagAura)
		{
			VRRig me = VRRig.LocalRig; if ((Object)(object)me != (Object)null)
			{
				VRRig near = null; float bd=3.5f;
				foreach (var r in OtherRigs()) { float d=Vector3.Distance(((Component)r).transform.position, ((Component)me).transform.position); if (d<bd && (IsMaster() || CanHit(r))) { bd=d; near=r; } }
				if (near!=null) { if (IsMaster()) Tagged(NetOf(near)); else { ((Component)me).transform.position=((Component)near).transform.position; ReportTag(near);} }
			}
		}
		if (UnTagAuraM && IsMaster()) { foreach (var r in OtherRigs()) { if (ActualTagged(r)) { UnTag(NetOf(r)); break; } } }
		if (UnTagOthersM && IsMaster()) { foreach (var r in OtherRigs()) { if (ActualTagged(r)) { UnTag(NetOf(r)); break; } } }
		if (MatAuraM && IsMaster()) { foreach (var r in OtherRigs()) { DoMat(NetOf(r)); break; } }

		if (GuardianToPointAll)
		{
			if ((Object)(object)gr != (Object)null && gr.gunLocked && locked != null)
				GuardianToPointAllFire(((Component)locked).transform.position);
		}
	}

	private void TickPaintBrawl()
	{
		if (NoPaintBrawlCooldownM && IsPaintbrawl() && IsMaster())
		{
			var val = GM() as GorillaPaintbrawlManager;
			if (val != null) { val.hitCooldown = 0f; val.stunGracePeriod = 0f; }
		}
	}

	private void RestorePaintCooldown()
	{
		try { if (GM() is GorillaPaintbrawlManager val) { val.hitCooldown = 3f; val.stunGracePeriod = 2f; } } catch { }
	}
}
