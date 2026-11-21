using System;
using System.Collections.Generic;
using System.IO;
using Photon.Pun;
using Sonder.Core;
using Sonder.Core.UI.Framework;
using Sonder.Managers;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.UI;

namespace Sonder.Menu;

public static class MenuBuilder
{
	public const float PanelWidth = 460f;

	public const float PanelHeight = 480f;

	public const int MenuLayerIndex = 2;

	private const float HeaderHeight = 64f;

	private const float FooterHeight = 28f;

	private const float ListMargin = 16f;

	private const float ButtonHeight = 56f;

	private const float ButtonSpacing = 8f;

	private const float ListWidth = 768f;

	public static Action IntroContinue;

	private static RectTransform _content;

	private static RectTransform _viewport;

	private static ContentSizeFitter _contentFitter;

	private static MenuButton _leftNav;

	private static MenuButton _rightNav;
	private static TextMeshProUGUI _tooltipText;

	private static GameObject _introRoot;

	private static CanvasGroup _introGroup;

	private static RectTransform _introContent;

	private static float _introEntrance = 1f;

	private static TMP_FontAsset _font;

	private static Sprite _panelSprite;

	private static Sprite _buttonSprite;

	private static Sprite _panelRing;

	private static Sprite _buttonRing;

	private static string _searchQuery = string.Empty;

	private static MenuButton _searchButton;

	private static GameObject _keyboardRoot;

	private static bool _keyboardBuilt;

	private static Sprite _searchSprite;

	private static bool _searchSpriteFailed;

	private const int SearchIconWidth = 474;

	private const int SearchIconHeight = 472;

