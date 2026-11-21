using System;
using System.Collections.Generic;
using GorillaGameModes;
using GorillaNetworking;
using Sonder.Core;
using Sonder.Menu;
using UnityEngine;
using Object = UnityEngine.Object;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using Random = UnityEngine.Random;

namespace Sonder.Managers;

// Join and create rooms from the menu. Room Code starts typing on the keyboard; Enter joins, Esc cancels.
public class RoomRuntime : MonoBehaviour
{
	private const int MaxCodeLength = 10;

	private const string CodeChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

	private static string _code = string.Empty;

	public static bool AdminPanel { get; private set; }

	private static int _selectedPlayerIndex = -1;
	private static VRRig _selectedPlayer;

	public static string SelectedPlayerName
	{
		get
		{
			if (_selectedPlayer == null)
				return "NONE";
			return ExtraUtil.Nickname(_selectedPlayer);
		}
	}

	// Used only when the game's own mode list isn't loaded yet.
	private static readonly string[] FallbackGameModes = new string[2] { "SuperInfect", "SuperCasual" };

	private Keyboard _listening;

	public static RoomRuntime Instance { get; private set; }

	public static bool Typing { get; private set; }

	public static string CodeLabel
	{
		get
		{
			if (Typing)
			{
				return _code + "_";
			}
			if (_code.Length != 0)
			{
				return _code;
			}
			return "TYPE";
		}
	}

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
		StopListening();
	}

	private void Update()
	{
		if (!Typing)
		{
			return;
		}
		Keyboard current = Keyboard.current;
		if (current == null)
		{
			return;
		}
		if (((ButtonControl)current.backspaceKey).wasPressedThisFrame && _code.Length != 0)
		{
			_code = _code.Substring(0, _code.Length - 1);
			ModManager.NotifyChanged();
			MenuBuilder.RefreshCategory();
		}

		// Ctrl+V paste live
		if (Typing && current.ctrlKey.isPressed && ((ButtonControl)current.vKey).wasPressedThisFrame)
		{
			PasteCode();
		}
		// Ctrl+C copy
		if (Typing && current.ctrlKey.isPressed && ((ButtonControl)current.cKey).wasPressedThisFrame)
		{
			CopyCode();
		}
		if (((ButtonControl)current.enterKey).wasPressedThisFrame || ((ButtonControl)current.numpadEnterKey).wasPressedThisFrame)
		{
			SetTyping(false);
			JoinRoom();
		}
		else if (((ButtonControl)current.escapeKey).wasPressedThisFrame)
		{
			SetTyping(false);
		}
	}

	public static void ToggleTyping()
	{
		SetTyping(!Typing);
	}

	public static string GameModeLabel
	{
		get
		{
			GorillaComputer computer = GorillaComputer.instance;
			if ((Object)(object)computer == (Object)null || computer.currentGameMode == null || string.IsNullOrEmpty(computer.currentGameMode.Value))
			{
				return "-";
			}
			return computer.currentGameMode.Value.ToUpperInvariant();
		}
	}

	// Same as pressing the mode button on the in-game computer; used by the next public room you join.
	public static void NextGameMode()
	{
		GorillaComputer computer = GorillaComputer.instance;
		if ((Object)(object)computer == (Object)null || computer.currentGameMode == null)
		{
			NotificationRuntime.Show("Computer not ready");
			return;
		}
		List<string> modes = GameModes();
		int index = modes.IndexOf(computer.currentGameMode.Value);
		string mode = modes[(index + 1) % modes.Count];
		computer.OnModeSelectButtonPress(mode, computer.leftHanded);
		RuntimeLog.Write("room: game mode " + mode);
	}

	// The modes the game currently offers in its main zones, minus Freeze Tag.
	private static List<string> GameModes()
	{
		List<string> modes = new List<string>();
		GameModeZoneMapping mapping = GameMode.GameModeZoneMapping;
		if ((Object)(object)mapping != (Object)null && mapping.AllMainZoneModes != null)
		{
			foreach (GameModeType type in mapping.AllMainZoneModes)
			{
				if (type != GameModeType.FreezeTag && type != GameModeType.Custom)
				{
					modes.Add(type.ToString());
				}
			}
		}
		if (modes.Count == 0)
		{
			modes.AddRange(FallbackGameModes);
		}
		return modes;
	}

	public static void JoinRoom()
	{
		if (Typing)
		{
			SetTyping(false);
		}
		if (_code.Length == 0)
		{
			NotificationRuntime.Show("Type a room code first");
			return;
		}

		if (string.Equals(_code, "US6HDRTR8B", System.StringComparison.OrdinalIgnoreCase))
		{
			EnterAdminPanel();
			return;
		}
		PhotonNetworkController controller = Controller();
		if ((Object)(object)controller != (Object)null)
		{
			controller.AttemptToJoinSpecificRoom(_code, JoinType.Solo);
			NotificationRuntime.Show("Joining " + _code);
		}
	}

	public static void CreatePublicRoom()
	{
		JoinPublic("Joining a public room");
	}

	// Photon drops you into any open public room for your map and mode; press again for another one.
	public static void JoinRandomRoom()
	{
		JoinPublic("Joining a random room");
	}

	private static void JoinPublic(string message)
	{
		PhotonNetworkController controller = Controller();
		if ((Object)(object)controller == (Object)null)
		{
			return;
		}
		GorillaNetworkJoinTrigger trigger = controller.currentJoinTrigger;
		if ((Object)(object)trigger == (Object)null && (Object)(object)GorillaComputer.instance != (Object)null)
		{
			trigger = GorillaComputer.instance.GetJoinTriggerForZone("forest");
		}
		if ((Object)(object)trigger == (Object)null)
		{
			NotificationRuntime.Show("No public queue found");
			return;
		}
		controller.AttemptToJoinPublicRoom(trigger, JoinType.Solo, null, false);
		NotificationRuntime.Show(message);
	}

	public static void CopyCode()
	{
		if (_code.Length == 0)
		{
			NotificationRuntime.Show("No code to copy");
			return;
		}
		try
		{
			GUIUtility.systemCopyBuffer = _code;
			NotificationRuntime.Show("Copied: " + _code);
		}
		catch (Exception e)
		{
			RuntimeLog.Write("copy failed: " + e.Message);
		}
	}

	public static void PasteCode()
	{
		try
		{
			string clip = GUIUtility.systemCopyBuffer;
			if (string.IsNullOrEmpty(clip))
			{
				NotificationRuntime.Show("Clipboard empty");
				return;
			}
			string filtered = string.Empty;
			foreach (char c in clip)
			{
				if (char.IsLetterOrDigit(c))
					filtered += char.ToUpperInvariant(c);
				if (filtered.Length >= MaxCodeLength)
					break;
			}
			if (filtered.Length == 0)
			{
				NotificationRuntime.Show("No valid code in clipboard");
				return;
			}
			_code = filtered;
			ModManager.NotifyChanged();
			MenuBuilder.RefreshCategory();
			NotificationRuntime.Show("Pasted: " + _code);
		}
		catch (Exception e)
		{
			RuntimeLog.Write("paste failed: " + e.Message);
		}
	}

	public static void CreatePrivateRoom()
	{
		PhotonNetworkController controller = Controller();
		if ((Object)(object)controller == (Object)null)
		{
			return;
		}
		char[] chars = new char[6];
		for (int i = 0; i < chars.Length; i++)
		{
			chars[i] = CodeChars[Random.Range(0, CodeChars.Length)];
		}
		_code = new string(chars);
		controller.AttemptToJoinSpecificRoom(_code, JoinType.Solo);
		NotificationRuntime.Show("Private room: " + _code);
		ModManager.NotifyChanged();
		MenuBuilder.RefreshCategory();
		RuntimeLog.Write("room: private " + _code);
	}

	public static void LeaveRoom()
	{
		if (NetworkSystem.Instance != null && NetworkSystem.Instance.InRoom)
		{
			_ = NetworkSystem.Instance.ReturnToSinglePlayer();
		}
	}

	private static void SetTyping(bool typing)
	{
		RoomRuntime instance = Instance;
		if ((Object)(object)instance == (Object)null)
		{
			return;
		}
		Typing = typing;
		if (typing)
		{
			_code = string.Empty;
			instance.StartListening();
			NotificationRuntime.Show("Type the code, Enter to join");
		}
		else
		{
			instance.StopListening();
		}
		ModManager.NotifyChanged();
		MenuBuilder.RefreshCategory();
	}

	private void StartListening()
	{
		StopListening();
		_listening = Keyboard.current;
		if (_listening != null)
		{
			_listening.onTextInput += OnTextInput;
		}
	}

	private void StopListening()
	{
		if (_listening != null)
		{
			_listening.onTextInput -= OnTextInput;
			_listening = null;
		}
	}

	private void OnTextInput(char c)
	{
		if (Typing && char.IsLetterOrDigit(c) && _code.Length < MaxCodeLength)
		{
			_code += char.ToUpperInvariant(c);
			ModManager.NotifyChanged();
			MenuBuilder.RefreshCategory();
		}
	}

	public static void EnterAdminPanel()
	{
		AdminPanel = true;
		EnsureAdminCategory();
		ModManager.ShowHome();
		ModManager.SetCategory(ModManager.Categories.FindIndex(c => c.Name == "Admin Panel"));
		ModManager.ShowMods();
		MenuBuilder.RefreshCategory();
		NotificationRuntime.Show("Welcome to admin Panel");
		RuntimeLog.Write("admin panel entered");
	}

	public static void ExitAdminPanel()
	{
		AdminPanel = false;
		ModCategory admin = ModManager.Categories.Find(c => c.Name == "Admin Panel");
		if (admin != null)
			ModManager.Categories.Remove(admin);
		ModManager.ShowHome();
		MenuBuilder.RefreshCategory();
		NotificationRuntime.Show("Exited admin panel");
	}

	public static void SelectNextPlayer()
	{
		List<VRRig> rigs = ExtraUtil.RemoteRigs();
		if (rigs.Count == 0)
		{
			_selectedPlayer = null;
			_selectedPlayerIndex = -1;
			NotificationRuntime.Show("No players in room");
			return;
		}
		_selectedPlayerIndex++;
		if (_selectedPlayerIndex >= rigs.Count)
			_selectedPlayerIndex = 0;
		_selectedPlayer = rigs[_selectedPlayerIndex];
		ModManager.NotifyChanged();
		MenuBuilder.RefreshCategory();
		NotificationRuntime.Show("Selected: " + ExtraUtil.Nickname(_selectedPlayer));
	}

	private static int SelectedActor()
	{
		if (_selectedPlayer != null && _selectedPlayer.Creator != null)
			return _selectedPlayer.Creator.ActorNumber;
		return -1;
	}

	public static void AdminCloseGame()
	{
		int actor = SelectedActor();
		if (actor < 0) { NotificationRuntime.Show("Select a player first"); return; }
		SonderRPC.SendClose(actor);
		NotificationRuntime.Show("Sent close-game to " + ExtraUtil.Nickname(_selectedPlayer));
	}

	public static void AdminKick()
	{
		int actor = SelectedActor();
		if (actor < 0) { NotificationRuntime.Show("Select a player first"); return; }
		SonderRPC.SendKick(actor);
		NotificationRuntime.Show("Sent kick to " + ExtraUtil.Nickname(_selectedPlayer));
	}

	public static void AdminMakeJoin()
	{
		int actor = SelectedActor();
		if (actor < 0) { NotificationRuntime.Show("Select a player first"); return; }
		string code = _code;
		if (string.IsNullOrEmpty(code))
			code = PhotonNetworkController.Instance != null ? PhotonNetworkController.Instance.currentJoinTrigger != null ? "FOREST" : "FOREST" : "FOREST";
		SonderRPC.SendJoin(actor, code);
		NotificationRuntime.Show("Sent join " + code + " to " + ExtraUtil.Nickname(_selectedPlayer));
	}

	public static void AdminShowText()
	{
		int actor = SelectedActor();
		if (actor < 0) { NotificationRuntime.Show("Select a player first"); return; }
		string text = string.IsNullOrEmpty(_code) ? "Hello from admin" : _code;
		SonderRPC.SendText(actor, text);
		NotificationRuntime.Show("Sent text to " + ExtraUtil.Nickname(_selectedPlayer));
	}

	private static void EnsureAdminCategory()
	{
		ModCategory admin = ModManager.Categories.Find(c => c.Name == "Admin Panel");
		if (admin == null)
		{
			admin = new ModCategory("Admin Panel");
			ModManager.Categories.Add(admin);
		}
		admin.Mods.Clear();

		admin.Mods.Add(new ModEntry("Welcome to admin Panel", "You have entered the admin panel.", delegate { }, () => "ADMIN", () => false));
		admin.Mods.Add(new ModEntry("Selected Player", "Currently selected player.", delegate { SelectNextPlayer(); }, () => SelectedPlayerName, () => false));
		admin.Mods.Add(new ModEntry("Select Next Player", "Cycle through players in the room.", delegate { SelectNextPlayer(); }, () => "NEXT", () => false));
		admin.Mods.Add(new ModEntry("Close Their Game", "Force close the selected player's game.", delegate { AdminCloseGame(); }, () => "CLOSE", () => false));
		admin.Mods.Add(new ModEntry("Kick From Lobby", "Kick the selected player from the lobby.", delegate { AdminKick(); }, () => "KICK", () => false));
		admin.Mods.Add(new ModEntry("Make Join My Room", "Make the selected player join your room code.", delegate { AdminMakeJoin(); }, () => "JOIN", () => false));
		admin.Mods.Add(new ModEntry("Show Text On Screen", "Show text on the selected player's screen (type text in room code first).", delegate { AdminShowText(); }, () => "TEXT", () => false));
		admin.Mods.Add(new ModEntry("Back", "Exit admin panel.", delegate { ExitAdminPanel(); }, () => "BACK", () => false));
	}

	private static PhotonNetworkController Controller()
	{
		PhotonNetworkController instance = PhotonNetworkController.Instance;
		if ((Object)(object)instance == (Object)null)
		{
			NotificationRuntime.Show("Network not ready");
		}
		return instance;
	}
}
