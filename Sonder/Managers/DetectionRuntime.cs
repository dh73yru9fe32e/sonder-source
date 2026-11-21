using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BepInEx;
using Sonder.Core;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Sonder.Managers;

public class DetectionRuntime : MonoBehaviour
{
	private sealed class RigLabel
	{
		public TextMesh Label;

		public bool Flagged;

		public bool Alt;
	}

	private readonly Dictionary<VRRig, RigLabel> _labels = new Dictionary<VRRig, RigLabel>();

	private readonly Dictionary<string, HashSet<int>> _nameGroups = new Dictionary<string, HashSet<int>>();

	private readonly Dictionary<string, HashSet<int>> _deviceGroups = new Dictionary<string, HashSet<int>>();

	private readonly HashSet<int> _knownActors = new HashSet<int>();

	private readonly HashSet<int> _altActors = new HashSet<int>();

	private readonly List<string> _watchTerms = new List<string>();

	private readonly List<VRRig> _stale = new List<VRRig>();

	private readonly StringBuilder _scanBuilder = new StringBuilder();

	private bool _knownInitialized;

	private bool _wasNear;

	private float _nextProximity;

	private float _nextRoomScan;

	private string _lastRoomSignature = string.Empty;

	public static DetectionRuntime Instance { get; private set; }

	public bool Watchlist { get; set; }

	public bool AlertPing { get; set; }

	public bool NameTags { get; set; }

	public bool DistanceReadout { get; set; }

	public bool JoinAlerts { get; set; }

	public bool RoomScan { get; set; }

	public bool PingHistory { get; set; }

	public bool AltAlert { get; set; }

	private void Awake()
	{
		Instance = this;
		LoadWatchlist();
	}

	private void OnDestroy()
	{
		if ((Object)(object)Instance == (Object)(object)this)
		{
			Instance = null;
		}
		foreach (KeyValuePair<VRRig, RigLabel> label in _labels)
		{
			if ((Object)(object)label.Value.Label != (Object)null)
			{
				Object.Destroy((Object)(object)((Component)label.Value.Label).gameObject);
			}
		}
		_labels.Clear();
	}

