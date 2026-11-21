using System;
using GorillaLocomotion;
using Sonder.Core;
using Sonder.Menu;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Sonder.Managers;

public class StumpBanner : MonoBehaviour
{
	private const string MenuName = "SONDER";

	private const float NameFontSize = 14f;

	private const float NameVertical = 0.55f;

	private const float PanelWidth = 24f;

	private const float HeightAboveStump = 1.6f;

	private const float GlowOuterBase = 0.38f;

	private const float GlowPulse = 0.14f;

	private const float RescanInterval = 2f;

	private static readonly Color Gold;

	private Transform _stump;

	private Vector3 _stumpCenter;

	private float _stumpTop;

	private GameObject _bannerRoot;

	private TextMeshPro _nameText;

	private float _nextScan;

	public static StumpBanner Instance { get; private set; }

	public bool HasAnchor => (Object)(object)_stump != (Object)null;

	public Vector3 Anchor
	{
		get
		{
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			return new Vector3(_stumpCenter.x, _stumpTop + 1.6f, _stumpCenter.z);
		}
	}

	public float Top => _stumpTop;

	public float Radius
	{
		get
		{
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)_stump == (Object)null)
			{
				return 0f;
			}
			Renderer[] componentsInChildren = ((Component)_stump).GetComponentsInChildren<Renderer>();
			if (componentsInChildren.Length == 0)
			{
				return 1f;
			}
			Bounds bounds = componentsInChildren[0].bounds;
			for (int i = 1; i < componentsInChildren.Length; i++)
			{
				bounds.Encapsulate(componentsInChildren[i].bounds);
			}
			return Mathf.Max(bounds.extents.x, bounds.extents.z);
		}
	}

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
		if ((Object)(object)_stump == (Object)null)
		{
			if (!(Time.unscaledTime < _nextScan))
			{
				_nextScan = Time.unscaledTime + 2f;
				FindStump();
			}
			return;
		}
		// The floating SONDER sign over the Stump is switched off. Finding the Stump still matters: Stump Dock
		// parks the menu using its position.
		if (ShowBanner)
		{
			if ((Object)(object)_bannerRoot == (Object)null || (Object)(object)_nameText == (Object)null)
			{
				BuildBanner();
			}
			if (!((Object)(object)_bannerRoot == (Object)null))
			{
				PositionBanner();
				FaceViewer();
				PulseGlow();
			}
		}
	}

	private const bool ShowBanner = true;

	private void FindStump()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		Transform val = null;
		Transform[] array = Object.FindObjectsByType<Transform>((FindObjectsInactive)0, (FindObjectsSortMode)0);
		foreach (Transform val2 in array)
		{
			if ((Object)(object)val2 == (Object)null)
			{
				continue;
			}
			Scene scene = ((Component)val2).gameObject.scene;
			if (scene.IsValid() && ((Component)val2).gameObject.activeInHierarchy)
			{
				string name = ((Object)val2).name;
				if (name.Equals("Stump", StringComparison.OrdinalIgnoreCase))
				{
					val = val2;
					break;
				}
				if ((Object)(object)val == (Object)null && name.IndexOf("stump", StringComparison.OrdinalIgnoreCase) >= 0)
				{
					val = val2;
				}
			}
		}
		if ((Object)(object)val == (Object)null)
		{
			return;
		}
		_stump = val;
		_stumpCenter = val.position;
		_stumpTop = val.position.y;
		Renderer[] componentsInChildren = ((Component)val).GetComponentsInChildren<Renderer>();
		if (componentsInChildren.Length != 0)
		{
			Bounds bounds = componentsInChildren[0].bounds;
			for (int j = 1; j < componentsInChildren.Length; j++)
			{
				bounds.Encapsulate(componentsInChildren[j].bounds);
			}
			_stumpCenter = bounds.center;
			_stumpTop = bounds.max.y;
		}
		// The match is often a wall piece (like the wood signs), so center on where the player spawns instead.
		GTPlayer player = GTPlayer.Instance;
		if ((Object)(object)player != (Object)null)
		{
			Vector3 spawn = ((Component)player).transform.position;
			_stumpCenter = new Vector3(spawn.x, _stumpCenter.y, spawn.z);
		}
		RuntimeLog.Write("stump banner anchored: " + ((Object)val).name);
	}

	private void BuildBanner()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		TMP_FontAsset val = FontLoader.Get();
		if (!((Object)(object)val == (Object)null))
		{
			_bannerRoot = new GameObject("Sonder_StumpBanner");
			_bannerRoot.transform.SetParent(((Component)this).transform, false);
			_bannerRoot.transform.localScale = Vector3.one;
			_nameText = CreateBannerText(val, "Name", "Welcome to Sonder", 14f, 0f);
		}
	}

	private TextMeshPro CreateBannerText(TMP_FontAsset font, string objectName, string content, float size, float vertical)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = new GameObject(objectName);
		val.transform.SetParent(_bannerRoot.transform, false);
		val.transform.localPosition = new Vector3(0f, vertical, 0f);
		val.transform.localRotation = Quaternion.identity;
		val.transform.localScale = Vector3.one;
		TextMeshPro obj = val.AddComponent<TextMeshPro>();
		((TMP_Text)obj).font = font;
		((TMP_Text)obj).text = content;
		((TMP_Text)obj).fontSize = size;
		((Graphic)obj).color = Gold;
		((TMP_Text)obj).alignment = (TextAlignmentOptions)514;
		((TMP_Text)obj).richText = false;
		((TMP_Text)obj).overflowMode = (TextOverflowModes)0;
		((TMP_Text)obj).fontStyle = (FontStyles)1;
		RectTransform rectTransform = ((TMP_Text)obj).rectTransform;
		rectTransform.sizeDelta = new Vector2(24f, size * 0.2f);
		rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
		rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
		rectTransform.pivot = new Vector2(0.5f, 0.5f);
		Material fontMaterial = ((TMP_Text)obj).fontMaterial;
		if ((Object)(object)fontMaterial != (Object)null && fontMaterial.HasProperty("_GlowColor"))
		{
			fontMaterial.EnableKeyword("GLOW_ON");
			fontMaterial.SetColor("_GlowColor", Gold);
			fontMaterial.SetFloat("_GlowInner", 0.05f);
			fontMaterial.SetFloat("_GlowOuter", 0.38f);
			fontMaterial.SetFloat("_GlowPower", 0.85f);
			fontMaterial.SetColor("_UnderlayColor", new Color(0f, 0f, 0f, 0.35f));
		}
		MeshRenderer component = val.GetComponent<MeshRenderer>();
		if ((Object)(object)component != (Object)null)
		{
			((Renderer)component).shadowCastingMode = (ShadowCastingMode)0;
			((Renderer)component).receiveShadows = false;
		}
		return obj;
	}

	private void PositionBanner()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)_stump == (Object)null))
		{
			_bannerRoot.transform.position = new Vector3(_stumpCenter.x, _stumpTop + 1.6f, _stumpCenter.z);
		}
	}

	private void FaceViewer()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		Camera main = Camera.main;
		Vector3 position;
		Vector3 up;
		if ((Object)(object)main != (Object)null)
		{
			position = ((Component)main).transform.position;
			up = ((Component)main).transform.up;
		}
		else
		{
			Transform val = ExtraUtil.Head();
			if ((Object)(object)val == (Object)null)
			{
				return;
			}
			position = val.position;
			up = val.up;
		}
		// Turn left and right only, so the sign does not roll or flip when you tilt your head or look up.
		Vector3 val2 = _bannerRoot.transform.position - position;
		val2.y = 0f;
		if (!(val2.sqrMagnitude < 0.001f))
		{
			_bannerRoot.transform.rotation = Quaternion.LookRotation(val2.normalized, Vector3.up);
		}
	}

	private void PulseGlow()
	{
		if (!((Object)(object)_nameText == (Object)null))
		{
			float num = 0.38f + Mathf.Sin(Time.unscaledTime * 1.6f) * 0.14f;
			Material fontMaterial = ((TMP_Text)_nameText).fontMaterial;
			if ((Object)(object)fontMaterial != (Object)null && fontMaterial.HasProperty("_GlowOuter"))
			{
				fontMaterial.SetFloat("_GlowOuter", num);
			}
		}
	}

	static StumpBanner()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		Gold = (new Color32((byte)222, (byte)184, (byte)135, byte.MaxValue));
	}
}
