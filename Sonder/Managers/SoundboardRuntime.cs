using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using BepInEx;
using Sonder.Core;
using Sonder.Menu;
using UnityEngine;
using Object = UnityEngine.Object;
using UnityEngine.Networking;

namespace Sonder.Managers;

public class SoundboardRuntime : MonoBehaviour
{
	private readonly List<string> _files = new List<string>();
	private readonly Dictionary<string, AudioClip> _clips = new Dictionary<string, AudioClip>();

	private AudioSource _player;
	private AudioSource _micOut;

	private static readonly float[] LoudnessSteps = new float[] { 0.25f, 0.5f, 0.75f, 1f, 1.5f, 2f, 3f, 4f, 5f };
	private int _loudnessIndex = 3; // 1.0x

	// Reflection cache for mic/recorder routing
	private Component _cachedRecorder;
	private object _cachedRecorderSourceType;
	private object _micSourceType;
	private object _clipSourceType;
	private bool _recorderScanned;

	public static SoundboardRuntime Instance { get; private set; }

	public float Loudness => LoudnessSteps[Mathf.Clamp(_loudnessIndex, 0, LoudnessSteps.Length - 1)];
	public string LoudnessLabel => Mathf.RoundToInt(Loudness * 100f) + "%";

	public static string SoundsFolder
	{
		get
		{
			string root = null;
			try { root = Paths.PluginPath; } catch { }
			if (string.IsNullOrEmpty(root))
			{
				try { root = Application.dataPath; } catch { root = "."; }
			}
			string text = Path.Combine(root, "Sonder", "Sounds");
			try
			{
				if (!Directory.Exists(text))
				{
					Directory.CreateDirectory(text);
				}
			}
			catch
			{
			}
			return text;
		}
	}

	private void Awake()
	{
		Instance = this;
		GameObject val = new GameObject("Sonder_Soundboard");
		val.transform.SetParent(((Component)this).transform, false);

		_player = val.AddComponent<AudioSource>();
		_player.playOnAwake = false;
		_player.spatialBlend = 0f;
		_player.loop = false;
		_player.volume = 1f;

		_micOut = val.AddComponent<AudioSource>();
		_micOut.playOnAwake = false;
		_micOut.spatialBlend = 0f;
		_micOut.loop = false;
		_micOut.volume = 1f;

		Scan();
		PopulateCategory();
	}

	private void Update()
	{
		// Keep mic output at selected loudness while playing
		if ((Object)(object)_micOut != (Object)null)
		{
			_micOut.volume = Loudness;
			if (!_micOut.isPlaying)
			{
				TryRestoreRecorder();
			}
		}
		if ((Object)(object)_player != (Object)null)
		{
			_player.volume = Mathf.Clamp01(Loudness);
		}
	}

	private void Scan()
	{
		_files.Clear();
		try
		{
			string soundsFolder = SoundsFolder;
			if (!Directory.Exists(soundsFolder))
			{
				return;
			}
			string[] array = new string[3] { "*.wav", "*.mp3", "*.ogg" };
			for (int i = 0; i < array.Length; i++)
			{
				string[] files = Directory.GetFiles(soundsFolder, array[i]);
				for (int j = 0; j < files.Length; j++)
				{
					_files.Add(files[j]);
				}
			}
		}
		catch (Exception ex)
		{
			RuntimeLog.Write("soundboard scan failed: " + ex.Message);
		}
	}

	private void CycleLoudness()
	{
		_loudnessIndex = (_loudnessIndex + 1) % LoudnessSteps.Length;
		NotificationRuntime.Show("Soundboard Volume: " + LoudnessLabel);
		ModManager.NotifyChanged();
	}

