using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using GorillaLocomotion;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Sonder.Managers;

public class SafetyRuntime : MonoBehaviour
{
	private const float ReportThreshold = 0.35f;

	private bool _overlayApplied;

	private bool _powerOffActive;

	private Vector3 _deadPosition;

	private Vector3 _deadVelocity;

	private float _reportActionDelay;

	private float _reportNotifyDelay;

	private float _cacheNextTime;

	private GameObject _gamemodeButton;

	private bool _oculusHooked;

	public static SafetyRuntime Instance { get; private set; }

	public bool NoFinger { get; set; }

	public bool FakeOculusMenu { get; set; }

	public bool FakeReportMenu { get; set; }

	public bool FakeBrokenController { get; set; }

	public bool FakePowerOff { get; set; }

	public bool FakeValveTracking { get; set; }

	public bool AntiReportDisconnect { get; set; }

	public bool AntiReportNotify { get; set; }

	public bool AntiReportOculus { get; set; }

	public bool AntiKick { get; set; }

	public bool AntiBreakAudio { get; set; }

	public bool AutoClearCache { get; set; }

	public bool HideGamemodeButton { get; set; }

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
		UnhookOculusEvents();
	}

	private void Update()
	{
		if (NoFinger)
		{
			ZeroInputs();
		}
		ApplyFakeOculusMenu();
		ApplyFakeReportMenu();
		ApplyFakeBrokenController();
		ApplyFakePowerOff();
		ApplyFakeValveTracking();
		ApplyAntiReport();
		ApplyAntiReportOculus();
		ApplyAutoClearCache();
		ApplyHideGamemodeButton();
	}

	private static void ZeroInputs()
	{
		ControllerInputPoller instance = ControllerInputPoller.instance;
		if (!((Object)(object)instance == (Object)null))
		{
			instance.leftControllerGripFloat = 0f;
			instance.rightControllerGripFloat = 0f;
			instance.leftControllerIndexFloat = 0f;
			instance.rightControllerIndexFloat = 0f;
			instance.leftControllerPrimaryButton = false;
			instance.leftControllerSecondaryButton = false;
			instance.rightControllerPrimaryButton = false;
			instance.rightControllerSecondaryButton = false;
			instance.leftControllerPrimaryButtonTouch = false;
			instance.leftControllerSecondaryButtonTouch = false;
			instance.rightControllerPrimaryButtonTouch = false;
			instance.rightControllerSecondaryButtonTouch = false;
		}
	}

	private static void SetHandEnabled(bool value)
	{
		GorillaTagger instance = GorillaTagger.Instance;
		if (!((Object)(object)instance == (Object)null))
		{
			if ((Object)(object)instance.leftHandTriggerCollider != (Object)null)
			{
				instance.leftHandTriggerCollider.SetActive(value);
			}
			if ((Object)(object)instance.rightHandTriggerCollider != (Object)null)
			{
				instance.rightHandTriggerCollider.SetActive(value);
			}
		}
	}

	private void ApplyFakeOculusMenu()
	{
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		GTPlayer instance = GTPlayer.Instance;
		if ((Object)(object)instance == (Object)null)
		{
			return;
		}
		if (!FakeOculusMenu)
		{
			if (_overlayApplied)
			{
				SetHandEnabled(value: true);
				_overlayApplied = false;
			}
			return;
		}
		ControllerInputPoller instance2 = ControllerInputPoller.instance;
		int num;
		if ((Object)(object)instance2 != (Object)null)
		{
			num = (instance2.leftControllerPrimaryButton ? 1 : 0);
			if (num != 0)
			{
				ZeroInputs();
				instance.inOverlay = true;
				if ((Object)(object)Camera.main != (Object)null)
				{
					instance.GetControllerTransform(true).localPosition = new Vector3(238f, -90f, 0f);
					instance.GetControllerTransform(false).localPosition = new Vector3(-190f, 90f, 0f);
					instance.GetControllerTransform(true).rotation = ((Component)Camera.main).transform.rotation * Quaternion.Euler(-55f, 90f, 0f);
					instance.GetControllerTransform(false).rotation = ((Component)Camera.main).transform.rotation * Quaternion.Euler(-55f, -49f, 0f);
				}
			}
		}
		else
		{
			num = 0;
		}
		SetHandEnabled(num == 0);
	}

	private void ApplyFakeReportMenu()
	{
		GTPlayer instance = GTPlayer.Instance;
		if ((Object)(object)instance == (Object)null)
		{
			return;
		}
		if (!FakeReportMenu)
		{
			if (_overlayApplied)
			{
				instance.inOverlay = false;
			}
			return;
		}
		ControllerInputPoller instance2 = ControllerInputPoller.instance;
		if ((Object)(object)instance2 != (Object)null && instance2.leftControllerSecondaryButton)
		{
			ZeroInputs();
		}
		instance.inOverlay = (Object)(object)instance2 != (Object)null && instance2.leftControllerPrimaryButton;
		_overlayApplied = instance.inOverlay;
	}

	private void ApplyFakeBrokenController()
	{
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		GTPlayer instance = GTPlayer.Instance;
		GorillaTagger instance2 = GorillaTagger.Instance;
		ControllerInputPoller instance3 = ControllerInputPoller.instance;
		if (!((Object)(object)instance == (Object)null) && !((Object)(object)instance2 == (Object)null) && !((Object)(object)instance3 == (Object)null) && !((Object)(object)Camera.main == (Object)null) && FakeBrokenController && instance3.leftControllerPrimaryButton)
		{
			Transform leftHandTransform = instance2.leftHandTransform;
			Transform rightHandTransform = instance2.rightHandTransform;
			if (!((Object)(object)leftHandTransform == (Object)null) && !((Object)(object)rightHandTransform == (Object)null))
			{
				instance.GetControllerTransform(true).position = ((Component)instance.headCollider).transform.position + ((Component)instance.headCollider).transform.up * (-0.5f * instance.scale);
				instance.GetControllerTransform(true).rotation = ((Component)Camera.main).transform.rotation * Quaternion.Euler(-55f, 90f, 0f);
				instance.GetControllerTransform(false).position = rightHandTransform.position;
				instance.GetControllerTransform(false).rotation = rightHandTransform.rotation;
				instance3.leftControllerGripFloat = 0f;
				instance3.leftControllerIndexFloat = 0f;
				instance3.leftControllerPrimaryButton = false;
				instance3.leftControllerSecondaryButton = false;
				instance3.leftControllerPrimaryButtonTouch = false;
				instance3.leftControllerSecondaryButtonTouch = false;
			}
		}
	}

	private void ApplyFakePowerOff()
	{
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		if (!FakePowerOff)
		{
			if (_powerOffActive)
			{
				VRRig localRig = VRRig.LocalRig;
				if ((Object)(object)localRig != (Object)null)
				{
					((Behaviour)localRig).enabled = true;
				}
				_powerOffActive = false;
			}
			return;
		}
		GorillaTagger instance = GorillaTagger.Instance;
		ControllerInputPoller instance2 = ControllerInputPoller.instance;
		VRRig localRig2 = VRRig.LocalRig;
		if ((Object)(object)instance == (Object)null || (Object)(object)instance2 == (Object)null || (Object)(object)localRig2 == (Object)null || (Object)(object)instance.rigidbody == (Object)null)
		{
			return;
		}
		Rigidbody rigidbody = instance.rigidbody;
		if (instance2.leftControllerPrimaryButton)
		{
			if (!_powerOffActive)
			{
				_deadPosition = ((Component)rigidbody).transform.position;
				_deadVelocity = rigidbody.linearVelocity;
				_powerOffActive = true;
			}
			((Behaviour)localRig2).enabled = false;
			((Component)rigidbody).transform.position = _deadPosition;
			rigidbody.linearVelocity = _deadVelocity;
		}
		else if (_powerOffActive)
		{
			((Behaviour)localRig2).enabled = true;
			_powerOffActive = false;
		}
	}

	private void ApplyFakeValveTracking()
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		if (FakeValveTracking)
		{
			ControllerInputPoller instance = ControllerInputPoller.instance;
			VRRig localRig = VRRig.LocalRig;
			if (!((Object)(object)instance == (Object)null) && !((Object)(object)localRig == (Object)null) && !((Object)(object)localRig.head.rigTarget == (Object)null) && instance.rightControllerPrimaryButton)
			{
				((Component)localRig.head.rigTarget).transform.rotation = Quaternion.identity;
			}
		}
	}

	private void ApplyAntiReport()
	{
		if (!AntiReportDisconnect && !AntiReportNotify)
		{
			return;
		}
		AntiReport(delegate(VRRig rig)
		{
			if (AntiReportDisconnect && Time.time > _reportActionDelay)
			{
				_reportActionDelay = Time.time + 1f;
				NetworkSystem.Instance.ReturnToSinglePlayer();
				Debug.Log((object)("[Sonder] Anti-Report: " + PlayerName(rig) + " attempted to report you. Disconnected."));
			}
			if (AntiReportNotify && Time.time > _reportNotifyDelay)
			{
				_reportNotifyDelay = Time.time + 0.1f;
				Debug.Log((object)("[Sonder] Anti-Report: " + PlayerName(rig) + " is reporting you."));
			}
		});
	}

	private static void AntiReport(Action<VRRig> onReport)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkSystem.Instance.InRoom)
		{
			return;
		}
		foreach (GorillaPlayerScoreboardLine allScoreboardLine in GorillaScoreboardTotalUpdater.allScoreboardLines)
		{
			if ((Object)(object)allScoreboardLine == (Object)null || allScoreboardLine.linePlayer != NetworkSystem.Instance.LocalPlayer)
			{
				continue;
			}
			Vector3 position = ((Component)allScoreboardLine.reportButton).gameObject.transform.position;
			foreach (VRRig rig in GetRigs())
			{
				if (!((Object)(object)rig == (Object)null) && !rig.isLocal && OverlappingButton(rig, position, 0.35f))
				{
					onReport(rig);
				}
			}
		}
	}

	private static bool OverlappingButton(VRRig rig, Vector3 position, float threshold)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)rig.rightHandTransform == (Object)null || (Object)(object)rig.leftHandTransform == (Object)null)
		{
			return false;
		}
		if (!(Vector3.Distance(rig.rightHandTransform.position, position) < threshold))
		{
			return Vector3.Distance(rig.leftHandTransform.position, position) < threshold;
		}
		return true;
	}

	private void ApplyAntiReportOculus()
	{
		if (AntiReportOculus && !_oculusHooked)
		{
			HookOculusEvents();
			_oculusHooked = true;
		}
		else if (!AntiReportOculus && _oculusHooked)
		{
			UnhookOculusEvents();
			_oculusHooked = false;
		}
	}

	private void ApplyAutoClearCache()
	{
		if (AutoClearCache && !(Time.time <= _cacheNextTime))
		{
			_cacheNextTime = Time.time + 60f;
			GC.Collect();
		}
	}

	private void ApplyHideGamemodeButton()
	{
		if (HideGamemodeButton)
		{
			if ((Object)(object)_gamemodeButton == (Object)null)
			{
				_gamemodeButton = GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/TreeRoomInteractables/UI/ModeSelector_Group");
			}
			if ((Object)(object)_gamemodeButton != (Object)null && _gamemodeButton.activeSelf)
			{
				_gamemodeButton.SetActive(false);
			}
		}
		else if ((Object)(object)_gamemodeButton != (Object)null)
		{
			_gamemodeButton.SetActive(true);
			_gamemodeButton = null;
		}
	}

	private static string PlayerName(VRRig rig)
	{
		if (!((Object)(object)rig != (Object)null) || rig.Creator == null)
		{
			return "Unknown";
		}
		return rig.Creator.NickName;
	}

	private static IEnumerable<VRRig> GetRigs()
	{
		if ((Object)(object)VRRigCache.Instance != (Object)null && VRRigCache.ActiveRigs != null)
		{
			foreach (VRRig activeRig in VRRigCache.ActiveRigs)
			{
				yield return activeRig;
			}
			yield break;
		}
		VRRig[] array = Object.FindObjectsByType<VRRig>((FindObjectsSortMode)0);
		for (int i = 0; i < array.Length; i++)
		{
			yield return array[i];
		}
	}

	public void HookOculusEvents()
	{
		PhotonNetwork.NetworkingClient.EventReceived += OnEventReceivedOculusReport;
	}

	public void UnhookOculusEvents()
	{
		PhotonNetwork.NetworkingClient.EventReceived -= OnEventReceivedOculusReport;
	}

	private void OnEventReceivedOculusReport(EventData data)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (data.Code != 200)
			{
				return;
			}
			string text = PhotonNetwork.PhotonServerSettings.RpcList[int.Parse(((Hashtable)data.CustomData)[(byte)5].ToString())];
			object[] array = (object[])((Hashtable)data.CustomData)[(byte)4];
			if (text != "RPC_PlayHandTap" || (int)array[0] != 67)
			{
				return;
			}
			Player player = PhotonNetwork.NetworkingClient.CurrentRoom.GetPlayer(data.Sender, false);
			if (player != null)
			{
				VRRig rigFromPlayer = GetRigFromPlayer(player);
				if (!((Object)(object)rigFromPlayer == (Object)null) && !((Object)(object)rigFromPlayer.leftHandTransform == (Object)null) && !((Object)(object)rigFromPlayer.rightHandTransform == (Object)null) && Vector3.Distance(rigFromPlayer.leftHandTransform.position, rigFromPlayer.rightHandTransform.position) < 0.1f)
				{
					NetworkSystem.Instance.ReturnToSinglePlayer();
					Debug.Log((object)("[Sonder] Anti-Report (Oculus): " + player.NickName + " tapped report. Disconnected."));
				}
			}
		}
		catch
		{
		}
	}

	private static VRRig GetRigFromPlayer(Player player)
	{
		foreach (VRRig rig in GetRigs())
		{
			if ((Object)(object)rig != (Object)null && rig.Creator != null && rig.Creator.UserId == player.UserId)
			{
				return rig;
			}
		}
		return null;
	}
}
