using System;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using Sonder.Core;
using Sonder.Menu;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Sonder.Managers;

public class VisualRuntime : MonoBehaviour
{
	private sealed class RigVis
	{
		public VRRig Rig;
		public readonly List<GameObject> Objects = new List<GameObject>();
		public readonly List<LineRenderer> Lines = new List<LineRenderer>();
		public readonly List<TextMesh> Labels = new List<TextMesh>();
		public readonly List<Renderer> Renderers = new List<Renderer>();
		public readonly List<Material> Materials = new List<Material>();
		public Material[] SavedMainMats;
		public Shader[] SavedMainShaders;
		public Color[] SavedMainColors;
	}

	private readonly Dictionary<int, RigVis> _rigs = new Dictionary<int, RigVis>();
	private readonly List<int> _stale = new List<int>();
	private readonly Dictionary<int, Vector3> _lastPos = new Dictionary<int, Vector3>();
	private readonly Dictionary<int, float> _lastPosTime = new Dictionary<int, float>();
	private readonly Dictionary<int, Queue<Vector3>> _gripHistory = new Dictionary<int, Queue<Vector3>>();

	private Material _lineMat;
	private Material _thickLineMat;
	private Material _thinLineMat;
	private Material _pulseLineMat;
	private Material _handMat;
	private Material _skyMaterial;
	private Material _trailMaterial;
	private Material _oldSkybox;
	private bool _skyApplied;
	private bool _nightApplied;
	private bool _fpsSaved;
	private float _fpsSize;
	private TextAlignmentOptions _fpsAlignment;
	private Color _fpsColor;
	private AmbientMode _ambientMode;
	private Color _ambientLight;
	private bool _fog;
	private Color _fogColor;
	private FogMode _fogMode;
	private float _fogDensity;

	public static VisualRuntime Instance { get; private set; }

	// Existing
	public bool ESP { get; set; }
	public bool Tracers { get; set; }
	public bool SkyReplacement { get; set; }
	public bool EnlargeFps { get; set; }
	public bool Outline { get; set; }
	public bool NightVision { get; set; }
	public bool Trails { get; set; }

	// New requested
	public bool ArrowPointerESP { get; set; }
	public bool BeaconESP { get; set; }
	public bool BoneESP { get; set; }
	public bool CapsuleESP { get; set; }
	public bool Chams { get; set; }
	public bool CylinderESP { get; set; }
	public bool DiamondESP { get; set; }
	public bool DistanceTracers { get; set; }
	public bool DoubleHandTracers { get; set; }
	public bool FloorMarkerESP { get; set; }
	public bool FullPlayerInfo { get; set; }
	public bool GripIndicator { get; set; }
	public bool GripTracker { get; set; }
	public bool HandComets { get; set; }
	public bool HandESP { get; set; }
	public bool HeadSphereESP { get; set; }
	public bool HeadTracers { get; set; }
	public bool LightningHands { get; set; }
	public bool ModderTracers { get; set; }
	public bool NeonOutline { get; set; }
	public bool PulseChams { get; set; }
	public bool PulseTracers { get; set; }
	public bool RingESP { get; set; }
	public bool ShadowClone { get; set; }
	public bool ShowDistance { get; set; }
	public bool ShowJoinDateAll { get; set; }
	public bool ShowPlatform { get; set; }
	public bool ShowVelocity { get; set; }
	public bool SkeletonESPV2 { get; set; }
	public bool SparklerHands { get; set; }
	public bool SphereESP { get; set; }
	public bool ThickTracers { get; set; }
	public bool ThinESP { get; set; }

	private void Awake() { Instance = this; }

	private void OnDestroy()
	{
		if ((Object)(object)Instance == (Object)(object)this) Instance = null;
		ClearAll();
		RestoreSky();
		RestoreNightVision();
		RestoreFps();
		DestroyMat(ref _lineMat);
		DestroyMat(ref _thickLineMat);
		DestroyMat(ref _thinLineMat);
		DestroyMat(ref _pulseLineMat);
		DestroyMat(ref _handMat);
		DestroyMat(ref _trailMaterial);
	}

	private static void DestroyMat(ref Material m)
	{
		if ((Object)(object)m != (Object)null) { Object.Destroy((Object)(object)m); m = null; }
	}

	private void Update()
	{
		ApplyFps();
		ApplySky();
		ApplyNightVision();
		ApplyTrails();
		UpdateRigVisuals();
	}

