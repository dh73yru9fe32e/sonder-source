using System;
using GorillaLocomotion;
using Sonder.Core;
using UnityEngine;
using Object = UnityEngine.Object;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Sonder.Managers;

public class ImportantRuntime : MonoBehaviour
{
	private bool _safeSpot;

	private bool _backupBinds;

	private Vector3 _safePosition;

	private bool _hasSafePosition;

	private Vector3 _homePosition;

	private bool _homeCaptured;

	private float _spawnTime;

	private Vector3 _lastSafePosition;

	private float _nextSafeSample;

	private float _nextSaveTime;

	private bool _comboHeld;

	private Action _backupHandler;

	public static ImportantRuntime Instance { get; private set; }

	public bool RespawnLock { get; set; }

	public bool AntiFall { get; set; }

	public bool AutoSave { get; set; }

	public bool PanicBind { get; set; }

	public bool SafeSpot
	{
		get
		{
			return _safeSpot;
		}
		set
		{
			if (_safeSpot != value)
			{
				_safeSpot = value;
				if (value)
				{
					SaveSpot();
				}
				else
				{
					RecallSpot();
				}
			}
		}
	}

	public bool BackupBinds
	{
		get
		{
			return _backupBinds;
		}
		set
		{
			if (_backupBinds != value)
			{
				_backupBinds = value;
				if (value)
				{
					EnsureBackupHandler();
					ModManager.Changed += _backupHandler;
					ModManager.SaveBinds("backup enabled");
				}
				else if (_backupHandler != null)
				{
					ModManager.Changed -= _backupHandler;
				}
			}
		}
	}

	private void Awake()
	{
		Instance = this;
		_spawnTime = Time.time;
	}

	private void OnDestroy()
	{
		if ((Object)(object)Instance == (Object)(object)this)
		{
			Instance = null;
		}
		if (_backupHandler != null)
		{
			ModManager.Changed -= _backupHandler;
		}
	}

	private void Update()
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		GTPlayer player = ExtraUtil.Player;
		if ((Object)(object)player == (Object)null)
		{
			return;
		}
		if (!_homeCaptured)
		{
			CaptureHome(player);
		}
		if (Time.time >= _nextSafeSample)
		{
			_nextSafeSample = Time.time + 0.25f;
			Vector3 position = ((Component)player).transform.position;
			if (player.BodyOnGround && position.y > -20f)
			{
				_lastSafePosition = position;
				if (!_homeCaptured)
				{
					CaptureHome(player);
				}
			}
		}
		ApplyRespawnLock(player);
		ApplyAntiFall(player);
		ApplyAutoSave();
		ApplyPanicBind();
	}

	private void CaptureHome(GTPlayer player)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		if (player.BodyOnGround || !(Time.time - _spawnTime < 5f))
		{
			_homePosition = ((Component)player).transform.position;
			_lastSafePosition = _homePosition;
			_homeCaptured = true;
		}
	}

	private void ApplyRespawnLock(GTPlayer player)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		if (!RespawnLock || !_homeCaptured)
		{
			return;
		}
		Vector3 position = ((Component)player).transform.position;
		if (position.y < _lastSafePosition.y - 35f || Vector3.Distance(position, _lastSafePosition) > 45f)
		{
			ExtraUtil.Teleport(_lastSafePosition);
			Rigidbody playerRigidBody = player.playerRigidBody;
			if ((Object)(object)playerRigidBody != (Object)null)
			{
				playerRigidBody.linearVelocity = Vector3.zero;
			}
			RuntimeLog.Write("respawn lock recalled");
		}
	}

	private void ApplyAntiFall(GTPlayer player)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		if (!AntiFall || !_homeCaptured)
		{
			return;
		}
		Vector3 position = ((Component)player).transform.position;
		if (position.y < -60f || position.y < _lastSafePosition.y - 45f)
		{
			ExtraUtil.Teleport(ExtraUtil.Grounded(_lastSafePosition));
			Rigidbody playerRigidBody = player.playerRigidBody;
			if ((Object)(object)playerRigidBody != (Object)null)
			{
				playerRigidBody.linearVelocity = Vector3.zero;
			}
			SonderAudio.Alert(0.4f);
			RuntimeLog.Write("anti fall caught");
		}
	}

	private void ApplyAutoSave()
	{
		if (AutoSave && !(Time.time < _nextSaveTime))
		{
			_nextSaveTime = Time.time + 60f;
			ModManager.SaveBinds("auto save");
		}
	}

	private void ApplyPanicBind()
	{
		if (!PanicBind)
		{
			_comboHeld = false;
			return;
		}
		ControllerInputPoller instance = ControllerInputPoller.instance;
		Keyboard current = Keyboard.current;
		bool flag = (Object)(object)instance != (Object)null && instance.leftControllerSecondaryButton && instance.rightControllerSecondaryButton;
		if (!flag && current != null)
		{
			flag = ((ButtonControl)current.f7Key).isPressed;
		}
		if (flag && !_comboHeld)
		{
			_comboHeld = true;
			ModManager.DisableAll();
			SonderAudio.Alert(0.8f);
			RuntimeLog.Write("panic bind fired");
		}
		else if (!flag)
		{
			_comboHeld = false;
		}
	}

	public void Panic()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = (_homeCaptured ? _homePosition : _lastSafePosition);
		GTPlayer player = ExtraUtil.Player;
		if (!((Object)(object)player == (Object)null))
		{
			if (val == Vector3.zero)
			{
				val = ((Component)player).transform.position;
			}
			ExtraUtil.Teleport(ExtraUtil.Grounded(val));
			Rigidbody playerRigidBody = player.playerRigidBody;
			if ((Object)(object)playerRigidBody != (Object)null)
			{
				playerRigidBody.linearVelocity = Vector3.zero;
			}
			SonderAudio.Alert(0.6f);
			RuntimeLog.Write("panic teleport");
		}
	}

	public void QuickHome()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		if (!_homeCaptured)
		{
			Panic();
			return;
		}
		ExtraUtil.Teleport(ExtraUtil.Grounded(_homePosition));
		GTPlayer player = ExtraUtil.Player;
		if ((Object)(object)player != (Object)null && (Object)(object)player.playerRigidBody != (Object)null)
		{
			player.playerRigidBody.linearVelocity = Vector3.zero;
		}
		SonderAudio.Alert(0.5f);
		RuntimeLog.Write("quick home");
	}

	private void SaveSpot()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		GTPlayer player = ExtraUtil.Player;
		if (!((Object)(object)player == (Object)null))
		{
			_safePosition = ((Component)player).transform.position;
			_hasSafePosition = true;
			SonderAudio.Alert(0.35f);
			RuntimeLog.Write("safe spot saved");
		}
	}

	private void RecallSpot()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		if (_hasSafePosition)
		{
			ExtraUtil.Teleport(ExtraUtil.Grounded(_safePosition));
			GTPlayer player = ExtraUtil.Player;
			if ((Object)(object)player != (Object)null && (Object)(object)player.playerRigidBody != (Object)null)
			{
				player.playerRigidBody.linearVelocity = Vector3.zero;
			}
			SonderAudio.Alert(0.35f);
			RuntimeLog.Write("safe spot recalled");
		}
	}

	private void EnsureBackupHandler()
	{
		if (_backupHandler == null)
		{
			_backupHandler = delegate
			{
				ModManager.SaveBinds("bind change");
			};
		}
	}
}
