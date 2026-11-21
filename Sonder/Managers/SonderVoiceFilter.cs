using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Sonder.Managers;

public sealed class SonderVoiceFilter : MonoBehaviour
{
	public bool Active;

	private float _phase;

	private void OnAudioFilterRead(float[] data, int channels)
	{
		if (!Active || data == null || channels <= 0)
		{
			return;
		}
		for (int i = 0; i < data.Length; i += channels)
		{
			_phase += 9E-05f;
			if (_phase > 1f)
			{
				_phase--;
			}
			float num = Mathf.Sin(_phase * 2f * (float)Math.PI);
			for (int j = 0; j < channels && i + j < data.Length; j++)
			{
				float num2 = data[i + j];
				float num3 = Mathf.Round(num2 * 14f) / 14f;
				data[i + j] = Mathf.Clamp(num3 * (0.65f + 0.35f * num) + num2 * 0.25f, -1f, 1f);
			}
		}
	}
}
