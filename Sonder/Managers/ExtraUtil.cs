using System.Collections.Generic;
using System.Text;
using GorillaLocomotion;
using UnityEngine;
using Object = UnityEngine.Object;
using UnityEngine.Rendering;

namespace Sonder.Managers;

internal static class ExtraUtil
{
	private static readonly Dictionary<string, float> TimeScales = new Dictionary<string, float>();

	private static Font _labelFont;

	private static Material _labelMaterial;

	public static GTPlayer Player => GTPlayer.Instance;

	public static GorillaTagger Tagger => GorillaTagger.Instance;

	public static GorillaTagManager TagManager
	{
		get
		{
			GorillaGameManager instance = GorillaGameManager.instance;
			return (GorillaTagManager)(object)((instance is GorillaTagManager) ? instance : null);
		}
	}

	public static List<VRRig> RemoteRigs()
	{
		List<VRRig> list = new List<VRRig>();
		IReadOnlyList<VRRig> activeRigs = VRRigCache.ActiveRigs;
		if (activeRigs == null)
		{
			return list;
		}
		for (int i = 0; i < activeRigs.Count; i++)
		{
			VRRig val = activeRigs[i];
			if (!((Object)(object)val == (Object)null) && !val.isLocal && val.Creator != null)
			{
				list.Add(val);
			}
		}
		return list;
	}

	public static Transform Hand(bool left)
	{
		GorillaTagger tagger = Tagger;
		if ((Object)(object)tagger == (Object)null)
		{
			return null;
		}
		if (!left)
		{
			return tagger.rightHandTransform;
		}
		return tagger.leftHandTransform;
	}

	public static Transform Head()
	{
		GorillaTagger tagger = Tagger;
		if ((Object)(object)tagger != (Object)null && (Object)(object)tagger.headCollider != (Object)null)
		{
			return ((Component)tagger.headCollider).transform;
		}
		Camera main = Camera.main;
		if (!((Object)(object)main != (Object)null))
		{
			return null;
		}
		return ((Component)main).transform;
	}