	private void UpdateRigVisuals()
	{
		List<VRRig> rigs = ExtraUtil.RemoteRigs();
		Transform head = ExtraUtil.Head();
		Vector3 origin = (head != null) ? head.position : ((Component)this).transform.position;
		Vector3 leftOrigin = origin;
		Vector3 rightOrigin = origin;
		VRRig local = VRRig.LocalRig;
		if ((Object)(object)local != (Object)null)
		{
			if ((Object)(object)local.leftHandTransform != (Object)null) leftOrigin = local.leftHandTransform.position;
			if ((Object)(object)local.rightHandTransform != (Object)null) rightOrigin = local.rightHandTransform.position;
		}

		bool any = ESP || Tracers || ArrowPointerESP || BeaconESP || BoneESP || CapsuleESP || Chams || CylinderESP || DiamondESP ||
			DistanceTracers || DoubleHandTracers || FloorMarkerESP || FullPlayerInfo || GripIndicator || GripTracker ||
			HandComets || HandESP || HeadSphereESP || HeadTracers || LightningHands || ModderTracers || NeonOutline ||
			PulseChams || PulseTracers || RingESP || ShadowClone || ShowDistance || ShowJoinDateAll || ShowPlatform ||
			ShowVelocity || SkeletonESPV2 || SparklerHands || SphereESP || ThickTracers || ThinESP || Outline;

		HashSet<int> live = new HashSet<int>();
		for (int i = 0; i < rigs.Count; i++)
		{
			VRRig rig = rigs[i];
			if ((Object)(object)rig == (Object)null) continue;
			int id = ((Component)rig).GetInstanceID();
			live.Add(id);
			if (!any)
			{
				if (_rigs.ContainsKey(id)) ClearRig(id);
				continue;
			}
			RigVis vis = GetOrCreateRigVis(rig, id);
			UpdateRig(rig, vis, origin, leftOrigin, rightOrigin);
		}

		_stale.Clear();
		foreach (var kv in _rigs)
		{
			if (!live.Contains(kv.Key) || (Object)(object)kv.Value.Rig == (Object)null) _stale.Add(kv.Key);
		}
		for (int i = 0; i < _stale.Count; i++) ClearRig(_stale[i]);
	}

	private RigVis GetOrCreateRigVis(VRRig rig, int id)
	{
		if (!_rigs.TryGetValue(id, out RigVis vis))
		{
			vis = new RigVis { Rig = rig };
			_rigs[id] = vis;
		}
		vis.Rig = rig;
		return vis;
	}

