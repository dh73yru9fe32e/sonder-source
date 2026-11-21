using System;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Sonder.Core;

public static class SonderSettings
{
	private const string FileName = "Sonder.settings";

	private const int CurrentVersion = 3;

	private static readonly Color[] Accents;

	private static readonly string[] AccentNames;

	private static readonly string[] ButtonNames;

	private static readonly string[] GunBeamNames;

	private static readonly float[] ScaleSteps;

	private static readonly float[] OpacitySteps;

	private static readonly float[] AnimationSteps;

	private static readonly float[] PointerSteps;

	private static readonly float[] GunBeamSteps;

	private static readonly float[] GunBallSteps;

	private static readonly float[] NotiDelaySteps;

	private static readonly int[] NotiSizeSteps;

	private static readonly float[] ClickCooldownSteps;

	private static bool _loaded;

	private static string _path;

	public static int AccentIndex { get; private set; } = 1;

	public static int ButtonIndex { get; private set; }

	public static int ScaleIndex { get; private set; }

	public static int OpacityIndex { get; private set; }

	public static int AnimationIndex { get; private set; }

	public static int PointerIndex { get; private set; }

	public static int HandIndex { get; private set; }

	public static int GunColorIndex { get; private set; }

	public static int GunBeamIndex { get; private set; }

	public static int GunBallIndex { get; private set; }

	public static int NotiDelayIndex { get; private set; }

	public static int NotiSizeIndex { get; private set; }

	public static int ClickCooldownIndex { get; private set; }

	public static bool SoundsOn { get; private set; }

	public static bool HumOn { get; private set; }

	public static bool Muted { get; private set; }

	public static bool DiscordOn { get; private set; }

