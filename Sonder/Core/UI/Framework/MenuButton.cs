using System;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;
using UnityEngine.UI;

namespace Sonder.Core.UI.Framework;

public class MenuButton : MonoBehaviour
{
	private const float PressScale = 0.965f;

	private const float RevealOffset = 36f;

	private const float Pad = 14f;

	private static readonly Color Invalid = new Color(-1f, -1f, -1f, -1f);

	public bool Active;

	private RectTransform rect;

	private RoundedImage switchTrack;

	private RoundedImage switchKnob;

	private RoundedImage sliderTrack;

	private RoundedImage sliderFill;

	private RoundedImage sliderThumb;

	private RoundedImage colorChip;

	private float knobOffX;

	private float knobOnX;

	private bool hoverTarget;

	private bool pressTarget;

	private float knobTarget;

	private float sliderTarget;

	private float colorChipHue;

	private float hoverT;

	private float pressT;

	private float knobT;

	private float sliderT;

	private Color lastColor = Invalid;

	private Color lastSwitchColor = Invalid;

	private float lastScale = -1f;

	private float lastKnobX = -1000f;

	private float lastSliderX = -1000f;

	public RoundedImage Background { get; private set; }

	public TMP_Text Label { get; private set; }

	public TMP_Text Value { get; private set; }

	public CanvasGroup Group { get; private set; }

	public MenuButtonKind Kind { get; private set; }

	public ButtonData Data { get; private set; }

