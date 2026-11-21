using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Sonder.Core;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Sonder.Managers;

public class SoundRuntime : MonoBehaviour
{
	private sealed class MuteState
	{
		public float Volume;

		public bool Muted;
	}

	private static readonly FieldInfo TaggedRigField = typeof(GorillaTagManager).GetField("taggedRig", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

	private static readonly FieldInfo TaggingRigField = typeof(GorillaTagManager).GetField("taggingRig", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

	private static readonly FieldInfo VoiceAudioField = typeof(VRRig).GetField("voiceAudio", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

	private static readonly FieldInfo VoiceViewField = typeof(VRRig).GetField("myPhotonVoiceView", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

	private readonly Dictionary<int, MuteState> _muted = new Dictionary<int, MuteState>();

	private readonly List<SonderVoiceFilter> _voiceFilters = new List<SonderVoiceFilter>();

	private VRRig _lastTagged;

	private VRRig _lastTagger;

	private float _nextTagSound;

	private float _nextDrip;

	private float _nextMuteCheck;

	private VRRig _voiceRig;

	public static SoundRuntime Instance { get; private set; }

	public bool TagHorn { get; set; }

	public bool AmbientDrip { get; set; }

	public bool VoiceChanger { get; set; }

	public bool MuteOthers { get; set; }

	public bool TagEcho { get; set; }

	private void Awake()
	{
		Instance = this;
	}

	private void OnDestroy()
	{
		if ((Object)(object)Instance == (Object)(object)this)
		{
			Instance = null;
		}
		RemoveVoiceFilters();
		RestoreMutes();
	}

	private void Update()
	{
		WatchTags();
		ApplyAmbientDrip();
		ApplyVoiceChanger();
		ApplyMuteOthers();
	}

	private void WatchTags()
	{
		GorillaTagManager tagManager = ExtraUtil.TagManager;
		if ((Object)(object)tagManager == (Object)null)
		{
			_lastTagged = null;
			_lastTagger = null;
			return;
		}
		object obj;
		if (!(TaggedRigField != null))
		{
			obj = null;
		}
		else
		{
			object value = TaggedRigField.GetValue(tagManager);
			obj = ((value is VRRig) ? value : null);
		}
		VRRig val = (VRRig)obj;
		object obj2;
		if (!(TaggingRigField != null))
		{
			obj2 = null;
		}
		else
		{
			object value2 = TaggingRigField.GetValue(tagManager);
			obj2 = ((value2 is VRRig) ? value2 : null);
		}
		VRRig val2 = (VRRig)obj2;
		if ((Object)(object)val == (Object)null)
		{
			_lastTagged = null;
			_lastTagger = null;
		}
		else
		{
			if ((Object)(object)val == (Object)(object)_lastTagged && (Object)(object)val2 == (Object)(object)_lastTagger)
			{
				return;
			}
			_lastTagged = val;
			_lastTagger = val2;
			if (!(Time.time < _nextTagSound))
			{
				_nextTagSound = Time.time + 0.12f;
				if (TagHorn)
				{
					SonderAudio.Horn(0.85f);
				}
				if (TagEcho)
				{
					((MonoBehaviour)this).StartCoroutine(EchoRoutine());
				}
			}
		}
	}

	private IEnumerator EchoRoutine()
	{
		yield return (object)new WaitForSeconds(0.35f);
		if (TagEcho)
		{
			SonderAudio.Horn(0.38f, 0.74f);
		}
	}

	private void ApplyAmbientDrip()
	{
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		if (!AmbientDrip || Time.time < _nextDrip)
		{
			return;
		}
		_nextDrip = Time.time + Random.Range(4.5f, 11f);
		Transform val = ExtraUtil.Head();
		if (!((Object)(object)val == (Object)null))
		{
			Vector3 val2 = default(Vector3);
			val2 = new Vector3(Random.Range(-1f, 1f), Random.Range(-0.25f, 0.25f), Random.Range(-1f, 1f));
			RaycastHit val3 = default(RaycastHit);
			if (!(val2.sqrMagnitude < 0.01f) && Physics.Raycast(val.position, val2.normalized, out val3, 9f, -1, (QueryTriggerInteraction)1))
			{
				SonderAudio.Drip(Random.Range(0.8f, 1.3f));
			}
		}
	}

	private void ApplyVoiceChanger()
	{
		if (!VoiceChanger)
		{
			RemoveVoiceFilters();
			return;
		}
		VRRig localRig = VRRig.LocalRig;
		if ((Object)(object)localRig == (Object)null)
		{
			return;
		}
		if ((Object)(object)_voiceRig != (Object)(object)localRig)
		{
			RemoveVoiceFilters();
			_voiceRig = localRig;
			AddFilter(((Component)localRig).gameObject);
			AudioSource val = Voice(localRig);
			if ((Object)(object)val != (Object)null)
			{
				AddFilter(((Component)val).gameObject);
			}
			Component val2 = VoiceView(localRig);
			if ((Object)(object)val2 != (Object)null)
			{
				AddFilter(val2.gameObject);
			}
		}
		for (int i = 0; i < _voiceFilters.Count; i++)
		{
			if ((Object)(object)_voiceFilters[i] != (Object)null)
			{
				_voiceFilters[i].Active = true;
			}
		}
	}

	private void AddFilter(GameObject target)
	{
		if (!((Object)(object)target == (Object)null) && !((Object)(object)target.GetComponent<SonderVoiceFilter>() != (Object)null))
		{
			SonderVoiceFilter sonderVoiceFilter = target.AddComponent<SonderVoiceFilter>();
			sonderVoiceFilter.Active = true;
			_voiceFilters.Add(sonderVoiceFilter);
		}
	}

	private void RemoveVoiceFilters()
	{
		for (int i = 0; i < _voiceFilters.Count; i++)
		{
			if ((Object)(object)_voiceFilters[i] != (Object)null)
			{
				Object.Destroy((Object)(object)_voiceFilters[i]);
			}
		}
		_voiceFilters.Clear();
		_voiceRig = null;
	}

	private void ApplyMuteOthers()
	{
		if (!MuteOthers)
		{
			RestoreMutes();
		}
		else
		{
			if (Time.time < _nextMuteCheck)
			{
				return;
			}
			_nextMuteCheck = Time.time + 0.5f;
			List<VRRig> list = ExtraUtil.RemoteRigs();
			for (int i = 0; i < list.Count; i++)
			{
				VRRig val = list[i];
				AudioSource val2 = Voice(val);
				if (!((Object)(object)val == (Object)null) && val.Creator != null && !((Object)(object)val2 == (Object)null))
				{
					int actorNumber = val.Creator.ActorNumber;
					if (!_muted.TryGetValue(actorNumber, out var _))
					{
						_muted[actorNumber] = new MuteState
						{
							Volume = val2.volume,
							Muted = val2.mute
						};
					}
					val2.volume = 0f;
					val2.mute = true;
				}
			}
		}
	}

	private void RestoreMutes()
	{
		if (_muted.Count == 0)
		{
			return;
		}
		List<VRRig> list = ExtraUtil.RemoteRigs();
		for (int i = 0; i < list.Count; i++)
		{
			VRRig val = list[i];
			AudioSource val2 = Voice(val);
			if (!((Object)(object)val == (Object)null) && val.Creator != null && !((Object)(object)val2 == (Object)null) && _muted.TryGetValue(val.Creator.ActorNumber, out var value))
			{
				val2.volume = value.Volume;
				val2.mute = value.Muted;
			}
		}
		_muted.Clear();
	}

	private static AudioSource Voice(VRRig rig)
	{
		if ((Object)(object)rig == (Object)null || VoiceAudioField == null)
		{
			return null;
		}
		object value = VoiceAudioField.GetValue(rig);
		return (AudioSource)((value is AudioSource) ? value : null);
	}

	private static Component VoiceView(VRRig rig)
	{
		if ((Object)(object)rig == (Object)null || VoiceViewField == null)
		{
			return null;
		}
		object value = VoiceViewField.GetValue(rig);
		return (Component)((value is Component) ? value : null);
	}
}