	private void UpdateRig(VRRig rig, RigVis vis, Vector3 origin, Vector3 leftOrigin, Vector3 rightOrigin)
	{
		// Reset per-frame dynamic containers by rebuilding visuals for simplicity/consistency
		ClearRigObjects(vis, keepSavedMats: true);

		Vector3 headPos = ((Object)(object)rig.headMesh != (Object)null) ? rig.headMesh.transform.position : (((Component)rig).transform.position + Vector3.up * 0.6f);
		Vector3 bodyPos = ((Component)rig).transform.position;
		Color col = RigColor(rig);
		string nick = SafeNick(rig);
		float dist = Vector3.Distance(origin, headPos);
		string platform = PlatformOf(rig);
		Vector3 vel = VelocityOf(rig);
		string joinDate = JoinDateOf(rig);

		if (ESP || FullPlayerInfo || ShowDistance || ShowPlatform || ShowVelocity || ShowJoinDateAll || GripIndicator)
		{
			string text = "";
			if (ESP) text += nick;
			if (ShowDistance || ESP || FullPlayerInfo) text += (text.Length > 0 ? "\n" : "") + Mathf.RoundToInt(dist) + "m";
			if (ShowPlatform || FullPlayerInfo) text += (text.Length > 0 ? "\n" : "") + platform;
			if (ShowVelocity || FullPlayerInfo) text += (text.Length > 0 ? "\n" : "") + vel.magnitude.ToString("0.0") + " m/s";
			if (ShowJoinDateAll || FullPlayerInfo) text += (text.Length > 0 ? "\n" : "") + joinDate;
			if (GripIndicator || FullPlayerInfo) text += (text.Length > 0 ? "\n" : "") + "L:" + GripState(rig, true) + " R:" + GripState(rig, false);
			if (FullPlayerInfo) text += "\n" + ((rig.Creator != null) ? ("ID:" + rig.Creator.ActorNumber) : "ID:?");
			CreateLabel(vis, headPos + Vector3.up * 0.45f, text, col, FullPlayerInfo ? 34 : 30);
		}

		if (Tracers) CreateLine(vis, origin + Vector3.down * 0.1f, headPos, col, 0.018f, 0.005f, "tracer");
		if (DistanceTracers) CreateLine(vis, origin + Vector3.down * 0.08f, headPos, col, 0.012f, 0.004f, "dtracer");
		if (ThickTracers) CreateLine(vis, origin + Vector3.down * 0.12f, headPos, col, 0.045f, 0.02f, "thick");
		if (PulseTracers)
		{
			float p = 0.008f + Mathf.Abs(Mathf.Sin(Time.time * 6f + ((Component)rig).GetInstanceID() * 0.01f)) * 0.03f;
			CreateLine(vis, origin + Vector3.down * 0.1f, headPos, Pulse(col, 1f), p, p * 0.35f, "pulse");
		}
		if (HeadTracers) CreateLine(vis, origin + Vector3.up * 0.02f, headPos, col, 0.015f, 0.004f, "headtracer");
		if (DoubleHandTracers)
		{
			CreateLine(vis, leftOrigin, headPos, col, 0.012f, 0.004f, "lhtr");
			CreateLine(vis, rightOrigin, headPos, col, 0.012f, 0.004f, "rhtr");
		}
		if (ModderTracers && IsSus(rig))
		{
			CreateLine(vis, origin, headPos, Color.red, 0.03f, 0.01f, "modder");
			CreateLabel(vis, headPos + Vector3.up * 0.8f, "SUS", Color.red, 36);
		}

		if (BeaconESP) CreatePrimitive(vis, PrimitiveType.Cylinder, bodyPos + Vector3.up * 10f, new Vector3(0.06f, 10f, 0.06f), Quaternion.identity, col);
		if (SphereESP) CreatePrimitive(vis, PrimitiveType.Sphere, bodyPos + Vector3.up * 0.2f, Vector3.one * 0.35f, Quaternion.identity, col);
		if (HeadSphereESP) CreatePrimitive(vis, PrimitiveType.Sphere, headPos, Vector3.one * 0.28f, Quaternion.identity, col);
		if (CapsuleESP) CreatePrimitive(vis, PrimitiveType.Capsule, bodyPos + Vector3.up * 0.2f, new Vector3(0.35f, 0.45f, 0.35f), Quaternion.identity, col);
		if (CylinderESP) CreatePrimitive(vis, PrimitiveType.Cylinder, bodyPos + Vector3.up * 0.2f, new Vector3(0.35f, 0.45f, 0.35f), Quaternion.identity, col);
		if (RingESP) CreateRing(vis, bodyPos + Vector3.up * 0.05f, 0.45f, col);
		if (FloorMarkerESP) CreateFloorMarker(vis, rig, col);
		if (DiamondESP) CreateDiamond(vis, headPos + Vector3.up * 0.65f, col);
		if (ThinESP) CreatePrimitive(vis, PrimitiveType.Cube, bodyPos + Vector3.up * 0.2f, new Vector3(0.02f, 0.9f, 0.02f), Quaternion.identity, col);
		if (ArrowPointerESP) CreateArrow(vis, headPos + Vector3.up * 0.9f, origin, col);
		if (ShadowClone) CreateShadowClone(vis, rig, col);

		if (HandESP)
		{
			if ((Object)(object)rig.leftHandTransform != (Object)null) CreatePrimitive(vis, PrimitiveType.Sphere, rig.leftHandTransform.position, Vector3.one * 0.12f, Quaternion.identity, col);
			if ((Object)(object)rig.rightHandTransform != (Object)null) CreatePrimitive(vis, PrimitiveType.Sphere, rig.rightHandTransform.position, Vector3.one * 0.12f, Quaternion.identity, col);
		}
		if (SparklerHands)
		{
			DrawHandSparks(vis, rig, true, col);
			DrawHandSparks(vis, rig, false, col);
		}
		if (HandComets)
		{
			DrawHandComet(vis, rig, true, col);
			DrawHandComet(vis, rig, false, col);
		}
		if (LightningHands) DrawLightningHands(vis, rig, col);
		if (GripTracker) DrawGripTracker(vis, rig, col);

		if (BoneESP || SkeletonESPV2) DrawBones(vis, rig, col, SkeletonESPV2);

		if (Outline || NeonOutline) ApplyOutlineGlow(rig, vis, col, NeonOutline);
		else RestoreGlow(vis);
		if (Chams || PulseChams) ApplyChams(rig, vis, col, PulseChams);
		else RestoreChams(vis);
	}

	private static string SafeNick(VRRig rig)
	{
		try { return ExtraUtil.Nickname(rig); } catch { return "player"; }
	}

	private static Color RigColor(VRRig rig)
	{
		try
		{
			if ((Object)(object)rig.mainSkin != (Object)null && (Object)(object)((Renderer)rig.mainSkin).material != (Object)null)
			{
				Color c = ((Renderer)rig.mainSkin).material.color;
				c.a = 1f;
				if (c.r + c.g + c.b > 0.1f) return c;
			}
		}
		catch { }
		return SonderSettings.Accent;
	}

	private static string PlatformOf(VRRig rig)
	{
		try
		{
			if ((Object)(object)rig != (Object)null)
			{
				var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
				var t = rig.GetType();
				foreach (string name in new[] { "platform", "currentPlatform", "Platform", "platformString" })
				{
					var f = t.GetField(name, flags);
					if (f != null)
					{
						object v = f.GetValue(rig);
						if (v != null) return v.ToString();
					}
					var p = t.GetProperty(name, flags);
					if (p != null)
					{
						object v = p.GetValue(rig, null);
						if (v != null) return v.ToString();
					}
				}
				try
				{
					if (rig.Creator != null && rig.Creator.GetPlayerRef() != null)
					{
						var props = rig.Creator.GetPlayerRef().CustomProperties;
						if (props != null)
						{
							foreach (object k in props.Keys)
							{
								string ks = k != null ? k.ToString().ToLowerInvariant() : "";
								if (ks.Contains("platform"))
									return props[k] != null ? props[k].ToString() : "UNKNOWN";
							}
						}
					}
				}
				catch { }
			}
		}
		catch { }
		return "UNKNOWN";
	}

