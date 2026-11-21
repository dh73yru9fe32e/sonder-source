using System.Collections.Generic;
using System.Reflection;
using GorillaLocomotion;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using UnityEngine.UI;

namespace Sonder.Managers;

public class FunRuntime : MonoBehaviour
{
	private const float FlySpeed = 10f;

	private const float HeadBangBpm = 159f;

	private bool _headWritten;

	private float _lastBangTime;

	private bool _autoClickState;

	private bool _tapVolumeSaved;

	private float _savedTapVolume;

	private bool _tapVolumeApplied;

	private GameObject _cameraObject;

	private Vector3 _cameraVelocity;

	private bool _djSetsApplied;

	private bool _questScoreApplied;

	private float _questScoreDelay;

	private float _questNoiseDelay;

	private bool _nameTagState;

	private bool _nameTagApplied;

	private float _nameTagDelay;

	private bool _hoverboardSpawned;

	private static FieldInfo _goldNameTagField;

	public static FunRuntime Instance { get; private set; }

	public bool UpsideDownHead { get; set; }

	public bool BrokenNeck { get; set; }

	public bool BackwardsHead { get; set; }

	public bool SidewaysHead { get; set; }

	public bool HeadBang { get; set; }

	public bool SpinHead { get; set; }

	public bool SpazHead { get; set; }

	public bool FlipHands { get; set; }

	public bool AutoClicker { get; set; }

	public bool LoudHandTaps { get; set; }

	public bool SilentHandTaps { get; set; }

	public bool SilentTapsOnTag { get; set; }

	public bool Freecam { get; set; }

	public bool ThirdPersonCamera { get; set; }

	public bool FlipCamera { get; set; }

	public bool Nausea { get; set; }

	public bool MuteDJSets { get; set; }

	public bool MaxQuestScore { get; set; }

	public bool QuestNoises { get; set; }

	public bool GoldenNameTag { get; set; }

	public bool FlashNameTag { get; set; }

