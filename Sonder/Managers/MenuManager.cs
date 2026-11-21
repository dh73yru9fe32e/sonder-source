using System;
using System.Collections;
using Sonder.Core;
using Sonder.Menu;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.XR;

namespace Sonder.Managers;

public class MenuManager : MonoBehaviour
{
	private const float BaseMenuScale = 0.00033f;

	private const float ScreenDistance = 0.42f;

	private static readonly Vector3 MenuOffset;

	private GameObject _canvasRoot;

	private GameObject _pointerRoot;

	private Transform _leftHand;

	private Transform _head;

	private Transform _rightHand;

	private TextMeshProUGUI _fpsText;

	private GatorInputModule _inputModule;

	private bool _yHeld;

	private bool _needsFreezeCapture;

	private Vector3 _frozenMenuPosition;

	private Vector3 _frozenHeadPosition;

	private Vector3 _frozenHeadUp = Vector3.up;

	private Coroutine _animRoutine;

	private CanvasGroup _canvasGroup;

	private float _animScale;

	private float _animRoll;

	private float _animYaw;

	private float _animPitch;

	private float _animPush;

	private bool _ready;

	private float _smoothedFps;

	private bool _firstUpdateLogged;

	private bool _introShown;

	private bool _settingsDirty;

	private const float DockScale = 0.0008f;

	private const float DockStandoff = 1.05f;

	private const float DockHeight = 14f;

	private const float DockFieldOfView = 55f;

	private const float DockRayLength = 12f;

	private static readonly int DockLayerMask;

	private static bool _docked;

	private static Vector3 _dockAnchor;

	private PointerEventData _dockPointer;

	private GameObject _dockHovered;

	private bool _dockInputModuleWasEnabled;

	private Vector3 _dockViewPosition;

	private Quaternion _dockViewRotation;

	private Camera _dockCamera;

	private GameObject _dockCameraObject;

	private bool _dockClickArmed = true;

	private bool _dockOpenedMenu;

	private bool _dockCursorWasVisible;

	private CursorLockMode _dockCursorWasLock;

	private static bool _screenMode;
	private static Texture2D _bubbleCursor;

	private bool _firstPersonApplied;

	private float _screenScale = 0.00033f;

	private bool _screenInputModuleWasEnabled;

	// On PC the game shows a third-person shoulder camera; turning it off puts the screen on your eyes.
	public static bool PcFirstPerson { get; private set; } = true;

	private bool _screenCursorWasVisible;

	private CursorLockMode _screenCursorWasLock;

	public static MenuManager Instance { get; private set; }

	public static bool IsOpen { get; private set; }

	public static Transform RightHandPointer { get; private set; }

	public static Transform PointerCursor { get; private set; }

	public static Transform MenuCanvas { get; private set; }

	public static LineRenderer PointerLine { get; private set; }

	public static bool Docked => _docked;

	// Q on PC: the menu opens in first person in front of the camera and is clicked with the mouse.
	public static bool ScreenMode => _screenMode;

	public static bool PointerOverMenu => (_docked || _screenMode) && (Object)(object)Instance != (Object)null && (Object)(object)Instance._dockHovered != (Object)null;

	public static Camera ActiveCamera
	{
		get
		{
			if (_docked && (Object)(object)Instance != (Object)null && (Object)(object)Instance._dockCamera != (Object)null)
			{
				return Instance._dockCamera;
			}
			return EyeCamera;
		}
	}

	// The first-person camera the game renders your eyes with; Camera.main can be the shoulder camera.
	public static Camera EyeCamera
	{
		get
		{
			Camera camera = GTFirstPersonCamera.camera;
			if ((Object)(object)camera != (Object)null && ((Behaviour)camera).isActiveAndEnabled)
			{
				return camera;
			}
			return Camera.main;
		}
	}

