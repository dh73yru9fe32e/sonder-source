using GorillaLocomotion;
using Sonder.Menu;
using UnityEngine;
using Object = UnityEngine.Object;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.XR;

namespace Sonder.Managers;

public class ModRuntime : MonoBehaviour
{
	private const float FlySpeed = 10f;

	private bool _speedApplied;

	private float _savedVelocityLimit;

	private float _savedSlideLimit;

	private bool _dashWasPressed;

	private bool _bouncyApplied;

	private float _preBounciness;

	private float _preFriction;

	private PhysicsMaterialCombine _preBounceCombine;

	private float _nextReverseTime;

	private float _flapTime;

	private Vector3 _wasdLastPosition;

	private Vector2 _driveLerp;

	private bool _slideApplied;

	private float _savedSlideControl;

	private bool _highJumpApplied;

	private float _savedJumpMultiplier;

	private bool _longArmsApplied;

	private float _savedMaxArmLength;

	private bool _gravityApplied;

	private Vector3 _savedGravity;

	private bool _furApplied;

	private Material _furMaterial;

	private Color _savedFurColor;

	// Portal Gun
	private struct Portal
	{
		public bool Active;
		public Vector3 Position;
		public Vector3 Normal;
		public Transform Surface;
		public LineRenderer Ring;
	}
	private readonly Portal[] _portals = new Portal[2] { new Portal(), new Portal() };
	private int _selectedPortal;
	private bool _portalSampled;
	private bool _portalTriggerHeld;
	private Vector3 _portalPrev;
	private float _placeAfter;
	private float _travelAfter;
	private readonly Collider[] _exitHits = new Collider[32];

	private const float WasdMouseSensitivity = 0.1f;

	private bool _wasdActive;

	private bool _wasdCursorLocked;

	private float _wasdYaw;

	private float _wasdPitch;

	public static ModRuntime Instance { get; private set; }

	public bool SpeedBoost { get; set; }

	public bool HighJump { get; set; }

	public bool LongArms { get; set; }

	public bool MoonGravity { get; set; }

	public bool Fly { get; set; }

	private bool _flyHeld;

	public bool RainbowFur { get; set; }

	public bool JoystickFly { get; set; }

	public bool Dash { get; set; }

	public bool ZeroGravity { get; set; }

	public bool LowGravity { get; set; }

	public bool Bouncy { get; set; }

	public bool SlingshotFly { get; set; }

	public bool FeatherFall { get; set; }

	public bool TriggerFly { get; set; }

	public bool HandFly { get; set; }

	public bool WasdFly { get; set; }

	public bool BarkFly { get; set; }

	public bool BirdFly { get; set; }

	public bool ReverseVelocity { get; set; }

	public bool HighGravity { get; set; }

	public bool GroundHelper { get; set; }

	public bool BunnyHop { get; set; }

	public bool Strafe { get; set; }

	public bool SlideControl { get; set; }

	public bool NoTagFreeze { get; set; }

	public bool Drive { get; set; }

	public bool IronMan { get; set; }

	public bool FlickJump { get; set; }