	private Vector3 VelocityOf(VRRig rig)
	{
		int id = ((Component)rig).GetInstanceID();
		Vector3 pos = ((Component)rig).transform.position;
		if (_lastPos.TryGetValue(id, out Vector3 prev) && _lastPosTime.TryGetValue(id, out float t))
		{
			float dt = Time.time - t;
			if (dt > 0.0001f)
			{
				Vector3 v = (pos - prev) / dt;
				_lastPos[id] = pos;
				_lastPosTime[id] = Time.time;
				return v;
			}
		}
		_lastPos[id] = pos;
		_lastPosTime[id] = Time.time;
		return Vector3.zero;
	}

	private static string JoinDateOf(VRRig rig)
	{
		try
		{
			if (rig.Creator != null)
			{
				var props = rig.Creator.GetPlayerRef()?.CustomProperties;
				if (props != null)
				{
					if (props.ContainsKey("joinDate")) return props["joinDate"].ToString();
					if (props.ContainsKey("CreationDate")) return props["CreationDate"].ToString();
					if (props.ContainsKey("created")) return props["created"].ToString();
				}
			}
		}
		catch { }
		return "join: n/a";
	}

	private static string GripState(VRRig rig, bool left)
	{
		try
		{
			bool grab = GetRigBool(rig, left ? new[] { "leftGrab", "leftGrabbed", "LeftGrab" } : new[] { "rightGrab", "rightGrabbed", "RightGrab" });
			bool trig = GetRigBool(rig, left ? new[] { "leftIndex", "leftTrigger", "LeftIndex" } : new[] { "rightIndex", "rightTrigger", "RightIndex" });
			if (grab) return "GRIP";
			if (trig) return "TRIG";
			return "OPEN";
		}
		catch { return "?"; }
	}

	private static bool GetRigBool(VRRig rig, string[] names)
	{
		try
		{
			var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
			var t = rig.GetType();
			for (int i = 0; i < names.Length; i++)
			{
				var f = t.GetField(names[i], flags);
				if (f != null)
				{
					object v = f.GetValue(rig);
					if (v is bool b) return b;
					if (v != null) { try { return Convert.ToInt32(v) != 0; } catch { } }
				}
				var p = t.GetProperty(names[i], flags);
				if (p != null)
				{
					object v = p.GetValue(rig, null);
					if (v is bool b) return b;
					if (v != null) { try { return Convert.ToInt32(v) != 0; } catch { } }
				}
			}
		}
		catch { }
		return false;
	}

	private static bool IsSus(VRRig rig)
	{
		try
		{
			string n = SafeNick(rig).ToLowerInvariant();
			if (n.Contains("mod") || n.Contains("hack") || n.Contains("menu")) return true;
		}
		catch { }
		return false;
	}

	private void CreateLabel(RigVis vis, Vector3 pos, string text, Color col, int fontSize)
	{
		TextMesh t = ExtraUtil.CreateLabel(((Component)this).transform, "Sonder_VisLabel", col);
		if (t == null) return;
		t.text = text;
		t.fontSize = fontSize;
		t.characterSize = 0.075f;
		t.color = col;
		((Component)t).transform.position = pos;
		ExtraUtil.FaceCamera(t);
		vis.Labels.Add(t);
	}

	private LineRenderer CreateLine(RigVis vis, Vector3 a, Vector3 b, Color col, float startW, float endW, string kind)
	{
		GameObject go = new GameObject("Sonder_Line_" + kind);
		go.transform.SetParent(((Component)this).transform, false);
		LineRenderer lr = go.AddComponent<LineRenderer>();
		lr.positionCount = 2;
		lr.useWorldSpace = true;
		lr.startWidth = startW;
		lr.endWidth = endW;
		lr.numCapVertices = 3;
		((Renderer)lr).shadowCastingMode = (ShadowCastingMode)0;
		((Renderer)lr).receiveShadows = false;
		((Renderer)lr).material = ThroughWallMat(col);
		lr.startColor = col;
		Color c2 = col; c2.a = Mathf.Clamp01(col.a * 0.35f);
		lr.endColor = c2;
		lr.SetPosition(0, a);
		lr.SetPosition(1, b);
		vis.Lines.Add(lr);
		vis.Objects.Add(go);
		return lr;
	}

