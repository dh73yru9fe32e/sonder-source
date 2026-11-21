using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;
using UnityEngine.UI;

namespace Sonder.Core.UI.Framework;

public static class MenuBuilder
{
	public sealed class Result
	{
		public GameObject Root;

		public RectTransform Panel;

		public RoundedImage Glow;

		public RoundedImage Background;

		public RoundedImage Sidebar;

		public RoundedImage GlassSheen;

		public RoundedImage NavIndicator;

		public RoundedImage ScrollTrack;

		public RoundedImage ScrollThumb;

		public CursorGraphic CursorOutline;

		public CursorGraphic CursorFill;

		public RoundedImage ThemeFlash;

		public TMP_Text Logo;

		public TMP_Text Version;

		public TMP_Text Breadcrumb;

		public TMP_Text WarningText;

		public MenuButton Search;

		public MenuButton ThemesButton;

		public MenuButton UpdatesButton;

		public RoundedImage CardLeft;

		public RoundedImage CardRight;

		public TMP_Text CardLeftHeader;

		public TMP_Text CardRightHeader;

		public RoundedImage UpdateCard;

		public TMP_Text UpdateBody;

		public RoundedImage CursorGlow;

		public RoundedImage CursorDot;

		public readonly List<MenuButton> Swatches = new List<MenuButton>();

		public readonly List<MenuButton> Nav = new List<MenuButton>();

		public readonly List<MenuButton> Rows = new List<MenuButton>();

		public readonly List<MenuButton> Keys = new List<MenuButton>();
	}

	public const float PanelWidth = 760f;

	public const float PanelHeight = 480f;

	public const float PanelWidthMeters = 0.66f;

	public const int MaxNav = 8;

	public const int MaxRows = 8;

	public const float SidebarWidth = 200f;

	public const float SidebarX = -280f;

	public const float NavWidth = 176f;

	public const float NavHeight = 40f;

	public const float NavTop = 150f;

	public const float NavPitch = 46f;

	public const float ContentCenterX = 100f;

	public const float RowWidth = 520f;

	public const float RowHeight = 40f;

	public const float RowPitch = 46f;

	public const float RowsTop = 118f;

	public const float SliderTrackWidth = 170f;

	public const int SwatchCount = 13;

	public const float ScrollTrackX = 366f;

	public const float ScrollTrackHeight = 208f;

	public const float ScrollTop = 74f;

	public const float CardWidth = 248f;

	public const float CardHeight = 260f;

	public const float CardTop = 128f;

	public const float CardCenterY = -2f;

	public const float CardLeftX = -36f;

	public const float CardRightX = 236f;

	public const float CardRowPitch = 52f;

	public const float CardRowTop = 70f;

	public const float CardRowWidth = 224f;

	public const float CardRowHeight = 44f;

	public const float KeyUnit = 52f;

	public const float KeyHeight = 40f;

	public static readonly string[][] KeyRows = new string[5][]
	{
		new string[10] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" },
		new string[10] { "q", "w", "e", "r", "t", "y", "u", "i", "o", "p" },
		new string[9] { "a", "s", "d", "f", "g", "h", "j", "k", "l" },
		new string[8] { "z", "x", "c", "v", "b", "n", "m", "Del" },
		new string[3] { "Space", "Clear", "Done" }
	};

	public static float MetersPerUnit => 0.0008684211f;

	public static TMP_FontAsset Font { get; private set; }

	public static float RowCenterY(int index)
	{
		return 118f - (float)index * 46f;
	}

	public static float NavCenterY(int index)
	{
		return 150f - (float)index * 46f;
	}