	public static void Initialize()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected O, but got Unknown
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)Instance != (Object)null))
		{
			RuntimeLog.Reset();
			RuntimeLog.Write("initialize");
			GameObject val = new GameObject("Sonder_Runtime");
			Object.DontDestroyOnLoad((Object)val);
			val.AddComponent<MenuManager>();
			val.AddComponent<ModRuntime>();
		val.AddComponent<PcRuntime>();
		val.AddComponent<SonderIntroUI>();
			val.AddComponent<MovementPlusRuntime>();
			val.AddComponent<SafetyRuntime>();
			val.AddComponent<FunRuntime>();
			val.AddComponent<AdvantageRuntime>();
			val.AddComponent<RoomRuntime>();
			val.AddComponent<DetectionRuntime>();
			val.AddComponent<ExperimentalRuntime>();
			val.AddComponent<ImportantRuntime>();
			val.AddComponent<OverpoweredRuntime>();
			val.AddComponent<SuperInfectionRuntime>();
			val.AddComponent<SpawnerRuntime>();
			val.AddComponent<BlocksRuntime>();
			val.AddComponent<ProjectileRuntime>();
			val.AddComponent<SoundRuntime>();
			val.AddComponent<VisualRuntime>();
			val.AddComponent<GunRuntime>();
			val.AddComponent<GamemodeRuntime>();
			val.AddComponent<NotificationRuntime>();
			val.AddComponent<StumpBanner>();
			val.AddComponent<SoundboardRuntime>();
			val.AddComponent<SonderRPC>();
			val.AddComponent<BoardTextReplacer>();
			val.AddComponent<ModListOverlay>();
			val.AddComponent<DiscordPresence>();
			RuntimeLog.Write("runtime created");
		}
	}

	private void Awake()
	{
		if ((Object)(object)Instance != (Object)null && (Object)(object)Instance != (Object)(object)this)
		{
			GameObject gameObject = ((Component)Instance).gameObject;
			if ((Object)(object)gameObject != (Object)null && (Object)(object)gameObject != (Object)(object)((Component)this).gameObject)
			{
				Object.Destroy((Object)(object)gameObject);
			}
		}
		Instance = this;
		RuntimeLog.Write("awake enter");
		try
		{
			ModManager.Initialize();
			RuntimeLog.Write("mods ready");
		}
		catch (Exception ex)
		{
			RuntimeLog.Write("mod init failed: " + ex);
		}
		try
		{
			SonderAudio.Initialize(((Component)this).gameObject);
			SonderSettings.Changed += OnSettingsChanged;
			RuntimeLog.Write("settings ready");
		}
		catch (Exception ex2)
		{
			RuntimeLog.Write("settings init failed: " + ex2);
		}
		RuntimeLog.Write("awake");
	}

	private void OnDestroy()
	{
		SonderSettings.Changed -= OnSettingsChanged;
	}

	private void OnSettingsChanged()
	{
		_settingsDirty = true;
	}

	private void OnApplicationQuit()
	{
		RuntimeLog.Write("quit");
	}

	private void Update()
	{
		if (!_firstUpdateLogged)
		{
			_firstUpdateLogged = true;
			RuntimeLog.Write("first update");
		}
		ApplyFirstPerson();
		HandleDockInput();
		RigBank.Tick(GorillaTagger.Instance, ControllerInputPoller.instance);
		if (_settingsDirty)
		{
			_settingsDirty = false;
			RebuildFromSettings();
		}
		if (_ready && ((Object)(object)_canvasRoot == (Object)null || (Object)(object)_leftHand == (Object)null))
		{
			_ready = false;
			RightHandPointer = null;
			PointerLine = null;
		}
		if (_ready || TryBuild())
		{
			UpdateFps();
			HandleToggleInput();
			MenuBuilder.UpdateSearch();
			if ((Object)(object)_inputModule == (Object)null)
			{
				BuildEventSystem();
			}
			if (_screenMode && !IsOpen)
			{
				ExitScreenMode();
			}
			if (!_docked && !_screenMode && (Object)(object)_inputModule != (Object)null)
			{
				((BaseInputModule)_inputModule).Process();
			}
		}
	}

	private void LateUpdate()
	{
		MenuBuilder.UpdateSwipe();
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		if (_docked)
		{
			DockLateUpdate();
		}
		else
		{
			if ((Object)(object)_canvasRoot == (Object)null || !_canvasRoot.activeSelf || (Object)(object)_leftHand == (Object)null)
			{
				return;
			}
			Transform val = ((SonderSettings.RightHandMenu && (Object)(object)_rightHand != (Object)null) ? _rightHand : _leftHand);
			if ((Object)(object)val == (Object)null)
			{
				return;
			}
			Transform transform = _canvasRoot.transform;
			Vector3 val2 = val.position + val.rotation * MenuOffset;
			Camera screenCamera = EyeCamera;
			if (_screenMode && (Object)(object)screenCamera != (Object)null)
			{
				// Ride along with the camera so it stays on screen while you fly and look around.
				Transform view = ((Component)screenCamera).transform;
				_needsFreezeCapture = false;
				_frozenMenuPosition = view.position + view.forward * ScreenDistance;
				_frozenHeadPosition = view.position;
				_frozenHeadUp = view.up;
				// Size it to fill the middle of the screen: about 70% of the height or 60% of the width.
				RectTransform rect = _canvasRoot.GetComponent<RectTransform>();
				if ((Object)(object)rect != (Object)null && rect.rect.height > 1f && rect.rect.width > 1f)
				{
					float viewHeight = 2f * ScreenDistance * Mathf.Tan(screenCamera.fieldOfView * 0.5f * ((float)Math.PI / 180f));
					float viewWidth = viewHeight * screenCamera.aspect;
					_screenScale = Mathf.Min(viewHeight * 0.7f / rect.rect.height, viewWidth * 0.6f / rect.rect.width);
				}
			}
			if (_needsFreezeCapture)
			{
				// Freeze straight in front of your eyes, standing upright, whatever angle the hand was at.
				Transform view = ((Object)(object)_head != (Object)null) ? _head : (((Object)(object)Camera.main != (Object)null) ? ((Component)Camera.main).transform : null);
				if ((Object)(object)view != (Object)null)
				{
					_needsFreezeCapture = false;
					Vector3 flat = view.forward;
					flat.y = 0f;
					flat = (flat.sqrMagnitude < 0.001f) ? Vector3.forward : flat.normalized;
					_frozenMenuPosition = view.position + flat * 0.45f - Vector3.up * 0.12f;
					_frozenHeadPosition = new Vector3(view.position.x, _frozenMenuPosition.y, view.position.z);
					_frozenHeadUp = Vector3.up;
				}
			}
			Vector3 position;
			Vector3 val3;
			if ((Object)(object)_head != (Object)null)
			{
				position = _head.position;
				val3 = _head.up;
			}
			else
			{
				Camera main = Camera.main;
				if ((Object)(object)main == (Object)null)
				{
					return;
				}
				position = ((Component)main).transform.position;
				val3 = ((Component)main).transform.up;
			}
			if (!XRSettings.isDeviceActive)
			{
				// Stay where the menu was opened instead of following the hand.
				val2 = _frozenMenuPosition;
				position = _frozenHeadPosition;
				val3 = _frozenHeadUp;
			}
			Vector3 val4 = val2 - position;
			if (!(val4.sqrMagnitude < 0.0001f))
			{
				val4.Normalize();
				Vector3 val5 = (transform.position = val2 + val4 * _animPush);
				Vector3 val7 = val5 - position;
				Vector3 normalized = val7.normalized;
				if (Mathf.Abs(Vector3.Dot(normalized, val3)) > 0.97f)
				{
					val3 = ((Mathf.Abs(Vector3.Dot(normalized, Vector3.forward)) < 0.97f) ? Vector3.forward : Vector3.right);
				}
				transform.rotation = Quaternion.LookRotation(normalized, val3) * Quaternion.Euler(_animPitch, _animYaw, _animRoll);
				float vrScale = XRSettings.isDeviceActive ? 0.0006f : 0.00033f;
		transform.localScale = Vector3.one * ((_screenMode ? _screenScale : (vrScale * SonderSettings.MenuScale)) * _animScale);
				UpdatePointerAnchor();
			}
		}
	}

	private void UpdatePointerAnchor()
	{
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_pointerRoot == (Object)null || (Object)(object)_rightHand == (Object)null)
		{
			return;
		}
		Transform transform = _pointerRoot.transform;
		// Out of the middle of the index and middle fingertips, following the controller (see FingerAim).
		FingerAim.Sample(left: false);
		if (FingerAim.TryGetRay(false, out var origin, out var direction))
		{
			transform.position = origin;
			transform.rotation = Quaternion.LookRotation(direction, _rightHand.up);
		}
		// The line was drawn earlier in the frame, before the hand moved; re-attach it so it starts at the hand.
		if ((Object)(object)PointerLine != (Object)null && PointerLine.positionCount >= 2)
		{
			float length = Vector3.Distance(PointerLine.GetPosition(0), PointerLine.GetPosition(1));
			PointerLine.SetPosition(0, transform.position);
			PointerLine.SetPosition(1, transform.position + transform.forward * length);
		}
	}

	private bool TryBuild()
	{
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Expected O, but got Unknown
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		GorillaTagger instance = GorillaTagger.Instance;
		if ((Object)(object)instance == (Object)null)
		{
			return false;
		}
		VRRig offlineVRRig = instance.offlineVRRig;
		if ((Object)(object)offlineVRRig == (Object)null || (Object)(object)offlineVRRig.leftHandTransform == (Object)null || (Object)(object)offlineVRRig.rightHandTransform == (Object)null)
		{
			return false;
		}
		if ((Object)(object)_canvasRoot != (Object)null)
		{
			_canvasRoot.SetActive(false);
			Object.Destroy((Object)(object)_canvasRoot);
			_canvasRoot = null;
		}
		if ((Object)(object)_pointerRoot != (Object)null)
		{
			_pointerRoot.SetActive(false);
			Object.Destroy((Object)(object)_pointerRoot);
			_pointerRoot = null;
		}
		RightHandPointer = null;
		PointerLine = null;
		PointerCursor = null;
		_leftHand = offlineVRRig.leftHandTransform;
		_rightHand = offlineVRRig.rightHandTransform;
		_head = (((Object)(object)instance.headCollider != (Object)null) ? ((Component)instance.headCollider).transform : null);
		if ((Object)(object)_head == (Object)null && (Object)(object)offlineVRRig.headMesh != (Object)null)
		{
			_head = offlineVRRig.headMesh.transform;
		}
		GameObject val = new GameObject("Sonder_Canvas");
		val.transform.SetParent(((Component)this).transform, false);
		val.transform.localPosition = Vector3.zero;
		val.transform.localRotation = Quaternion.identity;
		val.transform.localScale = Vector3.zero;
		Canvas obj = val.AddComponent<Canvas>();
		obj.renderMode = (RenderMode)2;
		obj.sortingOrder = 100;
		_canvasGroup = val.AddComponent<CanvasGroup>();
		_canvasGroup.alpha = 0f;
		val.GetComponent<RectTransform>().sizeDelta = new Vector2(460f, 460f);
		MenuBuilder.Build(val);
		_fpsText = MenuBuilder.FpsText;
		MenuCanvas = val.transform;
		BuildPointer(offlineVRRig);
		BuildEventSystem();
		_canvasRoot = val;
		val.SetActive(false);
		if ((Object)(object)_pointerRoot != (Object)null)
		{
			_pointerRoot.SetActive(false);
		}
		IsOpen = false;
		_ready = true;
		return true;
	}

	private void RebuildFromSettings()
	{
		if ((Object)(object)_canvasRoot == (Object)null)
		{
			return;
		}
		bool isOpen = IsOpen;
		bool inHome = ModManager.InHome;
		int currentCategoryIndex = ModManager.CurrentCategoryIndex;
		int modPage = ModManager.ModPage;
		if (_animRoutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(_animRoutine);
			_animRoutine = null;
		}
		IsOpen = false;
		_ready = false;
		_canvasGroup = null;
		_animScale = 1f;
		_animRoll = 0f;
		_animYaw = 0f;
		_animPitch = 0f;
		_animPush = 0f;
		if ((Object)(object)_canvasRoot != (Object)null)
		{
			_canvasRoot.SetActive(false);
		}
		if ((Object)(object)_pointerRoot != (Object)null)
		{
			_pointerRoot.SetActive(false);
		}
		if (!TryBuild())
		{
			return;
		}
		if (!inHome)
		{
			ModManager.SetCategory(currentCategoryIndex);
			ModManager.ShowMods();
			ModManager.SetModPage(modPage);
			MenuBuilder.RebuildCurrent();
		}
		if (isOpen)
		{
			IsOpen = true;
			_canvasRoot.SetActive(true);
			if ((Object)(object)_canvasGroup != (Object)null)
			{
				_canvasGroup.alpha = 1f;
			}
			if ((Object)(object)_pointerRoot != (Object)null)
			{
				_pointerRoot.SetActive(true);
			}
		}
	}

	private void BuildPointer(VRRig rig)
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_0240: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Unknown result type (might be due to invalid IL or missing references)
		//IL_0262: Unknown result type (might be due to invalid IL or missing references)
		//IL_0275: Unknown result type (might be due to invalid IL or missing references)
		_pointerRoot = GameObject.CreatePrimitive((PrimitiveType)0);
		((Object)_pointerRoot).name = "Sonder_Pointer";
		_pointerRoot.layer = 0;
		_pointerRoot.transform.SetParent(rig.rightHandTransform, false);
		_pointerRoot.transform.localPosition = new Vector3(0f, 0f, 0.04f);
		_pointerRoot.transform.localRotation = Quaternion.identity;
		_pointerRoot.transform.localScale = Vector3.one * (0.005f * SonderSettings.PointerSize);
		// Nothing presses buttons by touch, so the sphere's collider only bumped into the world. Its ball sat
		// on the fingers too; the line and the cursor at its end are all that should show.
		Object.Destroy((Object)(object)_pointerRoot.GetComponent<Collider>());
		Renderer component2 = _pointerRoot.GetComponent<Renderer>();
		if ((Object)(object)component2 != (Object)null)
		{
			component2.enabled = false;
		}
		PointerLine = _pointerRoot.AddComponent<LineRenderer>();
		PointerLine.useWorldSpace = true;
		PointerLine.positionCount = 2;
		PointerLine.startWidth = 0.002f * SonderSettings.PointerSize;
		PointerLine.endWidth = 0.0009f * SonderSettings.PointerSize;
		PointerLine.numCapVertices = 4;
		PointerLine.alignment = (LineAlignment)0;
		PointerLine.textureMode = (LineTextureMode)0;
		Color ink = SonderTheme.Border;
		PointerLine.startColor = ink;
		ink.a = 0.35f;
		PointerLine.endColor = ink;
		Material val2 = MenuBuilder.CreateMaterial(Color.white, "Sprites/Default", "UI/Default", "Unlit/Color");
		if ((Object)(object)val2 != (Object)null)
		{
			((Renderer)PointerLine).material = val2;
		}
		GameObject val3 = GameObject.CreatePrimitive((PrimitiveType)0);
		((Object)val3).name = "Sonder_Cursor";
		val3.layer = 0;
		Collider component3 = val3.GetComponent<Collider>();
		if ((Object)(object)component3 != (Object)null)
		{
			Object.Destroy((Object)(object)component3);
		}
		val3.transform.SetParent(_pointerRoot.transform, false);
		val3.transform.localPosition = Vector3.zero;
		val3.transform.localRotation = Quaternion.identity;
		val3.transform.localScale = Vector3.one;
		Renderer component4 = val3.GetComponent<Renderer>();
		Material val4 = MenuBuilder.CreateMaterial(SonderTheme.Border, "Unlit/Color", "Sprites/Default", "UI/Default");
		if ((Object)(object)component4 != (Object)null && (Object)(object)val4 != (Object)null)
		{
			component4.material = val4;
			component4.enabled = true;
		}
		PointerCursor = val3.transform;
		RightHandPointer = _pointerRoot.transform;
	}

	private void BuildEventSystem()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		if (!((Object)(object)_inputModule != (Object)null))
		{
			EventSystem val = EventSystem.current;
			if ((Object)(object)val == (Object)null)
			{
				val = Object.FindAnyObjectByType<EventSystem>((FindObjectsInactive)1);
			}
			if ((Object)(object)val == (Object)null)
			{
				GameObject val2 = new GameObject("Sonder_EventSystem");
				Object.DontDestroyOnLoad((Object)val2);
				val = val2.AddComponent<EventSystem>();
				val.sendNavigationEvents = false;
			}
			_inputModule = ((Component)val).gameObject.AddComponent<GatorInputModule>();
		}
	}

	public static void EnsureEventSystem()
	{
		if ((Object)(object)Instance != (Object)null)
		{
			Instance.BuildEventSystem();
		}
	}

	private void UpdateFps()
	{
		if (!((Object)(object)_fpsText == (Object)null))
		{
			float num = Mathf.Max(Time.unscaledDeltaTime, 0.0001f);
			float num2 = 1f / num;
			_smoothedFps = ((_smoothedFps <= 0f) ? num2 : Mathf.Lerp(_smoothedFps, num2, 0.08f));
			((TMP_Text)_fpsText).text = Mathf.RoundToInt(_smoothedFps) + " FPS";
		}
	}

	private void HandleToggleInput()
	{
		if (!_docked)
		{
			ControllerInputPoller instance = ControllerInputPoller.instance;
			bool flag = (Object)(object)instance != (Object)null && instance.leftControllerSecondaryButton;
			// Press Y once to open (the menu then stays where it opened), press again to close.
			if (flag && !_yHeld)
			{
				SetOpen(!IsOpen);
			}
			_yHeld = flag;
		}
	}

	public static void ForceCloseWorld()
	{
		if ((Object)(object)Instance != (Object)null && IsOpen)
		{
			Instance.SetOpen(open: false);
		}
	}

	public static void ToggleDock()
	{
		if (_docked)
		{
			Undock();
		}
		else
		{
			DockToStump();
		}
	}

	public static bool DockToStump()
	{
		if ((Object)(object)Instance == (Object)null)
		{
			return false;
		}
		return Instance.TakeDock();
	}

	public static void Undock()
	{
		if ((Object)(object)Instance != (Object)null)
		{
			Instance.ReleaseDock();
		}
	}

	private unsafe bool TakeDock()
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Expected O, but got Unknown
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Expected O, but got Unknown
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Expected O, but got Unknown
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Expected O, but got Unknown
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		StumpBanner instance = StumpBanner.Instance;
		if ((Object)(object)instance == (Object)null || !instance.HasAnchor)
		{
			RuntimeLog.Write("dock refused: no stump anchor");
			return false;
		}
		if ((Object)(object)_canvasRoot == (Object)null && !TryBuild())
		{
			return false;
		}
		BuildDockCamera();
		_dockAnchor = instance.Anchor + Vector3.up * 14f;
		float num = Mathf.Max(1.05f, instance.Radius * 0.5f);
		Vector3 val = ExtraUtil.PlayerPosition() - instance.Anchor;
		val.y = 0f;
		if (val.sqrMagnitude < 0.04f)
		{
			val = Vector3.forward;
		}
		val.Normalize();
		_dockViewPosition = _dockAnchor + val * num;
		Vector3 val2 = _dockAnchor - _dockViewPosition;
		_dockViewRotation = Quaternion.LookRotation(val2.normalized, Vector3.up);
		PlaceCamera();
		_dockHovered = null;
		_dockClickArmed = Mouse.current == null || !Mouse.current.leftButton.isPressed;
		if ((Object)(object)_inputModule != (Object)null)
		{
			_dockInputModuleWasEnabled = ((Behaviour)_inputModule).enabled;
			((Behaviour)_inputModule).enabled = false;
		}
		if (!IsOpen)
		{
			_dockOpenedMenu = true;
			SetOpen(open: true);
		}
		if ((Object)(object)_canvasRoot != (Object)null)
		{
			_canvasRoot.SetActive(true);
		}
		if ((Object)(object)_pointerRoot != (Object)null)
		{
			_pointerRoot.SetActive(false);
		}
		Camera.onPreCull = (Camera.CameraCallback)Delegate.Remove((Delegate)(object)Camera.onPreCull, (Delegate)new Camera.CameraCallback(DockPreCull));
		Camera.onPreCull = (Camera.CameraCallback)Delegate.Combine((Delegate)(object)Camera.onPreCull, (Delegate)new Camera.CameraCallback(DockPreCull));
		_dockCursorWasVisible = Cursor.visible;
		_dockCursorWasLock = Cursor.lockState;
		Cursor.visible = true;
		Cursor.lockState = (CursorLockMode)0;
		Cursor.SetCursor(EnsureBubbleCursor(), new Vector2(16f, 16f), CursorMode.Auto);
		_docked = true;
		val2 = _dockAnchor;
		RuntimeLog.Write("dock engaged at " + ((object)(*(Vector3*)(&val2))/*cast due to constrained. prefix*/).ToString());
		return true;
	}

	private void ReleaseDock()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected O, but got Unknown
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Expected O, but got Unknown
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (_docked)
		{
			_docked = false;
			_dockHovered = null;
			Camera.onPreCull = (Camera.CameraCallback)Delegate.Remove((Delegate)(object)Camera.onPreCull, (Delegate)new Camera.CameraCallback(DockPreCull));
			Cursor.visible = _dockCursorWasVisible;
			Cursor.lockState = _dockCursorWasLock;
		Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
			DestroyDockCamera();
			if ((Object)(object)_pointerRoot != (Object)null)
			{
				_pointerRoot.SetActive(IsOpen);
			}
			if ((Object)(object)_inputModule != (Object)null)
			{
				((Behaviour)_inputModule).enabled = _dockInputModuleWasEnabled;
			}
			if (_dockOpenedMenu)
			{
				_dockOpenedMenu = false;
				SetOpen(open: false);
			}
			RuntimeLog.Write("dock released");
		}
	}

	private void OnDisable()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		Camera.onPreCull = (Camera.CameraCallback)Delegate.Remove((Delegate)(object)Camera.onPreCull, (Delegate)new Camera.CameraCallback(DockPreCull));
		if (_docked)
		{
			_docked = false;
			DestroyDockCamera();
		}
	}

	private void EnsureDockCamera()
	{
		if (!((Object)(object)_canvasRoot == (Object)null))
		{
			Canvas component = _canvasRoot.GetComponent<Canvas>();
			if ((Object)(object)component != (Object)null)
			{
				component.worldCamera = (((Object)(object)_dockCamera != (Object)null) ? _dockCamera : Camera.main);
			}
		}
	}

	private void BuildDockCamera()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected O, but got Unknown
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Invalid comparison between Unknown and I4
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)_dockCamera != (Object)null))
		{
			Camera main = Camera.main;
			_dockCameraObject = new GameObject("Sonder_DockCamera");
			Object.DontDestroyOnLoad((Object)(object)_dockCameraObject);
			_dockCamera = _dockCameraObject.AddComponent<Camera>();
			((Behaviour)_dockCamera).enabled = true;
			((Component)_dockCamera).tag = "Untagged";
			if ((Object)(object)main != (Object)null)
			{
				_dockCamera.clearFlags = (CameraClearFlags)(((int)main.clearFlags != 2) ? 1 : 2);
				_dockCamera.backgroundColor = main.backgroundColor;
				_dockCamera.fieldOfView = main.fieldOfView;
				_dockCamera.nearClipPlane = main.nearClipPlane;
				_dockCamera.farClipPlane = main.farClipPlane;
				_dockCamera.targetDisplay = main.targetDisplay;
				_dockCamera.stereoTargetEye = main.stereoTargetEye;
				_dockCamera.allowHDR = main.allowHDR;
				_dockCamera.allowMSAA = main.allowMSAA;
				_dockCamera.depth = main.depth + 10f;
			}
			else
			{
				_dockCamera.clearFlags = (CameraClearFlags)1;
				_dockCamera.depth = 100f;
			}
			_dockCamera.cullingMask = (((Object)(object)main != (Object)null) ? main.cullingMask : (-1)) | DockLayerMask;
			_dockCamera.rect = new Rect(0f, 0f, 1f, 1f);
			_dockCamera.fieldOfView = 55f;
			_dockCamera.nearClipPlane = 0.01f;
		}
	}

	private void DestroyDockCamera()
	{
		if ((Object)(object)_canvasRoot != (Object)null)
		{
			Canvas component = _canvasRoot.GetComponent<Canvas>();
			if ((Object)(object)component != (Object)null)
			{
				component.worldCamera = Camera.main;
			}
		}
		if ((Object)(object)_dockCameraObject != (Object)null)
		{
			Object.Destroy((Object)(object)_dockCameraObject);
		}
		_dockCameraObject = null;
		_dockCamera = null;
	}

	private void DockLateUpdate()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)_canvasRoot == (Object)null))
		{
			PlaceCamera();
			Transform transform = _canvasRoot.transform;
			transform.position = _dockAnchor;
			Vector3 val = _dockAnchor - _dockViewPosition;
			if (val.sqrMagnitude < 0.0001f)
			{
				val = Vector3.forward;
			}
			transform.rotation = Quaternion.LookRotation(val.normalized, Vector3.up);
			transform.localScale = Vector3.one * (0.0008f * SonderSettings.MenuScale);
		}
	}

	private void PlaceCamera()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		Camera val = (((Object)(object)_dockCamera != (Object)null) ? _dockCamera : Camera.main);
		if (!((Object)(object)val == (Object)null))
		{
			((Component)val).transform.SetPositionAndRotation(_dockViewPosition, _dockViewRotation);
		}
	}

	private void DockPreCull(Camera camera)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		if (_docked && !((Object)(object)_dockCamera == (Object)null))
		{
			((Component)_dockCamera).transform.SetPositionAndRotation(_dockViewPosition, _dockViewRotation);
		}
	}

	private void HandleDockInput()
	{
		Keyboard current = Keyboard.current;
		bool typing = MenuBuilder.SearchActive || RoomRuntime.Typing;
		if (current != null && !typing && ((ButtonControl)current.qKey).wasPressedThisFrame)
		{
			ToggleScreenMode();
		}
		if (_screenMode)
		{
			ProcessDockMouse();
		}
		if (_docked)
		{
			EnsureDockCamera();
			ProcessDockMouse();
		}
	}

	public static void TogglePcFirstPerson()
	{
		PcFirstPerson = !PcFirstPerson;
	}

	private void ApplyFirstPerson()
	{
		GorillaTagger tagger = GorillaTagger.Instance;
		GameObject thirdPerson = (((Object)(object)tagger != (Object)null) ? tagger.thirdPersonCamera : null);
		if ((Object)(object)thirdPerson == (Object)null)
		{
			return;
		}
		bool want = PcFirstPerson && !XRSettings.isDeviceActive;
		if (want)
		{
			if (thirdPerson.activeSelf)
			{
				thirdPerson.SetActive(false);
				if (!_firstPersonApplied)
				{
					RuntimeLog.Write("first person on");
				}
			}
			_firstPersonApplied = true;
		}
		else if (_firstPersonApplied)
		{
			thirdPerson.SetActive(true);
			_firstPersonApplied = false;
			RuntimeLog.Write("first person off");
		}
	}

	private static Texture2D EnsureBubbleCursor()
	{
		if ((Object)(object)_bubbleCursor != (Object)null)
		{
			return _bubbleCursor;
		}
		int size = 32;
		Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
		tex.filterMode = FilterMode.Bilinear;
		Color[] pixels = new Color[size * size];
		float center = (size - 1) * 0.5f;
		for (int y = 0; y < size; y++)
		{
			for (int x = 0; x < size; x++)
			{
				float dx = x - center;
				float dy = y - center;
				float dist = Mathf.Sqrt(dx * dx + dy * dy);
				float radius = size * 0.45f;
				if (dist <= radius)
				{
					float t = dist / radius;
					float alpha = Mathf.Clamp01(1f - t * t);
					pixels[y * size + x] = new Color(0.7f, 0.9f, 1f, alpha * 0.85f);
				}
				else
				{
					pixels[y * size + x] = Color.clear;
				}
			}
		}
		tex.SetPixels(pixels);
		tex.Apply();
		_bubbleCursor = tex;
		return _bubbleCursor;
	}

	public static void ToggleScreenMode()
	{
		if ((Object)(object)Instance == (Object)null)
		{
			return;
		}
		if (_screenMode)
		{
			Instance.ExitScreenMode();
			Instance.SetOpen(open: false);
		}
		else
		{
			Instance.EnterScreenMode();
		}
	}

	private void EnterScreenMode()
	{
		if (_docked)
		{
			ReleaseDock();
		}
		if ((Object)(object)_canvasRoot == (Object)null && !TryBuild())
		{
			return;
		}
		_screenMode = true;
		_dockHovered = null;
		_dockClickArmed = Mouse.current == null || !Mouse.current.leftButton.isPressed;
		if ((Object)(object)_inputModule != (Object)null)
		{
			_inputModule.ClearHover();
			_screenInputModuleWasEnabled = ((Behaviour)_inputModule).enabled;
			((Behaviour)_inputModule).enabled = false;
		}
		_screenCursorWasVisible = Cursor.visible;
		_screenCursorWasLock = Cursor.lockState;
		Cursor.visible = true;
		Cursor.lockState = (CursorLockMode)0;
		Cursor.SetCursor(EnsureBubbleCursor(), new Vector2(16f, 16f), CursorMode.Auto);
		if (!IsOpen)
		{
			SetOpen(open: true);
		}
		if ((Object)(object)_pointerRoot != (Object)null)
		{
			_pointerRoot.SetActive(false);
		}
		RuntimeLog.Write("screen menu on");
	}

	private void ExitScreenMode()
	{
		if (_screenMode)
		{
			_screenMode = false;
			if ((Object)(object)_dockHovered != (Object)null && _dockPointer != null)
			{
				ExecuteEvents.Execute<IPointerExitHandler>(_dockHovered, (BaseEventData)(object)_dockPointer, ExecuteEvents.pointerExitHandler);
			}
			_dockHovered = null;
			if ((Object)(object)_inputModule != (Object)null)
			{
				((Behaviour)_inputModule).enabled = _screenInputModuleWasEnabled;
			}
			Cursor.visible = _screenCursorWasVisible;
			Cursor.lockState = _screenCursorWasLock;
		Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
			RuntimeLog.Write("screen menu off");
		}
	}

	private void ProcessDockMouse()
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Expected O, but got Unknown
		Mouse current = Mouse.current;
		EventSystem current2 = EventSystem.current;
		Camera val = (((Object)(object)_dockCamera != (Object)null) ? _dockCamera : EyeCamera);
		if (current == null || (Object)(object)current2 == (Object)null || (Object)(object)val == (Object)null || (Object)(object)_canvasRoot == (Object)null)
		{
			return;
		}
		if (_dockPointer == null)
		{
			_dockPointer = new PointerEventData(current2);
		}
		Vector2 val2 = ((InputControl<Vector2>)(object)((Pointer)current).position).ReadValue();
		_dockPointer.position = val2;
		_dockPointer.delta = ((InputControl<Vector2>)(object)((Pointer)current).delta).ReadValue();
		_dockPointer.scrollDelta = Vector2.zero;
		_dockPointer.button = (PointerEventData.InputButton)0;
		_dockPointer.clickCount = 0;
		MenuButton menuButton = null;
		RaycastHit val3 = default(RaycastHit);
		if (Physics.Raycast(val.ScreenPointToRay((val2)), out val3, 12f, DockLayerMask, (QueryTriggerInteraction)2))
		{
			menuButton = ((Component)val3.collider).GetComponentInParent<MenuButton>();
		}
		MenuButton.FavoriteModifier = Keyboard.current != null && ((ButtonControl)Keyboard.current.fKey).isPressed;
		if (!current.leftButton.isPressed)
		{
			_dockClickArmed = true;
		}
		GameObject val4 = (((Object)(object)menuButton != (Object)null) ? ((Component)menuButton).gameObject : null);
		if ((Object)(object)val4 != (Object)(object)_dockHovered)
		{
			if ((Object)(object)_dockHovered != (Object)null)
			{
				ExecuteEvents.Execute<IPointerExitHandler>(_dockHovered, (BaseEventData)(object)_dockPointer, ExecuteEvents.pointerExitHandler);
			}
			_dockHovered = val4;
			if ((Object)(object)_dockHovered != (Object)null)
			{
				ExecuteEvents.Execute<IPointerEnterHandler>(_dockHovered, (BaseEventData)(object)_dockPointer, ExecuteEvents.pointerEnterHandler);
			}
		}
		if ((Object)(object)val4 != (Object)null && current.leftButton.wasPressedThisFrame && _dockClickArmed)
		{
			_dockClickArmed = false;
			ExecuteEvents.Execute<IPointerClickHandler>(val4, (BaseEventData)(object)_dockPointer, ExecuteEvents.pointerClickHandler);
		}
	}

	private void SetOpen(bool open)
	{
		if (open && !IsOpen)
		{
			_needsFreezeCapture = !XRSettings.isDeviceActive;
		}
		// Closing the menu also closes the search keyboard, so it is not left floating.
		if (!open && MenuBuilder.SearchActive)
		{
			MenuBuilder.SetSearchActive(active: false);
		}
		IsOpen = open;
		if (!open)
		{
			MenuBuilder.SetSearchActive(active: false);
		}
		if ((Object)(object)_pointerRoot != (Object)null)
		{
			_pointerRoot.SetActive(open && !_screenMode);
		}
		if (!open && (Object)(object)_inputModule != (Object)null)
		{
			_inputModule.ClearHover();
		}
		if (!open)
		{
			MenuBuilder.HideIntro();
		}
		if (_animRoutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(_animRoutine);
		}
		_animRoutine = ((MonoBehaviour)this).StartCoroutine(open ? PlayOpen() : PlayClose());
		if (open && !_introShown)
		{
			MenuBuilder.ShowIntro();
			((MonoBehaviour)this).StartCoroutine(PlayIntroFade());
			_introShown = true;
		}
	}

	private IEnumerator PlayIntroFade()
	{
		if (!((Object)(object)MenuBuilder.IntroGroup == (Object)null))
		{
			float progress = 0f;
			float duration = 0.35f;
			while (progress < 1f)
			{
				progress += Time.unscaledDeltaTime / duration;
				MenuBuilder.IntroGroup.alpha = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(progress));
				yield return null;
			}
			MenuBuilder.IntroGroup.alpha = 1f;
		}
	}

	private IEnumerator PlayOpen()
	{
		if ((Object)(object)_canvasRoot == (Object)null)
		{
			yield break;
		}
		_canvasRoot.SetActive(true);
		float startScale = ((_animScale <= 0.001f) ? 0.55f : _animScale);
		float progress = 0f;
		float duration = 0.52f / SonderSettings.AnimationSpeed;
		if ((Object)(object)_canvasGroup != (Object)null)
		{
			_canvasGroup.alpha = 0f;
		}
		while (progress < 1f)
		{
			if ((Object)(object)_canvasRoot == (Object)null)
			{
				_animRoutine = null;
				yield break;
			}
			progress += Time.unscaledDeltaTime / duration;
			float num = Mathf.Clamp01(progress);
			float num2 = EaseOutCubic(num);
			_animScale = Mathf.LerpUnclamped(startScale, 1f, EaseOutBack(num));
			_animRoll = Mathf.LerpUnclamped(-4f, 0f, num2);
			_animYaw = Mathf.LerpUnclamped(-8f, 0f, num2);
			_animPitch = Mathf.LerpUnclamped(6f, 0f, num2);
			_animPush = Mathf.LerpUnclamped(0.06f, 0f, num2);
			if ((Object)(object)_canvasGroup != (Object)null)
			{
				_canvasGroup.alpha = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(num * 2.4f));
			}
			yield return null;
		}
		_animScale = 1f;
		_animRoll = 0f;
		_animYaw = 0f;
		_animPitch = 0f;
		_animPush = 0f;
		if ((Object)(object)_canvasGroup != (Object)null)
		{
			_canvasGroup.alpha = 1f;
		}
		float settleDuration = 0.24f / SonderSettings.AnimationSpeed;
		progress = 0f;
		while (progress < 1f)
		{
			if ((Object)(object)_canvasRoot == (Object)null)
			{
				_animRoutine = null;
				yield break;
			}
			progress += Time.unscaledDeltaTime / settleDuration;
			float num3 = Mathf.Sin(Mathf.Clamp01(progress) * (float)Math.PI);
			_animScale = 1f + 0.025f * num3;
			_animRoll = 0.6f * num3;
			yield return null;
		}
		_animScale = 1f;
		_animRoll = 0f;
		_animRoutine = null;
	}

	private IEnumerator PlayClose()
	{
		if ((Object)(object)_canvasRoot == (Object)null)
		{
			yield break;
		}
		float s0 = ((_animScale <= 0.001f) ? 1f : _animScale);
		float r0 = _animRoll;
		float y0 = _animYaw;
		float t0 = _animPitch;
		float p0 = _animPush;
		float a0 = (((Object)(object)_canvasGroup != (Object)null) ? _canvasGroup.alpha : 1f);
		float progress = 0f;
		float duration = 0.24f / SonderSettings.AnimationSpeed;
		while (progress < 1f)
		{
			progress += Time.unscaledDeltaTime / duration;
			float num = Mathf.Clamp01(progress);
			float num2 = EaseInCubic(num);
			_animScale = Mathf.LerpUnclamped(s0, 0.86f, num2);
			_animRoll = Mathf.LerpUnclamped(r0, r0 + 3f, num2);
			_animYaw = Mathf.LerpUnclamped(y0, y0 + 6f, num2);
			_animPitch = Mathf.LerpUnclamped(t0, t0 - 4f, num2);
			_animPush = Mathf.LerpUnclamped(p0, p0 + 0.06f, num2);
			if ((Object)(object)_canvasGroup != (Object)null)
			{
				_canvasGroup.alpha = Mathf.LerpUnclamped(a0, 0f, Mathf.SmoothStep(0f, 1f, num));
			}
			yield return null;
		}
		_animScale = 0f;
		_animRoll = 0f;
		_animYaw = 0f;
		_animPitch = 0f;
		_animPush = 0f;
		if ((Object)(object)_canvasGroup != (Object)null)
		{
			_canvasGroup.alpha = 0f;
		}
		if ((Object)(object)_canvasRoot != (Object)null)
		{
			_canvasRoot.SetActive(false);
		}
		_animRoutine = null;
	}

	private static float EaseOutCubic(float x)
	{
		float num = 1f - x;
		return 1f - num * num * num;
	}

	private static float EaseInCubic(float x)
	{
		return x * x * x;
	}

	private static float EaseOutBack(float x)
	{
		float num = x - 1f;
		return 1f + 2.70158f * num * num * num + 1.70158f * num * num;
	}

	private static float EaseOutElastic(float x)
	{
		if (x <= 0f)
		{
			return 0f;
		}
		if (x >= 1f)
		{
			return 1f;
		}
		float num = (float)Math.PI * 2f / 3f;
		return Mathf.Pow(2f, -10f * x) * Mathf.Sin((x * 10f - 0.75f) * num) + 1f;
	}

	static MenuManager()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		MenuOffset = new Vector3(0.1f, 0.25f, 0.15f);
		DockLayerMask = 4;
	}
}