	private GameObject CreatePrimitive(RigVis vis, PrimitiveType type, Vector3 pos, Vector3 scale, Quaternion rot, Color col)
	{
		GameObject go = GameObject.CreatePrimitive(type);
		Collider c = go.GetComponent<Collider>();
		if (c != null) Object.Destroy(c);
		go.transform.SetParent(((Component)this).transform, true);
		go.transform.position = pos;
		go.transform.rotation = rot;
		go.transform.localScale = scale;
		Renderer r = go.GetComponent<Renderer>();
		if (r != null)
		{
			Material m = ThroughWallMat(col);
			r.material = m;
			vis.Renderers.Add(r);
			vis.Materials.Add(m);
		}
		vis.Objects.Add(go);
		return go;
	}

	private void CreateRing(RigVis vis, Vector3 center, float radius, Color col)
	{
		GameObject go = new GameObject("Sonder_Ring");
		go.transform.SetParent(((Component)this).transform, false);
		LineRenderer lr = go.AddComponent<LineRenderer>();
		int n = 48;
		lr.positionCount = n;
		lr.loop = true;
		lr.useWorldSpace = true;
		lr.startWidth = 0.02f; lr.endWidth = 0.02f;
		((Renderer)lr).material = ThroughWallMat(col);
		lr.startColor = col; lr.endColor = col;
		for (int i = 0; i < n; i++)
		{
			float a = (float)i / n * Mathf.PI * 2f;
			lr.SetPosition(i, center + new Vector3(Mathf.Cos(a) * radius, 0f, Mathf.Sin(a) * radius));
		}
		vis.Lines.Add(lr);
		vis.Objects.Add(go);
	}

	private void CreateFloorMarker(RigVis vis, VRRig rig, Color col)
	{
		Vector3 p = ((Component)rig).transform.position;
		p = ExtraUtil.Grounded(p);
		CreatePrimitive(vis, PrimitiveType.Cylinder, p + Vector3.up * 0.02f, new Vector3(0.6f, 0.02f, 0.6f), Quaternion.identity, col);
		CreateRing(vis, p + Vector3.up * 0.03f, 0.35f, col);
	}

	private void CreateDiamond(RigVis vis, Vector3 pos, Color col)
	{
		GameObject go = CreatePrimitive(vis, PrimitiveType.Cube, pos, new Vector3(0.12f, 0.12f, 0.12f), Quaternion.Euler(45f, 45f, 45f), col);
		go.name = "Sonder_Diamond";
	}

	private void CreateArrow(RigVis vis, Vector3 top, Vector3 lookTarget, Color col)
	{
		Vector3 dir = (lookTarget - top); dir.y = 0f;
		if (dir.sqrMagnitude < 0.001f) dir = Vector3.forward;
		Quaternion rot = Quaternion.LookRotation(dir.normalized, Vector3.up) * Quaternion.Euler(90f, 0f, 0f);
		CreatePrimitive(vis, PrimitiveType.Capsule, top, new Vector3(0.04f, 0.18f, 0.04f), rot, col);
		CreatePrimitive(vis, PrimitiveType.Sphere, top + Vector3.up * 0.12f, Vector3.one * 0.07f, Quaternion.identity, col);
	}

	private void CreateShadowClone(RigVis vis, VRRig rig, Color col)
	{
		Vector3 pos = ((Component)rig).transform.position - ((Component)rig).transform.forward * 0.35f + Vector3.up * 0.15f;
		Color c = col; c.a = 0.35f;
		CreatePrimitive(vis, PrimitiveType.Capsule, pos, new Vector3(0.3f, 0.35f, 0.3f), ((Component)rig).transform.rotation, c);
		if ((Object)(object)rig.headMesh != (Object)null)
			CreatePrimitive(vis, PrimitiveType.Sphere, pos + Vector3.up * 0.45f, Vector3.one * 0.22f, Quaternion.identity, c);
	}

	private void DrawBones(RigVis vis, VRRig rig, Color col, bool v2)
	{
		Transform head = (rig.headMesh != null) ? rig.headMesh.transform : ((Component)rig).transform;
		Vector3 h = head.position;
		Vector3 b = ((Component)rig).transform.position + Vector3.up * 0.2f;
		Vector3 lh = (rig.leftHandTransform != null) ? rig.leftHandTransform.position : b + Vector3.left * 0.3f;
		Vector3 rh = (rig.rightHandTransform != null) ? rig.rightHandTransform.position : b + Vector3.right * 0.3f;
		Vector3 ls = b + Vector3.left * 0.15f + Vector3.up * 0.2f;
		Vector3 rs = b + Vector3.right * 0.15f + Vector3.up * 0.2f;
		Vector3 hip = ((Component)rig).transform.position + Vector3.up * 0.05f;
		float w = v2 ? 0.02f : 0.012f;
		void L(Vector3 a, Vector3 c) { CreateLine(vis, a, c, col, w, w, v2 ? "skel2" : "bone"); }
		L(h, b); L(b, hip); L(b, ls); L(ls, lh); L(b, rs); L(rs, rh);
		if (v2)
		{
			Vector3 lf = hip + Vector3.left * 0.1f - Vector3.up * 0.35f;
			Vector3 rf = hip + Vector3.right * 0.1f - Vector3.up * 0.35f;
			L(hip, lf); L(hip, rf);
			CreatePrimitive(vis, PrimitiveType.Sphere, h, Vector3.one * 0.08f, Quaternion.identity, col);
			CreatePrimitive(vis, PrimitiveType.Sphere, ls, Vector3.one * 0.05f, Quaternion.identity, col);
			CreatePrimitive(vis, PrimitiveType.Sphere, rs, Vector3.one * 0.05f, Quaternion.identity, col);
		}
	}

