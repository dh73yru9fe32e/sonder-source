using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;
using UnityEngine.UI;

namespace Sonder.Core.UI.Framework;

public class MenuPointer
{
	private const float ExtendRate = 12f;

	private GameObject root;

	private Transform tip;

	private Renderer tipRenderer;

	private float extend;

	public void Build()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Expected Obj, but got Unknown
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)root != (Object)null))
		{
			root = new GameObject("MatchaPointer");
			tip = CreatePart((PrimitiveType)0, "Tip", out tipRenderer);
			SetColor(tipRenderer, Color.white);
			root.SetActive(false);
		}
	}

	public void Tick(bool visible, Vector3 origin, Vector3 dir, float length, bool pressed)
	{
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)root == (Object)null))
		{
			if (visible && !root.activeSelf)
			{
				root.SetActive(true);
			}
			float num = (visible ? 1f : 0f);
			extend = Mathf.Lerp(extend, num, 1f - Mathf.Exp((0f - Time.unscaledDeltaTime) * 12f));
			if (!visible && extend < 0.01f)
			{
				extend = 0f;
				root.SetActive(false);
				return;
			}
			float num2 = extend * extend * (3f - 2f * extend);
			float num3 = Mathf.Max(length, 0.01f) * num2;
			tip.position = origin + dir * num3;
			float num4 = 0.028f * num2 * (pressed ? 0.7f : 1f);
			tip.rotation = Quaternion.LookRotation(dir);
			tip.localScale = new Vector3(num4 * 0.7f, num4 * 1.2f, num4 * 0.45f);
			tip.localScale = Vector3.Max(tip.localScale, Vector3.one * 0.0001f);
		}
	}

	public void Hide()
	{
		if ((Object)(object)root != (Object)null)
		{
			root.SetActive(false);
		}
		extend = 0f;
	}

	public void Destroy()
	{
		if ((Object)(object)root != (Object)null)
		{
			Object.Destroy((Object)(object)root);
		}
		root = null;
	}

	private Transform CreatePart(PrimitiveType type, string name, out Renderer renderer)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = GameObject.CreatePrimitive(type);
		((Object)val).name = name;
		val.transform.SetParent(root.transform, false);
		Collider component = val.GetComponent<Collider>();
		if ((Object)(object)component != (Object)null)
		{
			Object.Destroy((Object)(object)component);
		}
		renderer = val.GetComponent<Renderer>();
		return val.transform;
	}

	private static void SetColor(Renderer r, Color c)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)r == (Object)null))
		{
			Material material = r.material;
			if (material.HasProperty("_BaseColor"))
			{
				material.SetColor("_BaseColor", c);
			}
			if (material.HasProperty("_Color"))
			{
				material.SetColor("_Color", c);
			}
		}
	}
}



