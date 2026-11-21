using System;
using System.Collections;
using Sonder.Core;
using Sonder.Managers;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Sonder.Menu;

public class MenuButton : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerClickHandler
{
	public TextMeshProUGUI titleText;

	public TextMeshProUGUI descText;

	public Image background;

	public Image border;

	public Color normalBackground;

	public Color hoverBackground;

	public Color normalTitle;

	public Color hoverTitle;

	public Color normalBorder;

	public Color hoverBorder;

	public ModEntry BoundEntry;
	public string Tooltip;
	public bool Rainbow;
	public bool UseThemeColors = true;

	public static bool FavoriteModifier;

	private Vector3 _baseScale;

	private Action _onClick;

	private bool _hovered;

	private float _hoverAmount;

	private float _punch;

	private static float _lastClickTime;

	private static int _lastClickFrame = -1;
	private static int _rainbowCounter;
	private readonly int _rainbowPhase = _rainbowCounter++;

	public void Setup(string title, string description, Action action)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		_baseScale = ((Component)this).transform.localScale;
		if ((Object)(object)background != (Object)null)
		{
			normalBackground = ((Graphic)background).color;
		}
		_onClick = action;
		if ((Object)(object)titleText != (Object)null)
		{
			((TMP_Text)titleText).text = title;
		}
		if ((Object)(object)descText != (Object)null)
		{
			((TMP_Text)descText).text = description;
		}
	}

	private void Update()
	{
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		float unscaledDeltaTime = Time.unscaledDeltaTime;
		if (Rainbow)
		{
			float hue = (Time.unscaledTime * 0.5f + _rainbowPhase * 0.13f) % 1f;
			Color rainbow = Color.HSVToRGB(hue, 1f, 1f);
			normalBackground = Color.Lerp(SonderTheme.Row, rainbow, 0.45f);
			hoverBackground = Color.Lerp(SonderTheme.Row, rainbow, 0.65f);
			normalBorder = rainbow;
			hoverBorder = Color.Lerp(rainbow, Color.white, 0.4f);
		}
		else if (UseThemeColors)
		{
			normalBackground = SonderTheme.Row;
			hoverBackground = SonderTheme.RowHover;
			normalBorder = SonderTheme.Border;
			hoverBorder = SonderTheme.BorderHover;
		}
		_hoverAmount = Mathf.MoveTowards(_hoverAmount, _hovered ? 1f : 0f, unscaledDeltaTime * 12f);
		_punch = Mathf.Max(0f, _punch - unscaledDeltaTime * 6f);
		float num = Mathf.SmoothStep(0f, 1f, _hoverAmount);
		float num2 = 1f - 0.015f * Mathf.Sin(_punch * (float)Math.PI);
		((Component)this).transform.localScale = _baseScale * (1f + 0.006f * num) * num2;
		if ((Object)(object)background != (Object)null)
		{
			((Graphic)background).color = Color.Lerp(normalBackground, hoverBackground, num);
		}
		if ((Object)(object)border != (Object)null)
		{
			((Graphic)border).color = Color.Lerp(normalBorder, hoverBorder, num);
		}
		if ((Object)(object)titleText != (Object)null)
		{
			((Graphic)titleText).color = Color.Lerp(normalTitle, hoverTitle, num);
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		_hovered = true;
		if (!string.IsNullOrEmpty(Tooltip))
		{
			MenuBuilder.ShowTooltip(Tooltip);
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		_hovered = false;
		MenuBuilder.ShowTooltip(null);
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (Time.frameCount == _lastClickFrame)
		{
			return;
		}
		float clickCooldownSeconds = SonderSettings.ClickCooldownSeconds;
		if (!(Time.unscaledTime - _lastClickTime < clickCooldownSeconds))
		{
			_lastClickFrame = Time.frameCount;
			_lastClickTime = Time.unscaledTime;
			_punch = 1f;
			SonderAudio.Click();
			StartCoroutine(Ripple());
			if (FavoriteModifier && BoundEntry != null)
			{
				BoundEntry.ToggleFavorite();
				NotificationRuntime.Show((BoundEntry.Favorite ? "favorited: " : "unfavorited: ") + BoundEntry.Title);
				MenuBuilder.RebuildCurrent();
			}
			else if (_onClick != null)
			{
				_onClick();
			}
		}
	}

	private IEnumerator Ripple()
	{
		RectTransform host = (RectTransform)((Component)this).transform;
		GameObject sweep = new GameObject("Sweep");
		sweep.transform.SetParent(((Component)this).transform, false);
		RectTransform rRect = sweep.AddComponent<RectTransform>();
		rRect.anchorMin = new Vector2(0f, 0.5f);
		rRect.anchorMax = new Vector2(0f, 0.5f);
		rRect.pivot = new Vector2(0f, 0.5f);
		rRect.anchoredPosition = Vector2.zero;
		float target = Mathf.Max(32f, host.rect.width);
		rRect.sizeDelta = new Vector2(2f, 1f);
		Image rImg = sweep.AddComponent<Image>();
		rImg.sprite = SonderSprite.Flat(0);
		rImg.type = (Image.Type)0;
		Color baseColor = hoverBorder.a > 0f ? hoverBorder : SonderTheme.Border;
		baseColor.a = 0.7f;
		((Graphic)rImg).color = baseColor;
		((Graphic)rImg).raycastTarget = false;
		float duration = 0.28f;
		float elapsed = 0f;
		while (elapsed < duration)
		{
			elapsed += Time.unscaledDeltaTime;
			float t = Mathf.Clamp01(elapsed / duration);
			rRect.sizeDelta = new Vector2(Mathf.Lerp(2f, target, t), 1f);
			Color c = baseColor;
			c.a = Mathf.Lerp(0.7f, 0f, t);
			((Graphic)rImg).color = c;
			yield return null;
		}
		Object.Destroy((Object)(object)sweep);
	}

	public MenuButton()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		normalBackground = SonderTheme.Row;
		hoverBackground = SonderTheme.RowHover;
		normalTitle = SonderTheme.Title;
		hoverTitle = SonderTheme.Title;
		normalBorder = SonderTheme.Border;
		hoverBorder = SonderTheme.BorderHover;
		_baseScale = Vector3.one;
	}
}