	private void DrawHandSparks(RigVis vis, VRRig rig, bool left, Color col)
	{
		Transform t = left ? rig.leftHandTransform : rig.rightHandTransform;
		if ((Object)(object)t == (Object)null) return;
		for (int i = 0; i < 6; i++)
		{
			Vector3 off = UnityEngine.Random.insideUnitSphere * 0.08f;
			CreatePrimitive(vis, PrimitiveType.Sphere, t.position + off, Vector3.one * 0.025f, Quaternion.identity, Pulse(col, 1f + i * 0.2f));
		}
	}

	private void DrawHandComet(RigVis vis, VRRig rig, bool left, Color col)
	{
		Transform t = left ? rig.leftHandTransform : rig.rightHandTransform;
		if ((Object)(object)t == (Object)null) return;
		int id = ((Component)rig).GetInstanceID() * 2 + (left ? 0 : 1);
		if (!_gripHistory.TryGetValue(id, out Queue<Vector3> q))
		{
			q = new Queue<Vector3>();
			_gripHistory[id] = q;
		}
		q.Enqueue(t.position);
		while (q.Count > 10) q.Dequeue();
		Vector3[] pts = q.ToArray();
		for (int i = 1; i < pts.Length; i++)
		{
			Color c = col; c.a = (float)i / pts.Length;
			CreateLine(vis, pts[i - 1], pts[i], c, 0.03f * i / pts.Length, 0.01f, "comet");
		}
	}

	private void DrawLightningHands(RigVis vis, VRRig rig, Color col)
	{
		if ((Object)(object)rig.leftHandTransform == (Object)null || (Object)(object)rig.rightHandTransform == (Object)null) return;
		Vector3 a = rig.leftHandTransform.position;
		Vector3 b = rig.rightHandTransform.position;
		int segs = 8;
		Vector3 prev = a;
		for (int i = 1; i <= segs; i++)
		{
			Vector3 p = Vector3.Lerp(a, b, (float)i / segs);
			if (i < segs) p += UnityEngine.Random.insideUnitSphere * 0.05f;
			Color c = Color.Lerp(col, Color.white, 0.5f);
			CreateLine(vis, prev, p, c, 0.015f, 0.01f, "zap");
			prev = p;
		}
	}

	private void DrawGripTracker(RigVis vis, VRRig rig, Color col)
	{
		// simple grip history markers at hands when gripping
		try
		{
			if (GetRigBool(rig, new[] { "leftGrab", "leftGrabbed", "LeftGrab" }) && (Object)(object)rig.leftHandTransform != (Object)null)
				CreateRing(vis, rig.leftHandTransform.position, 0.08f, Color.green);
			if (GetRigBool(rig, new[] { "rightGrab", "rightGrabbed", "RightGrab" }) && (Object)(object)rig.rightHandTransform != (Object)null)
				CreateRing(vis, rig.rightHandTransform.position, 0.08f, Color.green);
		}
		catch { }
	}

	private void ApplyOutlineGlow(VRRig rig, RigVis vis, Color col, bool neon)
	{
		if ((Object)(object)rig.mainSkin == (Object)null) return;
		Renderer r = (Renderer)(object)rig.mainSkin;
		if ((Object)(object)r == (Object)null) return;
		Material m = r.material;
		if ((Object)(object)m == (Object)null) return;
		vis.Renderers.Add(r);
		float intensity = neon ? 1.2f : 0.65f;
		try
		{
			if (m.HasProperty("_EmissionColor"))
			{
				m.EnableKeyword("_EMISSION");
				m.SetColor("_EmissionColor", col * intensity);
			}
			if (m.HasProperty("_Color"))
			{
				Color baseCol = m.color;
				Color ncol = Color.Lerp(baseCol, col, neon ? 0.55f : 0.35f);
				ncol.a = baseCol.a;
				m.color = ncol;
			}
		}
		catch { }
	}

	private void RestoreGlow(RigVis vis)
	{
		// materials are shared instance on renderer.material; best-effort: disable emission keyword spike
		// (full restore is complex without cached per-rig originals; outline toggles are session-scoped)
	}