	private void PopulateCategory()
	{
		ModCategory modCategory = null;
		for (int i = 0; i < ModManager.Categories.Count; i++)
		{
			if (string.Equals(ModManager.Categories[i].Name, "Soundboard", StringComparison.OrdinalIgnoreCase))
			{
				modCategory = ModManager.Categories[i];
				break;
			}
		}
		if (modCategory == null)
		{
			modCategory = new ModCategory("Soundboard");
			ModManager.Categories.Add(modCategory);
		}
		modCategory.Mods.Clear();
		modCategory.Mods.Add(new ModEntry("Open Folder", "Opens the Sounds folder.", delegate
		{
			try { Process.Start("explorer.exe", SoundsFolder); }
			catch (Exception ex) { RuntimeLog.Write("open folder failed: " + ex.Message); }
		}, () => "OPEN", () => false));
		modCategory.Mods.Add(new ModEntry("Reload Sounds", "Rescans the Sounds folder.", delegate
		{
			if ((Object)(object)Instance != (Object)null)
			{
				Instance.Scan();
				Instance.PopulateCategory();
				NotificationRuntime.Show("Soundboard reloaded");
			}
		}, () => "RELOAD", () => false));
		modCategory.Mods.Add(new ModEntry("Soundboard Volume", "Loudness for soundboard playback/mic output.", CycleLoudness, () => LoudnessLabel, () => true));
		modCategory.Mods.Add(new ModEntry("Disable All Sounds", "Stops all playing sounds and restores mic.", delegate
		{
			if ((Object)(object)Instance != (Object)null) Instance.StopAll();
		}, () => "STOP", () => false));
		for (int num = 0; num < _files.Count; num++)
		{
			string text = _files[num];
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(text);
			string capturedPath = text;
			string capturedName = fileNameWithoutExtension;
			modCategory.Mods.Add(new ModEntry(capturedName, "Play sound through mic: " + capturedName, delegate
			{
				if ((Object)(object)Instance != (Object)null) Instance.Play(capturedPath, capturedName);
			}, () => "PLAY", () => false));
		}
		MenuBuilder.RefreshCategory();
	}

	public void Play(string path, string name)
	{
		((MonoBehaviour)this).StartCoroutine(PlayRoutine(path, name));
	}

	public void StopAll()
	{
		try { if ((Object)(object)_player != (Object)null) _player.Stop(); } catch { }
		try { if ((Object)(object)_micOut != (Object)null) _micOut.Stop(); } catch { }
		TryRestoreRecorder();
		NotificationRuntime.Show("All sounds stopped");
	}

