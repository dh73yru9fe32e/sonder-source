using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Sonder.Managers;

public class GrabIndicator : MonoBehaviour
{
	private const float BallSize = 0.045f;

	private const float BallHeight = 0.45f;

	private readonly Dictionary<VRRig, GameObject> _balls = new Dictionary<VRRig, GameObject>();

	private readonly List<VRRig> _stale = new List<VRRig>();

	private Material _green;

	private Material _red;

	public static GrabIndicator Instance { get; private set; }

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
		ClearAll();
	}

	private void Update()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		bool num = AnyGrabModEnabled();
		IReadOnlyList<VRRig> activeRigs = VRRigCache.ActiveRigs;
		if (!num || activeRigs == null)
		{
			ClearAll();
			return;
		}
		if ((Object)(object)_green == (Object)null)
		{
			_green = ExtraUtil.Unlit((new Color32((byte)70, byte.MaxValue, (byte)110, byte.MaxValue)), transparent: false);
		}
		if ((Object)(object)_red == (Object)null)
		{
			_red = ExtraUtil.Unlit((new Color32(byte.MaxValue, (byte)70, (byte)70, byte.MaxValue)), transparent: false);
		}
		for (int i = 0; i < activeRigs.Count; i++)
		{
			VRRig val = activeRigs[i];
			if (!((Object)(object)val == (Object)null) && !val.isLocal && !val.isOfflineVRRig)
			{
				if (!_balls.TryGetValue(val, out var value) || (Object)(object)value == (Object)null)
				{
					value = GameObject.CreatePrimitive((PrimitiveType)0);
					((Object)value).name = "Sonder_GripBall";
					value.layer = 2;
					Object.Destroy((Object)(object)value.GetComponent<Collider>());
					value.GetComponent<Renderer>().material = _red;
					value.transform.localScale = Vector3.one * 0.045f;
					_balls[val] = value;
				}
				Vector3 val2 = (((Object)(object)val.headMesh != (Object)null) ? val.headMesh.transform.position : ((Component)val).transform.position);
				value.transform.position = val2 + Vector3.up * 0.45f;
				bool flag = CanBeGrabbed(val);
				value.GetComponent<Renderer>().sharedMaterial = (flag ? _green : _red);
			}
		}
		_stale.Clear();
		foreach (KeyValuePair<VRRig, GameObject> ball in _balls)
		{
			VRRig key = ball.Key;
			bool flag2 = (Object)(object)key != (Object)null;
			if (flag2)
			{
				flag2 = false;
				for (int j = 0; j < activeRigs.Count; j++)
				{
					if ((Object)(object)activeRigs[j] == (Object)(object)key)
					{
						flag2 = true;
						break;
					}
				}
			}
			if (!flag2)
			{
				_stale.Add(key);
			}
		}
		for (int k = 0; k < _stale.Count; k++)
		{
			if (_balls.TryGetValue(_stale[k], out var value2) && (Object)(object)value2 != (Object)null)
			{
				Object.Destroy((Object)(object)value2);
			}
			_balls.Remove(_stale[k]);
		}
	}

	private static bool CanBeGrabbed(VRRig rig)
	{
		if ((Object)(object)rig.rightHandLink != (Object)null && rig.rightHandLink.CanBeGrabbed())
		{
			return true;
		}
		if ((Object)(object)rig.leftHandLink != (Object)null)
		{
			return rig.leftHandLink.CanBeGrabbed();
		}
		return false;
	}

	private static bool AnyGrabModEnabled()
	{
		OverpoweredRuntime instance3 = OverpoweredRuntime.Instance;
		if ((Object)(object)instance3 != (Object)null && instance3.ForceGrabGunEnabled)
		{
			return true;
		}
		return false;
	}

	private void ClearAll()
	{
		foreach (KeyValuePair<VRRig, GameObject> ball in _balls)
		{
			if ((Object)(object)ball.Value != (Object)null)
			{
				Object.Destroy((Object)(object)ball.Value);
			}
		}
		_balls.Clear();
	}
}