	private void ApplyChams(VRRig rig, RigVis vis, Color col, bool pulse)
	{
		if ((Object)(object)rig.mainSkin == (Object)null) return;
		try
		{
			Material mat = ((Renderer)rig.mainSkin).material;
			if ((Object)(object)mat == (Object)null) return;
			if (vis.SavedMainMats == null)
			{
				vis.SavedMainShaders = new Shader[] { mat.shader };
				vis.SavedMainColors = new Color[] { mat.HasProperty("_Color") ? mat.color : Color.white };
			}
			Shader sh = Shader.Find("GUI/Text Shader");
			if (sh == null) sh = Shader.Find("Hidden/Internal-Colored");
			if (sh != null) mat.shader = sh;
			Color c = pulse ? Pulse(col, 2.5f) : col;
			if (mat.HasProperty("_Color")) mat.color = c;
			try { mat.SetInt("_ZTest", (int)CompareFunction.Always); } catch { }
			mat.renderQueue = 4000;
		}
		catch { }
	}

	private void RestoreChams(RigVis vis)
	{
		if ((Object)(object)vis.Rig == (Object)null || vis.SavedMainShaders == null) return;
		try
		{
			if ((Object)(object)vis.Rig.mainSkin == (Object)null) return;
			Material mat = ((Renderer)vis.Rig.mainSkin).material;
			if ((Object)(object)mat == (Object)null) return;
			if (vis.SavedMainShaders.Length > 0 && vis.SavedMainShaders[0] != null) mat.shader = vis.SavedMainShaders[0];
			if (vis.SavedMainColors != null && vis.SavedMainColors.Length > 0 && mat.HasProperty("_Color")) mat.color = vis.SavedMainColors[0];
			vis.SavedMainShaders = null;
			vis.SavedMainColors = null;
		}
		catch { }
	}

	private static Color Pulse(Color c, float speed)
	{
		float t = 0.5f + 0.5f * Mathf.Sin(Time.time * speed);
		return Color.Lerp(c * 0.5f, c * 1.4f, t);
	}

	private static Material ThroughWallMat(Color col)
	{
		return ExtraUtil.ThroughWall(col);
	}

	private void ClearRigObjects(RigVis vis, bool keepSavedMats)
	{
		for (int i = 0; i < vis.Labels.Count; i++) if ((Object)(object)vis.Labels[i] != (Object)null) Object.Destroy((Object)(object)((Component)vis.Labels[i]).gameObject);
		for (int i = 0; i < vis.Lines.Count; i++) if ((Object)(object)vis.Lines[i] != (Object)null) Object.Destroy((Object)(object)((Component)vis.Lines[i]).gameObject);
		for (int i = 0; i < vis.Objects.Count; i++) if ((Object)(object)vis.Objects[i] != (Object)null) Object.Destroy((Object)(object)vis.Objects[i]);
		for (int i = 0; i < vis.Materials.Count; i++) if ((Object)(object)vis.Materials[i] != (Object)null) Object.Destroy((Object)(object)vis.Materials[i]);
		vis.Labels.Clear();
		vis.Lines.Clear();
		vis.Objects.Clear();
		vis.Renderers.Clear();
		vis.Materials.Clear();
		if (!keepSavedMats)
		{
			RestoreChams(vis);
			vis.SavedMainMats = null;
			vis.SavedMainShaders = null;
			vis.SavedMainColors = null;
		}
	}

	private void ClearRig(int id)
	{
		if (_rigs.TryGetValue(id, out RigVis vis))
		{
			RestoreChams(vis);
			ClearRigObjects(vis, keepSavedMats: false);
			_rigs.Remove(id);
		}
		_lastPos.Remove(id);
		_lastPosTime.Remove(id);
	}

	private void ClearAll()
	{
		List<int> keys = _rigs.Keys.ToList();
		for (int i = 0; i < keys.Count; i++) ClearRig(keys[i]);
		_rigs.Clear();
		_gripHistory.Clear();
	}

