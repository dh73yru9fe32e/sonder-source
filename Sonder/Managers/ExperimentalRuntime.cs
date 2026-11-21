using GorillaLocomotion;
using Sonder.Core;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Sonder.Managers;

public class ExperimentalRuntime : MonoBehaviour
{
	private static readonly string[] ChaosPool = new string[20]
	{
		"Speed Boost", "High Jump", "Moon Gravity", "Bouncy", "Feather Fall", "Low Gravity", "Ground Helper", "Slide Control", "Spin Head", "Head Bang",
		"Flip Hands", "Auto Clicker", "Loud Hand Taps", "Spawn Hoverboard", "Nausea", "Rainbow Fur", "ESP Dots", "Tracer Lines", "Trail Fade", "Player Outline"
	};

	private bool _iceApplied;

	private float _savedFriction;

	private float _savedDynamicFriction;

	private float _savedStaticFriction;

	private PhysicsMaterialCombine _savedFrictionCombine;

	private Vector3 _rubberAnchor;

	private bool _rubberAnchored;

	private float _chaosNext;

	private string _chaosTitle;

	private bool _phaseApplied;

	private bool _savedLeftTrigger;

	private bool _savedRightTrigger;

	public static ExperimentalRuntime Instance { get; private set; }

	public bool GlideMode { get; set; }

	public bool NoGravityPockets { get; set; }

	public bool IceFloor { get; set; }

	public bool RubberBody { get; set; }

	public bool ChaosWheel { get; set; }

	public bool MirrorMode { get; set; }

	public bool SlowWorld { get; set; }

