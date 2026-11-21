using System;
using System.Collections;
using System.IO;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using UnityEngine.Networking;

namespace Sonder.Core;

public static class SonderAudio
{
	private const int SampleRate = 44100;

	private static AudioSource _sfx;

	private static AudioSource _fx;

	private static AudioSource _hum;

	private static AudioClip _click;

	private static AudioClip _humClip;

	private static AudioClip _horn;

	private static AudioClip _alert;

	private static AudioClip _drip;

	private static bool _initialized;

	private static Action _handler;

	public static void Initialize(GameObject host)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		if (_initialized || (Object)(object)host == (Object)null)
		{
			return;
		}
		_initialized = true;
		GameObject val = new GameObject("Sonder_Audio");
		val.transform.SetParent(host.transform, false);
		_sfx = val.AddComponent<AudioSource>();
		_sfx.playOnAwake = false;
		_sfx.spatialBlend = 0f;
		_sfx.volume = 0.55f;
		_fx = val.AddComponent<AudioSource>();
		_fx.playOnAwake = false;
		_fx.spatialBlend = 0f;
		_fx.volume = 0.65f;
		_hum = val.AddComponent<AudioSource>();
		_hum.playOnAwake = false;
		_hum.loop = true;
		_hum.spatialBlend = 0f;
		_hum.volume = 0.12f;
		_click = BuildClick();
		_humClip = BuildHum();
		_horn = BuildHorn();
		_alert = BuildAlert();
		_drip = BuildDrip();
		_hum.clip = _humClip;
		_handler = SyncHum;
		SonderSettings.Changed += _handler;
		SyncHum();
		string[] array = new string[2]
		{
			"C:\\Users\\kenzo\\Downloads\\universfield-mouse-click-351398.mp3",
			Path.Combine(Application.persistentDataPath, "Sonder.click.mp3")
		};
		for (int i = 0; i < array.Length; i++)
		{
			if (File.Exists(array[i]))
			{
				GameObject val2 = new GameObject("Sonder_AudioLoader");
				val2.transform.SetParent(host.transform, false);
				((MonoBehaviour)val2.AddComponent<SonderAudioLoader>()).StartCoroutine(LoadClickClip(array[i]));
				break;
			}
		}
	}

	private static IEnumerator LoadClickClip(string path)
	{
		string text = "file://" + path.Replace("\\", "/");
		UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(text, (AudioType)13);
		try
		{
			yield return www.SendWebRequest();
			if ((int)www.result == 1)
			{
				AudioClip content = DownloadHandlerAudioClip.GetContent(www);
				if ((Object)(object)content != (Object)null)
				{
					_click = content;
				}
			}
			else
			{
				Debug.LogWarning((object)("[Sonder] click mp3 load failed: " + www.error));
			}
		}
		finally
		{
			((IDisposable)www)?.Dispose();
		}
	}

	public static void Click()
	{
		if (_initialized && !((Object)(object)_sfx == (Object)null) && !((Object)(object)_click == (Object)null) && !SonderSettings.Muted && SonderSettings.SoundsOn)
		{
			_sfx.pitch = Random.Range(0.94f, 1.06f);
			_sfx.PlayOneShot(_click, 0.5f);
		}
	}

	public static void Horn(float volume = 1f, float pitch = 1f)
	{
		Play(_horn, volume, pitch);
	}

	public static void Alert(float volume = 1f)
	{
		Play(_alert, volume, 0.98f);
	}

	public static void Drip(float pitch = 1f)
	{
		Play(_drip, 0.8f, pitch);
	}

	private static void Play(AudioClip clip, float volume, float pitch)
	{
		if (_initialized && !((Object)(object)_fx == (Object)null) && !((Object)(object)clip == (Object)null) && !SonderSettings.Muted)
		{
			_fx.pitch = Mathf.Clamp(pitch, 0.4f, 2f);
			_fx.PlayOneShot(clip, Mathf.Clamp01(volume));
		}
	}

	public static void SyncHum()
	{
		if (!((Object)(object)_hum == (Object)null) && !((Object)(object)_humClip == (Object)null))
		{
			bool flag = SonderSettings.HumOn && !SonderSettings.Muted;
			if (flag && !_hum.isPlaying)
			{
				_hum.Play();
			}
			else if (!flag && _hum.isPlaying)
			{
				_hum.Stop();
			}
		}
	}

	private static AudioClip BuildClick()
	{
		int num = Mathf.RoundToInt(2205f);
		float[] array = new float[num];
		System.Random random = new System.Random(6767);
		for (int i = 0; i < num; i++)
		{
			float num2 = (float)i / 44100f;
			float num3 = Mathf.Exp((0f - num2) * 90f);
			float num4 = Mathf.Sin((float)Math.PI * 2900f * num2) * 0.6f;
			float num5 = ((float)random.NextDouble() * 2f - 1f) * 0.35f * Mathf.Exp((0f - num2) * 220f);
			array[i] = (num4 + num5) * num3 * 0.8f;
		}
		AudioClip obj = AudioClip.Create("SonderClick", num, 1, 44100, false);
		Fill(obj, array);
		return obj;
	}

	private static AudioClip BuildHum()
	{
		int num = 88200;
		float[] array = new float[num];
		for (int i = 0; i < num; i++)
		{
			float num2 = (float)i / 44100f;
			array[i] = (Mathf.Sin((float)Math.PI * 110f * num2) * 0.5f + Mathf.Sin((float)Math.PI * 220f * num2) * 0.25f + Mathf.Sin((float)Math.PI * 330f * num2) * 0.12f) * 0.4f;
		}
		AudioClip obj = AudioClip.Create("SonderHum", num, 1, 44100, false);
		Fill(obj, array);
		return obj;
	}

	private static AudioClip BuildHorn()
	{
		float num = 0.62f;
		int num2 = Mathf.RoundToInt(44100f * num);
		float[] array = new float[num2];
		for (int i = 0; i < num2; i++)
		{
			float num3 = (float)i / 44100f;
			float num4 = Mathf.Clamp01(num3 / 0.025f);
			float num5 = Mathf.Clamp01((num - num3) / 0.08f);
			float num6 = 1f + Mathf.Sin((float)Math.PI * 11f * num3) * 0.018f;
			float num7 = (float)Math.PI * 392f * num6 * num3;
			float num8 = Mathf.Sin(num7) * 0.55f + Mathf.Sin(num7 * 2f) * 0.26f + Mathf.Sin(num7 * 3f) * 0.14f + Mathf.Sin(num7 * 4f) * 0.06f;
			array[i] = num8 * num4 * num5 * 0.9f;
		}
		AudioClip obj = AudioClip.Create("SonderHorn", num2, 1, 44100, false);
		Fill(obj, array);
		return obj;
	}

	private static AudioClip BuildAlert()
	{
		float num = 0.22f;
		int num2 = Mathf.RoundToInt(44100f * num);
		float[] array = new float[num2];
		for (int i = 0; i < num2; i++)
		{
			float num3 = (float)i / 44100f;
			float num4 = ((num3 < 0.1f) ? 880f : 1320f);
			float num5 = ((num3 < 0.1f) ? num3 : (num3 - 0.1f));
			float num6 = Mathf.Exp((0f - num5) * 42f) * Mathf.Clamp01(num5 / 0.004f);
			array[i] = Mathf.Sin((float)Math.PI * 2f * num4 * num3) * num6 * 0.7f;
		}
		AudioClip obj = AudioClip.Create("SonderAlert", num2, 1, 44100, false);
		Fill(obj, array);
		return obj;
	}

	private static AudioClip BuildDrip()
	{
		float num = 0.24f;
		int num2 = Mathf.RoundToInt(44100f * num);
		float[] array = new float[num2];
		System.Random random = new System.Random(4417);
		for (int i = 0; i < num2; i++)
		{
			float num3 = (float)i / 44100f;
			float num4 = 1500f * Mathf.Exp((0f - num3) * 14f) + 380f;
			float num5 = Mathf.Exp((0f - num3) * 18f) * Mathf.Clamp01(num3 / 0.003f);
			float num6 = ((float)random.NextDouble() * 2f - 1f) * Mathf.Exp((0f - num3) * 90f) * 0.25f;
			array[i] = (Mathf.Sin((float)Math.PI * 2f * num4 * num3) * 0.7f + num6) * num5;
		}
		AudioClip obj = AudioClip.Create("SonderDrip", num2, 1, 44100, false);
		Fill(obj, array);
		return obj;
	}

	private static void Fill(AudioClip clip, float[] data)
	{
		MethodInfo method = typeof(AudioClip).GetMethod("SetData", new Type[2]
		{
			typeof(float[]),
			typeof(int)
		});
		if (method != null)
		{
			method.Invoke(clip, new object[2] { data, 0 });
		}
	}
}