	private void ApplyFps()
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		TextMeshProUGUI fpsText = MenuBuilder.FpsText;
		if ((Object)(object)fpsText == (Object)null) return;
		if (!EnlargeFps) { RestoreFps(); return; }
		if (!_fpsSaved)
		{
			_fpsSaved = true;
			_fpsSize = ((TMP_Text)fpsText).fontSize;
			_fpsAlignment = ((TMP_Text)fpsText).alignment;
			_fpsColor = ((Graphic)fpsText).color;
		}
		((TMP_Text)fpsText).fontSize = 30f;
		((TMP_Text)fpsText).alignment = (TextAlignmentOptions)4100;
		((Graphic)fpsText).color = SonderSettings.Accent;
		((TMP_Text)fpsText).outlineWidth = 0.16f;
		((TMP_Text)fpsText).outlineColor = (Color.black);
	}

	private void RestoreFps()
	{
		if (!_fpsSaved) return;
		TextMeshProUGUI fpsText = MenuBuilder.FpsText;
		_fpsSaved = false;
		if ((Object)(object)fpsText == (Object)null) return;
		((TMP_Text)fpsText).fontSize = _fpsSize;
		((TMP_Text)fpsText).alignment = _fpsAlignment;
		((Graphic)fpsText).color = _fpsColor;
		((TMP_Text)fpsText).outlineWidth = 0f;
	}

	private void ApplySky()
	{
		if (!SkyReplacement) { RestoreSky(); return; }
		if (_skyApplied) return;
		Shader val = Shader.Find("Skybox/Procedural");
		if ((Object)(object)val == (Object)null) return;
		_oldSkybox = RenderSettings.skybox;
		_skyMaterial = new Material(val);
		if (_skyMaterial.HasProperty("_SkyTint")) _skyMaterial.SetColor("_SkyTint", SonderSettings.Accent);
		if (_skyMaterial.HasProperty("_GroundColor")) _skyMaterial.SetColor("_GroundColor", new Color(0.025f, 0.02f, 0.035f));
		if (_skyMaterial.HasProperty("_AtmosphereThickness")) _skyMaterial.SetFloat("_AtmosphereThickness", 1.35f);
		if (_skyMaterial.HasProperty("_Exposure")) _skyMaterial.SetFloat("_Exposure", 1.15f);
		if (_skyMaterial.HasProperty("_SunSize")) _skyMaterial.SetFloat("_SunSize", 0.035f);
		RenderSettings.skybox = _skyMaterial;
		DynamicGI.UpdateEnvironment();
		_skyApplied = true;
	}

	private void RestoreSky()
	{
		if (!_skyApplied) return;
		if ((Object)(object)RenderSettings.skybox == (Object)(object)_skyMaterial) RenderSettings.skybox = _oldSkybox;
		if ((Object)(object)_skyMaterial != (Object)null) Object.Destroy((Object)(object)_skyMaterial);
		_skyMaterial = null;
		_oldSkybox = null;
		_skyApplied = false;
		DynamicGI.UpdateEnvironment();
	}

	private void ApplyNightVision()
	{
		if (!NightVision) { RestoreNightVision(); return; }
		if (_nightApplied) return;
		_ambientMode = RenderSettings.ambientMode;
		_ambientLight = RenderSettings.ambientLight;
		_fog = RenderSettings.fog;
		_fogColor = RenderSettings.fogColor;
		_fogMode = RenderSettings.fogMode;
		_fogDensity = RenderSettings.fogDensity;
		Color ambientLight = SonderSettings.Accent * 0.35f; ambientLight.a = 1f;
		RenderSettings.ambientMode = (AmbientMode)3;
		RenderSettings.ambientLight = ambientLight;
		RenderSettings.fog = true;
		RenderSettings.fogMode = (FogMode)3;
		RenderSettings.fogColor = new Color(0.005f, 0.01f, 0.015f);
		RenderSettings.fogDensity = 0.012f;
		_nightApplied = true;
	}

	private void RestoreNightVision()
	{
		if (!_nightApplied) return;
		RenderSettings.ambientMode = _ambientMode;
		RenderSettings.ambientLight = _ambientLight;
		RenderSettings.fog = _fog;
		RenderSettings.fogColor = _fogColor;
		RenderSettings.fogMode = _fogMode;
		RenderSettings.fogDensity = _fogDensity;
		_nightApplied = false;
	}

	private void ApplyTrails()
	{
		VRRig localRig = VRRig.LocalRig;
		if ((Object)(object)localRig == (Object)null) return;
		ApplyTrail(localRig.leftHandTransform, "Sonder_Trail_L", Trails);
		ApplyTrail(localRig.rightHandTransform, "Sonder_Trail_R", Trails);
	}

	private void ApplyTrail(Transform hand, string name, bool active)
	{
		if ((Object)(object)hand == (Object)null) return;
		Transform val = hand.Find(name);
		if (!active)
		{
			if ((Object)(object)val != (Object)null) Object.Destroy((Object)(object)((Component)val).gameObject);
			return;
		}
		if ((Object)(object)val != (Object)null) return;
		GameObject val2 = new GameObject(name);
		val2.transform.SetParent(hand, false);
		TrailRenderer obj = val2.AddComponent<TrailRenderer>();
		obj.time = 0.45f;
		obj.startWidth = 0.035f;
		obj.endWidth = 0f;
		obj.minVertexDistance = 0.02f;
		((Renderer)obj).shadowCastingMode = (ShadowCastingMode)0;
		((Renderer)obj).receiveShadows = false;
		if ((Object)(object)_trailMaterial == (Object)null) _trailMaterial = ExtraUtil.Unlit(Color.white, transparent: true);
		((Renderer)obj).material = _trailMaterial;
		obj.startColor = SonderSettings.Accent;
		Color accent = SonderSettings.Accent; accent.a = 0f;
		obj.endColor = accent;
	}
}