	public static Result Build()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected Obj, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Unknown result type (might be due to invalid IL or missing references)
		//IL_0281: Unknown result type (might be due to invalid IL or missing references)
		//IL_0290: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0321: Unknown result type (might be due to invalid IL or missing references)
		//IL_0331: Unknown result type (might be due to invalid IL or missing references)
		//IL_0385: Unknown result type (might be due to invalid IL or missing references)
		//IL_0396: Unknown result type (might be due to invalid IL or missing references)
		//IL_03de: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0412: Unknown result type (might be due to invalid IL or missing references)
		//IL_0421: Unknown result type (might be due to invalid IL or missing references)
		//IL_0446: Unknown result type (might be due to invalid IL or missing references)
		//IL_0455: Unknown result type (might be due to invalid IL or missing references)
		//IL_047a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0489: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_052b: Unknown result type (might be due to invalid IL or missing references)
		//IL_053a: Unknown result type (might be due to invalid IL or missing references)
		//IL_056a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0579: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0614: Unknown result type (might be due to invalid IL or missing references)
		//IL_0619: Unknown result type (might be due to invalid IL or missing references)
		//IL_0651: Unknown result type (might be due to invalid IL or missing references)
		//IL_0656: Unknown result type (might be due to invalid IL or missing references)
		//IL_0680: Unknown result type (might be due to invalid IL or missing references)
		//IL_0685: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_06c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_06f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_06f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_079d: Unknown result type (might be due to invalid IL or missing references)
		//IL_07ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_07d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_07e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0833: Unknown result type (might be due to invalid IL or missing references)
		//IL_0838: Unknown result type (might be due to invalid IL or missing references)
		//IL_0873: Unknown result type (might be due to invalid IL or missing references)
		//IL_08a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_08b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_08fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_090d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0944: Unknown result type (might be due to invalid IL or missing references)
		//IL_0953: Unknown result type (might be due to invalid IL or missing references)
		Font = ResolveFont();
		Result result = new Result();
		result.Root = new GameObject("MatchaMenu");
		result.Root.layer = 5;
		RectTransform val = result.Root.AddComponent<RectTransform>();
		val.sizeDelta = new Vector2(760f, 480f);
		Canvas val2 = result.Root.AddComponent<Canvas>();
		val2.renderMode = (RenderMode)2;
		val2.additionalShaderChannels = (AdditionalCanvasShaderChannels)((int)val2.additionalShaderChannels | 1);
		val2.sortingOrder = 300;
		result.Panel = NewRect("Panel", (Transform)(object)val, new Vector2(760f, 480f), Vector2.zero);
		result.Glow = AddRounded("Glow", (Transform)(object)result.Panel, new Vector2(770f, 490f), Vector2.zero, 26f);
		result.Background = AddRounded("Background", (Transform)(object)result.Panel, new Vector2(760f, 480f), Vector2.zero, 24f);
		result.GlassSheen = AddRounded("Glass Sheen", (Transform)(object)result.Panel, new Vector2(758f, 478f), Vector2.zero, 23f);
		((Graphic)result.GlassSheen).raycastTarget = false;
		((Component)result.GlassSheen).gameObject.SetActive(false);
		result.GlassSheen.UseGradient = true;
		result.GlassSheen.TopColor = new Color(1f, 1f, 1f, 0.16f);
		result.GlassSheen.BottomColor = new Color(0.45f, 0.72f, 1f, 0.015f);
		((Graphic)result.GlassSheen).color = Color.white;
		result.Sidebar = AddRounded("Sidebar", (Transform)(object)result.Panel, new Vector2(200f, 480f), new Vector2(-280f, 0f), 16f);
		result.Sidebar.TopRight = 0f;
		result.Sidebar.BottomRight = 0f;
		RoundedImage roundedImage = AddRounded("Sidebar Separator", (Transform)(object)result.Panel, new Vector2(1f, 418f), new Vector2(-180f, 0f), 0.5f);
		((Graphic)roundedImage).color = new Color(1f, 1f, 1f, 0.07f);
		((Graphic)roundedImage).raycastTarget = false;
		result.Logo = AddText("Logo", (Transform)(object)result.Panel, new Vector2(160f, 34f), new Vector2(-280f, 200f), 26f, (FontStyles)1, (TextAlignmentOptions)514);
		result.Logo.text = "SONDER";
		result.Version = AddText("Version", (Transform)(object)result.Panel, new Vector2(160f, 20f), new Vector2(-280f, -206f), 14f, (FontStyles)0, (TextAlignmentOptions)514);
		result.Version.text = "v" + "1.0.0";
		result.NavIndicator = AddRounded("Category Indicator", (Transform)(object)result.Panel, new Vector2(4f, 24f), new Vector2(-361f, NavCenterY(0)), 2f);
		((Component)result.NavIndicator).gameObject.SetActive(false);
		for (int i = 0; i < 8; i++)
		{
			result.Nav.Add(CreateButton($"Nav ({i + 1})", result.Panel, new Vector2(176f, 40f), new Vector2(-278f, NavCenterY(i))));
		}
		((Component)result.NavIndicator).transform.SetAsLastSibling();
		result.Search = CreateButton("Search", result.Panel, new Vector2(280f, 40f), new Vector2(-20f, 200f));
		result.ThemesButton = CreateButton("Themes", result.Panel, new Vector2(84f, 40f), new Vector2(178f, 200f));
		result.UpdatesButton = CreateButton("Updates", result.Panel, new Vector2(84f, 40f), new Vector2(282f, 200f));
		result.CardLeft = AddRounded("Card Left", (Transform)(object)result.Panel, new Vector2(248f, 260f), new Vector2(-36f, -2f), 12f);
		result.CardRight = AddRounded("Card Right", (Transform)(object)result.Panel, new Vector2(248f, 260f), new Vector2(236f, -2f), 12f);
		result.CardLeftHeader = AddText("Header Left", (Transform)(object)result.Panel, new Vector2(200f, 22f), new Vector2(-46f, 110f), 15f, (FontStyles)1, (TextAlignmentOptions)4097);
		result.CardRightHeader = AddText("Header Right", (Transform)(object)result.Panel, new Vector2(200f, 22f), new Vector2(226f, 110f), 15f, (FontStyles)1, (TextAlignmentOptions)4097);
		result.UpdateCard = AddRounded("Update Card", (Transform)(object)result.Panel, new Vector2(520f, 150f), new Vector2(100f, -122f), 12f);
		result.UpdateBody = AddText("Update Body", (Transform)(object)result.Panel, new Vector2(486f, 130f), new Vector2(100f, -122f), 16f, (FontStyles)0, (TextAlignmentOptions)257);
		result.UpdateBody.enableWordWrapping = true;
		result.UpdateBody.overflowMode = (TextOverflowModes)0;
		for (int j = 0; j < 13; j++)
		{
			result.Swatches.Add(CreateButton($"Swatch ({j + 1})", result.Panel, new Vector2(116f, 66f), Vector2.zero));
		}
		result.CursorGlow = AddRounded("Cursor Glow", (Transform)(object)result.Panel, new Vector2(26f, 26f), Vector2.zero, 13f);
		result.CursorOutline = ((Component)NewRect("Cursor Outline", (Transform)(object)result.Panel, new Vector2(23f, 29f), Vector2.zero)).gameObject.AddComponent<CursorGraphic>();
		result.CursorFill = ((Component)NewRect("Cursor Fill", (Transform)(object)result.Panel, new Vector2(18f, 24f), new Vector2(0.3f, -0.2f))).gameObject.AddComponent<CursorGraphic>();
		result.CursorDot = AddRounded("Cursor Dot", (Transform)(object)result.Panel, new Vector2(8f, 8f), Vector2.zero, 4f);
		((Graphic)result.CursorGlow).raycastTarget = false;
		((Graphic)result.CursorDot).raycastTarget = false;
		((Graphic)result.CursorOutline).raycastTarget = false;
		((Graphic)result.CursorFill).raycastTarget = false;
		((Component)result.CursorGlow).gameObject.SetActive(false);
		((Component)result.CursorDot).gameObject.SetActive(false);
		((Component)result.CursorOutline).gameObject.SetActive(false);
		((Component)result.CursorFill).gameObject.SetActive(false);
		result.ScrollTrack = AddRounded("Scroll Track", (Transform)(object)result.Panel, new Vector2(12f, 208f), new Vector2(366f, -6f), 6f);
		result.ScrollThumb = AddRounded("Scroll Thumb", (Transform)(object)result.Panel, new Vector2(9f, 48f), new Vector2(366f, 74f), 4.5f);
		((Component)result.ScrollTrack).gameObject.SetActive(false);
		((Component)result.ScrollThumb).gameObject.SetActive(false);
		result.ThemeFlash = AddRounded("Theme Flash", (Transform)(object)result.Panel, new Vector2(758f, 478f), Vector2.zero, 23f);
		((Graphic)result.ThemeFlash).raycastTarget = false;
		((Graphic)result.ThemeFlash).color = new Color(1f, 1f, 1f, 0f);
		((Component)result.ThemeFlash).transform.SetAsLastSibling();
		result.Breadcrumb = AddText("Breadcrumb", (Transform)(object)result.Panel, new Vector2(400f, 22f), new Vector2(40f, 160f), 16f, (FontStyles)0, (TextAlignmentOptions)4097);
		for (int k = 0; k < 8; k++)
		{
			result.Rows.Add(CreateButton($"Row ({k + 1})", result.Panel, new Vector2(520f, 40f), new Vector2(100f, RowCenterY(k))));
		}
		result.WarningText = AddText("Warning", (Transform)(object)result.Panel, new Vector2(480f, 180f), new Vector2(100f, -60f), 20f, (FontStyles)0, (TextAlignmentOptions)258);
		result.WarningText.enableWordWrapping = true;
		result.WarningText.overflowMode = (TextOverflowModes)0;
		result.WarningText.text = "Are you sure?\nThese mods could get you banned and we are not responsible for your actions.";
		((Component)result.WarningText).gameObject.SetActive(false);
		BuildKeyboard(result);
		return result;
	}

	public static int KeyUnitsFor(string label)
	{
		return label switch
		{
			"Del" => 2, 
			"Space" => 6, 
			"Clear" => 2, 
			"Done" => 2, 
			_ => 1, 
		};
	}

	private static void BuildKeyboard(Result r)
	{
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < KeyRows.Length; i++)
		{
			string[] array = KeyRows[i];
			float num = 0f;
			string[] array2 = array;
			foreach (string label in array2)
			{
				num += (float)KeyUnitsFor(label);
			}
			float num2 = 100f - (num * 52f - 12f) * 0.5f;
			float num3 = 0f;
			string[] array3 = array;
			foreach (string text in array3)
			{
				float num4 = KeyUnitsFor(text);
				float num5 = num4 * 52f - 12f;
				float num6 = num2 + num3 + num5 * 0.5f;
				num3 += num4 * 52f;
				r.Keys.Add(CreateButton("Key (" + text + ")", r.Panel, new Vector2(num5, 40f), new Vector2(num6, RowCenterY(i))));
			}
		}
	}

	private static MenuButton CreateButton(string name, RectTransform parent, Vector2 size, Vector2 pos)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		RoundedImage roundedImage = AddRounded(name, (Transform)(object)parent, size, pos, 8f);
		TMP_Text label = AddText("Label", (Transform)(object)((Graphic)roundedImage).rectTransform, size, Vector2.zero, 18f, (FontStyles)0, (TextAlignmentOptions)4097);
		TMP_Text value = AddText("Value", (Transform)(object)((Graphic)roundedImage).rectTransform, new Vector2(80f, size.y), Vector2.zero, 16f, (FontStyles)0, (TextAlignmentOptions)4100);
		RoundedImage switchTrackImg = AddRounded("Switch", (Transform)(object)((Graphic)roundedImage).rectTransform, new Vector2(44f, 22f), Vector2.zero, 11f);
		RoundedImage switchKnobImg = AddRounded("Knob", (Transform)(object)((Graphic)roundedImage).rectTransform, new Vector2(16f, 16f), Vector2.zero, 8f);
		RoundedImage roundedImage2 = AddRounded("Track", (Transform)(object)((Graphic)roundedImage).rectTransform, new Vector2(170f, 6f), Vector2.zero, 3f);
		RoundedImage roundedImage3 = AddRounded("Fill", (Transform)(object)((Graphic)roundedImage2).rectTransform, new Vector2(0f, 6f), Vector2.zero, 3f);
		AnchorLeft(((Graphic)roundedImage3).rectTransform);
		RoundedImage roundedImage4 = AddRounded("Thumb", (Transform)(object)((Graphic)roundedImage2).rectTransform, new Vector2(14f, 14f), Vector2.zero, 7f);
		AnchorLeft(((Graphic)roundedImage4).rectTransform);
		RoundedImage roundedImage5 = AddRounded("Color Chip", (Transform)(object)((Graphic)roundedImage).rectTransform, new Vector2(15f, 15f), Vector2.zero, 7.5f);
		((Component)roundedImage5).gameObject.SetActive(false);
		MenuButton menuButton = ((Component)roundedImage).gameObject.AddComponent<MenuButton>();
		menuButton.Wire(roundedImage, label, value, switchTrackImg, switchKnobImg, roundedImage2, roundedImage3, roundedImage4, roundedImage5);
		menuButton.Hide();
		return menuButton;
	}

	private static void AnchorLeft(RectTransform rt)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		rt.anchorMin = new Vector2(0f, 0.5f);
		rt.anchorMax = new Vector2(0f, 0.5f);
		rt.pivot = new Vector2(0f, 0.5f);
		rt.anchoredPosition = Vector2.zero;
	}

	private static RectTransform NewRect(string name, Transform parent, Vector2 size, Vector2 pos)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected Obj, but got Unknown
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected Obj, but got Unknown
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = new GameObject(name, new Type[1] { typeof(RectTransform) });
		val.layer = 5;
		RectTransform val2 = (RectTransform)val.transform;
		((Transform)val2).SetParent(parent, false);
		val2.anchorMin = new Vector2(0.5f, 0.5f);
		val2.anchorMax = new Vector2(0.5f, 0.5f);
		val2.pivot = new Vector2(0.5f, 0.5f);
		val2.sizeDelta = size;
		val2.anchoredPosition = pos;
		return val2;
	}

	private static RoundedImage AddRounded(string name, Transform parent, Vector2 size, Vector2 pos, float radius)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		RectTransform val = NewRect(name, parent, size, pos);
		RoundedImage roundedImage = ((Component)val).gameObject.AddComponent<RoundedImage>();
		roundedImage.TopLeft = radius;
		roundedImage.TopRight = radius;
		roundedImage.BottomLeft = radius;
		roundedImage.BottomRight = radius;
		roundedImage.Segments = 16;
		((Graphic)roundedImage).color = Color.white;
		return roundedImage;
	}

	private static TMP_Text AddText(string name, Transform parent, Vector2 size, Vector2 pos, float fontSize, FontStyles style, TextAlignmentOptions alignment)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		RectTransform val = NewRect(name, parent, size, pos);
		TextMeshProUGUI val2 = ((Component)val).gameObject.AddComponent<TextMeshProUGUI>();
		if ((Object)(object)Font != (Object)null)
		{
			((TMP_Text)val2).font = Font;
		}
		((TMP_Text)val2).text = string.Empty;
		((TMP_Text)val2).fontSize = fontSize;
		((TMP_Text)val2).fontStyle = style;
		((TMP_Text)val2).alignment = alignment;
		((Graphic)val2).color = Color.white;
		((Graphic)val2).raycastTarget = false;
		((TMP_Text)val2).enableWordWrapping = false;
		((TMP_Text)val2).overflowMode = (TextOverflowModes)1;
		return (TMP_Text)(object)val2;
	}

	private static TMP_FontAsset ResolveFont()
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Invalid comparison between Unknown and I4
		TMP_FontAsset val = null;
		try
		{
			val = TMP_Settings.defaultFontAsset;
		}
		catch
		{
		}
		if ((Object)(object)val == (Object)null)
		{
			TMP_FontAsset[] array = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
			TMP_FontAsset[] array2 = array;
			foreach (TMP_FontAsset val2 in array2)
			{
				if (!((Object)(object)val2 == (Object)null) && (int)val2.atlasPopulationMode <= 0 && !((Object)(object)((TMP_Asset)val2).material == (Object)null) && val2.atlasTextures != null && val2.atlasTextures.Length != 0 && !((Object)(object)((TMP_Asset)val2).material.shader == (Object)null) && ((Object)((TMP_Asset)val2).material.shader).name.Length != 0)
				{
					val = val2;
					break;
				}
			}
		}
		return val;
	}
}