	public bool UpAndDown { get; set; }
	public bool PortalGun { get; set; }

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
	}

	private void Update()
	{
		ApplyPlayer();
		ApplyGravity();
		ApplyFur();
		ApplyFly();
		ApplyJoystickFly();
		ApplyTriggerFly();
		ApplyHandFly();
		ApplyWasdFly();
		ApplyBarkFly();
		ApplyBirdFly();
		ApplyDash();
		ApplySlingshotFly();
		ApplyReverseVelocity();
		ApplyFeatherFall();
		ApplyBouncy();
		ApplyGroundHelper();
		ApplyBunnyHop();
		ApplyStrafe();
		ApplySlideControl();
		ApplyNoTagFreeze();
		ApplyDrive();
		ApplyIronMan();
		ApplyFlickJump();
		ApplyUpAndDown();
		ApplyPortalGun();
	}

	private void ApplyPlayer()
	{
		GTPlayer instance = GTPlayer.Instance;
		if ((Object)(object)instance == (Object)null)
		{
			return;
		}
		if (SpeedBoost)
		{
			if (!_speedApplied)
			{
				_savedVelocityLimit = instance.velocityLimit;
				_savedSlideLimit = instance.slideVelocityLimit;
				_speedApplied = true;
			}
			instance.velocityLimit = 25f;
			instance.slideVelocityLimit = 25f;
		}
		else if (_speedApplied)
		{
			instance.velocityLimit = _savedVelocityLimit;
			instance.slideVelocityLimit = _savedSlideLimit;
			_speedApplied = false;
		}
		if (HighJump)
		{
			if (!_highJumpApplied)
			{
				_savedJumpMultiplier = instance.jumpMultiplier;
				_highJumpApplied = true;
			}
			instance.jumpMultiplier = 1.8f;
		}
		else if (_highJumpApplied)
		{
			instance.jumpMultiplier = _savedJumpMultiplier;
			_highJumpApplied = false;
		}
		if (LongArms)
		{
			if (!_longArmsApplied)
			{
				_savedMaxArmLength = instance.maxArmLength;
				_longArmsApplied = true;
			}
			instance.maxArmLength = 2.5f;
		}
		else if (_longArmsApplied)
		{
			instance.maxArmLength = _savedMaxArmLength;
			_longArmsApplied = false;
		}
	}

	private void ApplyGravity()
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		if (MoonGravity)
		{
			if (!_gravityApplied)
			{
				_savedGravity = Physics.gravity;
				_gravityApplied = true;
			}
			Physics.gravity = new Vector3(0f, -4.9f, 0f);
		}
		else if (_gravityApplied)
		{
			Physics.gravity = _savedGravity;
			_gravityApplied = false;
		}
		Rigidbody playerBody = GetPlayerBody();
		if (!((Object)(object)playerBody == (Object)null))
		{
			if (ZeroGravity)
			{
				playerBody.AddForce(-Physics.gravity, (ForceMode)5);
			}
			if (LowGravity)
			{
				playerBody.AddForce(Vector3.up * 6.66f, (ForceMode)5);
			}
			if (HighGravity)
			{
				playerBody.AddForce(Vector3.down * 7.77f, (ForceMode)5);
			}
		}
	}

	private void ApplyFly()
	{
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		if (!Fly)
		{
			_flyHeld = false;
			return;
		}
		// In VR only the A button counts; the mouse button is only used on PC, since it can read as held in VR.
		bool held;
		if (XRSettings.isDeviceActive)
		{
			ControllerInputPoller instance = ControllerInputPoller.instance;
			held = (Object)(object)instance != (Object)null && instance.rightControllerPrimaryButton;
		}
		else
		{
			held = Mouse.current != null && Mouse.current.leftButton.isPressed;
		}
		if (!held)
		{
			if (_flyHeld)
			{
				// Stop on release instead of drifting on.
				_flyHeld = false;
				Rigidbody body = GetPlayerBody();
				if ((Object)(object)body != (Object)null)
				{
					body.linearVelocity = Vector3.zero;
				}
			}
			return;
		}
		_flyHeld = true;
		Transform lookTransform = GetLookTransform();
		if ((Object)(object)lookTransform == (Object)null)
		{
			return;
		}
		// Moving the transform gets undone by the game's own movement, so drive the body's velocity instead.
		Rigidbody playerBody = GetPlayerBody();
		if ((Object)(object)playerBody != (Object)null)
		{
			playerBody.linearVelocity = lookTransform.forward * FlySpeedValue;
		}
		else
		{
			FlyStep(lookTransform.forward);
		}
	}

	private static readonly float[] FlySpeeds = new float[5] { 5f, 10f, 15f, 25f, 40f };

	public static int FlySpeedIndex = 1;

	public static float FlySpeedValue => FlySpeeds[FlySpeedIndex];

	public static string FlySpeedLabel => FlySpeedValue.ToString("0") + " m/s";

	public static void CycleFlySpeed()
	{
		FlySpeedIndex = (FlySpeedIndex + 1) % FlySpeeds.Length;
	}

	// Moves the player root (the camera follows it) and cancels gravity for this frame.
	private static void FlyStep(Vector3 direction)
	{
		TranslateLocalPlayer(direction * (Time.deltaTime * FlySpeedValue));
		Rigidbody playerBody = GetPlayerBody();
		if ((Object)(object)playerBody != (Object)null)
		{
			playerBody.linearVelocity = Vector3.zero;
		}
	}

	public static void FlyMod(float flyspeed)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		GTPlayer instance = GTPlayer.Instance;
		if (!((Object)(object)instance == (Object)null) && !((Object)(object)GorillaTagger.Instance == (Object)null))
		{
			Transform transform = ((Component)instance).transform;
			transform.position += ((Component)GorillaTagger.Instance.headCollider).transform.forward * (Time.deltaTime * flyspeed);
			Rigidbody rigidbody = GorillaTagger.Instance.rigidbody;
			if ((Object)(object)rigidbody != (Object)null)
			{
				rigidbody.linearVelocity = Vector3.zero;
			}
		}
	}

	private void ApplyJoystickFly()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		if (!JoystickFly)
		{
			return;
		}
		ControllerInputPoller instance = ControllerInputPoller.instance;
		if ((Object)(object)instance == (Object)null)
		{
			return;
		}
		Vector2 leftControllerPrimary2DAxis = instance.leftControllerPrimary2DAxis;
		if (Mathf.Abs(leftControllerPrimary2DAxis.x) <= 0.3f && Mathf.Abs(leftControllerPrimary2DAxis.y) <= 0.3f)
		{
			return;
		}
		Transform lookTransform = GetLookTransform();
		if (!((Object)(object)lookTransform == (Object)null))
		{
			FlyStep(lookTransform.forward * leftControllerPrimary2DAxis.y + lookTransform.right * leftControllerPrimary2DAxis.x);
		}
	}

	private void ApplyDash()
	{
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		if (!Dash)
		{
			return;
		}
		Mouse current = Mouse.current;
		ControllerInputPoller instance = ControllerInputPoller.instance;
		if (((current != null && current.leftButton.wasPressedThisFrame) || ((Object)(object)instance != (Object)null && instance.rightControllerPrimaryButton && !_dashWasPressed)) && !_dashWasPressed)
		{
			Camera main = Camera.main;
			Vector3 val = (((Object)(object)main != (Object)null) ? ((Component)main).transform.forward : Vector3.forward);
			Rigidbody playerBody = GetPlayerBody();
			if ((Object)(object)playerBody != (Object)null)
			{
				playerBody.linearVelocity += val * 10f;
			}
		}
		_dashWasPressed = (current != null && current.leftButton.isPressed) || ((Object)(object)instance != (Object)null && instance.rightControllerPrimaryButton);
	}

	private void ApplySlingshotFly()
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		if (!SlingshotFly)
		{
			return;
		}
		ControllerInputPoller instance = ControllerInputPoller.instance;
		if ((Object)(object)instance == (Object)null || !instance.rightControllerPrimaryButton)
		{
			return;
		}
		Rigidbody playerBody = GetPlayerBody();
		Transform lookTransform = GetLookTransform();
		if (!((Object)(object)playerBody == (Object)null) && !((Object)(object)lookTransform == (Object)null))
		{
			playerBody.linearVelocity += lookTransform.forward * (Time.deltaTime * 20f);
		}
	}

	private void ApplyFeatherFall()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		if (!FeatherFall)
		{
			return;
		}
		Rigidbody playerBody = GetPlayerBody();
		if ((Object)(object)playerBody == (Object)null)
		{
			return;
		}
		Vector3 linearVelocity = playerBody.linearVelocity;
		if (linearVelocity.magnitude > 0.1f || !GTPlayer.Instance.BodyOnGround)
		{
			Vector3 val = playerBody.linearVelocity * 4f;
			playerBody.linearVelocity -= val * Time.fixedDeltaTime;
			if (GTPlayer.Instance.BodyOnGround && playerBody.linearVelocity.y < 0f)
			{
				playerBody.linearVelocity = new Vector3(playerBody.linearVelocity.x, Mathf.Max(playerBody.linearVelocity.y, -2f), playerBody.linearVelocity.z);
			}
		}
	}

	private void ApplyBouncy()
	{
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		GorillaTagger instance = GorillaTagger.Instance;
		CapsuleCollider val = (((Object)(object)instance != (Object)null) ? instance.bodyCollider : null);
		if ((Object)(object)val == (Object)null || (Object)(object)((Collider)val).material == (Object)null)
		{
			return;
		}
		if (Bouncy)
		{
			if (!_bouncyApplied)
			{
				_preBounciness = ((Collider)val).material.bounciness;
				_preBounceCombine = ((Collider)val).material.bounceCombine;
				_preFriction = ((Collider)val).material.dynamicFriction;
				_bouncyApplied = true;
			}
			((Collider)val).material.bounciness = 1f;
			((Collider)val).material.bounceCombine = (PhysicsMaterialCombine)3;
			((Collider)val).material.dynamicFriction = 0f;
		}
		else if (_bouncyApplied)
		{
			((Collider)val).material.bounciness = _preBounciness;
			((Collider)val).material.bounceCombine = _preBounceCombine;
			((Collider)val).material.dynamicFriction = _preFriction;
			_bouncyApplied = false;
		}
	}

	private void ApplyTriggerFly()
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		if (!TriggerFly)
		{
			return;
		}
		ControllerInputPoller instance = ControllerInputPoller.instance;
		if ((Object)(object)instance == (Object)null || instance.rightControllerIndexFloat <= 0.5f)
		{
			return;
		}
		Transform lookTransform = GetLookTransform();
		if (!((Object)(object)lookTransform == (Object)null))
		{
			FlyStep(lookTransform.forward);
		}
	}

	private void ApplyHandFly()
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		if (!HandFly)
		{
			return;
		}
		ControllerInputPoller instance = ControllerInputPoller.instance;
		if ((Object)(object)instance == (Object)null || !instance.rightControllerPrimaryButton)
		{
			return;
		}
		GorillaTagger instance2 = GorillaTagger.Instance;
		if ((Object)(object)instance2 == (Object)null || (Object)(object)instance2.rightHandTransform == (Object)null)
		{
			return;
		}
		FlyStep(instance2.rightHandTransform.forward);
	}

	// PC spectator fly. With the menu closed the mouse is locked and looks around (FPS style, crosshair in
	// the middle, guns aim at it). Q opens the menu and frees the mouse. Moves by velocity so it is smooth.
	private void ApplyWasdFly()
	{
		Rigidbody body = GetPlayerBody();
		if (!WasdFly)
		{
			if (_wasdActive)
			{
				_wasdActive = false;
				if ((Object)(object)body != (Object)null)
				{
					body.useGravity = true;
				}
				SetWasdCursor(locked: false);
			}
			return;
		}
		Camera camera = MenuManager.EyeCamera;
		Keyboard keyboard = Keyboard.current;
		if ((Object)(object)camera == (Object)null || (Object)(object)body == (Object)null || keyboard == null)
		{
			return;
		}
		Transform view = ((Component)camera).transform;
		if (!_wasdActive)
		{
			_wasdActive = true;
			Vector3 angles = view.eulerAngles;
			_wasdYaw = angles.y;
			_wasdPitch = ((angles.x > 180f) ? (angles.x - 360f) : angles.x);
		}
		body.useGravity = false;
		bool typing = MenuBuilder.SearchActive || RoomRuntime.Typing;
		bool look = !MenuManager.ScreenMode && Application.isFocused;
		SetWasdCursor(look);
		Mouse mouse = Mouse.current;
		if (look && mouse != null)
		{
			Vector2 delta = ((InputControl<Vector2>)(object)((Pointer)mouse).delta).ReadValue();
			_wasdYaw += delta.x * WasdMouseSensitivity;
			_wasdPitch -= delta.y * WasdMouseSensitivity;
		}
		if (!typing)
		{
			float turn = 120f * Time.deltaTime;
			if (((ButtonControl)keyboard.leftArrowKey).isPressed)
			{
				_wasdYaw -= turn;
			}
			if (((ButtonControl)keyboard.rightArrowKey).isPressed)
			{
				_wasdYaw += turn;
			}
			if (((ButtonControl)keyboard.upArrowKey).isPressed)
			{
				_wasdPitch -= turn;
			}
			if (((ButtonControl)keyboard.downArrowKey).isPressed)
			{
				_wasdPitch += turn;
			}
		}
		_wasdPitch = Mathf.Clamp(_wasdPitch, -89f, 89f);
		ApplyWasdLook();
		Vector3 move = Vector3.zero;
		if (!typing)
		{
			if (((ButtonControl)keyboard.wKey).isPressed)
			{
				move += view.forward;
			}
			if (((ButtonControl)keyboard.sKey).isPressed)
			{
				move -= view.forward;
			}
			if (((ButtonControl)keyboard.dKey).isPressed)
			{
				move += view.right;
			}
			if (((ButtonControl)keyboard.aKey).isPressed)
			{
				move -= view.right;
			}
			if (((ButtonControl)keyboard.spaceKey).isPressed)
			{
				move += Vector3.up;
			}
			if (((ButtonControl)keyboard.leftCtrlKey).isPressed)
			{
				move -= Vector3.up;
			}
		}
		float speed = FlySpeedValue * (((ButtonControl)keyboard.leftShiftKey).isPressed ? 3f : 1f);
		Vector3 target = ((move.sqrMagnitude > 0.0001f) ? (move.normalized * speed) : Vector3.zero);
		body.linearVelocity = Vector3.Lerp(body.linearVelocity, target, 1f - Mathf.Exp(-12f * Time.deltaTime));
	}

	// The game can re-pose the camera after Update, so the look is set again here.
	private void LateUpdate()
	{
		if (WasdFly && _wasdActive)
		{
			ApplyWasdLook();
		}
	}

	private void ApplyWasdLook()
	{
		Camera camera = MenuManager.EyeCamera;
		if ((Object)(object)camera != (Object)null)
		{
			((Component)camera).transform.rotation = Quaternion.Euler(_wasdPitch, _wasdYaw, 0f);
		}
	}

	private void SetWasdCursor(bool locked)
	{
		if (locked == _wasdCursorLocked)
		{
			return;
		}
		_wasdCursorLocked = locked;
		if (locked)
		{
			Cursor.lockState = (CursorLockMode)1;
			Cursor.visible = false;
		}
		else
		{
			Cursor.lockState = (CursorLockMode)0;
			Cursor.visible = true;
		}
	}

	public static bool WasdLooking => (Object)(object)Instance != (Object)null && Instance._wasdCursorLocked;

	private void OnGUI()
	{
		if (_wasdCursorLocked)
		{
			float x = (float)Screen.width * 0.5f;
			float y = (float)Screen.height * 0.5f;
			GUI.color = new Color(1f, 1f, 1f, 0.85f);
			GUI.DrawTexture(new Rect(x - 1f, y - 7f, 2f, 14f), Texture2D.whiteTexture);
			GUI.DrawTexture(new Rect(x - 7f, y - 1f, 14f, 2f), Texture2D.whiteTexture);
		}
	}

	private void ApplyBarkFly()
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		if (!BarkFly)
		{
			return;
		}
		ControllerInputPoller instance = ControllerInputPoller.instance;
		Rigidbody playerBody = GetPlayerBody();
		GorillaTagger instance2 = GorillaTagger.Instance;
		if (!((Object)(object)instance == (Object)null) && !((Object)(object)playerBody == (Object)null) && !((Object)(object)instance2 == (Object)null) && !((Object)(object)instance2.bodyCollider == (Object)null))
		{
			Vector2 leftControllerPrimary2DAxis = instance.leftControllerPrimary2DAxis;
			Vector2 rightControllerPrimary2DAxis = instance.rightControllerPrimary2DAxis;
			Vector3 val = default(Vector3);
			val = new Vector3(leftControllerPrimary2DAxis.x, rightControllerPrimary2DAxis.y, leftControllerPrimary2DAxis.y);
			Vector3 val2 = Xz(((Component)instance2.bodyCollider).transform.forward);
			Vector3 val3 = Xz(((Component)instance2.bodyCollider).transform.right);
			Vector3 val4 = val.x * val3 + val.y * Vector3.up + val.z * val2;
			val4 *= FlySpeedValue;
			playerBody.linearVelocity = Vector3.Lerp(playerBody.linearVelocity, val4, 0.12875f);
			playerBody.AddForce(-Physics.gravity, (ForceMode)5);
		}
	}

	private void ApplyBirdFly()
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		if (!BirdFly)
		{
			return;
		}
		GorillaTagger instance = GorillaTagger.Instance;
		Rigidbody playerBody = GetPlayerBody();
		if ((Object)(object)instance == (Object)null || (Object)(object)playerBody == (Object)null || (Object)(object)instance.headCollider == (Object)null || (Object)(object)instance.bodyCollider == (Object)null)
		{
			return;
		}
		Vector3 position = ((Component)instance.headCollider).transform.position;
		RaycastHit val = default(RaycastHit);
		if ((Object)(object)instance.leftHandTransform == (Object)null || (Object)(object)instance.rightHandTransform == (Object)null || Vector3.Distance(instance.leftHandTransform.position, position) < 0.63f || Vector3.Distance(instance.rightHandTransform.position, position) < 0.63f || Vector3.Distance(instance.leftHandTransform.position, instance.rightHandTransform.position) < 1f || Physics.Raycast(((Collider)instance.bodyCollider).attachedRigidbody.position, Vector3.down, out val, -1f))
		{
			return;
		}
		UnityEngine.XR.InputDevice deviceAtXRNode = InputDevices.GetDeviceAtXRNode((XRNode)4);
		UnityEngine.XR.InputDevice deviceAtXRNode2 = InputDevices.GetDeviceAtXRNode((XRNode)5);
		Vector3 val2 = default(Vector3);
		Vector3 val3 = default(Vector3);
		if (deviceAtXRNode.TryGetFeatureValue(UnityEngine.XR.CommonUsages.deviceVelocity, out val2) && deviceAtXRNode2.TryGetFeatureValue(UnityEngine.XR.CommonUsages.deviceVelocity, out val3) && !(Time.time - _flapTime < 0.4f) && val2.y < -1.2f && val3.y < -1.2f)
		{
			float num = Mathf.Min(6f * ((Mathf.Abs(val2.y) + Mathf.Abs(val3.y)) / 2f) / 1.2f, 9f);
			((Collider)instance.bodyCollider).attachedRigidbody.AddForce(Vector3.up * num, (ForceMode)2);
			_flapTime = Time.time;
		}
	}

	private void ApplyReverseVelocity()
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		if (!ReverseVelocity || Time.time < _nextReverseTime)
		{
			return;
		}
		ControllerInputPoller instance = ControllerInputPoller.instance;
		if ((Object)(object)instance == (Object)null || !instance.rightControllerPrimaryButton)
		{
			return;
		}
		Rigidbody playerBody = GetPlayerBody();
		if (!((Object)(object)playerBody == (Object)null))
		{
			playerBody.linearVelocity = -playerBody.linearVelocity;
			_nextReverseTime = Time.time + 0.5f;
		}
	}

	private void ApplyGroundHelper()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		if (!GroundHelper)
		{
			return;
		}
		ControllerInputPoller instance = ControllerInputPoller.instance;
		if ((Object)(object)instance == (Object)null || instance.rightControllerGripFloat <= 0.5f)
		{
			return;
		}
		Rigidbody playerBody = GetPlayerBody();
		if (!((Object)(object)playerBody == (Object)null))
		{
			Vector3 linearVelocity = playerBody.linearVelocity;
			if (linearVelocity.y > 0f)
			{
				playerBody.linearVelocity = new Vector3(linearVelocity.x, 0f, linearVelocity.z);
			}
		}
	}

	private void ApplyBunnyHop()
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		if (!BunnyHop)
		{
			return;
		}
		GorillaTagger instance = GorillaTagger.Instance;
		GTPlayer instance2 = GTPlayer.Instance;
		Rigidbody playerBody = GetPlayerBody();
		if (!((Object)(object)instance == (Object)null) && !((Object)(object)instance2 == (Object)null) && !((Object)(object)playerBody == (Object)null) && !((Object)(object)instance.bodyCollider == (Object)null))
		{
			RaycastHit val = default(RaycastHit);
			Physics.Raycast(((Component)instance.bodyCollider).transform.position - new Vector3(0f, 0.2f, 0f), Vector3.down, out val, 512f, (instance2.locomotionEnabledLayers));
			if (val.distance < 0.15f)
			{
				Vector3 linearVelocity = playerBody.linearVelocity;
				playerBody.linearVelocity = new Vector3(linearVelocity.x, instance2.jumpMultiplier * 2.7272727f, linearVelocity.z);
			}
		}
	}

	private void ApplyStrafe()
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		if (!Strafe)
		{
			return;
		}
		GorillaTagger instance = GorillaTagger.Instance;
		GTPlayer instance2 = GTPlayer.Instance;
		Rigidbody playerBody = GetPlayerBody();
		if (!((Object)(object)instance == (Object)null) && !((Object)(object)instance2 == (Object)null) && !((Object)(object)playerBody == (Object)null) && !((Object)(object)instance.bodyCollider == (Object)null))
		{
			Vector3 val = ((Component)instance.bodyCollider).transform.forward * instance2.maxJumpSpeed;
			playerBody.linearVelocity = new Vector3(val.x, playerBody.linearVelocity.y, val.z);
		}
	}

	private void ApplySlideControl()
	{
		GTPlayer instance = GTPlayer.Instance;
		if ((Object)(object)instance == (Object)null)
		{
			return;
		}
		if (SlideControl)
		{
			if (!_slideApplied)
			{
				_savedSlideControl = instance.slideControl;
				_slideApplied = true;
			}
			instance.slideControl = 1f;
		}
		else if (_slideApplied)
		{
			instance.slideControl = _savedSlideControl;
			_slideApplied = false;
		}
	}

	private void ApplyNoTagFreeze()
	{
		if (NoTagFreeze)
		{
			GTPlayer instance = GTPlayer.Instance;
			if ((Object)(object)instance != (Object)null)
			{
				instance.disableMovement = false;
			}
		}
	}

	private void ApplyDrive()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		if (!Drive)
		{
			return;
		}
		ControllerInputPoller instance = ControllerInputPoller.instance;
		GorillaTagger instance2 = GorillaTagger.Instance;
		GTPlayer instance3 = GTPlayer.Instance;
		Rigidbody playerBody = GetPlayerBody();
		if (!((Object)(object)instance == (Object)null) && !((Object)(object)instance2 == (Object)null) && !((Object)(object)instance3 == (Object)null) && !((Object)(object)playerBody == (Object)null) && !((Object)(object)instance2.bodyCollider == (Object)null))
		{
			Vector2 leftControllerPrimary2DAxis = instance.leftControllerPrimary2DAxis;
			_driveLerp = Vector2.Lerp(_driveLerp, leftControllerPrimary2DAxis, 0.05f);
			Vector3 val = ((Component)instance2.bodyCollider).transform.forward * _driveLerp.y + ((Component)instance2.bodyCollider).transform.right * _driveLerp.x;
			RaycastHit val2 = default(RaycastHit);
			Physics.Raycast(((Component)instance2.bodyCollider).transform.position - new Vector3(0f, 0.2f, 0f), Vector3.down, out val2, 512f, (instance3.locomotionEnabledLayers));
			Vector3 val3 = val * 10f;
			if (val2.distance < 0.2f && (Mathf.Abs(_driveLerp.x) > 0.05f || Mathf.Abs(_driveLerp.y) > 0.05f))
			{
				playerBody.linearVelocity = new Vector3(val3.x, playerBody.linearVelocity.y, val3.z);
			}
		}
	}

	private void ApplyIronMan()
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		if (!IronMan)
		{
			return;
		}
		ControllerInputPoller instance = ControllerInputPoller.instance;
		GorillaTagger instance2 = GorillaTagger.Instance;
		Rigidbody playerBody = GetPlayerBody();
		if (!((Object)(object)instance == (Object)null) && !((Object)(object)instance2 == (Object)null) && !((Object)(object)playerBody == (Object)null))
		{
			Vector3 linearVelocity;
			if (instance.leftControllerPrimaryButton && (Object)(object)instance2.leftHandTransform != (Object)null)
			{
				Vector3 val = 10f * -instance2.leftHandTransform.right;
				playerBody.AddForce(val * Time.deltaTime, (ForceMode)2);
				float num = instance2.tapHapticStrength / 50f;
				linearVelocity = playerBody.linearVelocity;
				float num2 = num * linearVelocity.magnitude;
				instance2.StartVibration(true, num2, instance2.tapHapticDuration);
			}
			if (instance.rightControllerPrimaryButton && (Object)(object)instance2.rightHandTransform != (Object)null)
			{
				Vector3 val2 = 10f * instance2.rightHandTransform.right;
				playerBody.AddForce(val2 * Time.deltaTime, (ForceMode)2);
				float num3 = instance2.tapHapticStrength / 50f;
				linearVelocity = playerBody.linearVelocity;
				float num4 = num3 * linearVelocity.magnitude;
				instance2.StartVibration(false, num4, instance2.tapHapticDuration);
			}
		}
	}

	private void ApplyFlickJump()
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		if (!FlickJump)
		{
			return;
		}
		ControllerInputPoller instance = ControllerInputPoller.instance;
		GorillaTagger instance2 = GorillaTagger.Instance;
		GTPlayer instance3 = GTPlayer.Instance;
		if (!((Object)(object)instance == (Object)null) && !((Object)(object)instance2 == (Object)null) && !((Object)(object)instance3 == (Object)null) && !((Object)(object)instance2.rightHandTransform == (Object)null) && instance.rightControllerPrimaryButton)
		{
			Vector3 position = instance2.rightHandTransform.position + new Vector3(0f, -1.5f, 0f);
			((Component)instance3.GetControllerTransform(false)).transform.position = position;
		}
	}

	private void ApplyUpAndDown()
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		if (!UpAndDown)
		{
			return;
		}
		ControllerInputPoller instance = ControllerInputPoller.instance;
		Rigidbody playerBody = GetPlayerBody();
		if (!((Object)(object)instance == (Object)null) && !((Object)(object)playerBody == (Object)null))
		{
			bool flag = instance.rightControllerIndexFloat > 0.5f;
			bool flag2 = instance.rightControllerGripFloat > 0.5f;
			if (flag | flag2)
			{
				playerBody.AddForce(-Physics.gravity, (ForceMode)5);
			}
			if (flag)
			{
				playerBody.linearVelocity += Vector3.up * (Time.deltaTime * 10f * 3f);
			}
			if (flag2)
			{
				playerBody.linearVelocity += Vector3.up * (Time.deltaTime * 10f * -3f);
			}
		}
	}

	private void ApplyPortalGun()
	{
		if (!PortalGun)
		{
			// cleanup
			for (int i = 0; i < _portals.Length; i++)
			{
				if (_portals[i].Ring != null)
					Object.Destroy(_portals[i].Ring.gameObject);
				_portals[i] = new Portal();
			}
			_portalSampled = false;
			return;
		}
		GorillaTagger tagger = GorillaTagger.Instance;
		if (tagger == null) return;

		// switch portal selection with A (desktop Q)
		Keyboard kb = Keyboard.current;
		bool switchInput = (kb != null && kb.qKey.wasPressedThisFrame) ||
			(ControllerInputPoller.instance != null && ControllerInputPoller.instance.leftControllerPrimaryButton);
		// Actually use right controller A button via poller if available
		if (switchInput)
			_selectedPortal ^= 1;

		// aim + place
		Mouse mouse = Mouse.current;
		ControllerInputPoller poller = ControllerInputPoller.instance;
		bool triggerNow = (mouse != null && mouse.leftButton.isPressed) ||
			(poller != null && poller.rightControllerIndexFloat > 0.5f);
		bool placePressed = triggerNow && !_portalTriggerHeld;
		_portalTriggerHeld = triggerNow;
		if (placePressed && Time.unscaledTime >= _placeAfter)
		{
			Transform hand = tagger.rightHandTransform;
			if (hand != null)
			{
				Vector3 origin = hand.position;
				Vector3 dir = hand.forward;
				if (Physics.Raycast(origin, dir, out RaycastHit hit, 50f, Physics.AllLayers, QueryTriggerInteraction.Ignore))
				{
					Collider col = hit.collider;
					if (col != null && !col.isTrigger && col.GetComponentInParent<VRRig>() == null && col.GetComponentInParent<GTPlayer>() == null)
					{
						SetPortal(_selectedPortal, hit);
						_placeAfter = Time.unscaledTime + 0.5f;
						_selectedPortal ^= 1;
						NotificationRuntime.Show("Portal placed");
					}
				}
			}
		}

		// draw rings
		for (int i = 0; i < _portals.Length; i++)
		{
			if (!_portals[i].Active) continue;
			if (!PortalPose(i, out Vector3 pt, out Vector3 nrm)) continue;
			if (_portals[i].Ring == null)
			{
				GameObject go = new GameObject("PortalRing" + i);
				_portals[i].Ring = go.AddComponent<LineRenderer>();
				_portals[i].Ring.positionCount = 40;
				_portals[i].Ring.startWidth = 0.025f;
				_portals[i].Ring.endWidth = 0.025f;
				_portals[i].Ring.useWorldSpace = true;
				_portals[i].Ring.loop = true;
				Shader sh = Shader.Find("Sprites/Default");
				if (sh == null) sh = Shader.Find("Unlit/Color");
				if (sh == null) sh = Shader.Find("UI/Default");
				Material m = new Material(sh);
				_portals[i].Ring.material = m;
				_portals[i].Ring.startColor = _portals[i].Ring.endColor = (i == 0 ? new Color(0.1f, 0.65f, 1f) : new Color(1f, 0.5f, 0.05f));
			}
			_portals[i].Ring.enabled = true;
			DrawCircle(_portals[i].Ring, pt + nrm * 0.015f, nrm, 0.5f);
		}

		// travel
		if (!_portalSampled)
		{
			_portalPrev = Center(tagger);
			_portalSampled = true;
			return;
		}
		Vector3 pos = Center(tagger);
		if (Time.unscaledTime >= _travelAfter)
		{
			for (int i = 0; i < _portals.Length; i++)
			{
				if (!_portals[i].Active) continue;
				if (!PortalPose(i, out Vector3 pt, out Vector3 nrm)) continue;
				float radius = 0.5f;
				if (Vector3.Distance(pos, pt) < radius && Vector3.Distance(_portalPrev, pt) >= radius)
				{
					// travel to other portal
					int other = i ^ 1;
					if (_portals[other].Active && PortalPose(other, out Vector3 otherPt, out Vector3 otherNrm))
					{
						Vector3 dest = otherPt + otherNrm * 1.0f;
						GTPlayer player = GTPlayer.Instance;
						if (player != null)
							player.TeleportTo(dest, player.transform.rotation, false, false);
						_travelAfter = Time.unscaledTime + 0.5f;
						break;
					}
				}
			}
		}
		_portalPrev = pos;
	}

	private static Vector3 Center(GorillaTagger tagger)
	{
		if (tagger != null && tagger.bodyCollider != null)
			return tagger.bodyCollider.transform.TransformPoint(tagger.bodyCollider.center);
		GTPlayer player = GTPlayer.Instance;
		if (player != null) return player.transform.position;
		return Vector3.zero;
	}

	private void SetPortal(int index, RaycastHit hit)
	{
		if (hit.collider == null) return;
		_portals[index].Active = true;
		_portals[index].Surface = hit.collider.transform;
		_portals[index].Position = hit.collider.transform.InverseTransformPoint(hit.point);
		_portals[index].Normal = hit.collider.transform.InverseTransformDirection(hit.normal);
		if (_portals[index].Ring != null)
			Object.Destroy(_portals[index].Ring.gameObject);
		_portals[index].Ring = null;
	}

	private bool PortalPose(int index, out Vector3 point, out Vector3 normal)
	{
		point = normal = Vector3.zero;
		if (!_portals[index].Active || _portals[index].Surface == null) return false;
		point = _portals[index].Surface.TransformPoint(_portals[index].Position);
		normal = _portals[index].Surface.TransformDirection(_portals[index].Normal).normalized;
		return true;
	}

	// Portal struct Pose method (instance) - add via extension
	private static void DrawCircle(LineRenderer lr, Vector3 center, Vector3 normal, float radius)
	{
		if (lr == null) return;
		Vector3 up = (Mathf.Abs(Vector3.Dot(normal, Vector3.up)) > 0.99f) ? Vector3.right : Vector3.up;
		Vector3 right = Vector3.Cross(normal, up).normalized;
		up = Vector3.Cross(right, normal).normalized;
		for (int i = 0; i < lr.positionCount; i++)
		{
			float ang = (float)i / lr.positionCount * Mathf.PI * 2f;
			Vector3 pt = center + (right * Mathf.Cos(ang) + up * Mathf.Sin(ang)) * radius;
			lr.SetPosition(i, pt);
		}
	}

	private static Vector3 Xz(Vector3 value)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		return new Vector3(value.x, 0f, value.z);
	}

	private void ApplyFur()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		if (!RainbowFur)
		{
			if (_furApplied)
			{
				if ((Object)(object)_furMaterial != (Object)null)
				{
					_furMaterial.color = _savedFurColor;
				}
				_furApplied = false;
			}
			return;
		}
		GorillaTagger instance = GorillaTagger.Instance;
		VRRig val = (((Object)(object)instance != (Object)null) ? instance.offlineVRRig : null);
		if ((Object)(object)val == (Object)null || (Object)(object)val.mainSkin == (Object)null)
		{
			return;
		}
		if (!_furApplied)
		{
			Material material = ((Renderer)val.mainSkin).material;
			if ((Object)(object)material == (Object)null || !material.HasProperty("_Color"))
			{
				return;
			}
			_furMaterial = material;
			_savedFurColor = material.color;
			_furApplied = true;
		}
		if ((Object)(object)_furMaterial != (Object)null)
		{
			_furMaterial.color = Color.HSVToRGB(Mathf.Repeat(Time.time * 0.25f, 1f), 0.85f, 1f);
		}
	}

	private static Rigidbody GetPlayerBody()
	{
		GTPlayer instance = GTPlayer.Instance;
		if ((Object)(object)instance != (Object)null && (Object)(object)instance.playerRigidBody != (Object)null)
		{
			return instance.playerRigidBody;
		}
		GorillaTagger instance2 = GorillaTagger.Instance;
		if (!((Object)(object)instance2 != (Object)null))
		{
			return null;
		}
		return instance2.rigidbody;
	}

	private static void TranslateLocalPlayer(Vector3 delta)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		if (delta == Vector3.zero)
		{
			return;
		}
		GTPlayer instance = GTPlayer.Instance;
		if ((Object)(object)instance != (Object)null)
		{
			Transform transform = ((Component)instance).transform;
			transform.position += delta;
			return;
		}
		GorillaTagger instance2 = GorillaTagger.Instance;
		Rigidbody val = ((instance2 != null) ? instance2.rigidbody : null);
		if ((Object)(object)val != (Object)null)
		{
			Transform transform2 = ((Component)val).transform;
			transform2.position += delta;
		}
	}

	private static Transform GetLookTransform()
	{
		GorillaTagger instance = GorillaTagger.Instance;
		if ((Object)(object)instance != (Object)null && (Object)(object)instance.headCollider != (Object)null)
		{
			return ((Component)instance.headCollider).transform;
		}
		Camera main = Camera.main;
		if (!((Object)(object)main != (Object)null))
		{
			return null;
		}
		return ((Component)main).transform;
	}
}
