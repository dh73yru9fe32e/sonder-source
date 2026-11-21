using System.Collections.Generic;
using GorillaLocomotion;
using Sonder.Core;
using UnityEngine;
using Object = UnityEngine.Object;
using UnityEngine.Rendering;

namespace Sonder.Managers;

// Self-only movement mods. Nothing here touches other players' rigs.
public class MovementPlusRuntime : MonoBehaviour
{
	private GameObject _leftPlatform;

	private GameObject _rightPlatform;

	private Material _platformMaterial;

	private readonly List<MeshCollider> _noclipDisabled = new List<MeshCollider>();

	private bool _noclipApplied;

	private LineRenderer _teleportLine;

	private GameObject _teleportMarker;

	private bool _teleportTriggerPrev;

	private bool _hasCheckpoint;

	private Vector3 _checkpoint;

	private GameObject _checkpointMarker;

	private bool _checkpointSavePrev;

	private bool _checkpointLoadPrev;

	private bool _hasWallNormal;

	private Vector3 _wallNormal;

	private bool _superGravityOff;

	private bool _superTogglePrev;

	private bool _handWasColliding;

	private bool _jumpPrev;

	public static MovementPlusRuntime Instance { get; private set; }

	public bool Platforms { get; set; }

	public bool Noclip { get; set; }

	public bool TeleportPointer { get; set; }

	public bool Checkpoint { get; set; }

	public bool WallWalk { get; set; }

	public bool SuperMonke { get; set; }

	public bool PullMod { get; set; }