	public static Vector3 PlayerPosition()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		GTPlayer player = Player;
		if ((Object)(object)player != (Object)null)
		{
			return ((Component)player).transform.position;
		}
		Rigidbody val = Body();
		if (!((Object)(object)val != (Object)null))
		{
			return Vector3.zero;
		}
		return ((Component)val).transform.position;
	}

	public static Rigidbody Body()
	{
		GTPlayer player = Player;
		if ((Object)(object)player != (Object)null && (Object)(object)player.playerRigidBody != (Object)null)
		{
			return player.playerRigidBody;
		}
		GorillaTagger tagger = Tagger;
		if (!((Object)(object)tagger != (Object)null))
		{
			return null;
		}
		return tagger.rigidbody;
	}

	public static void Teleport(Vector3 position)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		GTPlayer player = Player;
		if (!((Object)(object)player == (Object)null))
		{
			player.TeleportTo(position, ((Component)player).transform.rotation, false, false);
		}
	}

	public static Vector3 Grounded(Vector3 hint)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		GTPlayer player = Player;
		int num = (((Object)(object)player != (Object)null) ? (player.locomotionEnabledLayers) : (-1));
		RaycastHit val = default(RaycastHit);
		if (Physics.Raycast(hint + Vector3.up * 12f, Vector3.down, out val, 80f, num, (QueryTriggerInteraction)1))
		{
			return val.point + Vector3.up * 0.05f;
		}
		return hint;
	}

	public static string Nickname(VRRig rig)
	{
		if ((Object)(object)rig == (Object)null)
		{
			return "player";
		}
		NetPlayer creator = rig.Creator;
		if (creator != null)
		{
			string sanitizedNickName = creator.SanitizedNickName;
			if (!string.IsNullOrEmpty(sanitizedNickName))
			{
				return sanitizedNickName;
			}
			sanitizedNickName = creator.NickName;
			if (!string.IsNullOrEmpty(sanitizedNickName))
			{
				return sanitizedNickName;
			}
		}
		if (!string.IsNullOrEmpty(rig.playerNameVisible))
		{
			return rig.playerNameVisible;
		}
		return "player";
	}

	public static string NormalizeName(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder(name.Length);
		foreach (char c in name)
		{
			if (char.IsLetterOrDigit(c))
			{
				stringBuilder.Append(char.ToLowerInvariant(c));
			}
		}
		while (stringBuilder.Length > 0 && char.IsDigit(stringBuilder[stringBuilder.Length - 1]))
		{
			stringBuilder.Length--;
		}
		return stringBuilder.ToString();
	}

	public static bool IsLocalCollider(Collider collider)
	{
		if ((Object)(object)collider == (Object)null)
		{
			return false;
		}
		GorillaTagger tagger = Tagger;
		if ((Object)(object)tagger != (Object)null)
		{
			if ((Object)(object)tagger.bodyCollider != (Object)null && ((Component)collider).transform.IsChildOf(((Component)tagger.bodyCollider).transform))
			{
				return true;
			}
			if ((Object)(object)tagger.headCollider != (Object)null && ((Component)collider).transform.IsChildOf(((Component)tagger.headCollider).transform))
			{
				return true;
			}
			if ((Object)(object)tagger.leftHandTriggerCollider != (Object)null && ((Component)collider).transform.IsChildOf(tagger.leftHandTriggerCollider.transform))
			{
				return true;
			}
			if ((Object)(object)tagger.rightHandTriggerCollider != (Object)null && ((Component)collider).transform.IsChildOf(tagger.rightHandTriggerCollider.transform))
			{
				return true;
			}
		}
		VRRig localRig = VRRig.LocalRig;
		if ((Object)(object)localRig != (Object)null)
		{
			return ((Component)collider).transform.IsChildOf(((Component)localRig).transform);
		}
		return false;
	}

	public static Material Unlit(Color color, bool transparent)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Expected O, but got Unknown
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		Shader val = Shader.Find(transparent ? "Sprites/Default" : "Unlit/Color");
		if ((Object)(object)val == (Object)null)
		{
			val = Shader.Find("Sprites/Default");
		}
		Material val2 = new Material(val);
		if (val2.HasProperty("_Color"))
		{
			val2.SetColor("_Color", color);
		}
		else
		{
			val2.color = color;
		}
		if (transparent)
		{
			val2.renderQueue = 3000;
		}
		return val2;
	}

	public static Material ThroughWall(Color color)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		Shader val = Shader.Find("GUI/Text Shader");
		if ((Object)(object)val == (Object)null)
		{
			val = Shader.Find("Hidden/Internal-Colored");
		}
		if ((Object)(object)val == (Object)null)
		{
			val = Shader.Find("Sprites/Default");
		}
		Material val2 = new Material(val);
		if (val2.HasProperty("_Color"))
		{
			val2.SetColor("_Color", color);
		}
		else
		{
			val2.color = color;
		}
		try
		{
			val2.SetInt("_ZTest", (int)CompareFunction.Always);
			val2.SetInt("_ZWrite", 0);
		}
		catch { }
		val2.renderQueue = 4000;
		return val2;
	}

	public static TextMesh CreateLabel(Transform parent, string name, Color color)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		if (!EnsureLabelAssets())
		{
			return null;
		}
		GameObject val = new GameObject(name);
		val.transform.SetParent(parent, false);
		TextMesh val2 = val.AddComponent<TextMesh>();
		val2.font = _labelFont;
		val2.fontSize = 40;
		val2.characterSize = 0.1f;
		val2.anchor = (TextAnchor)4;
		val2.alignment = (TextAlignment)1;
		val2.color = color;
		val2.text = string.Empty;
		MeshRenderer component = val.GetComponent<MeshRenderer>();
		if ((Object)(object)component != (Object)null)
		{
			((Renderer)component).sharedMaterial = _labelMaterial;
			((Renderer)component).shadowCastingMode = (ShadowCastingMode)0;
			((Renderer)component).receiveShadows = false;
		}
		return val2;
	}

	public static void FaceCamera(TextMesh text)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)text == (Object)null))
		{
			Camera main = Camera.main;
			if (!((Object)(object)main == (Object)null))
			{
				((Component)text).transform.rotation = ((Component)main).transform.rotation;
			}
		}
	}

	private static bool EnsureLabelAssets()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_labelFont != (Object)null && (Object)(object)_labelMaterial != (Object)null)
		{
			return true;
		}
		Font val = null;
		try
		{
			val = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
		}
		catch
		{
		}
		if ((Object)(object)val == (Object)null)
		{
			try
			{
				val = Resources.GetBuiltinResource<Font>("Arial.ttf");
			}
			catch
			{
			}
		}
		if ((Object)(object)val == (Object)null)
		{
			try
			{
				val = Font.CreateDynamicFontFromOSFont("Arial", 32);
			}
			catch
			{
			}
		}
		if ((Object)(object)val == (Object)null)
		{
			return false;
		}
		_labelFont = val;
		Material val2 = new Material(val.material);
		Shader val3 = Shader.Find("GUI/Text Shader");
		if ((Object)(object)val3 != (Object)null)
		{
			val2.shader = val3;
		}
		val2.color = Color.white;
		_labelMaterial = val2;
		return true;
	}

	public static void SetTimeScale(string key, bool enabled, float scale)
	{
		if (enabled)
		{
			TimeScales[key] = scale;
		}
		else
		{
			TimeScales.Remove(key);
		}
		ApplyTimeScale();
	}

	private static void ApplyTimeScale()
	{
		if (TimeScales.Count == 0)
		{
			Time.timeScale = 1f;
			return;
		}
		float num = 1f;
		foreach (KeyValuePair<string, float> timeScale in TimeScales)
		{
			if (timeScale.Value < num)
			{
				num = timeScale.Value;
			}
		}
		Time.timeScale = num;
	}
}
