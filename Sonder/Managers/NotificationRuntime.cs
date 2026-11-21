using System.Collections.Generic;
using ExitGames.Client.Photon;
using GorillaNotifications.Core;
using Photon.Pun;
using Photon.Realtime;
using Sonder.Core;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Sonder.Managers;

public class NotificationRuntime : MonoBehaviour, IInRoomCallbacks
{
	private static readonly List<NotificationEntry> _activeEntries = new List<NotificationEntry>();
	private static readonly Queue<(string msg, float delay)> _pending = new Queue<(string, float)>();

	public static NotificationRuntime Instance { get; private set; }

	private void Awake()
	{
		Instance = this;
		PhotonNetwork.AddCallbackTarget((object)this);
		Object.DontDestroyOnLoad((Object)(object)gameObject);
	}

	private void Update()
	{
		TryEnsureController();
		FlushPending();
	}

	private void OnDestroy()
	{
		if ((Object)(object)Instance == (Object)(object)this)
		{
			Instance = null;
		}
		try { PhotonNetwork.RemoveCallbackTarget((object)this); } catch { }
	}

	private static void TryEnsureController()
	{
		try
		{
			if ((Object)(object)NotificationController.Instance != (Object)null) return;
			// If GorillaNotifications.Plugin is present in assembly it will add the controller on spawn;
			// also try to attach if GTPlayer is ready.
			var gtPlayerType = typeof(GorillaLocomotion.GTPlayer);
			var instProp = gtPlayerType.GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
			var player = instProp != null ? instProp.GetValue(null, null) as Component : null;
			if ((Object)(object)player == (Object)null) return;
			var head = player.GetType().GetField("headCollider", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)?.GetValue(player) as Component;
			// Only add when head/camera context is likely ready; NotificationController.Start expects headCollider.
			if ((Object)(object)head == (Object)null) return;
			GameObject host = new GameObject("Sonder_GorillaNotifications");
			Object.DontDestroyOnLoad((Object)(object)host);
			host.AddComponent<NotificationController>();
		}
		catch { }
	}

	private static float GetDelay()
	{
		try { return SonderSettings.NotiDelaySeconds; } catch { return 3f; }
	}

	public static void Show(string message)
	{
		if (string.IsNullOrEmpty(message)) return;
		float delay = GetDelay();
		try
		{
			TryEnsureController();
			if ((Object)(object)NotificationController.Instance != (Object)null)
			{
				var entry = NotificationController.SendNotification("Sonder", message, delay);
				if (entry != null)
				{
					_activeEntries.Add(entry);
					CleanupDestroyedEntries();
				}
				return;
			}
		}
		catch { }
		// queue until controller ready
		if (_pending.Count < 64) _pending.Enqueue((message, delay));
	}

	private static void FlushPending()
	{
		if ((Object)(object)NotificationController.Instance == (Object)null) return;
		int guard = 16;
		while (_pending.Count > 0 && guard-- > 0)
		{
			var item = _pending.Dequeue();
			try
			{
				var entry = NotificationController.SendNotification("Sonder", item.msg, item.delay);
				if (entry != null) _activeEntries.Add(entry);
			}
			catch { }
		}
		CleanupDestroyedEntries();
	}

	private static void CleanupDestroyedEntries()
	{
		for (int i = _activeEntries.Count - 1; i >= 0; i--)
		{
			try
			{
				if (_activeEntries[i] == null || _activeEntries[i].HasBeenDestroyed)
					_activeEntries.RemoveAt(i);
			}
			catch { _activeEntries.RemoveAt(i); }
		}
	}

	public static void ClearNow()
	{
		try { _pending.Clear(); } catch { }
		try
		{
			CleanupDestroyedEntries();
			for (int i = _activeEntries.Count - 1; i >= 0; i--)
			{
				try { _activeEntries[i].RemoveNotification(); } catch { }
			}
			_activeEntries.Clear();
		}
		catch { }
	}

	public void OnPlayerEnteredRoom(Player newPlayer)
	{
		Show((newPlayer != null) ? (newPlayer.NickName + " joined the lobby") : "a player joined the lobby");
	}

	public void OnPlayerLeftRoom(Player otherPlayer)
	{
		Show((otherPlayer != null) ? (otherPlayer.NickName + " left the lobby") : "a player left the lobby");
	}

	public void OnJoinedRoom()
	{
		Show("you joined the lobby");
	}

	public void OnLeftRoom()
	{
		Show("you left the lobby");
	}

	public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged) { }
	public void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps) { }
	public void OnMasterClientSwitched(Player newMasterClient) { }
}