	public bool SpawnHoverboard { get; set; }

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
		DestroyCameraObject();
	}

	private void Update()
	{
		ApplyHead();
		ApplyFlipHands();
		ApplyAutoClicker();
		ApplyHandTaps();
		ApplyCameras();
		ApplyMuteDJSets();
		ApplyQuestScore();
		ApplyQuestNoises();
		ApplyNameTags();
		ApplyHoverboard();
	}

	public void FixHeadNow()
	{
		VRRig localRig = VRRig.LocalRig;
		if (!((Object)(object)localRig == (Object)null) && localRig.head != null)
		{
			localRig.head.trackingRotationOffset.x = 0f;
			localRig.head.trackingRotationOffset.y = 0f;
			localRig.head.trackingRotationOffset.z = 0f;
			_headWritten = false;
		}
	}

	private void ApplyHead()
	{
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		if (!UpsideDownHead && !BrokenNeck && !BackwardsHead && !SidewaysHead && !HeadBang && !SpinHead && !SpazHead)
		{
			if (_headWritten)
			{
				FixHeadNow();
			}
			return;
		}
		VRRig localRig = VRRig.LocalRig;
		if ((Object)(object)localRig == (Object)null || localRig.head == null)
		{
			return;
		}
		_headWritten = true;
		if (UpsideDownHead)
		{
			localRig.head.trackingRotationOffset.z = 180f;
		}
		if (BrokenNeck)
		{
			localRig.head.trackingRotationOffset.z = 90f;
		}
		if (BackwardsHead)
		{
			localRig.head.trackingRotationOffset.y = 180f;
		}
		if (SidewaysHead)
		{
			localRig.head.trackingRotationOffset.y = 90f;
		}
		if (HeadBang)
		{
			if (Time.time > _lastBangTime)
			{
				localRig.head.trackingRotationOffset.x = 50f;
				_lastBangTime = Time.time + 0.3773585f;
			}
			else
			{
				localRig.head.trackingRotationOffset.x = Mathf.Lerp(localRig.head.trackingRotationOffset.x, 0f, 0.1f);
			}
		}
		if (SpinHead)
		{
			localRig.head.trackingRotationOffset.y += 10f;
		}
		if (SpazHead)
		{
			localRig.head.trackingRotationOffset = new Vector3(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f));
		}
	}

	private void ApplyFlipHands()
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		if (!FlipHands)
		{
			return;
		}
		GorillaTagger instance = GorillaTagger.Instance;
		GTPlayer instance2 = GTPlayer.Instance;
		if ((Object)(object)instance == (Object)null || (Object)(object)instance2 == (Object)null)
		{
			return;
		}
		Transform leftHandTransform = instance.leftHandTransform;
		Transform rightHandTransform = instance.rightHandTransform;
		if (!((Object)(object)leftHandTransform == (Object)null) && !((Object)(object)rightHandTransform == (Object)null))
		{
			Vector3 position = leftHandTransform.position;
			Vector3 position2 = rightHandTransform.position;
			Quaternion rotation = leftHandTransform.rotation;
			Quaternion rotation2 = rightHandTransform.rotation;
			Transform controllerTransform = instance2.GetControllerTransform(false);
			Transform controllerTransform2 = instance2.GetControllerTransform(true);
			if (!((Object)(object)controllerTransform == (Object)null) && !((Object)(object)controllerTransform2 == (Object)null))
			{
				controllerTransform.position = position;
				controllerTransform.rotation = rotation;
				controllerTransform2.position = position2;
				controllerTransform2.rotation = rotation2;
			}
		}
	}

	private void ApplyAutoClicker()
	{
		if (!AutoClicker)
		{
			return;
		}
		ControllerInputPoller instance = ControllerInputPoller.instance;
		if (!((Object)(object)instance == (Object)null))
		{
			_autoClickState = !_autoClickState;
			if (instance.leftControllerIndexFloat > 0.5f)
			{
				ApplyFinger(left: true);
			}
			if (instance.rightControllerIndexFloat > 0.5f)
			{
				ApplyFinger(left: false);
			}
		}
	}

	private void ApplyFinger(bool left)
	{
		ControllerInputPoller instance = ControllerInputPoller.instance;
		VRRig localRig = VRRig.LocalRig;
		if ((Object)(object)instance == (Object)null)
		{
			return;
		}
		if (left)
		{
			instance.leftControllerIndexFloat = (_autoClickState ? 1f : 0f);
			if ((Object)(object)localRig != (Object)null && localRig.leftHand != null)
			{
				localRig.leftHand.calcT = (_autoClickState ? 1f : 0f);
				localRig.leftHand.MapMyFinger(1f);
			}
		}
		else
		{
			instance.rightControllerIndexFloat = (_autoClickState ? 1f : 0f);
			if ((Object)(object)localRig != (Object)null && localRig.rightHand != null)
			{
				localRig.rightHand.calcT = (_autoClickState ? 1f : 0f);
				localRig.rightHand.MapMyFinger(1f);
			}
		}
	}

	private void ApplyHandTaps()
	{
		if (!LoudHandTaps && !SilentHandTaps && !SilentTapsOnTag)
		{
			if (_tapVolumeApplied)
			{
				GorillaTagger instance = GorillaTagger.Instance;
				if ((Object)(object)instance != (Object)null && _tapVolumeSaved)
				{
					instance.handTapVolume = _savedTapVolume;
				}
				_tapVolumeApplied = false;
			}
			return;
		}
		GorillaTagger instance2 = GorillaTagger.Instance;
		if (!((Object)(object)instance2 == (Object)null))
		{
			if (!_tapVolumeSaved)
			{
				_savedTapVolume = instance2.handTapVolume;
				_tapVolumeSaved = true;
			}
			float handTapVolume = instance2.handTapVolume;
			if (LoudHandTaps)
			{
				handTapVolume = 99999f;
			}
			if (SilentHandTaps)
			{
				handTapVolume = 0f;
			}
			if (SilentTapsOnTag)
			{
				handTapVolume = (LocalIsTagged() ? 0f : 0.1f);
			}
			instance2.handTapVolume = handTapVolume;
			_tapVolumeApplied = true;
		}
	}

	private static bool LocalIsTagged()
	{
		GorillaGameManager instance = GorillaGameManager.instance;
		GorillaTagManager val = (GorillaTagManager)(object)((instance is GorillaTagManager) ? instance : null);
		if (val == null)
		{
			return false;
		}
		if ((Object)(object)NetworkSystem.Instance == (Object)null || NetworkSystem.Instance.LocalPlayer == null)
		{
			return false;
		}
		return val.IsInfected(NetworkSystem.Instance.LocalPlayer);
	}

	private void ApplyCameras()
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0275: Unknown result type (might be due to invalid IL or missing references)
		//IL_028b: Unknown result type (might be due to invalid IL or missing references)
		//IL_029f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_031a: Unknown result type (might be due to invalid IL or missing references)
		//IL_031f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		if (!Freecam && !ThirdPersonCamera && !FlipCamera && !Nausea)
		{
			if ((Object)(object)_cameraObject != (Object)null)
			{
				DestroyCameraObject();
			}
			return;
		}
		GorillaTagger instance = GorillaTagger.Instance;
		if ((Object)(object)instance == (Object)null || (Object)(object)instance.headCollider == (Object)null)
		{
			return;
		}
		EnsureCameraObject(((Component)instance.headCollider).transform.position);
		Camera component = _cameraObject.GetComponent<Camera>();
		if ((Object)(object)component == (Object)null)
		{
			return;
		}
		component.nearClipPlane = 0.01f;
		component.cameraType = (CameraType)1;
		Transform transform = ((Component)instance.headCollider).transform;
		if (Freecam)
		{
			ControllerInputPoller instance2 = ControllerInputPoller.instance;
			float num = (((Object)(object)instance2 != (Object)null) ? instance2.leftControllerPrimary2DAxis.x : 0f);
			float num2 = (((Object)(object)instance2 != (Object)null) ? instance2.rightControllerPrimary2DAxis.y : 0f);
			float num3 = (((Object)(object)instance2 != (Object)null) ? instance2.leftControllerPrimary2DAxis.y : 0f);
			Vector3 val = Vector3.forward;
			Vector3 val2 = Vector3.right;
			if ((Object)(object)instance.bodyCollider != (Object)null)
			{
				val = Flatten(((Component)instance.bodyCollider).transform.forward);
				val2 = Flatten(((Component)instance.bodyCollider).transform.right);
			}
			Vector3 val3 = num * val2 + num2 * Vector3.up + num3 * val;
			val3 *= 10f;
			_cameraVelocity = Vector3.Lerp(_cameraVelocity, val3, 0.12875f);
			Transform transform2 = _cameraObject.transform;
			transform2.position += _cameraVelocity * Time.unscaledDeltaTime;
			_cameraObject.transform.rotation = transform.rotation;
		}
		else if (ThirdPersonCamera)
		{
			Vector3 position = (((Object)(object)instance.bodyCollider != (Object)null) ? ((Component)instance.bodyCollider).transform.TransformPoint(new Vector3(0f, 0.5f, -1.5f)) : (transform.position - transform.forward * 1.5f));
			_cameraObject.transform.position = position;
			_cameraObject.transform.rotation = transform.rotation;
		}
		else if (FlipCamera)
		{
			_cameraObject.transform.position = transform.position;
			_cameraObject.transform.rotation = transform.rotation * Quaternion.Euler(0f, 180f, 0f);
		}
		else if (Nausea)
		{
			_cameraObject.transform.position = transform.position;
			_cameraObject.transform.rotation = transform.rotation * Quaternion.Euler(Mathf.Sin(Time.time) * 15f, Mathf.Cos(Time.time * 0.7f) * 15f, Mathf.Sin(Time.time * 1.3f) * 15f);
		}
	}

	private void EnsureCameraObject(Vector3 position)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)_cameraObject != (Object)null))
		{
			_cameraObject = new GameObject("Sonder_CameraObj");
			_cameraObject.AddComponent<Camera>();
			_cameraObject.transform.position = position;
			_cameraVelocity = Vector3.zero;
		}
	}

	private void DestroyCameraObject()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)_cameraObject == (Object)null))
		{
			Object.Destroy((Object)(object)_cameraObject.GetComponent<Camera>());
			Object.Destroy((Object)(object)_cameraObject);
			_cameraObject = null;
			_cameraVelocity = Vector3.zero;
		}
	}

	private static Vector3 Flatten(Vector3 value)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = default(Vector3);
		val = new Vector3(value.x, 0f, value.z);
		if (!(val.sqrMagnitude < 0.0001f))
		{
			return val.normalized;
		}
		return Vector3.forward;
	}

	private void ApplyMuteDJSets()
	{
		if (MuteDJSets == _djSetsApplied)
		{
			return;
		}
		_djSetsApplied = MuteDJSets;
		RadioButtonGroupWearable[] array = Object.FindObjectsByType<RadioButtonGroupWearable>((FindObjectsSortMode)0);
		foreach (RadioButtonGroupWearable val in array)
		{
			if ((Object)(object)val != (Object)null)
			{
				((Behaviour)val).enabled = !MuteDJSets;
			}
		}
	}

	private void ApplyQuestScore()
	{
		if (!MaxQuestScore)
		{
			if (_questScoreApplied)
			{
				_questScoreApplied = false;
				VRRig localRig = VRRig.LocalRig;
				if ((Object)(object)localRig != (Object)null)
				{
					localRig.SetQuestScore(0);
				}
			}
		}
		else if (!(Time.time <= _questScoreDelay))
		{
			_questScoreDelay = Time.time + 1f;
			_questScoreApplied = true;
			VRRig localRig2 = VRRig.LocalRig;
			if ((Object)(object)localRig2 != (Object)null)
			{
				localRig2.SetQuestScore(int.MaxValue);
			}
		}
	}

	private void ApplyQuestNoises()
	{
		if (!QuestNoises)
		{
			return;
		}
		ControllerInputPoller instance = ControllerInputPoller.instance;
		if (!((Object)(object)instance == (Object)null) && !(instance.rightControllerIndexFloat <= 0.5f) && !(Time.time <= _questNoiseDelay))
		{
			_questNoiseDelay = Time.time + 0.1f;
			MonkeBusinessStation[] array = Object.FindObjectsByType<MonkeBusinessStation>((FindObjectsSortMode)0);
			if (array.Length != 0 && (Object)(object)array[0] != (Object)null)
			{
				array[0].RedeemProgress();
			}
		}
	}

	private void ApplyNameTags()
	{
		if (!FlashNameTag && !GoldenNameTag)
		{
			if (_nameTagApplied)
			{
				_nameTagApplied = false;
				_nameTagState = false;
				VRRig localRig = VRRig.LocalRig;
				if ((Object)(object)localRig != (Object)null)
				{
					SetGoldNameTag(localRig, value: false);
				}
			}
			return;
		}
		VRRig localRig2 = VRRig.LocalRig;
		if ((Object)(object)localRig2 == (Object)null)
		{
			return;
		}
		if (FlashNameTag)
		{
			if (Time.time > _nameTagDelay)
			{
				_nameTagDelay = Time.time + 0.1f;
				_nameTagState = !_nameTagState;
			}
			SetGoldNameTag(localRig2, _nameTagState);
			_nameTagApplied = true;
		}
		else
		{
			SetGoldNameTag(localRig2, value: true);
			_nameTagApplied = true;
		}
	}

	private static void SetGoldNameTag(VRRig rig, bool value)
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		if (_goldNameTagField == null)
		{
			_goldNameTagField = typeof(VRRig).GetField("showGoldNameTag", BindingFlags.Instance | BindingFlags.NonPublic);
		}
		if (_goldNameTagField != null)
		{
			_goldNameTagField.SetValue(rig, value);
		}
		if ((Object)(object)rig.playerText1 != (Object)null)
		{
			((Graphic)rig.playerText1).color = (Color)(value ? new Color(1f, 0.83f, 0.32f) : Color.white);
		}
	}

	private void ApplyHoverboard()
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		GorillaTagger instance = GorillaTagger.Instance;
		if ((Object)(object)instance == (Object)null)
		{
			return;
		}
		if (SpawnHoverboard)
		{
			if (!_hoverboardSpawned)
			{
				GTPlayer instance2 = GTPlayer.Instance;
				Transform rightHandTransform = instance.rightHandTransform;
				if (!((Object)(object)instance2 == (Object)null) && !((Object)(object)rightHandTransform == (Object)null))
				{
					int actor = 0;
					try { if (NetworkSystem.Instance != null && NetworkSystem.Instance.LocalPlayer != null) actor = NetworkSystem.Instance.LocalPlayer.ActorNumber; } catch { }
					instance2.GrabPersonalHoverboard(false, rightHandTransform.position, rightHandTransform.rotation, Color.white, actor);
					_hoverboardSpawned = true;
				}
			}
		}
		else if (_hoverboardSpawned)
		{
			_hoverboardSpawned = false;
			GTPlayer instance3 = GTPlayer.Instance;
			if ((Object)(object)instance3 != (Object)null)
			{
				instance3.SetHoverActive(false);
			}
		}
	}

	public List<string> ActiveMods()
	{
		List<string> list = new List<string>();
		if (UpsideDownHead)
		{
			list.Add("Upside Down Head");
		}
		if (BrokenNeck)
		{
			list.Add("Broken Neck");
		}
		if (BackwardsHead)
		{
			list.Add("Backwards Head");
		}
		if (SidewaysHead)
		{
			list.Add("Sideways Head");
		}
		if (HeadBang)
		{
			list.Add("Head Bang");
		}
		if (SpinHead)
		{
			list.Add("Spin Head");
		}
		if (SpazHead)
		{
			list.Add("Spaz Head");
		}
		if (FlipHands)
		{
			list.Add("Flip Hands");
		}
		if (AutoClicker)
		{
			list.Add("Auto Clicker");
		}
		if (LoudHandTaps)
		{
			list.Add("Loud Hand Taps");
		}
		if (SilentHandTaps)
		{
			list.Add("Silent Hand Taps");
		}
		if (SilentTapsOnTag)
		{
			list.Add("Silent Taps On Tag");
		}
		if (Freecam)
		{
			list.Add("Freecam");
		}
		if (ThirdPersonCamera)
		{
			list.Add("Third Person");
		}
		if (FlipCamera)
		{
			list.Add("Flip Camera");
		}
		if (Nausea)
		{
			list.Add("Nausea");
		}
		if (MuteDJSets)
		{
			list.Add("Mute DJ Sets");
		}
		if (MaxQuestScore)
		{
			list.Add("Max Quest Score");
		}
		if (QuestNoises)
		{
			list.Add("Quest Noises");
		}
		if (GoldenNameTag)
		{
			list.Add("Golden Name Tag");
		}
		if (FlashNameTag)
		{
			list.Add("Flash Name Tag");
		}
		if (SpawnHoverboard)
		{
			list.Add("Spawn Hoverboard");
		}
		return list;
	}
}