	public static Color Accent
	{
		get
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			EnsureLoaded();
			return Accents[AccentIndex];
		}
	}

	public static float PanelAlpha => 0.30f * OpacitySteps[OpacityIndex];

	public static float MenuScale => ScaleSteps[ScaleIndex];

	public static float AnimationSpeed => AnimationSteps[AnimationIndex];

	public static float PointerSize => PointerSteps[PointerIndex];

	public static bool RightHandMenu => HandIndex == 1;

	public static Color GunColor
	{
		get
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			return Accents[GunColorIndex];
		}
	}

	public static float GunBeamWidth => GunBeamSteps[GunBeamIndex];

	public static float GunBallSize => GunBallSteps[GunBallIndex];

	public static float NotiDelaySeconds => NotiDelaySteps[NotiDelayIndex];

	public static int NotiFontSize => NotiSizeSteps[NotiSizeIndex];

	public static float ClickCooldownSeconds => ClickCooldownSteps[ClickCooldownIndex];

	public static int ButtonRadius
	{
		get
		{
			if (ButtonIndex != 0)
			{
				if (ButtonIndex != 1)
				{
					return 24;
				}
				return 2;
			}
			return 12;
		}
	}

	public static int PanelRadius
	{
		get
		{
			if (ButtonIndex != 0)
			{
				if (ButtonIndex != 1)
				{
					return 30;
				}
				return 3;
			}
			return 16;
		}
	}

	public static string AccentLabel => AccentNames[AccentIndex];

	public static string ButtonLabel => ButtonNames[ButtonIndex];

	public static string ScaleLabel => Mathf.RoundToInt(ScaleSteps[ScaleIndex] * 100f) + "%";

	public static string OpacityLabel => Mathf.RoundToInt(OpacitySteps[OpacityIndex] * 100f) + "%";

	public static string AnimationLabel => AnimationSteps[AnimationIndex].ToString("0.00") + "x";

	public static string PointerLabel => Mathf.RoundToInt(PointerSteps[PointerIndex] * 100f) + "%";

	public static string GunColorLabel => AccentNames[GunColorIndex];

	public static string GunBeamLabel => GunBeamNames[GunBeamIndex];

	public static string GunBallLabel => Mathf.RoundToInt(GunBallSteps[GunBallIndex] * 1000f) + "%";

	public static string NotiDelayLabel => NotiDelaySteps[NotiDelayIndex].ToString("0.0") + "s";

	public static string NotiSizeLabel => NotiSizeSteps[NotiSizeIndex] + "pt";

	public static string ClickCooldownLabel => ClickCooldownSteps[ClickCooldownIndex].ToString("0.00") + "s";

	public static string HandLabel
	{
		get
		{
			if (HandIndex != 1)
			{
				return "LEFT";
			}
			return "RIGHT";
		}
	}

	public static string SoundsLabel
	{
		get
		{
			if (!SoundsOn)
			{
				return "OFF";
			}
			return "ON";
		}
	}

	public static string DiscordLabel
	{
		get
		{
			if (!DiscordOn)
			{
				return "OFF";
			}
			return "ON";
		}
	}

	public static string HumLabel
	{
		get
		{
			if (!HumOn)
			{
				return "OFF";
			}
			return "ON";
		}
	}

	public static string MuteLabel
	{
		get
		{
			if (!Muted)
			{
				return "OFF";
			}
			return "ON";
		}
	}

	public static event Action Changed;

	static SonderSettings()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		Accents = (Color[])(object)new Color[7]
		{
			(new Color32((byte)77, (byte)224, (byte)204, byte.MaxValue)),
			(new Color32((byte)120, (byte)215, byte.MaxValue, byte.MaxValue)),
			(new Color32(byte.MaxValue, (byte)140, (byte)190, byte.MaxValue)),
			(new Color32(byte.MaxValue, (byte)206, (byte)120, byte.MaxValue)),
			(new Color32((byte)190, (byte)150, byte.MaxValue, byte.MaxValue)),
			(new Color32((byte)232, (byte)194, (byte)122, byte.MaxValue)),
			(new Color32(byte.MaxValue, (byte)120, (byte)120, byte.MaxValue))
		};
		AccentNames = new string[7] { "AURORA", "ICE", "ROSE", "AMBER", "VIOLET", "SAND", "BLOOD" };
		ButtonNames = new string[3] { "ROUNDED", "SHARP", "PILL" };
		GunBeamNames = new string[4] { "THIN", "NORMAL", "THICK", "FAT" };
		ScaleSteps = new float[5] { 0.8f, 0.9f, 1f, 1.15f, 1.3f };
		OpacitySteps = new float[5] { 1f, 0.9f, 0.8f, 0.68f, 0.52f };
		AnimationSteps = new float[5] { 0.7f, 0.85f, 1f, 1.25f, 1.5f };
		PointerSteps = new float[4] { 0.75f, 1f, 1.3f, 1.65f };
		GunBeamSteps = new float[4] { 0.003f, 0.006f, 0.012f, 0.02f };
		GunBallSteps = new float[4] { 0.02f, 0.035f, 0.05f, 0.08f };
		NotiDelaySteps = new float[4] { 1f, 2f, 3f, 4.5f };
		NotiSizeSteps = new int[4] { 16, 20, 26, 32 };
		ClickCooldownSteps = new float[4] { 0f, 0.15f, 0.3f, 0.5f };
		AccentIndex = 1;
		ButtonIndex = 0;
		ScaleIndex = 2;
		OpacityIndex = 3;
		AnimationIndex = 2;
		PointerIndex = 1;
		GunBeamIndex = 1;
		GunBallIndex = 1;
		NotiDelayIndex = 1;
		NotiSizeIndex = 1;
		ClickCooldownIndex = 1;
		SoundsOn = true;
		DiscordOn = true;
		EnsureLoaded();
	}

	public static void CycleAccent()
	{
		EnsureLoaded();
		AccentIndex = (AccentIndex + 1) % Accents.Length;
		Apply();
	}

	public static void CycleButtons()
	{
		EnsureLoaded();
		ButtonIndex = (ButtonIndex + 1) % ButtonNames.Length;
		Apply();
	}

	public static void CycleScale()
	{
		EnsureLoaded();
		ScaleIndex = (ScaleIndex + 1) % ScaleSteps.Length;
		Apply();
	}

	public static void CycleOpacity()
	{
		EnsureLoaded();
		OpacityIndex = (OpacityIndex + 1) % OpacitySteps.Length;
		Apply();
	}

	public static void CycleAnimation()
	{
		EnsureLoaded();
		AnimationIndex = (AnimationIndex + 1) % AnimationSteps.Length;
		Apply();
	}

	public static void CyclePointer()
	{
		EnsureLoaded();
		PointerIndex = (PointerIndex + 1) % PointerSteps.Length;
		Apply();
	}

	public static void CycleHand()
	{
		EnsureLoaded();
		HandIndex = (HandIndex + 1) % 2;
		Apply();
	}

	public static void CycleGunColor()
	{
		EnsureLoaded();
		GunColorIndex = (GunColorIndex + 1) % Accents.Length;
		Apply();
	}

	public static void CycleGunBeam()
	{
		EnsureLoaded();
		GunBeamIndex = (GunBeamIndex + 1) % GunBeamSteps.Length;
		Apply();
	}

	public static void CycleGunBall()
	{
		EnsureLoaded();
		GunBallIndex = (GunBallIndex + 1) % GunBallSteps.Length;
		Apply();
	}

	public static void CycleNotiDelay()
	{
		EnsureLoaded();
		NotiDelayIndex = (NotiDelayIndex + 1) % NotiDelaySteps.Length;
		Apply();
	}

	public static void CycleNotiSize()
	{
		EnsureLoaded();
		NotiSizeIndex = (NotiSizeIndex + 1) % NotiSizeSteps.Length;
		Apply();
	}

	public static void CycleClickCooldown()
	{
		EnsureLoaded();
		ClickCooldownIndex = (ClickCooldownIndex + 1) % ClickCooldownSteps.Length;
		Apply();
	}

	public static void SaveNow()
	{
		EnsureLoaded();
		Save();
	}

	public static void LoadNow()
	{
		_loaded = false;
		EnsureLoaded();
	}

	public static void ToggleSounds()
	{
		EnsureLoaded();
		SoundsOn = !SoundsOn;
		Apply();
	}

	public static void ToggleHum()
	{
		EnsureLoaded();
		HumOn = !HumOn;
		Apply();
	}

	public static void ToggleDiscord()
	{
		EnsureLoaded();
		DiscordOn = !DiscordOn;
		Apply();
	}

	public static void ToggleMute()
	{
		EnsureLoaded();
		Muted = !Muted;
		Apply();
	}

	public static void ResetAll()
	{
		EnsureLoaded();
		AccentIndex = 0;
		ButtonIndex = 0;
		ScaleIndex = 2;
		OpacityIndex = 3;
		AnimationIndex = 2;
		PointerIndex = 1;
		HandIndex = 0;
		SoundsOn = true;
		HumOn = false;
		Muted = false;
		DiscordOn = true;
		Apply();
	}

	private static void Apply()
	{
		Save();
		Changed?.Invoke();
	}

	private static void EnsureLoaded()
	{
		if (!_loaded)
		{
			_loaded = true;
			Load();
		}
	}

	private static void Load()
	{
		try
		{
			string path = SettingsPath();
			if (!File.Exists(path))
			{
				return;
			}
			int result = 0;
			string[] array = File.ReadAllLines(path);
			for (int i = 0; i < array.Length; i++)
			{
				string text = array[i].Trim();
				int num = text.IndexOf('=');
				if (num > 0)
				{
					string text2 = text.Substring(0, num).Trim();
					string text3 = text.Substring(num + 1).Trim();
					switch (text2)
					{
					case "version":
						int.TryParse(text3, out result);
						break;
					case "accent":
						AccentIndex = ClampIndex(text3, Accents.Length, AccentIndex);
						break;
					case "buttons":
						ButtonIndex = ClampIndex(text3, ButtonNames.Length, ButtonIndex);
						break;
					case "scale":
						ScaleIndex = ClampIndex(text3, ScaleSteps.Length, ScaleIndex);
						break;
					case "opacity":
						OpacityIndex = ClampIndex(text3, OpacitySteps.Length, OpacityIndex);
						break;
					case "animation":
						AnimationIndex = ClampIndex(text3, AnimationSteps.Length, AnimationIndex);
						break;
					case "pointer":
						PointerIndex = ClampIndex(text3, PointerSteps.Length, PointerIndex);
						break;
					case "hand":
						HandIndex = ClampIndex(text3, 2, HandIndex);
						break;
					case "guncolor":
						GunColorIndex = ClampIndex(text3, Accents.Length, GunColorIndex);
						break;
					case "gunbeam":
						GunBeamIndex = ClampIndex(text3, GunBeamSteps.Length, GunBeamIndex);
						break;
					case "gunball":
						GunBallIndex = ClampIndex(text3, GunBallSteps.Length, GunBallIndex);
						break;
					case "notidelay":
						NotiDelayIndex = ClampIndex(text3, NotiDelaySteps.Length, NotiDelayIndex);
						break;
					case "notisize":
						NotiSizeIndex = ClampIndex(text3, NotiSizeSteps.Length, NotiSizeIndex);
						break;
					case "clickcooldown":
						ClickCooldownIndex = ClampIndex(text3, ClickCooldownSteps.Length, ClickCooldownIndex);
						break;
					case "sounds":
						SoundsOn = ParseBool(text3, SoundsOn);
						break;
					case "hum":
						HumOn = ParseBool(text3, HumOn);
						break;
					case "muted":
						Muted = ParseBool(text3, Muted);
						break;
					case "discord":
						DiscordOn = ParseBool(text3, DiscordOn);
						break;
					}
				}
			}
			if (result < 3)
			{
				AccentIndex = 0;
				ButtonIndex = 0;
				ScaleIndex = 2;
				OpacityIndex = 3;
				AnimationIndex = 2;
				Save();
			}
		}
		catch (Exception ex)
		{
			Debug.Log((object)("[Sonder] settings load failed: " + ex.Message));
		}
	}

	private static int ClampIndex(string value, int count, int fallback)
	{
		if (!int.TryParse(value, out var result))
		{
			return fallback;
		}
		return Mathf.Clamp(result, 0, count - 1);
	}

	private static bool ParseBool(string value, bool fallback)
	{
		if (!bool.TryParse(value, out var result))
		{
			return fallback;
		}
		return result;
	}

	private static void Save()
	{
		try
		{
			string path = SettingsPath();
			string directoryName = Path.GetDirectoryName(path);
			if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
			{
				Directory.CreateDirectory(directoryName);
			}
			File.WriteAllLines(path, new string[18]
			{
				"version=" + 3,
				"accent=" + AccentIndex,
				"buttons=" + ButtonIndex,
				"scale=" + ScaleIndex,
				"opacity=" + OpacityIndex,
				"animation=" + AnimationIndex,
				"pointer=" + PointerIndex,
				"hand=" + HandIndex,
				"guncolor=" + GunColorIndex,
				"gunbeam=" + GunBeamIndex,
				"gunball=" + GunBallIndex,
				"notidelay=" + NotiDelayIndex,
				"notisize=" + NotiSizeIndex,
				"clickcooldown=" + ClickCooldownIndex,
				"sounds=" + SoundsOn,
				"hum=" + HumOn,
				"muted=" + Muted,
				"discord=" + DiscordOn
			});
		}
		catch (Exception ex)
		{
			Debug.Log((object)("[Sonder] settings save failed: " + ex.Message));
		}
	}

	private static string SettingsPath()
	{
		if (!string.IsNullOrEmpty(_path))
		{
			return _path;
		}
		string text = null;
		try
		{
			string dataPath = Application.dataPath;
			if (!string.IsNullOrEmpty(dataPath))
			{
				string directoryName = Path.GetDirectoryName(dataPath);
				if (!string.IsNullOrEmpty(directoryName))
				{
					string text2 = Path.Combine(directoryName, "BepInEx");
					if (Directory.Exists(text2))
					{
						text = Path.Combine(text2, "config");
					}
				}
			}
		}
		catch
		{
		}
		if (string.IsNullOrEmpty(text))
		{
			text = Application.persistentDataPath;
		}
		_path = Path.Combine(text, "Sonder.settings");
		return _path;
	}
}
