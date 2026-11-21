using UnityEngine;
using Object = UnityEngine.Object;

namespace Sonder.Core;

// A ray that starts between the index and middle fingertips and points along those fingers. Used by the menu
// pointer and the gun line. The rig's finger bones are only trustworthy while the rig follows the controller
// and the hand is open, so the pose is sampled then and stored relative to the controller. Grip curls, ghost,
// invis and rig guns move the bones, but the stored pose keeps following the controller you are holding.
public static class FingerAim
{
	private static readonly Vector3[] LocalOrigin = new Vector3[2];

	private static readonly Vector3[] LocalDirection = new Vector3[2];

	private static readonly bool[] Sampled = new bool[2];

	private static readonly float[] OpenSince = new float[2];

	// The rig's fingers animate open over several frames after the trigger or grip is let go. Sampling during
	// that swing caught half-curled fingers and flicked the ray down or sideways, so wait until the hand has
	// been open this long, and ease into each new sample instead of snapping.
	private const float SettleTime = 0.5f;

	private const float Blend = 0.1f;

	public static void Sample(bool left)
	{
		GorillaTagger tagger = GorillaTagger.Instance;
		VRRig rig = VRRig.LocalRig;
		ControllerInputPoller input = ControllerInputPoller.instance;
		if ((Object)(object)tagger == (Object)null || (Object)(object)rig == (Object)null || !((Behaviour)rig).enabled || (Object)(object)input == (Object)null)
		{
			return;
		}
		int slot = left ? 1 : 0;
		if ((left ? input.leftControllerGripFloat : input.rightControllerGripFloat) > 0.2f || (left ? input.leftControllerIndexFloat : input.rightControllerIndexFloat) > 0.2f)
		{
			OpenSince[slot] = Time.time;
			return;
		}
		if (Time.time - OpenSince[slot] < SettleTime)
		{
			return;
		}
		Transform controller = left ? tagger.leftHandTransform : tagger.rightHandTransform;
		Transform wrist = left ? rig.leftHandTransform : rig.rightHandTransform;
		VRMapIndex index = left ? rig.leftIndex : rig.rightIndex;
		VRMapMiddle middle = left ? rig.leftMiddle : rig.rightMiddle;
		if ((Object)(object)controller == (Object)null || (Object)(object)wrist == (Object)null || index == null || middle == null || (Object)(object)index.fingerBone1 == (Object)null || (Object)(object)middle.fingerBone1 == (Object)null || (Object)(object)index.fingerBone3 == (Object)null || (Object)(object)middle.fingerBone3 == (Object)null)
		{
			return;
		}
		if (Vector3.Distance(wrist.position, controller.position) > 0.3f)
		{
			return;
		}
		Vector3 knuckles = (index.fingerBone1.position + middle.fingerBone1.position) * 0.5f;
		Vector3 tips = (index.fingerBone3.position + middle.fingerBone3.position) * 0.5f;
		Vector3 along = tips - knuckles;
		if (along.sqrMagnitude < 1E-06f)
		{
			return;
		}
		// fingerBone3 is the last joint, not the tip; the tip sits roughly a third further along.
		Vector3 origin = controller.InverseTransformPoint(tips + along * 0.3f);
		Vector3 direction = controller.InverseTransformDirection(along.normalized);
		if (Sampled[slot])
		{
			origin = Vector3.Lerp(LocalOrigin[slot], origin, Blend);
			direction = Vector3.Slerp(LocalDirection[slot], direction, Blend);
		}
		LocalOrigin[slot] = origin;
		LocalDirection[slot] = direction.normalized;
		Sampled[slot] = true;
	}

	public static bool TryGetRay(bool left, out Vector3 origin, out Vector3 direction)
	{
		origin = Vector3.zero;
		direction = Vector3.forward;
		GorillaTagger tagger = GorillaTagger.Instance;
		if ((Object)(object)tagger == (Object)null)
		{
			return false;
		}
		Transform controller = left ? tagger.leftHandTransform : tagger.rightHandTransform;
		if ((Object)(object)controller == (Object)null)
		{
			return false;
		}
		int slot = left ? 1 : 0;
		if (Sampled[slot])
		{
			origin = controller.TransformPoint(LocalOrigin[slot]);
			direction = controller.TransformDirection(LocalDirection[slot]);
		}
		else
		{
			origin = controller.position + controller.forward * 0.1f;
			direction = controller.forward;
		}
		return true;
	}
}