	private void Update()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		List<VRRig> rigs = ExtraUtil.RemoteRigs();
		Transform val = ExtraUtil.Head();
		Vector3 origin = (((Object)(object)val != (Object)null) ? val.position : ((Component)this).transform.position);
		ScanIdentities(rigs);
		ScanJoins(rigs);
		UpdateLabels(rigs, origin);
		UpdateProximity(rigs, origin);
		UpdateRoomScan(rigs);
	}

	private void ScanIdentities(List<VRRig> rigs)
	{
		for (int i = 0; i < rigs.Count; i++)
		{
			VRRig val = rigs[i];
			if (!((Object)(object)val == (Object)null) && val.Creator != null)
			{
				NetPlayer creator = val.Creator;
				int actorNumber = creator.ActorNumber;
				string text = ExtraUtil.NormalizeName(ExtraUtil.Nickname(val));
				string mothershipId = creator.MothershipId;
				if (!string.IsNullOrEmpty(text))
				{
					AddGroup(_nameGroups, text, actorNumber);
				}
				if (!string.IsNullOrEmpty(mothershipId))
				{
					AddGroup(_deviceGroups, mothershipId, actorNumber);
				}
				if (IsAlt(text, mothershipId, actorNumber) && _altActors.Add(actorNumber) && AltAlert)
				{
					SonderAudio.Alert(0.6f);
					LogPing("alt", val, text + " device=" + (mothershipId ?? "?"));
				}
			}
		}
	}

	private static void AddGroup(Dictionary<string, HashSet<int>> groups, string key, int actor)
	{
		if (!groups.TryGetValue(key, out var value))
		{
			value = (groups[key] = new HashSet<int>());
		}
		value.Add(actor);
	}

	private bool IsAlt(string nameKey, string deviceKey, int actor)
	{
		if (!AltAlert)
		{
			return false;
		}
		if (!string.IsNullOrEmpty(nameKey) && _nameGroups.TryGetValue(nameKey, out var value) && value.Count > 1)
		{
			foreach (int item in value)
			{
				if (item != actor)
				{
					return true;
				}
			}
		}
		if (!string.IsNullOrEmpty(deviceKey) && _deviceGroups.TryGetValue(deviceKey, out var value2) && value2.Count > 1)
		{
			foreach (int item2 in value2)
			{
				if (item2 != actor)
				{
					return true;
				}
			}
		}
		return false;
	}

	private void ScanJoins(List<VRRig> rigs)
	{
		if (!_knownInitialized)
		{
			for (int i = 0; i < rigs.Count; i++)
			{
				if ((Object)(object)rigs[i] != (Object)null && rigs[i].Creator != null)
				{
					_knownActors.Add(rigs[i].Creator.ActorNumber);
				}
			}
			_knownInitialized = true;
			return;
		}
		for (int j = 0; j < rigs.Count; j++)
		{
			VRRig val = rigs[j];
			if (!((Object)(object)val == (Object)null) && val.Creator != null)
			{
				int actorNumber = val.Creator.ActorNumber;
				if (_knownActors.Add(actorNumber) && JoinAlerts)
				{
					SonderAudio.Alert(0.5f);
					LogPing("join", val, val.Creator.Platform ?? "?");
				}
			}
		}
	}

	private void UpdateLabels(List<VRRig> rigs, Vector3 origin)
	{
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0305: Unknown result type (might be due to invalid IL or missing references)
		//IL_030a: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b7: Unknown result type (might be due to invalid IL or missing references)
		if (!NameTags && !DistanceReadout && !Watchlist && !AltAlert)
		{
			foreach (KeyValuePair<VRRig, RigLabel> label in _labels)
			{
				if ((Object)(object)label.Value.Label != (Object)null)
				{
					Object.Destroy((Object)(object)((Component)label.Value.Label).gameObject);
				}
			}
			_labels.Clear();
			return;
		}
		for (int i = 0; i < rigs.Count; i++)
		{
			VRRig val = rigs[i];
			if ((Object)(object)val == (Object)null)
			{
				continue;
			}
			if (!_labels.TryGetValue(val, out var value))
			{
				TextMesh val2 = ExtraUtil.CreateLabel(((Component)this).transform, "Sonder_Detect", Color.white);
				if ((Object)(object)val2 == (Object)null)
				{
					continue;
				}
				val2.fontSize = 32;
				val2.characterSize = 0.07f;
				value = new RigLabel
				{
					Label = val2
				};
				_labels[val] = value;
			}
			value.Flagged = Watchlist && IsWatchlisted(val);
			value.Alt = AltAlert && _altActors.Contains((val.Creator != null) ? val.Creator.ActorNumber : (-1));
			bool flag = NameTags || DistanceReadout || value.Flagged || value.Alt;
			if ((Object)(object)value.Label == (Object)null || !flag)
			{
				if ((Object)(object)value.Label != (Object)null)
				{
					((Component)value.Label).gameObject.SetActive(false);
				}
				continue;
			}
			Vector3 val3 = (((Object)(object)val.headMesh != (Object)null) ? val.headMesh.transform.position : (((Component)val).transform.position + Vector3.up * 1.2f));
			float num = Vector3.Distance(origin, val3);
			StringBuilder stringBuilder = new StringBuilder();
			if (NameTags || value.Flagged || value.Alt)
			{
				stringBuilder.Append(ExtraUtil.Nickname(val));
			}
			if (value.Flagged)
			{
				stringBuilder.Append(" [WATCH]");
			}
			if (value.Alt)
			{
				stringBuilder.Append(" [ALT]");
			}
			if (DistanceReadout)
			{
				if (stringBuilder.Length > 0)
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append(Mathf.RoundToInt(num)).Append('m');
			}
			value.Label.text = stringBuilder.ToString();
			value.Label.color = (Color)(value.Flagged ? new Color(1f, 0.24f, 0.22f) : (value.Alt ? new Color(1f, 0.62f, 0.18f) : SonderSettings.Accent));
			((Component)value.Label).transform.position = val3 + Vector3.up * 0.45f;
			((Component)value.Label).gameObject.SetActive(true);
			ExtraUtil.FaceCamera(value.Label);
		}
		_stale.Clear();
		foreach (KeyValuePair<VRRig, RigLabel> label2 in _labels)
		{
			if ((Object)(object)label2.Key == (Object)null || !rigs.Contains(label2.Key))
			{
				_stale.Add(label2.Key);
			}
		}
		for (int j = 0; j < _stale.Count; j++)
		{
			RemoveLabel(_stale[j]);
		}
	}

	private void RemoveLabel(VRRig rig)
	{
		if (_labels.TryGetValue(rig, out var value))
		{
			if ((Object)(object)value.Label != (Object)null)
			{
				Object.Destroy((Object)(object)((Component)value.Label).gameObject);
			}
			_labels.Remove(rig);
		}
	}

	private bool IsWatchlisted(VRRig rig)
	{
		if ((Object)(object)rig == (Object)null || _watchTerms.Count == 0)
		{
			return false;
		}
		string text = ExtraUtil.Nickname(rig).ToLowerInvariant();
		string text2 = ExtraUtil.NormalizeName(text);
		for (int i = 0; i < _watchTerms.Count; i++)
		{
			string value = _watchTerms[i];
			if (text2.Contains(value) || text.Contains(value))
			{
				return true;
			}
		}
		return false;
	}

	private void UpdateProximity(List<VRRig> rigs, Vector3 origin)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		if (!AlertPing)
		{
			_wasNear = false;
			return;
		}
		float num = float.MaxValue;
		for (int i = 0; i < rigs.Count; i++)
		{
			VRRig val = rigs[i];
			if (!((Object)(object)val == (Object)null))
			{
				Vector3 val2 = ((Component)val).transform.position - origin;
				float sqrMagnitude = val2.sqrMagnitude;
				if (sqrMagnitude < num)
				{
					num = sqrMagnitude;
				}
			}
		}
		bool flag = num < 49f;
		if (flag && !_wasNear && Time.time >= _nextProximity)
		{
			_nextProximity = Time.time + 2f;
			SonderAudio.Alert(0.32f);
			LogPing("near", null, Mathf.Sqrt(num).ToString("0.0") + "m");
		}
		_wasNear = flag;
	}

	private void UpdateRoomScan(List<VRRig> rigs)
	{
		if (!RoomScan || Time.time < _nextRoomScan)
		{
			return;
		}
		_nextRoomScan = Time.time + 15f;
		List<string> list = new List<string>();
		for (int i = 0; i < rigs.Count; i++)
		{
			VRRig val = rigs[i];
			if (!((Object)(object)val == (Object)null) && val.Creator != null)
			{
				list.Add(val.Creator.ActorNumber + "|" + ExtraUtil.Nickname(val) + "|" + (val.Creator.Platform ?? "?") + "|" + (val.Creator.MothershipId ?? "?"));
			}
		}
		list.Sort(StringComparer.Ordinal);
		_scanBuilder.Length = 0;
		for (int j = 0; j < list.Count; j++)
		{
			if (j > 0)
			{
				_scanBuilder.Append(';');
			}
			_scanBuilder.Append(list[j]);
		}
		string text = _scanBuilder.ToString();
		if (text == _lastRoomSignature)
		{
			return;
		}
		_lastRoomSignature = text;
		string text2 = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + text;
		RuntimeLog.Write("room scan " + list.Count + " players");
		try
		{
			File.AppendAllText(Path.Combine(Paths.ConfigPath, "Sonder.roomscan.log"), text2 + Environment.NewLine);
		}
		catch (Exception ex)
		{
			RuntimeLog.Write("room scan write failed: " + ex.Message);
		}
	}

	private void LogPing(string kind, VRRig rig, string detail)
	{
		if (!PingHistory)
		{
			return;
		}
		string text = (((Object)(object)rig != (Object)null) ? ExtraUtil.Nickname(rig) : "-");
		string text2 = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + kind + " " + text + " " + detail;
		RuntimeLog.Write("alert " + kind + " " + text);
		try
		{
			File.AppendAllText(Path.Combine(Paths.ConfigPath, "Sonder.pings.log"), text2 + Environment.NewLine);
		}
		catch (Exception ex)
		{
			RuntimeLog.Write("ping log write failed: " + ex.Message);
		}
	}

	private void LoadWatchlist()
	{
		_watchTerms.Clear();
		string path = Path.Combine(Paths.ConfigPath, "Sonder.watchlist");
		try
		{
			if (!File.Exists(path))
			{
				File.WriteAllText(path, "# Sonder watchlist" + Environment.NewLine + "# One substring per line. Names are compared lower-case." + Environment.NewLine + "moderator" + Environment.NewLine + "administrator" + Environment.NewLine + "admin" + Environment.NewLine + "staff" + Environment.NewLine + "official" + Environment.NewLine + "owner" + Environment.NewLine + "anotheraxiom" + Environment.NewLine);
			}
			string[] array = File.ReadAllLines(path);
			for (int i = 0; i < array.Length; i++)
			{
				string text = array[i].Trim();
				if (text.Length != 0 && !text.StartsWith("#", StringComparison.Ordinal))
				{
					_watchTerms.Add(text.ToLowerInvariant());
				}
			}
		}
		catch (Exception ex)
		{
			RuntimeLog.Write("watchlist load failed: " + ex.Message);
		}
	}
}
