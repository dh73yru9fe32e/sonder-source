using UnityEngine;
using Object = UnityEngine.Object;

namespace Sonder.Managers;

public sealed class SonderPulse : MonoBehaviour
{
	public Vector3 StartScale;

	public Vector3 EndScale;

	public float Duration;

	public Material Material;

	private float _elapsed;

	private void Update()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		_elapsed += Time.deltaTime;
		float num = Mathf.Clamp01(_elapsed / Duration);
		((Component)this).transform.localScale = Vector3.Lerp(StartScale, EndScale, num);
		if (num >= 1f)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
	}

	private void OnDestroy()
	{
		if ((Object)(object)Material != (Object)null)
		{
			Object.Destroy((Object)(object)Material);
		}
	}

	public SonderPulse()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		StartScale = Vector3.one * 0.2f;
		EndScale = Vector3.one * 4f;
		Duration = 0.35f;
	}
}