	public bool PhaseShift { get; set; }

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
		RestoreIce();
		RestorePhase();
		StopChaos();
		ExtraUtil.SetTimeScale("slow-world", enabled: false, 0.5f);
	}

	private void Update()
	{
		ApplyGlide();
		ApplyNoGravity();
		ApplyIce();
		ApplyRubber();
		ApplyChaos();
		ApplySlowWorld();
		ApplyPhaseShift();
	}

	private void LateUpdate()
	{
		ApplyMirror();
	}

	private static void ApplyGlide()
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		ExperimentalRuntime instance = Instance;
		GTPlayer player = ExtraUtil.Player;
		if ((Object)(object)instance == (Object)null || !instance.GlideMode || (Object)(object)player == (Object)null)
		{
			return;
		}
		Rigidbody playerRigidBody = player.playerRigidBody;
		if ((Object)(object)playerRigidBody == (Object)null || player.BodyOnGround)
		{
			return;
		}
		Vector3 val = playerRigidBody.linearVelocity;
		if (val.y < -4f)
		{
			val.y = -4f;
		}
		Transform val2 = ExtraUtil.Head();
		if ((Object)(object)val2 != (Object)null)
		{
			Vector3 val3 = Vector3.ProjectOnPlane(val2.forward, Vector3.up);
			if (val3.sqrMagnitude > 0.001f)
			{
				val += val3.normalized * (4f * Time.deltaTime);
			}
		}
		playerRigidBody.linearVelocity = val;
		playerRigidBody.AddForce(-Physics.gravity * 0.65f, (ForceMode)5);
	}

	private static void ApplyNoGravity()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		ExperimentalRuntime instance = Instance;
		if ((Object)(object)instance == (Object)null || !instance.NoGravityPockets)
		{
			return;
		}
		Rigidbody val = ExtraUtil.Body();
		if (!((Object)(object)val == (Object)null))
		{
			Vector3 linearVelocity = val.linearVelocity;
			if (linearVelocity.sqrMagnitude < 0.36f)
			{
				val.AddForce(-Physics.gravity + Vector3.up * 1.5f, (ForceMode)5);
				val.linearVelocity *= 0.94f;
			}
		}
	}

	private void ApplyIce()
	{
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		GorillaTagger tagger = ExtraUtil.Tagger;
		GTPlayer player = ExtraUtil.Player;
		CapsuleCollider val = (((Object)(object)tagger != (Object)null) ? tagger.bodyCollider : null);
		if ((Object)(object)val == (Object)null || (Object)(object)((Collider)val).material == (Object)null || (Object)(object)player == (Object)null)
		{
			return;
		}
		if (IceFloor)
		{
			if (!_iceApplied)
			{
				_savedFriction = player.frictionConstant;
				_savedDynamicFriction = ((Collider)val).material.dynamicFriction;
				_savedStaticFriction = ((Collider)val).material.staticFriction;
				_savedFrictionCombine = ((Collider)val).material.frictionCombine;
				_iceApplied = true;
			}
			player.frictionConstant = 0f;
			((Collider)val).material.dynamicFriction = 0f;
			((Collider)val).material.staticFriction = 0f;
			((Collider)val).material.frictionCombine = (PhysicsMaterialCombine)2;
		}
		else
		{
			RestoreIce();
		}
	}

	private void RestoreIce()
	{
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		if (_iceApplied)
		{
			GTPlayer player = ExtraUtil.Player;
			if ((Object)(object)player != (Object)null)
			{
				player.frictionConstant = _savedFriction;
			}
			GorillaTagger tagger = ExtraUtil.Tagger;
			CapsuleCollider val = (((Object)(object)tagger != (Object)null) ? tagger.bodyCollider : null);
			if ((Object)(object)val != (Object)null && (Object)(object)((Collider)val).material != (Object)null)
			{
				((Collider)val).material.dynamicFriction = _savedDynamicFriction;
				((Collider)val).material.staticFriction = _savedStaticFriction;
				((Collider)val).material.frictionCombine = _savedFrictionCombine;
			}
			_iceApplied = false;
		}
	}

	private void ApplyRubber()
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		if (!RubberBody)
		{
			_rubberAnchored = false;
			return;
		}
		GTPlayer player = ExtraUtil.Player;
		Rigidbody val = ExtraUtil.Body();
		if (!((Object)(object)player == (Object)null) && !((Object)(object)val == (Object)null))
		{
			if (!_rubberAnchored)
			{
				_rubberAnchor = ((Component)player).transform.position;
				_rubberAnchored = true;
			}
			Vector3 val2 = _rubberAnchor - ((Component)player).transform.position;
			float magnitude = val2.magnitude;
			if (magnitude > 7f)
			{
				ExtraUtil.Teleport(_rubberAnchor);
				val.linearVelocity = Vector3.zero;
			}
			else if (magnitude > 1.2f)
			{
				val.AddForce(val2.normalized * (magnitude * 10f), (ForceMode)5);
			}
		}
	}

	private void ApplyChaos()
	{
		if (!ChaosWheel)
		{
			StopChaos();
		}
		else if (!(Time.time < _chaosNext))
		{
			_chaosNext = Time.time + 30f;
			StopChaos();
			string text = ChaosPool[Random.Range(0, ChaosPool.Length)];
			ModManager.SetEnabled(text, enabled: true);
			_chaosTitle = text;
			SonderAudio.Alert(0.5f);
			RuntimeLog.Write("chaos wheel: " + text);
		}
	}

	private void StopChaos()
	{
		if (!string.IsNullOrEmpty(_chaosTitle))
		{
			ModManager.SetEnabled(_chaosTitle, enabled: false);
			_chaosTitle = null;
		}
	}

	private void ApplySlowWorld()
	{
		ExtraUtil.SetTimeScale("slow-world", SlowWorld, 0.5f);
	}

	private void ApplyPhaseShift()
	{
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		GorillaTagger tagger = ExtraUtil.Tagger;
		if ((Object)(object)tagger == (Object)null || (Object)(object)tagger.bodyCollider == (Object)null)
		{
			return;
		}
		ControllerInputPoller instance = ControllerInputPoller.instance;
		bool flag = (Object)(object)instance != (Object)null && instance.rightControllerPrimaryButton;
		if (PhaseShift & flag)
		{
			if (!_phaseApplied)
			{
				_savedLeftTrigger = (Object)(object)tagger.leftHandTriggerCollider != (Object)null && tagger.leftHandTriggerCollider.activeSelf;
				_savedRightTrigger = (Object)(object)tagger.rightHandTriggerCollider != (Object)null && tagger.rightHandTriggerCollider.activeSelf;
				_phaseApplied = true;
			}
			((Collider)tagger.bodyCollider).isTrigger = true;
			if ((Object)(object)tagger.leftHandTriggerCollider != (Object)null)
			{
				tagger.leftHandTriggerCollider.SetActive(false);
			}
			if ((Object)(object)tagger.rightHandTriggerCollider != (Object)null)
			{
				tagger.rightHandTriggerCollider.SetActive(false);
			}
			Rigidbody val = ExtraUtil.Body();
			Transform val2 = ExtraUtil.Head();
			if ((Object)(object)val != (Object)null)
			{
				val.AddForce(-Physics.gravity, (ForceMode)5);
				if ((Object)(object)val2 != (Object)null)
				{
					val.linearVelocity += val2.forward * (2.5f * Time.deltaTime);
				}
			}
		}
		else
		{
			RestorePhase();
		}
	}

	private void RestorePhase()
	{
		if (!_phaseApplied)
		{
			return;
		}
		GorillaTagger tagger = ExtraUtil.Tagger;
		if ((Object)(object)tagger != (Object)null)
		{
			if ((Object)(object)tagger.bodyCollider != (Object)null)
			{
				((Collider)tagger.bodyCollider).isTrigger = false;
			}
			if ((Object)(object)tagger.leftHandTriggerCollider != (Object)null)
			{
				tagger.leftHandTriggerCollider.SetActive(_savedLeftTrigger);
			}
			if ((Object)(object)tagger.rightHandTriggerCollider != (Object)null)
			{
				tagger.rightHandTriggerCollider.SetActive(_savedRightTrigger);
			}
		}
		_phaseApplied = false;
	}

	private static void ApplyMirror()
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		ExperimentalRuntime instance = Instance;
		GTPlayer player = ExtraUtil.Player;
		if (!((Object)(object)instance == (Object)null) && instance.MirrorMode && !((Object)(object)player == (Object)null))
		{
			Transform controllerTransform = player.GetControllerTransform(true);
			Transform controllerTransform2 = player.GetControllerTransform(false);
			if (!((Object)(object)controllerTransform == (Object)null) && !((Object)(object)controllerTransform2 == (Object)null))
			{
				Vector3 position = controllerTransform.position;
				Quaternion rotation = controllerTransform.rotation;
				controllerTransform.SetPositionAndRotation(controllerTransform2.position, controllerTransform2.rotation);
				controllerTransform2.SetPositionAndRotation(position, rotation);
			}
		}
	}
}
