using Sonder.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Sonder.Menu;

public sealed class MenuScanline : MonoBehaviour
{
	private RectTransform _rect;
	private Image _image;
	private float _span;
	private float _phase;

	private void Awake()
	{
		_rect = (RectTransform)transform;
		_image = GetComponent<Image>();
		Transform parent = transform.parent;
		_span = parent is RectTransform parentRect ? Mathf.Max(64f, parentRect.rect.width) : 420f;
		_phase = Mathf.Repeat(Time.unscaledTime * 0.37f, 1f);
	}

	private void Update()
	{
		Transform parent = transform.parent;
		if (parent is RectTransform parentRect)
		{
			_span = Mathf.Max(64f, parentRect.rect.width);
		}
		float speed = Mathf.Max(0.2f, SonderSettings.AnimationSpeed);
		_phase = Mathf.Repeat(_phase + Time.unscaledDeltaTime * 0.45f * speed, 1f);
		float x = Mathf.Lerp(-_span * 0.5f, _span * 0.5f, _phase);
		Vector2 pos = _rect.anchoredPosition;
		pos.x = x;
		_rect.anchoredPosition = pos;

		if (_image == null) return;
		Color color = SonderTheme.Border;
		color.a = 0.06f + 0.26f * Mathf.Sin(_phase * Mathf.PI);
		_image.color = color;
	}
}
