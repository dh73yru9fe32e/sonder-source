using Sonder.Core;
using UnityEngine;

namespace Sonder.Menu;

public sealed class MenuCardEntrance : MonoBehaviour
{
	public int Index;

	private CanvasGroup _group;
	private float _elapsed;
	private float _delay;

	private void Awake()
	{
		_group = GetComponent<CanvasGroup>();
		if (_group == null) _group = gameObject.AddComponent<CanvasGroup>();
		_delay = Mathf.Min(0.14f, Mathf.Max(0, Index) * 0.012f);
		_group.alpha = 0f;
		_group.interactable = true;
		_group.blocksRaycasts = true;
	}

	private void Update()
	{
		float speed = Mathf.Max(0.25f, SonderSettings.AnimationSpeed);
		_elapsed += Time.unscaledDeltaTime * speed;
		float t = Mathf.Clamp01((_elapsed - _delay) / 0.16f);
		_group.alpha = t * t * (3f - 2f * t);
		if (t >= 1f)
		{
			_group.alpha = 1f;
			Destroy(this);
		}
	}
}