	private static Color AccentColor
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			return SonderTheme.Border;
		}
	}

	private static Color AccentSoftColor
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			Color ink = SonderTheme.Border;
			ink.a = 0.35f;
			return ink;
		}
	}

	private static Color AccentGlowColor
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			Color sheen = Color.white;
			sheen.a = 0.16f;
			return sheen;
		}
	}

	private static Color CategoryColor
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			return SonderTheme.Description;
		}
	}

	public static TextMeshProUGUI FpsText { get; private set; }

	public static CanvasGroup IntroGroup => _introGroup;

	public static bool IntroVisible { get; private set; }

	public static TextMeshProUGUI NameText { get; private set; }

	public static TextMeshProUGUI CategoryText { get; private set; }

	public static bool SearchActive { get; private set; }

	public static void Build(GameObject root)
	{
		_font = FontLoader.Get();
		_content = null;
		_panelSprite = SonderSprite.Fill(SonderSettings.PanelRadius, 0.62f);
		_buttonSprite = SonderSprite.Fill(SonderSettings.ButtonRadius, 0.76f);
		_panelRing = SonderSprite.Border(SonderSettings.PanelRadius, 1);
		_buttonRing = SonderSprite.Border(SonderSettings.ButtonRadius, 1);
		RectTransform component = root.GetComponent<RectTransform>();
		BuildBackground(component);
		BuildHeader(component);
		BuildList(component);
		BuildBottomNav(component);
		BuildDisconnectButton(component);
		BuildSearchButton(component);
		BuildIntro(component);
		ShowHome();
	}

	private static void BuildDisconnectButton(RectTransform root)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		MenuButton menuButton = CreateButton((Transform)(object)root, "DISCONNECT", string.Empty, new Vector2(92f, 24f), 10f, centered: true, Disconnect);
		RectTransform component = ((Component)menuButton).GetComponent<RectTransform>();
		component.anchorMin = new Vector2(1f, 1f);
		component.anchorMax = new Vector2(1f, 1f);
		component.pivot = new Vector2(1f, 1f);
		component.anchoredPosition = new Vector2(-62f, -14f);
		menuButton.normalBackground = SonderTheme.Danger;
		menuButton.hoverBackground = SonderTheme.DangerHover;
		menuButton.normalTitle = SonderTheme.DangerTitle;
		menuButton.hoverTitle = SonderTheme.DangerTitle;
		((Graphic)menuButton.background).color = SonderTheme.Danger;
		Color glassInk = SonderTheme.Border;
		glassInk.a = 0.62f;
		menuButton.normalBorder = glassInk;
		menuButton.hoverBorder = SonderTheme.BorderHover;
		if ((Object)(object)menuButton.border != (Object)null)
		{
			((Graphic)menuButton.border).color = glassInk;
		}
	}

	private static void BuildSearchButton(RectTransform root)
	{
		_searchButton = CreateButton((Transform)(object)root, "", string.Empty, new Vector2(32f, 32f), 12f, centered: true, ToggleSearch);
		if ((Object)(object)_searchButton.background != (Object)null)
		{
			((Graphic)_searchButton.background).color = Color.clear;
		}
		if ((Object)(object)_searchButton.border != (Object)null)
		{
			((Graphic)_searchButton.border).color = Color.clear;
		}
		_searchButton.normalBackground = Color.clear;
		_searchButton.hoverBackground = Color.clear;
		_searchButton.normalBorder = Color.clear;
		_searchButton.hoverBorder = Color.clear;
		// Magnifying glass icon (no font dependency).
		GameObject ring = CreateUIObject("SearchRing", ((Component)_searchButton).transform);
		RectTransform ringRect = ring.GetComponent<RectTransform>();
		ringRect.anchorMin = new Vector2(0.5f, 0.5f);
		ringRect.anchorMax = new Vector2(0.5f, 0.5f);
		ringRect.pivot = new Vector2(0.5f, 0.5f);
		ringRect.anchoredPosition = new Vector2(-3f, 3f);
		ringRect.sizeDelta = new Vector2(14f, 14f);
		Image ringImg = ring.AddComponent<Image>();
		ringImg.sprite = SonderSprite.Border(10, 3);
		ringImg.type = (Image.Type)0;
		((Graphic)ringImg).color = AccentColor;
		((Graphic)ringImg).raycastTarget = false;
		GameObject handle = CreateUIObject("SearchHandle", ((Component)_searchButton).transform);
		RectTransform handleRect = handle.GetComponent<RectTransform>();
		handleRect.anchorMin = new Vector2(0.5f, 0.5f);
		handleRect.anchorMax = new Vector2(0.5f, 0.5f);
		handleRect.pivot = new Vector2(0.5f, 0.5f);
		handleRect.anchoredPosition = new Vector2(5f, -5f);
		handleRect.sizeDelta = new Vector2(9f, 2f);
		handleRect.localRotation = Quaternion.Euler(0f, 0f, -45f);
		Image handleImg = handle.AddComponent<Image>();
		handleImg.sprite = SonderSprite.Flat(2);
		handleImg.type = (Image.Type)0;
		((Graphic)handleImg).color = AccentColor;
		((Graphic)handleImg).raycastTarget = false;
		RectTransform component = ((Component)_searchButton).GetComponent<RectTransform>();
		component.anchorMin = new Vector2(1f, 1f);
		component.anchorMax = new Vector2(1f, 1f);
		component.pivot = new Vector2(1f, 1f);
		component.anchoredPosition = new Vector2(-14f, -14f);
	}

	private static void Disconnect()
	{
		if (PhotonNetwork.IsConnected)
		{
			PhotonNetwork.Disconnect();
		}
	}

	private static void BuildBottomNav(RectTransform root)
	{
		// Bottom navigation bar: prev / home / next.
		MenuButton prev = CreateButton((Transform)(object)root, "<", string.Empty, new Vector2(44f, 44f), 20f, centered: true, NavPrevious);
		prev.Tooltip = "Previous page";
		RectTransform pr = ((Component)prev).GetComponent<RectTransform>();
		pr.anchorMin = new Vector2(0f, 0f);
		pr.anchorMax = new Vector2(0f, 0f);
		pr.pivot = new Vector2(0f, 0f);
		pr.anchoredPosition = new Vector2(16f, 8f);
		_leftNav = prev;
		MenuButton home = CreateButton((Transform)(object)root, "\u2302", string.Empty, new Vector2(64f, 44f), 20f, centered: true, HomeButton);
		home.Tooltip = "Home";
		RectTransform hr = ((Component)home).GetComponent<RectTransform>();
		hr.anchorMin = new Vector2(0.5f, 0f);
		hr.anchorMax = new Vector2(0.5f, 0f);
		hr.pivot = new Vector2(0.5f, 0f);
		hr.anchoredPosition = new Vector2(0f, 8f);
		MenuButton next = CreateButton((Transform)(object)root, ">", string.Empty, new Vector2(44f, 44f), 20f, centered: true, NavNext);
		next.Tooltip = "Next page";
		RectTransform nr = ((Component)next).GetComponent<RectTransform>();
		nr.anchorMin = new Vector2(1f, 0f);
		nr.anchorMax = new Vector2(1f, 0f);
		nr.pivot = new Vector2(1f, 0f);
		nr.anchoredPosition = new Vector2(-16f, 8f);
		_rightNav = next;
		// Tooltip text (above nav).
		_tooltipText = CreateText((Transform)(object)root, "Tooltip", string.Empty, 10f, SonderTheme.Description, (TextAlignmentOptions)514, bold: false);
		RectTransform tr = ((TMP_Text)_tooltipText).rectTransform;
		tr.anchorMin = new Vector2(0.5f, 0f);
		tr.anchorMax = new Vector2(0.5f, 0f);
		tr.pivot = new Vector2(0.5f, 0f);
		tr.anchoredPosition = new Vector2(0f, 56f);
		tr.sizeDelta = new Vector2(400f, 14f);
	}

	public static void ShowTooltip(string text)
	{
		if ((Object)(object)_tooltipText != (Object)null)
		{
			((TMP_Text)_tooltipText).text = (string.IsNullOrEmpty(text) ? string.Empty : text);
		}
	}

	private static void BuildIntro(RectTransform root)
	{
		_introRoot = CreateUIObject("Intro", (Transform)(object)root);
		Stretch(_introRoot.GetComponent<RectTransform>(), 0f, 0f, 0f, 0f);
		_introGroup = _introRoot.AddComponent<CanvasGroup>();
		_introGroup.alpha = 0f;
		_introGroup.interactable = true;
		_introGroup.blocksRaycasts = true;

		GameObject backdrop = CreateUIObject("IntroDim", _introRoot.transform);
		Stretch(backdrop.GetComponent<RectTransform>(), 0f, 0f, 0f, 0f);
		Image backImage = backdrop.AddComponent<Image>();
		backImage.sprite = SonderSprite.Fill(20, 1f);
		backImage.type = (Image.Type)1;
		Color backColor = Color.black;
		backColor.a = 0.42f;
		((Graphic)backImage).color = backColor;
		((Graphic)backImage).raycastTarget = false;

		GameObject content = CreateUIObject("IntroContent", _introRoot.transform);
		RectTransform contentRect = content.GetComponent<RectTransform>();
		contentRect.anchorMin = new Vector2(0.5f, 0.5f);
		contentRect.anchorMax = new Vector2(0.5f, 0.5f);
		contentRect.pivot = new Vector2(0.5f, 0.5f);
		contentRect.sizeDelta = new Vector2(520f, 150f);
		contentRect.anchoredPosition = Vector2.zero;
		_introContent = contentRect;

		TextMeshProUGUI prompt = CreateText(content.transform, "IntroPrompt", "welcome! press continue to enter", 20f, Color.white, (TextAlignmentOptions)514, bold: true);
		RectTransform promptRect = ((TMP_Text)prompt).rectTransform;
		promptRect.anchorMin = new Vector2(0.5f, 0.5f);
		promptRect.anchorMax = new Vector2(0.5f, 0.5f);
		promptRect.pivot = new Vector2(0.5f, 0.5f);
		promptRect.anchoredPosition = new Vector2(0f, 28f);
		promptRect.sizeDelta = new Vector2(520f, 36f);

		RectTransform enter = ((Component)CreateButton(content.transform, "CONTINUE", string.Empty, new Vector2(180f, 44f), 16f, centered: true, delegate
		{
			HideIntro();
			IntroContinue?.Invoke();
		})).GetComponent<RectTransform>();
		enter.anchorMin = new Vector2(0.5f, 0.5f);
		enter.anchorMax = new Vector2(0.5f, 0.5f);
		enter.pivot = new Vector2(0.5f, 0.5f);
		enter.anchoredPosition = new Vector2(0f, -32f);

		_introRoot.SetActive(false);
		IntroVisible = false;
	}

	public static void ShowIntro()
	{
		if (!((Object)(object)_introRoot == (Object)null))
		{
			_introRoot.SetActive(true);
			IntroVisible = true;
			_introEntrance = 0f;
			if ((Object)(object)_introContent != (Object)null)
			{
				_introContent.localScale = Vector3.one * 0.92f;
				_introContent.anchoredPosition = new Vector2(0f, -18f);
			}
			if ((Object)(object)_introGroup != (Object)null)
			{
				_introGroup.alpha = 0f;
				_introGroup.interactable = true;
				_introGroup.blocksRaycasts = true;
			}
		}
	}

	public static void HideIntro()
	{
		if (!((Object)(object)_introRoot == (Object)null))
		{
			_introRoot.SetActive(false);
			IntroVisible = false;
			_introEntrance = 1f;
			if ((Object)(object)_introGroup != (Object)null)
			{
				_introGroup.alpha = 0f;
				_introGroup.blocksRaycasts = false;
			}
		}
	}

	public static Sprite CreateRoundedSprite(int size, int radius)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Expected O, but got Unknown
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		Texture2D val = new Texture2D(size, size, (TextureFormat)4, false);
		((Texture)val).filterMode = (FilterMode)1;
		((Texture)val).wrapMode = (TextureWrapMode)1;
		for (int i = 0; i < size; i++)
		{
			for (int j = 0; j < size; j++)
			{
				float num = 0f;
				float num2 = 0f;
				if (j < radius)
				{
					num = (float)(radius - j) - 0.5f;
				}
				else if (j >= size - radius)
				{
					num = (float)(j - (size - radius)) + 0.5f;
				}
				if (i < radius)
				{
					num2 = (float)(radius - i) - 0.5f;
				}
				else if (i >= size - radius)
				{
					num2 = (float)(i - (size - radius)) + 0.5f;
				}
				float num3 = 1f;
				if (num > 0f || num2 > 0f)
				{
					num3 = Mathf.Clamp01((float)radius - Mathf.Sqrt(num * num + num2 * num2) + 0.5f);
				}
				val.SetPixel(j, i, new Color(1f, 1f, 1f, num3));
			}
		}
		val.Apply(false, false);
		return Sprite.Create(val, new Rect(0f, 0f, (float)size, (float)size), new Vector2(0.5f, 0.5f), 100f, 0u, (SpriteMeshType)0, new Vector4((float)radius, (float)radius, (float)radius, (float)radius));
	}

	public static Material CreateMaterial(Color color, params string[] shaderNames)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		Shader val = null;
		for (int i = 0; i < shaderNames.Length; i++)
		{
			if (!((Object)(object)val == (Object)null))
			{
				break;
			}
			val = Shader.Find(shaderNames[i]);
		}
		if ((Object)(object)val == (Object)null)
		{
			return null;
		}
		Material val2 = new Material(val);
		if (val2.HasProperty("_Color"))
		{
			val2.color = color;
		}
		return val2;
	}

	public static void ShowHome()
	{
		ModManager.ShowHome();
		RefreshCategory();
	}

	public static void RebuildCurrent()
	{
		if (!((Object)(object)CategoryText == (Object)null))
		{
			RefreshCategory();
		}
	}

	public static void ShowCategory(int index)
	{
		ModManager.SetCategory(index);
		ModManager.ShowMods();
		RefreshCategory();
	}

	public static void NextCategory()
	{
		ModManager.NextCategory();
		ModManager.ShowMods();
		RefreshCategory();
	}

	public static void PreviousCategory()
	{
		ModManager.PreviousCategory();
		ModManager.ShowMods();
		RefreshCategory();
	}

	private static int _homePage;

	private static int HomePageCount => Mathf.CeilToInt(ModManager.Categories.Count / 6f);

	private static int HomePerPage => 6;

	// Page motion: one timed ease so chained clicks stay predictable.
	private static Vector2 _swipeOffset;
	private static Vector2 _swipeFrom;
	private static bool _swipeActive;
	private static float _swipeAlpha = 1f;
	private static float _swipeElapsed;
	private const float SwipeDuration = 0.34f;

	private static void BeginSwipe(Vector2 from)
	{
		_swipeFrom = from;
		_swipeOffset = from;
		_swipeElapsed = 0f;
		_swipeAlpha = 0.12f;
		_swipeActive = true;
		ApplySwipe();
	}

	public static void UpdateSwipe()
	{
		UpdateIntroEntrance();
		if (!_swipeActive || (Object)(object)_content == (Object)null)
		{
			return;
		}
		float dt = Time.unscaledDeltaTime;
		if (dt > 0.05f) dt = 0.05f;
		_swipeElapsed += dt;
		float t = Mathf.Clamp01(_swipeElapsed / SwipeDuration);
		float eased = 1f - (1f - t) * (1f - t) * (1f - t);
		_swipeOffset = Vector2.Lerp(_swipeFrom, Vector2.zero, eased);
		_swipeAlpha = Mathf.Lerp(0.12f, 1f, eased);
		if (t >= 1f)
		{
			_swipeOffset = Vector2.zero;
			_swipeAlpha = 1f;
			_swipeActive = false;
		}
		ApplySwipe();
	}

	private static void ApplySwipe()
	{
		if ((Object)(object)_content == (Object)null) return;
		RectTransform rt = _content;
		rt.anchoredPosition = _contentBasePos + _swipeOffset;
		float settle = _swipeActive ? Mathf.Lerp(0.985f, 1f, _swipeAlpha) : 1f;
		rt.localScale = Vector3.one * settle;

		CanvasGroup cg = ((Component)_content).GetComponent<CanvasGroup>();
		if ((Object)(object)cg == (Object)null)
		{
			cg = ((Component)_content).gameObject.GetComponent<CanvasGroup>();
			if ((Object)(object)cg == (Object)null) cg = ((Component)_content).gameObject.AddComponent<CanvasGroup>();
		}
		cg.alpha = _swipeAlpha;
		cg.blocksRaycasts = true;
		cg.interactable = true;
	}

	private static void UpdateIntroEntrance()
	{
		if (!IntroVisible || (Object)(object)_introContent == (Object)null || _introEntrance >= 1f)
		{
			return;
		}
		float dt = Time.unscaledDeltaTime;
		if (dt > 0.05f) dt = 0.05f;
		_introEntrance = Mathf.Clamp01(_introEntrance + dt / 0.42f);
		float eased = Mathf.SmoothStep(0f, 1f, _introEntrance);
		_introContent.localScale = Vector3.one * Mathf.Lerp(0.92f, 1f, eased);
		_introContent.anchoredPosition = new Vector2(0f, Mathf.Lerp(-18f, 0f, eased));
	}

	private static Vector2 _contentBasePos = Vector2.zero;

	private static void CaptureContentBase()
	{
		if ((Object)(object)_content == (Object)null) return;
		_contentBasePos = _content.anchoredPosition;
	}

	private static void NavNext()
	{
		if (ModManager.InHome)
		{
			_homePage = (_homePage + 1) % HomePageCount;
			RefreshCategory();
		}
		else
		{
			ModManager.SetModPage(ModManager.ModPage + 1);
			RefreshCategory();
		}
		BeginSwipe(new Vector2(420f, 0f));
	}

	private static void NavPrevious()
	{
		if (ModManager.InHome)
		{
			_homePage = (_homePage + HomePageCount - 1) % HomePageCount;
			RefreshCategory();
		}
		else
		{
			ModManager.SetModPage(ModManager.ModPage - 1);
			RefreshCategory();
		}
		BeginSwipe(new Vector2(-420f, 0f));
	}

	public static void RefreshCategory()
	{
		if ((Object)(object)CategoryText != (Object)null)
		{
			if (SearchActive)
			{
				((TMP_Text)CategoryText).text = (string.IsNullOrEmpty(_searchQuery) ? "SEARCH / TYPE..." : ("SEARCH / " + _searchQuery));
			}
			else if (ModManager.InHome)
			{
				((TMP_Text)CategoryText).text = (HomePageCount > 1) ? ("CATEGORIES  /  " + (_homePage + 1) + " OF " + HomePageCount) : "CATEGORIES";
			}
			else if (ModManager.InSettingsHome && ModManager.SettingsSubIndex < 0)
			{
				((TMP_Text)CategoryText).text = "SETTINGS";
			}
			else
			{
				ModCategory currentDisplayCategory = ModManager.CurrentDisplayCategory;
				string text = ((currentDisplayCategory != null) ? currentDisplayCategory.Name : "SETTINGS").ToUpperInvariant();
				text = text + "  /  " + (ModManager.ModPage + 1) + " OF " + ModManager.ModPageCount;
				((TMP_Text)CategoryText).text = text;
			}
		}
		bool flag = (ModManager.InHome && HomePageCount < 2) || (!ModManager.InHome && ModManager.InSettingsHome && ModManager.SettingsSubIndex < 0);
		if ((Object)(object)_leftNav != (Object)null)
		{
			((Component)_leftNav).gameObject.SetActive(!flag);
		}
		if ((Object)(object)_rightNav != (Object)null)
		{
			((Component)_rightNav).gameObject.SetActive(!flag);
		}
		RebuildList();
	}

	private static void RebuildList()
	{
		if (!((Object)(object)_content == (Object)null))
		{
			for (int num = ((Transform)_content).childCount - 1; num >= 0; num--)
			{
				Transform child = ((Transform)_content).GetChild(num);
				child.SetParent((Transform)null, false);
				Object.Destroy((Object)(object)((Component)child).gameObject);
			}
			if (SearchActive && !string.IsNullOrEmpty(_searchQuery))
			{
				BuildSearchRows();
			}
			else if (ModManager.InHome)
			{
				BuildCategoryRows();
			}
			else if (ModManager.InSettingsHome && ModManager.SettingsSubIndex < 0)
			{
				BuildSettingsSubCategoryRows();
			}
			else
			{
				BuildModRows();
			}
			ConfigureContentLayout();
			LayoutRebuilder.ForceRebuildLayoutImmediate(_content);
			ScrollRect componentInParent = ((Component)_content).GetComponentInParent<ScrollRect>();
			if ((Object)(object)componentInParent != (Object)null)
			{
				componentInParent.verticalNormalizedPosition = 1f;
			}
			CaptureContentBase();
			ApplySwipe();
		}
	}

	private static void ConfigureContentLayout()
	{
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Expected O, but got Unknown
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_023b: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Expected O, but got Unknown
		bool num = ModManager.InHome || (ModManager.InSettingsHome && ModManager.SettingsSubIndex < 0);
		VerticalLayoutGroup val = ((Component)_content).GetComponent<VerticalLayoutGroup>();
		GridLayoutGroup val2 = ((Component)_content).GetComponent<GridLayoutGroup>();
		if (num)
		{
			if ((Object)(object)val != (Object)null)
			{
				((Behaviour)val).enabled = false;
				Object.DestroyImmediate((Object)(object)val);
			}
			if ((Object)(object)val2 == (Object)null)
			{
				val2 = ((Component)_content).gameObject.AddComponent<GridLayoutGroup>();
			}
			if ((Object)(object)_contentFitter != (Object)null)
			{
				((Behaviour)_contentFitter).enabled = false;
			}
			_content.anchorMin = Vector2.zero;
			_content.anchorMax = Vector2.one;
			_content.pivot = new Vector2(0.5f, 0.5f);
			_content.offsetMin = Vector2.zero;
			_content.offsetMax = Vector2.zero;
			int count = ModManager.InHome ? HomePerPage : ModManager.SettingsSubCategories.Count;
			int num2 = Mathf.Max(1, count);
			float available = ((Object)(object)_viewport != (Object)null && _viewport.rect.height > 1f)
				? _viewport.rect.height
				: (PanelHeight - 192f);
			float num3 = Mathf.Max(120f, available - 12f);
			float num4 = 412f;
			float gap = 6f;
			float pad = 4f;
			float num5 = Mathf.Max(24f, (num3 - gap * (float)(num2 - 1) - pad) / (float)num2);
			val2.constraint = (GridLayoutGroup.Constraint)1;
			val2.constraintCount = 1;
			val2.cellSize = new Vector2(num4, num5);
			val2.spacing = new Vector2(6f, gap);
			((LayoutGroup)val2).padding = new RectOffset(0, 0, 0, (int)pad);
			((LayoutGroup)val2).childAlignment = (TextAnchor)1;
		}
		else
		{
			if ((Object)(object)val2 != (Object)null)
			{
				((Behaviour)val2).enabled = false;
				Object.DestroyImmediate((Object)(object)val2);
			}
			if ((Object)(object)val == (Object)null)
			{
				val = ((Component)_content).gameObject.AddComponent<VerticalLayoutGroup>();
			}
			((HorizontalOrVerticalLayoutGroup)val).spacing = 8f;
			((LayoutGroup)val).padding = new RectOffset(0, 0, 0, 8);
			((LayoutGroup)val).childAlignment = (TextAnchor)1;
			((HorizontalOrVerticalLayoutGroup)val).childControlWidth = true;
			((HorizontalOrVerticalLayoutGroup)val).childControlHeight = true;
			((HorizontalOrVerticalLayoutGroup)val).childForceExpandWidth = true;
			((HorizontalOrVerticalLayoutGroup)val).childForceExpandHeight = false;
			if ((Object)(object)_contentFitter != (Object)null)
			{
				((Behaviour)_contentFitter).enabled = true;
			}
			_content.anchorMin = new Vector2(0f, 1f);
			_content.anchorMax = new Vector2(1f, 1f);
			_content.pivot = new Vector2(0.5f, 1f);
			_content.anchoredPosition = Vector2.zero;
			_content.sizeDelta = Vector2.zero;
		}
	}

	private static void BuildModRows()
	{
		ModCategory currentDisplayCategory = ModManager.CurrentDisplayCategory;
		if (currentDisplayCategory != null)
		{
			int num = ModManager.ModPage * 4;
			int num2 = Mathf.Min(num + 4, currentDisplayCategory.Mods.Count);
			for (int i = num; i < num2; i++)
			{
				CreateModButton((Transform)(object)_content, currentDisplayCategory.Mods[i]);
			}
		}
	}

	private static void BuildSearchRows()
	{
		string value = _searchQuery.ToLowerInvariant();
		List<ModEntry> list = new List<ModEntry>();
		List<ModCategory> list2 = new List<ModCategory>(ModManager.Categories);
		list2.AddRange(ModManager.SettingsSubCategories);
		for (int i = 0; i < list2.Count; i++)
		{
			ModCategory modCategory = list2[i];
			if (modCategory.Name == "Favorite" || modCategory.Name == "Enabled" || modCategory.Name == "Settings")
			{
				continue;
			}
			for (int j = 0; j < modCategory.Mods.Count; j++)
			{
				ModEntry modEntry = modCategory.Mods[j];
				if ((modEntry.Title.ToLowerInvariant().Contains(value) || (modEntry.Description != null && modEntry.Description.ToLowerInvariant().Contains(value))) && !list.Contains(modEntry))
				{
					list.Add(modEntry);
				}
			}
		}
		for (int k = 0; k < list.Count; k++)
		{
			CreateModButton((Transform)(object)_content, list[k]);
		}
	}

	private static void BuildCategoryRows()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		List<ModCategory> categories = ModManager.Categories;
		_homePage = Mathf.Clamp(_homePage, 0, HomePageCount - 1);
		int end = Mathf.Min(categories.Count, (_homePage + 1) * HomePerPage);
		for (int i = _homePage * HomePerPage; i < end; i++)
		{
			int captured = i;
			string label = categories[captured].Name.ToUpperInvariant();
			MenuButton _catBtn = CreateButton((Transform)(object)_content, label, string.Empty, new Vector2(412f, 40f), 15f, centered: false, delegate
			{
				ShowCategory(captured);
			});
			AddRowMeta(_catBtn, (captured + 1).ToString("00"), true);
		}
	}

	private static void BuildSettingsSubCategoryRows()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		List<ModCategory> settingsSubCategories = ModManager.SettingsSubCategories;
		for (int i = 0; i < settingsSubCategories.Count; i++)
		{
			int captured = i;
			string label = settingsSubCategories[captured].Name.ToUpperInvariant();
			MenuButton subBtn = CreateButton((Transform)(object)_content, label, string.Empty, new Vector2(412f, 40f), 15f, centered: false, delegate
			{
				ModManager.ShowSettingsSub(captured);
				RefreshCategory();
			});
			AddRowMeta(subBtn, (captured + 1).ToString("00"), true);
		}
	}

	private static bool IsOverpoweredMod(ModEntry mod)
	{
		for (int i = 0; i < ModManager.Categories.Count; i++)
		{
			if (string.Equals(ModManager.Categories[i].Name, "Overpowered", StringComparison.OrdinalIgnoreCase) && ModManager.Categories[i].Mods.Contains(mod))
			{
				return true;
			}
		}
		return false;
	}

	private static void CreateModButton(Transform parent, ModEntry mod)
	{
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		string text = (mod.Favorite ? "* " : string.Empty);
		string label = text + mod.Title;
		MenuButton _modBtn = CreateButton(parent, label, mod.Description, new Vector2(412f, 58f), 17f, centered: false, delegate
		{
			mod.Trigger();
			RebuildList();
		});
		_modBtn.BoundEntry = mod;
		AddRowMeta(_modBtn, string.IsNullOrEmpty(mod.State) ? "--" : mod.State.ToUpperInvariant(), false);
	}

	private static void AddRowMeta(MenuButton button, string meta, bool category)
	{
		TextMeshProUGUI text = CreateText(((Component)button).transform, "RowMeta", meta, 10f,
			category ? SonderTheme.Description : CategoryColor, (TextAlignmentOptions)4100, bold: true);
		RectTransform rect = ((TMP_Text)text).rectTransform;
		rect.anchorMin = new Vector2(1f, 0.5f);
		rect.anchorMax = new Vector2(1f, 0.5f);
		rect.pivot = new Vector2(1f, 0.5f);
		rect.anchoredPosition = new Vector2(-14f, 0f);
		rect.sizeDelta = new Vector2(76f, 20f);
		((TMP_Text)text).characterSpacing = 1f;
	}

	private static void BuildBackground(RectTransform root)
	{
		GameObject background = CreateUIObject("GlassPanel", root);
		Stretch(background.GetComponent<RectTransform>(), 0f, 0f, 0f, 0f);
		Image backgroundImage = background.AddComponent<Image>();
		backgroundImage.sprite = SonderSprite.Fill(20, 0.94f);
		backgroundImage.type = (Image.Type)1;
		((Graphic)backgroundImage).color = SonderTheme.Panel;
		((Graphic)backgroundImage).raycastTarget = false;
		AddRing(background.transform, SonderSprite.Border(20, 2), SonderTheme.Border);

		GameObject edge = CreateUIObject("CrystalEdge", background.transform);
		RectTransform edgeRect = edge.GetComponent<RectTransform>();
		edgeRect.anchorMin = new Vector2(0f, 1f);
		edgeRect.anchorMax = new Vector2(1f, 1f);
		edgeRect.pivot = new Vector2(0.5f, 1f);
		edgeRect.anchoredPosition = new Vector2(0f, -16f);
		edgeRect.sizeDelta = new Vector2(-48f, 2f);
		Image edgeImage = edge.AddComponent<Image>();
		edgeImage.sprite = SonderSprite.Flat(2);
		edgeImage.type = (Image.Type)0;
		Color edgeColor = Color.white;
		edgeColor.a = 0.22f;
		((Graphic)edgeImage).color = edgeColor;
		((Graphic)edgeImage).raycastTarget = false;

		GameObject glint = CreateUIObject("CrystalGlint", background.transform);
		RectTransform glintRect = glint.GetComponent<RectTransform>();
		glintRect.anchorMin = new Vector2(0f, 1f);
		glintRect.anchorMax = new Vector2(0f, 1f);
		glintRect.pivot = new Vector2(0f, 1f);
		glintRect.anchoredPosition = new Vector2(22f, -22f);
		glintRect.sizeDelta = new Vector2(2f, 72f);
		Image glintImage = glint.AddComponent<Image>();
		glintImage.sprite = SonderSprite.Flat(2);
		glintImage.type = (Image.Type)0;
		Color glintColor = Color.white;
		glintColor.a = 0.14f;
		((Graphic)glintImage).color = glintColor;
		((Graphic)glintImage).raycastTarget = false;
	}

	private static Image AddRing(Transform parent, Sprite ring, Color color)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		GameObject obj = CreateUIObject("Ring", parent);
		Stretch(obj.GetComponent<RectTransform>(), 0f, 0f, 0f, 0f);
		Image obj2 = obj.AddComponent<Image>();
		obj2.sprite = ring;
		obj2.type = (Image.Type)1;
		((Graphic)obj2).color = color;
		((Graphic)obj2).raycastTarget = false;
		return obj2;
	}

	private static void BuildHeader(RectTransform root)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_0265: Unknown result type (might be due to invalid IL or missing references)
		//IL_028f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0301: Unknown result type (might be due to invalid IL or missing references)
		//IL_032b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0340: Unknown result type (might be due to invalid IL or missing references)
		//IL_0355: Unknown result type (might be due to invalid IL or missing references)
		//IL_036a: Unknown result type (might be due to invalid IL or missing references)
		//IL_037e: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03be: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03de: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0402: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = CreateUIObject("Header", root);
		RectTransform component = val.GetComponent<RectTransform>();
		component.anchorMin = new Vector2(0f, 1f);
		component.anchorMax = new Vector2(1f, 1f);
		component.pivot = new Vector2(0.5f, 1f);
		component.anchoredPosition = Vector2.zero;
		component.sizeDelta = new Vector2(0f, 108f);

		TextMeshProUGUI brandText = CreateText(val.transform, "BrandText", "SONDER", 30f, SonderTheme.Title, (TextAlignmentOptions)514, bold: true);
		RectTransform brandRect = ((TMP_Text)brandText).rectTransform;
		brandRect.anchorMin = new Vector2(0f, 1f);
		brandRect.anchorMax = new Vector2(0f, 1f);
		brandRect.pivot = new Vector2(0f, 1f);
		brandRect.anchoredPosition = new Vector2(20f, -12f);
		brandRect.sizeDelta = new Vector2(230f, 54f);
		((TMP_Text)brandText).characterSpacing = 6f;

		CategoryText = CreateText(val.transform, "Category", string.Empty, 10f, AccentColor, (TextAlignmentOptions)4097, bold: true);
		RectTransform rectTransform2 = ((TMP_Text)CategoryText).rectTransform;
		rectTransform2.anchorMin = new Vector2(0f, 1f);
		rectTransform2.anchorMax = new Vector2(0f, 1f);
		rectTransform2.pivot = new Vector2(0f, 1f);
		rectTransform2.anchoredPosition = new Vector2(24f, -82f);
		rectTransform2.sizeDelta = new Vector2(300f, 14f);
		((TMP_Text)CategoryText).characterSpacing = 3f;

		GameObject rule = CreateUIObject("HeaderRule", val.transform);
		RectTransform ruleRect = rule.GetComponent<RectTransform>();
		ruleRect.anchorMin = new Vector2(0f, 0f);
		ruleRect.anchorMax = new Vector2(1f, 0f);
		ruleRect.pivot = new Vector2(0.5f, 0f);
		ruleRect.anchoredPosition = Vector2.zero;
		ruleRect.sizeDelta = new Vector2(0f, 1f);
		Image ruleImage = rule.AddComponent<Image>();
		ruleImage.sprite = SonderSprite.Flat(0);
		ruleImage.type = (Image.Type)0;
		Color ruleColor = AccentColor;
		ruleColor.a = 0.22f;
		((Graphic)ruleImage).color = ruleColor;
		((Graphic)ruleImage).raycastTarget = false;

		NameText = CreateText(val.transform, "PlayerName", GetPlayerName(), 10f, SonderTheme.Description, (TextAlignmentOptions)4100, bold: false);
		RectTransform rectTransform3 = ((TMP_Text)NameText).rectTransform;
		rectTransform3.anchorMin = new Vector2(1f, 0f);
		rectTransform3.anchorMax = new Vector2(1f, 0f);
		rectTransform3.pivot = new Vector2(1f, 0f);
		rectTransform3.anchoredPosition = new Vector2(-24f, 24f);
		rectTransform3.sizeDelta = new Vector2(220f, 14f);

		FpsText = CreateText(val.transform, "Fps", "-- FPS", 10f, SonderTheme.Description, (TextAlignmentOptions)4100, bold: false);
		RectTransform rectTransform4 = ((TMP_Text)FpsText).rectTransform;
		rectTransform4.anchorMin = new Vector2(1f, 0f);
		rectTransform4.anchorMax = new Vector2(1f, 0f);
		rectTransform4.pivot = new Vector2(1f, 0f);
		rectTransform4.anchoredPosition = new Vector2(-24f, 8f);
		rectTransform4.sizeDelta = new Vector2(120f, 14f);
	}

	private static void BuildList(RectTransform root)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Expected O, but got Unknown
		GameObject val = CreateUIObject("ScrollView", (Transform)(object)root);
		RectTransform component = val.GetComponent<RectTransform>();
		component.anchorMin = Vector2.zero;
		component.anchorMax = Vector2.one;
		component.offsetMin = new Vector2(24f, 76f);
		component.offsetMax = new Vector2(-24f, -116f);
		ScrollRect val2 = val.AddComponent<ScrollRect>();
		val2.horizontal = false;
		val2.vertical = true;
		val2.movementType = (ScrollRect.MovementType)2;
		val2.inertia = true;
		val2.decelerationRate = 0.135f;
		val2.scrollSensitivity = 30f;
		GameObject val3 = CreateUIObject("Viewport", val.transform);
		RectTransform component2 = val3.GetComponent<RectTransform>();
		component2.anchorMin = Vector2.zero;
		component2.anchorMax = Vector2.one;
		component2.offsetMin = Vector2.zero;
		component2.offsetMax = Vector2.zero;
		val3.AddComponent<RectMask2D>();
		GameObject obj = CreateUIObject("Content", val3.transform);
		RectTransform component3 = obj.GetComponent<RectTransform>();
		component3.anchorMin = new Vector2(0f, 1f);
		component3.anchorMax = new Vector2(1f, 1f);
		component3.pivot = new Vector2(0.5f, 1f);
		component3.anchoredPosition = Vector2.zero;
		component3.sizeDelta = Vector2.zero;
		VerticalLayoutGroup obj2 = obj.AddComponent<VerticalLayoutGroup>();
		((HorizontalOrVerticalLayoutGroup)obj2).spacing = 2f;
		((LayoutGroup)obj2).padding = new RectOffset(0, 0, 0, 4);
		((LayoutGroup)obj2).childAlignment = (TextAnchor)1;
		((HorizontalOrVerticalLayoutGroup)obj2).childControlWidth = true;
		((HorizontalOrVerticalLayoutGroup)obj2).childControlHeight = true;
		((HorizontalOrVerticalLayoutGroup)obj2).childForceExpandWidth = true;
		((HorizontalOrVerticalLayoutGroup)obj2).childForceExpandHeight = false;
		ContentSizeFitter obj3 = obj.AddComponent<ContentSizeFitter>();
		obj3.verticalFit = (ContentSizeFitter.FitMode)2;
		val2.viewport = component2;
		val2.content = component3;
		_content = component3;
		_viewport = component2;
		_contentFitter = obj3;
	}

	private static void BuildFooter(RectTransform root)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Unknown result type (might be due to invalid IL or missing references)
		//IL_0270: Unknown result type (might be due to invalid IL or missing references)
		//IL_027f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0302: Unknown result type (might be due to invalid IL or missing references)
		//IL_0316: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = CreateUIObject("Footer", (Transform)(object)root);
		RectTransform component = val.GetComponent<RectTransform>();
		component.anchorMin = new Vector2(0f, 0f);
		component.anchorMax = new Vector2(1f, 0f);
		component.pivot = new Vector2(0.5f, 0f);
		component.anchoredPosition = Vector2.zero;
		component.sizeDelta = new Vector2(0f, 64f);
		GameObject obj = CreateUIObject("Separator", val.transform);
		RectTransform component2 = obj.GetComponent<RectTransform>();
		component2.anchorMin = new Vector2(0f, 1f);
		component2.anchorMax = new Vector2(1f, 1f);
		component2.pivot = new Vector2(0.5f, 1f);
		component2.anchoredPosition = Vector2.zero;
		component2.sizeDelta = new Vector2(0f, 2f);
		Image obj2 = obj.AddComponent<Image>();
		((Graphic)obj2).color = AccentSoftColor;
		((Graphic)obj2).raycastTarget = false;
		MenuButton menuButton = CreateButton(val.transform, "<", string.Empty, new Vector2(64f, 40f), 26f, centered: true, NavPrevious);
		AnchorButton(menuButton, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(18f, 0f));
		_leftNav = menuButton;
		AnchorButton(CreateButton(val.transform, "HOME", string.Empty, new Vector2(170f, 40f), 20f, centered: true, HomeButton), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero);
		MenuButton menuButton2 = CreateButton(val.transform, ">", string.Empty, new Vector2(64f, 40f), 26f, centered: true, NavNext);
		AnchorButton(menuButton2, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-18f, 0f));
		_rightNav = menuButton2;
		RectTransform rectTransform = ((TMP_Text)CreateText(val.transform, "Attr", "absolute ollie, built by olivia", 0f, Color.clear, (TextAlignmentOptions)514, bold: true)).rectTransform;
		rectTransform.anchorMin = new Vector2(0.5f, 0f);
		rectTransform.anchorMax = new Vector2(0.5f, 0f);
		rectTransform.pivot = new Vector2(0.5f, 0f);
		rectTransform.anchoredPosition = new Vector2(0f, 2f);
		rectTransform.sizeDelta = new Vector2(400f, 10f);
	}

	private static void HomeButton()
	{
		ShowHome();
		BeginSwipe(new Vector2(0f, 360f));
	}

	private static void AnchorButton(MenuButton button, Vector2 min, Vector2 max, Vector2 pivot, Vector2 position)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		RectTransform component = ((Component)button).GetComponent<RectTransform>();
		component.anchorMin = min;
		component.anchorMax = max;
		component.pivot = pivot;
		component.anchoredPosition = position;
	}

	private static MenuButton CreateButton(Transform parent, string label, string description, Vector2 size, float titleSize, bool centered, Action onClick)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_023b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0250: Unknown result type (might be due to invalid IL or missing references)
		//IL_0265: Unknown result type (might be due to invalid IL or missing references)
		//IL_0279: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = CreateUIObject("Button_" + label, parent);
		val.GetComponent<RectTransform>().sizeDelta = size;
		Image val2 = val.AddComponent<Image>();
		val2.sprite = SonderSprite.Fill(12, 0.96f);
		val2.type = (Image.Type)1;
		((Graphic)val2).color = SonderTheme.Row;
		((Graphic)val2).raycastTarget = false;

		GameObject outline = CreateUIObject("GlassOutline", val.transform);
		Stretch(outline.GetComponent<RectTransform>(), 0f, 0f, 0f, 0f);
		Image border = outline.AddComponent<Image>();
		border.sprite = SonderSprite.Border(12, 2);
		border.type = (Image.Type)0;
		Color dimInk = SonderTheme.Border;
		dimInk.a = 0.62f;
		((Graphic)border).color = dimInk;
		((Graphic)border).raycastTarget = false;

		GameObject obj = CreateUIObject("Hitbox", val.transform);
		obj.layer = 2;
		obj.AddComponent<BoxCollider>().size = new Vector3(size.x, size.y, 6f);
		LayoutElement obj2 = val.AddComponent<LayoutElement>();
		obj2.preferredWidth = size.x;
		obj2.preferredHeight = size.y;
		MenuButton menuButton = val.AddComponent<MenuButton>();
		menuButton.background = val2;
		menuButton.border = border;
		menuButton.normalBackground = SonderTheme.Row;
		menuButton.hoverBackground = SonderTheme.RowHover;
		menuButton.normalTitle = SonderTheme.Title;
		menuButton.hoverTitle = SonderTheme.Title;
		menuButton.normalBorder = dimInk;
		menuButton.hoverBorder = SonderTheme.BorderHover;
		menuButton.UseThemeColors = false;
		if (centered)
		{
			TextMeshProUGUI val3 = CreateText(val.transform, "Label", label, titleSize, SonderTheme.Title, (TextAlignmentOptions)514, bold: true);
			Stretch(((TMP_Text)val3).rectTransform, 0f, 0f, 0f, 0f);
			menuButton.titleText = val3;
			menuButton.descText = null;
		}
		else
		{
			TextMeshProUGUI val4 = CreateText(val.transform, "Title", label, titleSize, SonderTheme.Title, (TextAlignmentOptions)257, bold: true);
			RectTransform rectTransform = ((TMP_Text)val4).rectTransform;
			rectTransform.anchorMin = new Vector2(0f, 1f);
			rectTransform.anchorMax = new Vector2(1f, 1f);
			rectTransform.pivot = new Vector2(0.5f, 1f);
			rectTransform.anchoredPosition = new Vector2(-42f, -6f);
			rectTransform.sizeDelta = new Vector2(-124f, 26f);
			TextMeshProUGUI val5 = CreateText(val.transform, "Description", description, 13f, SonderTheme.Description, (TextAlignmentOptions)257, bold: false);
			RectTransform rectTransform2 = ((TMP_Text)val5).rectTransform;
			rectTransform2.anchorMin = new Vector2(0f, 1f);
			rectTransform2.anchorMax = new Vector2(1f, 1f);
			rectTransform2.pivot = new Vector2(0.5f, 1f);
			rectTransform2.anchoredPosition = new Vector2(-42f, -31f);
			rectTransform2.sizeDelta = new Vector2(-124f, 20f);
			menuButton.titleText = val4;
			menuButton.descText = val5;
		}
		menuButton.Setup(label, description, onClick);
		return menuButton;
	}

	private static TextMeshProUGUI CreateText(Transform parent, string name, string content, float size, Color color, TextAlignmentOptions alignment, bool bold)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		TextMeshProUGUI val = CreateUIObject(name, parent).AddComponent<TextMeshProUGUI>();
		if ((Object)(object)_font != (Object)null)
		{
			((TMP_Text)val).font = _font;
		}
		((TMP_Text)val).text = content;
		((TMP_Text)val).fontSize = size;
		((Graphic)val).color = color;
		((TMP_Text)val).alignment = alignment;
		((Graphic)val).raycastTarget = false;
		((TMP_Text)val).richText = false;
		((TMP_Text)val).overflowMode = (TextOverflowModes)1;
		if (bold)
		{
			((TMP_Text)val).fontStyle = (FontStyles)1;
		}
		return val;
	}

	private static GameObject CreateUIObject(string name, Transform parent)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected O, but got Unknown
		GameObject val = new GameObject(name, new Type[1] { typeof(RectTransform) });
		val.transform.SetParent(parent, false);
		return val;
	}

	private static void Stretch(RectTransform rect, float left, float bottom, float right, float top)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		rect.anchorMin = Vector2.zero;
		rect.anchorMax = Vector2.one;
		rect.offsetMin = new Vector2(left, bottom);
		rect.offsetMax = new Vector2(right, top);
	}

	private static Sprite GetPanelSprite()
	{
		if ((Object)(object)_panelSprite == (Object)null)
		{
			_panelSprite = SonderSprite.Fill(SonderSettings.PanelRadius, 0.62f);
		}
		return _panelSprite;
	}

	private static Sprite GetButtonSprite()
	{
		if ((Object)(object)_buttonSprite == (Object)null)
		{
			_buttonSprite = SonderSprite.Fill(SonderSettings.ButtonRadius, 0.76f);
		}
		return _buttonSprite;
	}

	private static Sprite GetPanelRingSprite()
	{
		if ((Object)(object)_panelRing == (Object)null)
		{
			_panelRing = SonderSprite.Border(SonderSettings.PanelRadius, 1);
		}
		return _panelRing;
	}

	private static Sprite GetButtonRingSprite()
	{
		if ((Object)(object)_buttonRing == (Object)null)
		{
			_buttonRing = SonderSprite.Border(SonderSettings.ButtonRadius, 1);
		}
		return _buttonRing;
	}

	private static string GetPlayerName()
	{
		string text = PlayerPrefs.GetString("playerName", "GORILLA");
		if (string.IsNullOrEmpty(text))
		{
			text = "GORILLA";
		}
		return text;
	}

	public static void ToggleSearch()
	{
		SetSearchActive(!SearchActive);
	}

	public static void SetSearchActive(bool active)
	{
		SearchActive = active;
		if (!active)
		{
			_searchQuery = string.Empty;
			HideKeyboard();
		}
		else
		{
			ShowKeyboardIfVr();
		}
		RefreshCategory();
		RebuildList();
	}

	public static void AppendSearchChar(char c)
	{
		if (SearchActive)
		{
			_searchQuery += c;
			RefreshCategory();
			RebuildList();
		}
	}

	public static void BackspaceSearch()
	{
		if (SearchActive && _searchQuery.Length != 0)
		{
			_searchQuery = _searchQuery.Substring(0, _searchQuery.Length - 1);
			RefreshCategory();
			RebuildList();
		}
	}

	public static void ClearSearch()
	{
		if (SearchActive)
		{
			_searchQuery = string.Empty;
			RefreshCategory();
			RebuildList();
		}
	}

	private static void ShowKeyboardIfVr()
	{
		if (MenuManager.Docked || MenuManager.ScreenMode)
		{
			HideKeyboard();
		}
		else
		{
			ShowKeyboard();
		}
	}

	public static void ShowKeyboard()
	{
		if ((Object)(object)_keyboardRoot == (Object)null)
		{
			BuildKeyboard();
		}
		if ((Object)(object)_keyboardRoot != (Object)null)
		{
			_keyboardRoot.SetActive(true);
		}
		PositionKeyboard();
	}

	public static void HideKeyboard()
	{
		if ((Object)(object)_keyboardRoot != (Object)null)
		{
			_keyboardRoot.SetActive(false);
		}
	}

	private static void PositionKeyboard()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_keyboardRoot == (Object)null)
		{
			return;
		}
		// Line up under the menu (which is frozen while searching), with the same facing so it reads the right way up.
		Transform menuCanvas = MenuManager.MenuCanvas;
		if ((Object)(object)menuCanvas != (Object)null && menuCanvas.lossyScale.x > 0f)
		{
			float scale = menuCanvas.lossyScale.x * 1.4f;
			float menuHalfHeight = ((Component)menuCanvas).GetComponent<RectTransform>().rect.height * 0.5f * menuCanvas.lossyScale.y;
			_keyboardRoot.transform.localScale = Vector3.one * scale;
			_keyboardRoot.transform.rotation = menuCanvas.rotation;
			_keyboardRoot.transform.position = menuCanvas.position - menuCanvas.up * (menuHalfHeight + 150f * scale);
			return;
		}
		Camera main = Camera.main;
		if ((Object)(object)main == (Object)null)
		{
			return;
		}
		Transform view = ((Component)main).transform;
		Vector3 position = view.position + view.forward * 0.45f - Vector3.up * 0.3f;
		_keyboardRoot.transform.localScale = Vector3.one * 0.0005f;
		_keyboardRoot.transform.position = position;
		_keyboardRoot.transform.rotation = Quaternion.LookRotation(position - view.position, view.up);
	}

	private static MenuButton _keyHovered;

	private static bool _keyTriggerHeld;

	// The menu pointer stops at the first thing on its layer, and game trigger volumes share that layer,
	// so the keyboard does its own ray that only looks at keys.
	private static void PointAtKeyboard()
	{
		Transform hand = MenuManager.RightHandPointer;
		if ((Object)(object)hand == (Object)null || (Object)(object)_keyboardRoot == (Object)null)
		{
			return;
		}
		MenuButton hit = null;
		float best = float.MaxValue;
		foreach (RaycastHit item in Physics.SphereCastAll(hand.position, 0.01f, hand.forward, 5f, 4, QueryTriggerInteraction.Collide))
		{
			if (item.distance < best && item.collider.transform.IsChildOf(_keyboardRoot.transform))
			{
				MenuButton button = item.collider.GetComponentInParent<MenuButton>();
				if ((Object)(object)button != (Object)null)
				{
					best = item.distance;
					hit = button;
				}
			}
		}
		UnityEngine.EventSystems.PointerEventData data = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current);
		if ((Object)(object)hit != (Object)(object)_keyHovered)
		{
			if ((Object)(object)_keyHovered != (Object)null)
			{
				_keyHovered.OnPointerExit(data);
			}
			_keyHovered = hit;
			if ((Object)(object)_keyHovered != (Object)null)
			{
				_keyHovered.OnPointerEnter(data);
			}
		}
		if ((Object)(object)hit != (Object)null && MenuManager.PointerLine != null)
		{
			MenuManager.PointerLine.SetPosition(1, hand.position + hand.forward * best);
		}
		ControllerInputPoller input = ControllerInputPoller.instance;
		bool trigger = (Object)(object)input != (Object)null && input.rightControllerTriggerButton;
		if (trigger && !_keyTriggerHeld && (Object)(object)hit != (Object)null)
		{
			hit.OnPointerClick(data);
		}
		_keyTriggerHeld = trigger;
	}

	private static void BuildKeyboard()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected O, but got Unknown
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_026b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02de: Unknown result type (might be due to invalid IL or missing references)
		//IL_0310: Unknown result type (might be due to invalid IL or missing references)
		//IL_034b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0360: Unknown result type (might be due to invalid IL or missing references)
		//IL_0375: Unknown result type (might be due to invalid IL or missing references)
		//IL_0383: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0409: Unknown result type (might be due to invalid IL or missing references)
		//IL_041e: Unknown result type (might be due to invalid IL or missing references)
		//IL_042c: Unknown result type (might be due to invalid IL or missing references)
		if (_keyboardBuilt)
		{
			return;
		}
		_keyboardBuilt = true;
		GameObject val = new GameObject("Sonder_Keyboard");
		Object.DontDestroyOnLoad((Object)(object)val);
		Canvas obj = val.AddComponent<Canvas>();
		obj.renderMode = (RenderMode)2;
		obj.sortingOrder = 101;
		val.GetComponent<RectTransform>().sizeDelta = new Vector2(720f, 260f);
		// World-space canvas: without this it is 720 meters wide.
		val.transform.localScale = Vector3.one * 0.0006f;
		_keyboardRoot = val;
		val.SetActive(false);
		string[] array = new string[3] { "QWERTYUIOP", "ASDFGHJKL", "ZXCVBNM" };
		float num = 58f;
		float num2 = 6f;
		float num3 = 60f;
		for (int i = 0; i < array.Length; i++)
		{
			string text = array[i];
			float num4 = (0f - ((float)text.Length * (num + num2) - num2)) * 0.5f + num * 0.5f;
			float num5 = num3 - (float)i * (num + num2);
			for (int j = 0; j < text.Length; j++)
			{
				char captured;
				char c = (captured = text[j]);
				RectTransform component = ((Component)CreateButton(val.transform, c.ToString(), string.Empty, new Vector2(num, num), 22f, centered: true, delegate
				{
					AppendSearchChar(captured);
				})).GetComponent<RectTransform>();
				component.anchorMin = new Vector2(0.5f, 0.5f);
				component.anchorMax = new Vector2(0.5f, 0.5f);
				component.pivot = new Vector2(0.5f, 0.5f);
				component.anchoredPosition = new Vector2(num4 + (float)j * (num + num2), num5);
			}
		}
		float num6 = num3 - 3f * (num + num2);
		float num7 = -260f;
		RectTransform component2 = ((Component)CreateButton(val.transform, "SPACE", string.Empty, new Vector2(200f, num), 16f, centered: true, delegate
		{
			AppendSearchChar(' ');
		})).GetComponent<RectTransform>();
		component2.anchorMin = new Vector2(0.5f, 0.5f);
		component2.anchorMax = new Vector2(0.5f, 0.5f);
		component2.pivot = new Vector2(0.5f, 0.5f);
		component2.anchoredPosition = new Vector2(num7, num6);
		num7 += 200f + num2 + 60f;
		RectTransform component3 = ((Component)CreateButton(val.transform, "<-", string.Empty, new Vector2(90f, num), 18f, centered: true, BackspaceSearch)).GetComponent<RectTransform>();
		component3.anchorMin = new Vector2(0.5f, 0.5f);
		component3.anchorMax = new Vector2(0.5f, 0.5f);
		component3.pivot = new Vector2(0.5f, 0.5f);
		component3.anchoredPosition = new Vector2(num7, num6);
		num7 += 90f + num2 + 60f;
		RectTransform component4 = ((Component)CreateButton(val.transform, "CLEAR", string.Empty, new Vector2(90f, num), 14f, centered: true, ClearSearch)).GetComponent<RectTransform>();
		component4.anchorMin = new Vector2(0.5f, 0.5f);
		component4.anchorMax = new Vector2(0.5f, 0.5f);
		component4.pivot = new Vector2(0.5f, 0.5f);
		component4.anchoredPosition = new Vector2(num7, num6);
		num7 += 90f + num2 + 60f;
		RectTransform component5 = ((Component)CreateButton(val.transform, "DONE", string.Empty, new Vector2(90f, num), 14f, centered: true, delegate
		{
			SetSearchActive(active: false);
		})).GetComponent<RectTransform>();
		component5.anchorMin = new Vector2(0.5f, 0.5f);
		component5.anchorMax = new Vector2(0.5f, 0.5f);
		component5.pivot = new Vector2(0.5f, 0.5f);
		component5.anchoredPosition = new Vector2(num7, num6);
	}

	public static void UpdateSearch()
	{
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		if (!SearchActive)
		{
			if ((Object)(object)_keyboardRoot != (Object)null && _keyboardRoot.activeSelf)
			{
				HideKeyboard();
			}
			return;
		}
		if (MenuManager.Docked || MenuManager.ScreenMode)
		{
			if ((Object)(object)_keyboardRoot != (Object)null && _keyboardRoot.activeSelf)
			{
				HideKeyboard();
			}
		}
		else if ((Object)(object)_keyboardRoot == (Object)null || !_keyboardRoot.activeSelf)
		{
			ShowKeyboard();
		}
		else
		{
			PointAtKeyboard();
		}
		Keyboard current = Keyboard.current;
		if (current == null)
		{
			return;
		}
		if (((ButtonControl)current.backspaceKey).wasPressedThisFrame)
		{
			BackspaceSearch();
		}
		if (((ButtonControl)current.enterKey).wasPressedThisFrame || ((ButtonControl)current.escapeKey).wasPressedThisFrame)
		{
			SetSearchActive(active: false);
		}
		var enumerator = current.allKeys.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				KeyControl current2 = enumerator.Current;
				if (!((ButtonControl)current2).wasPressedThisFrame)
				{
					continue;
				}
				string name = ((InputControl)current2).name;
				if (string.IsNullOrEmpty(name))
				{
					continue;
				}
				if (name == "space")
				{
					AppendSearchChar(' ');
				}
				else if (name.Length == 1)
				{
					char c = name[0];
					if (char.IsLetterOrDigit(c))
					{
						AppendSearchChar(char.ToLowerInvariant(c));
					}
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
	}

	private static MenuButton CreateIconButton(Transform parent, Sprite icon, Vector2 size, Action onClick)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		MenuButton menuButton = CreateButton(parent, "", string.Empty, size, 12f, centered: true, onClick);
		if ((Object)(object)menuButton.background != (Object)null)
		{
			((Graphic)menuButton.background).color = Color.clear;
		}
		if ((Object)(object)menuButton.border != (Object)null)
		{
			((Graphic)menuButton.border).color = Color.clear;
		}
		menuButton.normalBackground = Color.clear;
		menuButton.hoverBackground = Color.clear;
		menuButton.normalBorder = Color.clear;
		menuButton.hoverBorder = Color.clear;
		if ((Object)(object)icon != (Object)null)
		{
			GameObject obj = CreateUIObject("Icon", ((Component)menuButton).transform);
			Stretch(obj.GetComponent<RectTransform>(), 6f, 6f, -6f, -6f);
			Image obj2 = obj.AddComponent<Image>();
			obj2.sprite = icon;
			obj2.preserveAspect = true;
			((Graphic)obj2).raycastTarget = false;
		}
		return menuButton;
	}

	private static Sprite GetSearchSprite()
	{
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Expected O, but got Unknown
		if ((Object)(object)_searchSprite != (Object)null || _searchSpriteFailed)
		{
			return _searchSprite;
		}
		try
		{
			using Stream stream = typeof(MenuBuilder).Assembly.GetManifestResourceStream("Sonder.SearchIcon.rgba");
			if (stream == null)
			{
				_searchSpriteFailed = true;
				RuntimeLogSafe("search icon: embedded resource missing");
				return null;
			}
			int num = 894912;
			byte[] array = new byte[num];
			int i;
			int num2;
			for (i = 0; i < num; i += num2)
			{
				num2 = stream.Read(array, i, num - i);
				if (num2 <= 0)
				{
					break;
				}
			}
			if (i != num)
			{
				_searchSpriteFailed = true;
				return null;
			}
			Texture2D val = new Texture2D(474, 472, (TextureFormat)4, false)
			{
				name = "SonderSearchIcon",
				filterMode = (FilterMode)1,
				wrapMode = (TextureWrapMode)1
			};
			// The raw image is stored top row first, Unity expects bottom row first, so flip it.
			int rowBytes = 474 * 4;
			byte[] row = new byte[rowBytes];
			for (int top = 0, bottom = 471; top < bottom; top++, bottom--)
			{
				Buffer.BlockCopy(array, top * rowBytes, row, 0, rowBytes);
				Buffer.BlockCopy(array, bottom * rowBytes, array, top * rowBytes, rowBytes);
				Buffer.BlockCopy(row, 0, array, bottom * rowBytes, rowBytes);
			}
			val.LoadRawTextureData(array);
			val.Apply(false, true);
			_searchSprite = Sprite.Create(val, new Rect(0f, 0f, 474f, 472f), new Vector2(0.5f, 0.5f), 100f, 0u, (SpriteMeshType)0);
			((Object)_searchSprite).name = "SonderSearchIcon";
		}
		catch (Exception ex)
		{
			_searchSpriteFailed = true;
			Debug.LogWarning((object)("[Sonder] search icon load failed: " + ex.Message));
		}
		return _searchSprite;
	}

	private static void RuntimeLogSafe(string message)
	{
		try
		{
			RuntimeLog.Write(message);
		}
		catch
		{
		}
	}
}