	public Vector2 HomePosition
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return field;
		}
		[CompilerGenerated]
		private set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			field = value;
		}
	}

	public void Wire(RoundedImage bg, TMP_Text label, TMP_Text value, RoundedImage switchTrackImg, RoundedImage switchKnobImg, RoundedImage track, RoundedImage fill, RoundedImage thumb, RoundedImage colorChipImg)
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		Background = bg;
		Label = label;
		Value = value;
		switchTrack = switchTrackImg;
		switchKnob = switchKnobImg;
		sliderTrack = track;
		sliderFill = fill;
		sliderThumb = thumb;
		colorChip = colorChipImg;
		Transform transform = ((Component)this).transform;
		rect = (RectTransform)(object)((transform is RectTransform) ? transform : null);
		HomePosition = rect.anchoredPosition;
		Group = ((Component)this).GetComponent<CanvasGroup>();
		if ((Object)(object)Group == (Object)null)
		{
			Group = ((Component)this).gameObject.AddComponent<CanvasGroup>();
		}
	}

	public void Show(ButtonData data, MenuButtonKind kind, string valueText)
	{
		Data = data;
		Kind = kind;
		Present();
		SyncToData();
		if (kind != MenuButtonKind.Slider)
		{
			Value.text = valueText ?? string.Empty;
		}
		knobT = knobTarget;
		sliderT = sliderTarget;
		ApplyTheme();
		((Component)this).gameObject.SetActive(true);
	}

	public void ShowPlain(string text, MenuButtonKind kind)
	{
		Data = null;
		Kind = kind;
		Present();
		Label.text = text;
		Value.text = string.Empty;
		knobT = 0f;
		sliderT = 0f;
		ApplyTheme();
		((Component)this).gameObject.SetActive(true);
	}

	public void Place(Vector2 center, Vector2 size)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)rect == (Object)null))
		{
			HomePosition = center;
			rect.sizeDelta = size;
			rect.anchoredPosition = center;
		}
	}

	public void ShowSwatch(string name, Color top, Color bottom, bool active)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		Data = null;
		Kind = MenuButtonKind.Swatch;
		Present();
		Background.UseGradient = true;
		Background.TopColor = top;
		Background.BottomColor = bottom;
		((Graphic)Background).SetVerticesDirty();
		Label.text = name;
		Label.fontSize = 15f;
		Label.fontStyle = (FontStyles)1;
		Value.text = string.Empty;
		Active = active;
		ApplyTheme();
		((Component)this).gameObject.SetActive(true);
	}

	public void Hide()
	{
		Data = null;
		((Component)this).gameObject.SetActive(false);
	}

	private void Present()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		rect.anchoredPosition = HomePosition;
		((Transform)rect).localScale = Vector3.one;
		hoverTarget = false;
		pressTarget = false;
		hoverT = 0f;
		pressT = 0f;
		lastScale = -1f;
		lastKnobX = -1000f;
		lastSliderX = -1000f;
		lastColor = Invalid;
		lastSwitchColor = Invalid;
		LayoutFor(Kind);
	}

	private void LayoutFor(MenuButtonKind kind)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_034b: Unknown result type (might be due to invalid IL or missing references)
		float x = rect.sizeDelta.x;
		float y = rect.sizeDelta.y;
		bool flag = kind == MenuButtonKind.Toggle;
		bool flag2 = kind == MenuButtonKind.Slider;
		bool flag3 = kind == MenuButtonKind.Key;
		bool flag4 = kind == MenuButtonKind.Swatch;
		float num = x * 0.5f - 34f;
		((Graphic)switchTrack).rectTransform.anchoredPosition = new Vector2(num, 0f);
		((Graphic)switchKnob).rectTransform.anchoredPosition = new Vector2(num, 0f);
		knobOffX = num - 11f;
		knobOnX = num + 11f;
		float num2 = x - 28f;
		((Graphic)sliderTrack).rectTransform.sizeDelta = new Vector2(num2, 6f);
		((Graphic)sliderTrack).rectTransform.anchoredPosition = new Vector2(0f, -10f);
		if (flag3 | flag4)
		{
			Label.rectTransform.sizeDelta = (flag4 ? new Vector2(x - 4f, 20f) : new Vector2(x, y));
			Label.rectTransform.anchoredPosition = (Vector2)(flag4 ? new Vector2(0f, (0f - y) * 0.5f + 8f) : Vector2.zero);
			Label.alignment = (TextAlignmentOptions)514;
		}
		else
		{
			float num3;
			if (flag)
			{
				num3 = x - 28f - 56f;
			}
			else
			{
				num3 = (flag2 ? (x - 28f - 64f) : (x - 28f));
			}
			float num4 = (flag2 ? 10f : 0f);
			Label.rectTransform.sizeDelta = new Vector2(num3, flag2 ? 20f : y);
			Label.rectTransform.anchoredPosition = new Vector2((0f - x) * 0.5f + 14f + num3 * 0.5f, num4);
			Label.alignment = (TextAlignmentOptions)4097;
		}
		Value.rectTransform.sizeDelta = new Vector2(60f, flag2 ? 20f : y);
		Value.rectTransform.anchoredPosition = (flag2 ? new Vector2(x * 0.5f - 14f - 30f, 10f) : new Vector2(x * 0.5f - 14f - 30f, 0f));
		((Component)switchTrack).gameObject.SetActive(flag);
		((Component)switchKnob).gameObject.SetActive(flag);
		((Component)sliderTrack).gameObject.SetActive(flag2);
		((Component)Value).gameObject.SetActive(!flag3 && !flag4 && !flag);
		if ((Object)(object)colorChip != (Object)null)
		{
			bool active = Data != null && Data.Name.IndexOf("color", StringComparison.OrdinalIgnoreCase) >= 0;
			((Component)colorChip).gameObject.SetActive(active);
			((Graphic)colorChip).rectTransform.anchoredPosition = new Vector2(x * 0.5f - 14f - 8f, 0f);
		}
	}

	public void SyncToData()
	{
		if (Data != null)
		{
			Label.text = Data.Name;
			knobTarget = (Data.Enabled ? 1f : 0f);
			if (Kind == MenuButtonKind.Slider && Data.Options != null && Data.Options.Length != 0)
			{
				int num = Data.Options.Length;
				int num2 = Mathf.Clamp(Data.CurrentIndex, 0, num - 1);
				sliderTarget = ((num > 1) ? ((float)num2 / (float)(num - 1)) : 0f);
				Value.text = Data.Options[num2];
			}
		}
	}

	public void SetSliderIndex(int index)
	{
		if (Data != null && Data.Options != null && Data.Options.Length != 0)
		{
			int num = Data.Options.Length;
			int num2 = Mathf.Clamp(index, 0, num - 1);
			sliderTarget = ((num > 1) ? ((float)num2 / (float)(num - 1)) : 0f);
			Value.text = Data.Options[num2];
		}
	}

	public float SliderFractionAt(Vector3 world)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		RectTransform rectTransform = ((Graphic)sliderTrack).rectTransform;
		Vector3 val = ((Transform)rectTransform).InverseTransformPoint(world);
		Rect val2 = rectTransform.rect;
		float width = val2.width;
		return Mathf.Clamp01((val.x + width * 0.5f) / width);
	}

	public bool Contains(Vector3 world)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = ((Transform)rect).InverseTransformPoint(world);
		Rect val2 = rect.rect;
		return val2.Contains(new Vector2(val.x, val.y));
	}

	public void SetHover(bool hover)
	{
		hoverTarget = hover;
	}

	public void SetPressed(bool pressed)
	{
		pressTarget = pressed;
	}

	public void SetReveal(float e)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)Group == (Object)null) && !((Object)(object)rect == (Object)null))
		{
			Group.alpha = e;
			rect.anchoredPosition = HomePosition + new Vector2(0f, (0f - (1f - e)) * 36f);
		}
	}

	public void SetActiveTab(bool active)
	{
		Active = active;
		ApplyTheme();
	}

	public void ApplyTheme()
	{
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		MenuTheme current = MenuTheme.Current;
		if (Kind != MenuButtonKind.Swatch && (Object)(object)Background != (Object)null)
		{
			Background.UseGradient = false;
		}
		if (Kind != MenuButtonKind.Swatch && (Object)(object)Label != (Object)null)
		{
			Label.fontSize = 18f;
			Label.fontStyle = (FontStyles)0;
		}
		((Graphic)Label).color = current.ButtonText;
		if (Kind == MenuButtonKind.Swatch)
		{
			Label.fontSize = 15f;
			Label.fontStyle = (FontStyles)1;
		}
		if (Kind == MenuButtonKind.Category && Active)
		{
			((Graphic)Label).color = Color.white;
		}
		else if (Kind == MenuButtonKind.Swatch && Active)
		{
			((Graphic)Label).color = current.Accent;
		}
		else if (Data != null && Data.CustomColor.HasValue)
		{
			((Graphic)Label).color = Data.CustomColor.Value;
		}
		if ((Object)(object)Value != (Object)null)
		{
			((Graphic)Value).color = Color.Lerp(current.ButtonText, current.Background, 0.35f);
		}
		if ((Object)(object)sliderFill != (Object)null)
		{
			((Graphic)sliderFill).color = current.Accent;
		}
		if ((Object)(object)sliderTrack != (Object)null)
		{
			((Graphic)sliderTrack).color = current.ButtonPressed;
		}
		if ((Object)(object)sliderThumb != (Object)null)
		{
			((Graphic)sliderThumb).color = current.TitleText;
		}
		if ((Object)(object)switchKnob != (Object)null)
		{
			((Graphic)switchKnob).color = current.TitleText;
		}
		lastColor = Invalid;
		lastSwitchColor = Invalid;
	}

	private void Update()
	{
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02da: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_029a: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0300: Unknown result type (might be due to invalid IL or missing references)
		//IL_0302: Unknown result type (might be due to invalid IL or missing references)
		//IL_0363: Unknown result type (might be due to invalid IL or missing references)
		//IL_035a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0368: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_052d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0537: Unknown result type (might be due to invalid IL or missing references)
		//IL_0554: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_041f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0453: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a7: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)rect == (Object)null)
		{
			return;
		}
		float unscaledDeltaTime = Time.unscaledDeltaTime;
		float num = 1f - Mathf.Exp((0f - unscaledDeltaTime) * 16f);
		float num2 = 1f - Mathf.Exp((0f - unscaledDeltaTime) * 24f);
		hoverT = Mathf.Lerp(hoverT, hoverTarget ? 1f : 0f, num);
		pressT = Mathf.Lerp(pressT, pressTarget ? 1f : 0f, num2);
		knobT = Mathf.Lerp(knobT, knobTarget, num);
		sliderT = Mathf.Lerp(sliderT, sliderTarget, num);
		MenuTheme current = MenuTheme.Current;
		Color val;
		if (Kind == MenuButtonKind.Swatch)
		{
			val = Color.white;
		}
		else if (Kind != MenuButtonKind.Category)
		{
			val = Color.Lerp(current.Button, current.Accent, 0.12f * knobT);
		}
		else
		{
			val = (Active ? Color.Lerp(current.ButtonPressed, current.Accent, 0.2f) : Color.Lerp(Color.Lerp(current.Background, current.Accent, 0.16f), current.Button, 0.18f));
		}
		Color val2 = Color.Lerp(val, current.ButtonPressed, pressT * 0.7f);
		val2 = Color.Lerp(val2, Color.white, hoverT * ((Kind == MenuButtonKind.Category) ? 0.1f : 0.07f));
		if (!NearlyEqual(val2, lastColor))
		{
			((Graphic)Background).color = val2;
			lastColor = val2;
		}
		float num3 = Mathf.Lerp(1f, 0.965f, pressT);
		float num4 = num3 + hoverT * 0.012f;
		if (Mathf.Abs(num4 - lastScale) > 0.0005f)
		{
			((Transform)rect).localScale = new Vector3(num4, num4, 1f);
			lastScale = num4;
		}
		if (Kind == MenuButtonKind.Toggle && (Object)(object)switchTrack != (Object)null)
		{
			float num5 = Mathf.Lerp(knobOffX, knobOnX, knobT);
			if (Mathf.Abs(num5 - lastKnobX) > 0.01f)
			{
				((Graphic)switchKnob).rectTransform.anchoredPosition = new Vector2(num5, 0f);
				lastKnobX = num5;
			}
			Color val3 = Color.Lerp(new Color(0.165f, 0.173f, 0.2f, 1f), current.Accent, knobT);
			if (!NearlyEqual(val3, lastSwitchColor))
			{
				((Graphic)switchTrack).color = val3;
				lastSwitchColor = val3;
			}
		}
		if ((Object)(object)colorChip != (Object)null && ((Component)colorChip).gameObject.activeSelf && Data != null)
		{
			Color color = Data.CustomColor ?? MenuTheme.Current.Accent;
			if (Data.Options != null && Data.Options.Length != 0)
			{
				string text = Data.Options[Mathf.Clamp(Data.CurrentIndex, 0, Data.Options.Length - 1)];
				if (text.IndexOf("red", StringComparison.OrdinalIgnoreCase) >= 0)
				{
					color = new Color(1f, 0.32f, 0.42f);
				}
				else if (text.IndexOf("green", StringComparison.OrdinalIgnoreCase) >= 0)
				{
					color = new Color(0.28f, 0.88f, 0.62f);
				}
				else if (text.IndexOf("blue", StringComparison.OrdinalIgnoreCase) >= 0)
				{
					color = new Color(0.3f, 0.62f, 1f);
				}
				else if (text.IndexOf("rainbow", StringComparison.OrdinalIgnoreCase) >= 0)
				{
					colorChipHue = Mathf.Repeat(colorChipHue + unscaledDeltaTime * 0.18f, 1f);
					color = Color.HSVToRGB(colorChipHue, 0.72f, 1f);
				}
			}
			((Graphic)colorChip).color = color;
		}
		if (Kind == MenuButtonKind.Slider && (Object)(object)sliderTrack != (Object)null)
		{
			float num6 = sliderT;
			Rect val4 = ((Graphic)sliderTrack).rectTransform.rect;
			float num7 = num6 * val4.width;
			if (Mathf.Abs(num7 - lastSliderX) > 0.05f)
			{
				RectTransform rectTransform = ((Graphic)sliderFill).rectTransform;
				rectTransform.sizeDelta = new Vector2(num7, rectTransform.sizeDelta.y);
				((Graphic)sliderThumb).rectTransform.anchoredPosition = new Vector2(num7, 0f);
				lastSliderX = num7;
			}
		}
	}

	private static bool NearlyEqual(Color a, Color b)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		return Mathf.Abs(a.r - b.r) + Mathf.Abs(a.g - b.g) + Mathf.Abs(a.b - b.b) < 0.0015f;
	}

	public MenuButton()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
	}

	static MenuButton()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
	}
}