	public bool GripJump { get; set; }

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
		ClearPlatforms();
		RestoreNoclip();
		DestroyObject(ref _checkpointMarker);
		DestroyObject(ref _teleportMarker);
		if ((Object)(object)_teleportLine != (Object)null)
		{
			Object.Destroy((Object)(object)((Component)_teleportLine).gameObject);
		}
	}

	private void Update()
	{
		GTPlayer player = ExtraUtil.Player;
		ControllerInputPoller input = ControllerInputPoller.instance;
		if ((Object)(object)player == (Object)null || (Object)(object)input == (Object)null)
		{
			return;
		}
		ApplyPlatforms(input);
		ApplyNoclip();
		ApplyTeleportPointer(player, input);
		ApplyCheckpoint(input);
		ApplyWallWalk(player, input);
		ApplySuperMonke(input);
		ApplyPullMod(player);
		ApplyGripJump(input);
	}

	private void ApplyPlatforms(ControllerInputPoller input)
	{
		if (!Platforms)
		{
			ClearPlatforms();
			return;
		}
		UpdatePlatform(ref _leftPlatform, input.leftControllerGripFloat > 0.5f, ExtraUtil.Hand(left: true));
		UpdatePlatform(ref _rightPlatform, input.rightControllerGripFloat > 0.5f, ExtraUtil.Hand(left: false));
	}

	// The platform is placed once when grip is pressed and stays put until release,
	// so you can push off it like a real surface.
	private void UpdatePlatform(ref GameObject platform, bool held, Transform hand)
	{
		if (!held || (Object)(object)hand == (Object)null)
		{
			DestroyObject(ref platform);
			return;
		}
		if ((Object)(object)platform != (Object)null)
		{
			return;
		}
		if ((Object)(object)_platformMaterial == (Object)null)
		{
			_platformMaterial = ExtraUtil.Unlit(SonderSettings.Accent, transparent: false);
		}
		platform = GameObject.CreatePrimitive((PrimitiveType)3);
		((Object)platform).name = "Sonder_Platform";
		platform.layer = 0;
		platform.transform.localScale = new Vector3(0.28f, 0.02f, 0.28f);
		platform.transform.position = hand.position - Vector3.up * 0.07f;
		platform.transform.rotation = Quaternion.Euler(0f, hand.eulerAngles.y, 0f);
		Renderer component = platform.GetComponent<Renderer>();
		component.sharedMaterial = _platformMaterial;
		component.shadowCastingMode = (ShadowCastingMode)0;
	}

	private void ClearPlatforms()
	{
		DestroyObject(ref _leftPlatform);
		DestroyObject(ref _rightPlatform);
	}

	private void ApplyNoclip()
	{
		if (Noclip && !_noclipApplied)
		{
			_noclipDisabled.Clear();
			MeshCollider[] array = Object.FindObjectsByType<MeshCollider>((FindObjectsSortMode)0);
			foreach (MeshCollider val in array)
			{
				if (!((Object)(object)val == (Object)null) && ((Collider)val).enabled && !ExtraUtil.IsLocalCollider((Collider)(object)val))
				{
					((Collider)val).enabled = false;
					_noclipDisabled.Add(val);
				}
			}
			_noclipApplied = true;
			NotificationRuntime.Show("Noclip: pair with a fly mod");
		}
		else if (!Noclip && _noclipApplied)
		{
			RestoreNoclip();
		}
	}

	private void RestoreNoclip()
	{
		for (int i = 0; i < _noclipDisabled.Count; i++)
		{
			if ((Object)(object)_noclipDisabled[i] != (Object)null)
			{
				((Collider)_noclipDisabled[i]).enabled = true;
			}
		}
		_noclipDisabled.Clear();
		_noclipApplied = false;
	}

	private void ApplyTeleportPointer(GTPlayer player, ControllerInputPoller input)
	{
		Transform hand = ExtraUtil.Hand(left: false);
		bool flag = TeleportPointer && input.rightControllerGripFloat > 0.5f && (Object)(object)hand != (Object)null;
		bool flag2 = input.rightControllerIndexFloat > 0.5f;
		if (!flag)
		{
			SetTeleportVisible(visible: false);
			_teleportTriggerPrev = flag2;
			return;
		}
		RaycastHit val = default(RaycastHit);
		bool flag3 = Physics.Raycast(hand.position, hand.forward, out val, 120f, (player.locomotionEnabledLayers), (QueryTriggerInteraction)1);
		Vector3 val2 = (flag3 ? val.point : (hand.position + hand.forward * 120f));
		EnsureTeleportVisuals();
		SetTeleportVisible(visible: true);
		_teleportLine.SetPosition(0, hand.position);
		_teleportLine.SetPosition(1, val2);
		_teleportMarker.SetActive(flag3);
		_teleportMarker.transform.position = val2;
		if (flag3 && flag2 && !_teleportTriggerPrev)
		{
			ExtraUtil.Teleport(val.point + val.normal * 0.4f);
			Rigidbody val3 = ExtraUtil.Body();
			if ((Object)(object)val3 != (Object)null)
			{
				val3.linearVelocity = Vector3.zero;
			}
		}
		_teleportTriggerPrev = flag2;
	}

	private void EnsureTeleportVisuals()
	{
		if ((Object)(object)_teleportLine == (Object)null)
		{
			GameObject val = new GameObject("Sonder_TeleportLine");
			val.layer = 2;
			_teleportLine = val.AddComponent<LineRenderer>();
			_teleportLine.positionCount = 2;
			_teleportLine.startWidth = 0.01f;
			_teleportLine.endWidth = 0.004f;
			((Renderer)_teleportLine).shadowCastingMode = (ShadowCastingMode)0;
			((Renderer)_teleportLine).material = ExtraUtil.Unlit(SonderSettings.Accent, transparent: true);
		}
		if ((Object)(object)_teleportMarker == (Object)null)
		{
			_teleportMarker = MakeMarker("Sonder_TeleportMarker", SonderSettings.Accent, 0.12f);
		}
	}

	private void SetTeleportVisible(bool visible)
	{
		if ((Object)(object)_teleportLine != (Object)null)
		{
			((Component)_teleportLine).gameObject.SetActive(visible);
		}
		if ((Object)(object)_teleportMarker != (Object)null && !visible)
		{
			_teleportMarker.SetActive(false);
		}
	}

	// X saves where you stand, Y sends you back.
	private void ApplyCheckpoint(ControllerInputPoller input)
	{
		bool leftControllerPrimaryButton = input.leftControllerPrimaryButton;
		bool leftControllerSecondaryButton = input.leftControllerSecondaryButton;
		if (!Checkpoint)
		{
			if ((Object)(object)_checkpointMarker != (Object)null)
			{
				_checkpointMarker.SetActive(false);
			}
		}
		else
		{
			if (leftControllerPrimaryButton && !_checkpointSavePrev)
			{
				_checkpoint = ExtraUtil.PlayerPosition();
				_hasCheckpoint = true;
				if ((Object)(object)_checkpointMarker == (Object)null)
				{
					_checkpointMarker = MakeMarker("Sonder_Checkpoint", Color.green, 0.2f);
				}
				_checkpointMarker.transform.position = _checkpoint;
				NotificationRuntime.Show("Checkpoint saved");
			}
			if (leftControllerSecondaryButton && !_checkpointLoadPrev && _hasCheckpoint)
			{
				ExtraUtil.Teleport(_checkpoint);
				Rigidbody val = ExtraUtil.Body();
				if ((Object)(object)val != (Object)null)
				{
					val.linearVelocity = Vector3.zero;
				}
			}
			if ((Object)(object)_checkpointMarker != (Object)null)
			{
				_checkpointMarker.SetActive(_hasCheckpoint);
			}
		}
		_checkpointSavePrev = leftControllerPrimaryButton;
		_checkpointLoadPrev = leftControllerSecondaryButton;
	}

	// Touch a wall with grip held and gravity pulls you into that surface instead of down.
	private void ApplyWallWalk(GTPlayer player, ControllerInputPoller input)
	{
		bool flag = input.leftControllerGripFloat > 0.5f || input.rightControllerGripFloat > 0.5f;
		if (!WallWalk || !flag)
		{
			_hasWallNormal = false;
			return;
		}
		if (player.HandContactingSurface)
		{
			RaycastHit touchHitInfo = player.GetTouchHitInfo(input.leftControllerGripFloat > 0.5f);
			if (touchHitInfo.normal != Vector3.zero)
			{
				_wallNormal = touchHitInfo.normal;
				_hasWallNormal = true;
			}
		}
		Rigidbody val = ExtraUtil.Body();
		if (_hasWallNormal && !((Object)(object)val == (Object)null))
		{
			val.AddForce(-Physics.gravity, (ForceMode)5);
			val.AddForce(-_wallNormal * 9.81f, (ForceMode)5);
		}
	}

	// A flies where you look, B toggles zero gravity.
	private void ApplySuperMonke(ControllerInputPoller input)
	{
		bool rightControllerSecondaryButton = input.rightControllerSecondaryButton;
		if (!SuperMonke)
		{
			_superGravityOff = false;
			_superTogglePrev = rightControllerSecondaryButton;
			return;
		}
		if (rightControllerSecondaryButton && !_superTogglePrev)
		{
			_superGravityOff = !_superGravityOff;
			NotificationRuntime.Show(_superGravityOff ? "Super Monke: gravity off" : "Super Monke: gravity on");
		}
		_superTogglePrev = rightControllerSecondaryButton;
		Rigidbody val = ExtraUtil.Body();
		if ((Object)(object)val == (Object)null)
		{
			return;
		}
		if (_superGravityOff)
		{
			val.AddForce(-Physics.gravity, (ForceMode)5);
		}
		if (input.rightControllerPrimaryButton)
		{
			Transform val2 = ExtraUtil.Head();
			if ((Object)(object)val2 != (Object)null)
			{
				val.linearVelocity = val2.forward * ModRuntime.FlySpeedValue;
			}
		}
	}

	// Each time a hand leaves a surface, your horizontal speed gets a small kick.
	private void ApplyPullMod(GTPlayer player)
	{
		bool anyHandIsColliding = player.HandContactingSurface;
		if (PullMod && _handWasColliding && !anyHandIsColliding)
		{
			Rigidbody val = ExtraUtil.Body();
			if ((Object)(object)val != (Object)null)
			{
				Vector3 linearVelocity = val.linearVelocity;
				Vector3 val2 = new Vector3(linearVelocity.x, 0f, linearVelocity.z) * 1.2f;
				val2 = Vector3.ClampMagnitude(val2, Mathf.Max(player.velocityLimit, 8f));
				val.linearVelocity = new Vector3(val2.x, linearVelocity.y, val2.z);
			}
		}
		_handWasColliding = anyHandIsColliding;
	}

	// Press B for a straight-up launch.
	private void ApplyGripJump(ControllerInputPoller input)
	{
		bool flag = input.rightControllerSecondaryButton;
		if (GripJump && flag && !_jumpPrev && !SuperMonke)
		{
			Rigidbody val = ExtraUtil.Body();
			if ((Object)(object)val != (Object)null)
			{
				val.linearVelocity = new Vector3(val.linearVelocity.x, 9f, val.linearVelocity.z);
			}
		}
		_jumpPrev = flag;
	}

	private static GameObject MakeMarker(string name, Color color, float size)
	{
		GameObject val = GameObject.CreatePrimitive((PrimitiveType)0);
		((Object)val).name = name;
		val.layer = 2;
		Object.Destroy((Object)(object)val.GetComponent<Collider>());
		val.transform.localScale = Vector3.one * size;
		Renderer component = val.GetComponent<Renderer>();
		component.sharedMaterial = ExtraUtil.Unlit(new Color(color.r, color.g, color.b, 0.6f), transparent: true);
		component.shadowCastingMode = (ShadowCastingMode)0;
		return val;
	}

	private static void DestroyObject(ref GameObject obj)
	{
		if ((Object)(object)obj != (Object)null)
		{
			Object.Destroy((Object)(object)obj);
		}
		obj = null;
	}
}