	private IEnumerator PlayRoutine(string path, string name)
	{
		AudioClip value = null;
		if (!_clips.TryGetValue(path, out value) || (Object)(object)value == (Object)null)
		{
			string text = "file://" + path.Replace("\\", "/");
			AudioType val = (AudioType)20; // WAV
			string text2 = path.ToLowerInvariant();
			if (text2.EndsWith(".mp3")) val = (AudioType)13;
			else if (text2.EndsWith(".ogg")) val = (AudioType)14;

			UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(text, val);
			try
			{
				yield return www.SendWebRequest();
				if ((int)www.result != 1)
				{
					RuntimeLog.Write("soundboard load failed: " + www.error);
					yield break;
				}
				value = DownloadHandlerAudioClip.GetContent(www);
				if ((Object)(object)value != (Object)null) _clips[path] = value;
			}
			finally
			{
				((IDisposable)www)?.Dispose();
			}
		}
		if ((Object)(object)value == (Object)null) yield break;

		// Local monitor playback
		try
		{
			if ((Object)(object)_player != (Object)null)
			{
				_player.clip = value;
				_player.loop = false;
				_player.volume = Mathf.Clamp01(Loudness);
				_player.Play();
			}
		}
		catch { }

		// Mic/voice output routing (others hear)
		bool micRouted = false;
		try { micRouted = RouteToMic(value); } catch (Exception ex) { RuntimeLog.Write("soundboard mic route failed: " + ex.Message); }
		if (!micRouted)
		{
			// Fallback: play on rig voiceAudio at boosted volume
			try
			{
				VRRig localRig = VRRig.LocalRig;
				if ((Object)(object)localRig != (Object)null)
				{
					FieldInfo field = typeof(VRRig).GetField("voiceAudio", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					if (field != null)
					{
						object value2 = field.GetValue(localRig);
						AudioSource val2 = (AudioSource)((value2 is AudioSource) ? value2 : null);
						if ((Object)(object)val2 != (Object)null)
						{
							val2.clip = value;
							val2.loop = false;
							val2.volume = Loudness;
							val2.Play();
						}
					}
				}
			}
			catch { }
		}

		NotificationRuntime.Show("Playing: " + name + " (" + LoudnessLabel + ")");
	}

	private bool RouteToMic(AudioClip clip)
	{
		if ((Object)(object)clip == (Object)null) return false;

		// Prepare dedicated output source
		if ((Object)(object)_micOut == (Object)null) return false;
		_micOut.clip = clip;
		_micOut.loop = false;
		_micOut.volume = Loudness;
		_micOut.spatialBlend = 0f;

		Component recorder = FindRecorderComponent();
		if ((Object)(object)recorder != (Object)null)
		{
			try
			{
				// Try to switch recorder input to AudioClip/AudioSource
				if (TrySetRecorderToAudioSource(recorder, _micOut, clip))
				{
					_micOut.Play();
					return true;
				}
			}
			catch (Exception ex)
			{
				RuntimeLog.Write("recorder route error: " + ex.Message);
			}
		}

		// Best-effort: play on micOut anyway (if any pipeline reads from it) and on voiceAudio
		try { _micOut.Play(); } catch { }
		return false;
	}

	private Component FindRecorderComponent()
	{
		try
		{
			if ((Object)(object)_cachedRecorder != (Object)null) return _cachedRecorder;

			Type recorderType = Type.GetType("Photon.Voice.Unity.Recorder, PhotonVoice")
				?? Type.GetType("Photon.Voice.Unity.Recorder, PhotonVoice.API")
				?? Type.GetType("Photon.Voice.Unity.Recorder");
			if (recorderType == null)
			{
				// scan loaded assemblies
				Assembly[] asms = AppDomain.CurrentDomain.GetAssemblies();
				for (int a = 0; a < asms.Length && recorderType == null; a++)
				{
					try { recorderType = asms[a].GetType("Photon.Voice.Unity.Recorder"); } catch { }
				}
			}
			if (recorderType == null) return null;

			// Try rig's voice view children
			VRRig rig = VRRig.LocalRig;
			if ((Object)(object)rig != (Object)null)
			{
				FieldInfo vvf = typeof(VRRig).GetField("myPhotonVoiceView", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
					?? typeof(VRRig).GetField("photonVoiceView", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (vvf != null)
				{
					Component vv = vvf.GetValue(rig) as Component;
					if ((Object)(object)vv != (Object)null)
					{
						Component r = vv.GetComponentInChildren(recorderType, true) as Component;
						if ((Object)(object)r != (Object)null) { _cachedRecorder = r; CacheRecorderEnums(r); return r; }
					}
				}
				Component rr = ((Component)rig).GetComponentInChildren(recorderType, true) as Component;
				if ((Object)(object)rr != (Object)null) { _cachedRecorder = rr; CacheRecorderEnums(rr); return rr; }
			}

			// Try GorillaTagger
			try
			{
				GorillaTagger tagger = GorillaTagger.Instance;
				if ((Object)(object)tagger != (Object)null)
				{
					Component r = ((Component)tagger).GetComponentInChildren(recorderType, true) as Component;
					if ((Object)(object)r != (Object)null) { _cachedRecorder = r; CacheRecorderEnums(r); return r; }
				}
			}
			catch { }

			// Global search
			UnityEngine.Object[] objs = Resources.FindObjectsOfTypeAll(recorderType);
			for (int i = 0; i < objs.Length; i++)
			{
				Component c = objs[i] as Component;
				if ((Object)(object)c == (Object)null) continue;
				if (!c.gameObject.scene.IsValid()) continue;
				_cachedRecorder = c;
				CacheRecorderEnums(c);
				return c;
			}
		}
		catch (Exception ex)
		{
			RuntimeLog.Write("find recorder failed: " + ex.Message);
		}
		return null;
	}

	private void CacheRecorderEnums(Component recorder)
	{
		if (_recorderScanned || (Object)(object)recorder == (Object)null) return;
		_recorderScanned = true;
		try
		{
			PropertyInfo sp = recorder.GetType().GetProperty("SourceType", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (sp != null)
			{
				try { _cachedRecorderSourceType = sp.GetValue(recorder, null); } catch { }
				Type et = sp.PropertyType;
				if (et.IsEnum)
				{
					try { _clipSourceType = Enum.Parse(et, "AudioClip"); } catch { }
					try { _micSourceType = Enum.Parse(et, "Microphone"); } catch { }
					if (_micSourceType == null) try { _micSourceType = Enum.Parse(et, "UnityMicrophone"); } catch { }
				}
			}
		}
		catch { }
	}

	private bool TrySetRecorderToAudioSource(Component recorder, AudioSource src, AudioClip clip)
	{
		bool any = false;
		Type t = recorder.GetType();
		BindingFlags f = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

		// Assign clip/audio source fields/properties if present
		TrySetMember(t, recorder, "AudioClip", clip, f, ref any);
		TrySetMember(t, recorder, "audioClip", clip, f, ref any);
		TrySetMember(t, recorder, "Source", src, f, ref any);
		TrySetMember(t, recorder, "AudioSource", src, f, ref any);
		TrySetMember(t, recorder, "audioSource", src, f, ref any);

		// Switch source type to AudioClip
		if (_clipSourceType != null)
		{
			PropertyInfo sp = t.GetProperty("SourceType", f);
			if (sp != null && sp.CanWrite)
			{
				try { sp.SetValue(recorder, _clipSourceType, null); any = true; } catch { }
			}
			else
			{
				FieldInfo sf = t.GetField("SourceType", f) ?? t.GetField("sourceType", f);
				if (sf != null) { try { sf.SetValue(recorder, _clipSourceType); any = true; } catch { } }
			}
		}

		// Ensure transmitting
		TrySetMember(t, recorder, "TransmitEnabled", true, f, ref any);
		TrySetMember(t, recorder, "transmitEnabled", true, f, ref any);
		TrySetMember(t, recorder, "IsRecording", true, f, ref any);

		// Restart/reinit if methods exist
		TryInvoke(t, recorder, "RestartRecording", f);
		TryInvoke(t, recorder, "ReInit", f);
		TryInvoke(t, recorder, "ResetLocalAudio", f);

		return any;
	}

	private void TryRestoreRecorder()
	{
		try
		{
			if ((Object)(object)_micOut != (Object)null && _micOut.isPlaying) return;
			Component recorder = _cachedRecorder ?? FindRecorderComponent();
			if ((Object)(object)recorder == (Object)null) return;
			Type t = recorder.GetType();
			BindingFlags f = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

			if (_micSourceType != null)
			{
				PropertyInfo sp = t.GetProperty("SourceType", f);
				if (sp != null && sp.CanWrite)
				{
					try { sp.SetValue(recorder, _micSourceType, null); } catch { }
				}
				else
				{
					FieldInfo sf = t.GetField("SourceType", f) ?? t.GetField("sourceType", f);
					if (sf != null) { try { sf.SetValue(recorder, _micSourceType); } catch { } }
				}
				TryInvoke(t, recorder, "RestartRecording", f);
			}
		}
		catch { }
	}

	private static void TrySetMember(Type t, object obj, string name, object value, BindingFlags f, ref bool ok)
	{
		try
		{
			PropertyInfo p = t.GetProperty(name, f);
			if (p != null && p.CanWrite)
			{
				p.SetValue(obj, value, null); ok = true; return;
			}
		}
		catch { }
		try
		{
			FieldInfo fi = t.GetField(name, f);
			if (fi != null)
			{
				fi.SetValue(obj, value); ok = true;
			}
		}
		catch { }
	}

	private static void TryInvoke(Type t, object obj, string name, BindingFlags f)
	{
		try
		{
			MethodInfo m = t.GetMethod(name, f, null, Type.EmptyTypes, null);
			if (m != null) m.Invoke(obj, null);
		}
		catch { }
	}
}

