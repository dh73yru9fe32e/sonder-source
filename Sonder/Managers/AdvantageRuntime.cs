using System.Reflection;
using GorillaLocomotion;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Sonder.Managers;

public class AdvantageRuntime : MonoBehaviour
{
	private static readonly FieldInfo SidewaysDragField = typeof(GTPlayer).GetField("sidewaysDrag", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

	private bool _dragApplied;

	private float _savedDrag;

	public static AdvantageRuntime Instance { get; private set; }

	public bool StickyHands { get; set; }

	public bool IronLegs { get; set; }

	public bool SturdyGrip { get; set; }

	public bool LowDrag { get; set; }

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
		RestoreDrag();
	}

	private void Update()
	{
		ApplyStickyHands();
		ApplyIronLegs();
		ApplySturdyGrip();
		ApplyLowDrag();
	}

	private static void ApplyStickyHands()
	{
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		AdvantageRuntime instance = Instance;
		GTPlayer player = ExtraUtil.Player;
		if ((Object)(object)instance == (Object)null || !instance.StickyHands || (Object)(object)player == (Object)null)
		{
			return;
		}
		ControllerInputPoller instance2 = ControllerInputPoller.instance;
		if ((Object)(object)instance2 != (Object)null && (instance2.leftControllerGripFloat > 0.5f || instance2.rightControllerGripFloat > 0.5f) && player.HandContactingSurface)
		{
			Rigidbody playerRigidBody = player.playerRigidBody;
			if (!((Object)(object)playerRigidBody == (Object)null))
			{
				Vector3 linearVelocity = playerRigidBody.linearVelocity;
				playerRigidBody.linearVelocity = Vector3.Lerp(linearVelocity, new Vector3(0f, linearVelocity.y, 0f), 0.35f);
			}
		}
	}

	private static void ApplyIronLegs()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		AdvantageRuntime instance = Instance;
		if ((Object)(object)instance == (Object)null || !instance.IronLegs)
		{
			return;
		}
		Rigidbody val = ExtraUtil.Body();
		if (!((Object)(object)val == (Object)null))
		{
			Vector3 linearVelocity = val.linearVelocity;
			if (linearVelocity.y < -22f)
			{
				linearVelocity.y = -22f;
				val.linearVelocity = linearVelocity;
			}
		}
	}

	private static void ApplySturdyGrip()
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		AdvantageRuntime instance = Instance;
		GTPlayer player = ExtraUtil.Player;
		if (!((Object)(object)instance == (Object)null) && instance.SturdyGrip && !((Object)(object)player == (Object)null) && player.HandContactingSurface && !player.BodyOnGround)
		{
			Rigidbody playerRigidBody = player.playerRigidBody;
			if ((Object)(object)playerRigidBody != (Object)null)
			{
				playerRigidBody.linearVelocity *= 0.985f;
			}
		}
	}

	private void ApplyLowDrag()
	{
		GTPlayer player = ExtraUtil.Player;
		if ((Object)(object)player == (Object)null || SidewaysDragField == null)
		{
			return;
		}
		if (LowDrag)
		{
			if (!_dragApplied)
			{
				_savedDrag = (float)SidewaysDragField.GetValue(player);
				_dragApplied = true;
			}
			SidewaysDragField.SetValue(player, 0f);
		}
		else
		{
			RestoreDrag();
		}
	}

	private void RestoreDrag()
	{
		if (_dragApplied)
		{
			GTPlayer player = ExtraUtil.Player;
			if ((Object)(object)player != (Object)null && SidewaysDragField != null)
			{
				SidewaysDragField.SetValue(player, _savedDrag);
			}
			_dragApplied = false;
		}
	}
}
