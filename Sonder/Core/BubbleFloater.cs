using UnityEngine;
using Object = UnityEngine.Object;

namespace Sonder.Core;

public class BubbleFloater : MonoBehaviour
{
	private Vector3 _basePos;
	private float _phase;
	private float _speed;

	private void Awake()
	{
		_basePos = transform.localPosition;
		_phase = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
		_speed = UnityEngine.Random.Range(0.6f, 1.2f);
	}

	private void Update()
	{
		float t = Time.unscaledTime * _speed + _phase;
		Vector3 pos = _basePos;
		pos.x += Mathf.Sin(t * 0.7f) * 12f;
		pos.y += Mathf.Sin(t) * 10f;
		transform.localPosition = pos;
		transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * 0.5f) * 8f);
	}
}