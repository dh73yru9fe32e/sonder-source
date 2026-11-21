using UnityEngine;
using Object = UnityEngine.Object;

namespace Sonder.Managers;

public sealed class SonderProjectile : MonoBehaviour
{
	public ProjectileRuntime Runtime;

	public SonderShotType Type;

	public Vector3 Velocity;

	public float Gravity;

	public float Life;

	public float Radius;

	public Color PaintColor;

	public float SpawnTime;

	public Material Material;

	private float _ignoreUntil;

	private void Start()
	{
		_ignoreUntil = SpawnTime + 0.08f;
	}

	private void Update()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		Life -= Time.deltaTime;
		if (Life <= 0f || ((Component)this).transform.position.y < -120f)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
			return;
		}
		Vector3 position = ((Component)this).transform.position;
		Velocity += Vector3.down * (Gravity * Time.deltaTime);
		Vector3 val = position + Velocity * Time.deltaTime;
		Vector3 val2 = val - position;
		float magnitude = val2.magnitude;
		if (magnitude > 0.0001f)
		{
			RaycastHit[] array = Physics.RaycastAll(position, val2 / magnitude, magnitude, -1, (QueryTriggerInteraction)1);
			RaycastHit val3 = default(RaycastHit);
			bool flag = false;
			float num = float.MaxValue;
			for (int i = 0; i < array.Length; i++)
			{
				Collider collider = array[i].collider;
				if (!((Object)(object)collider == (Object)null) && !(array[i].distance < 0.03f) && (!(Time.time < _ignoreUntil) || !(array[i].distance < 0.5f)) && !ExtraUtil.IsLocalCollider(collider) && !((Object)(object)((Component)collider).gameObject == (Object)(object)((Component)this).gameObject) && !(array[i].distance >= num))
				{
					val3 = array[i];
					num = array[i].distance;
					flag = true;
				}
			}
			if (flag)
			{
				if ((Object)(object)Runtime != (Object)null)
				{
					Runtime.Hit(this, val3.collider, val3.point);
				}
				Object.Destroy((Object)(object)((Component)this).gameObject);
				return;
			}
		}
		((Component)this).transform.position = val;
	}

	private void OnDestroy()
	{
		if ((Object)(object)Material != (Object)null)
		{
			Object.Destroy((Object)(object)Material);
		}
	}
}
