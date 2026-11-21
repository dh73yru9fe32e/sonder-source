using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BepInEx;
using Sonder.Core;
using Sonder.Menu;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Sonder.Managers;

public static class ModManager
{
	public const int ModsPerPage = 4;

	public static readonly List<ModCategory> Categories = new List<ModCategory>();

	public static readonly List<ModCategory> SettingsSubCategories = new List<ModCategory>();

	public static int CurrentCategoryIndex { get; private set; }

	public static bool InHome { get; private set; }

	public static bool InSettingsHome { get; private set; }

	public static int SettingsSubIndex { get; private set; } = -1;

	public static int ModPage { get; private set; }

	public static ModCategory CurrentCategory => Categories[CurrentCategoryIndex];

	public static ModCategory CurrentSettingsSub
	{
		get
		{
			if (SettingsSubIndex < 0 || SettingsSubIndex >= SettingsSubCategories.Count)
			{
				return null;
			}
			return SettingsSubCategories[SettingsSubIndex];
		}
	}

	public static ModCategory CurrentDisplayCategory
	{
		get
		{
			if (InSettingsHome && SettingsSubIndex >= 0)
			{
				return CurrentSettingsSub;
			}
			if (Categories.Count == 0)
			{
				return null;
			}
			return Categories[CurrentCategoryIndex];
		}
	}

	public static int ModPageCount
	{
		get
		{
			if (InSettingsHome && SettingsSubIndex >= 0)
			{
				ModCategory currentSettingsSub = CurrentSettingsSub;
				if (currentSettingsSub == null)
				{
					return 1;
				}
				return (currentSettingsSub.Mods.Count + 4 - 1) / 4;
			}
			if (Categories.Count == 0)
			{
				return 1;
			}
			return (CurrentCategory.Mods.Count + 4 - 1) / 4;
		}
	}

	public static event Action Changed;

	public static void Initialize()
	{
		if (Categories.Count > 0)
		{
			return;
		}
		ModCategory modCategory = new ModCategory("Rig");
		modCategory.Mods.Add(BuildToggle("Ghost Monke (A)", "Press A: your body freezes in place for everyone while you move freely."));
		modCategory.Mods.Add(BuildToggle("Invis Monke (X)", "Press X: your body vanishes for everyone else."));
		modCategory.Mods.Add(BuildToggle("Spaz Monke (B)", "Hold B: head and body spaz out."));
		modCategory.Mods.Add(BuildToggle("Wacky Monke (B)", "Hold B: body wiggles, hops and head-bobs."));
		modCategory.Mods.Add(BuildToggle("Rig Gun", "Hold trigger: body flies to the beam end."));
		modCategory.Mods.Add(BuildToggle("Confuse Player Gun", "Their rig spams around them."));
		modCategory.Mods.Add(BuildToggle("Chase Player Gun", "Your body chases the locked player."));
		modCategory.Mods.Add(BuildToggle("Fast Chase Player Gun", "Chase at double speed."));
		modCategory.Mods.Add(BuildToggle("Orbit Player Gun", "Your body circles the locked player."));
		modCategory.Mods.Add(BuildToggle("Fake Lag", "Freezes the rig on a 2s cycle."));
		modCategory.Mods.Add(BuildToggle("Fast Fake Lag", "Freezes on a 1s cycle."));
		modCategory.Mods.Add(new ModEntry("Fix Head", "Resets every head + rig transform.", delegate
		{
			RigBank.FixHead();
		}, () => "FIX", Never));
		modCategory.Mods.Add(BuildToggle("Head Spin V1", "Head spins at 180 deg/s."));
		modCategory.Mods.Add(BuildToggle("Head Spin V2", "Head spins at 420 deg/s."));
		modCategory.Mods.Add(BuildToggle("Head Spin V3", "Head spins at 900 deg/s."));
		modCategory.Mods.Add(BuildToggle("Fake Grey Screen (RJ)", "Local velocity zero + hands pinned."));
		modCategory.Mods.Add(BuildToggle("Backwards Head", "Head faces behind you."));
		modCategory.Mods.Add(BuildToggle("Backwards Body", "Body half-turned to face behind you."));
		modCategory.Mods.Add(BuildToggle("Backwards Rig", "Whole rig faces backwards."));
		modCategory.Mods.Add(BuildToggle("Upside Down Head", "Head flipped 180."));
		modCategory.Mods.Add(BuildToggle("Upside Down Body", "Body upside down."));
		modCategory.Mods.Add(BuildToggle("Upside Down Rig", "Whole rig upside down."));
		modCategory.Mods.Add(BuildToggle("Forwards Body", "Body pitched forward."));
		modCategory.Mods.Add(BuildToggle("Forwards Rig", "Locks the rig upright, facing forward."));
		modCategory.Mods.Add(BuildToggle("Crawling Body", "Body pitched 70 degrees."));
		modCategory.Mods.Add(BuildToggle("Crawling Rig", "Whole rig pitched 70 degrees."));
		modCategory.Mods.Add(BuildToggle("Spaz Head", "Random head rotation."));
		modCategory.Mods.Add(BuildToggle("Spaz Body", "Random body jitter."));
		modCategory.Mods.Add(BuildToggle("Spaz Rig", "Random whole-rig rotation."));
		modCategory.Mods.Add(BuildToggle("Fake Full Body Tracking", "Subtle head + hand sway."));
		modCategory.Mods.Add(BuildToggle("Spin Bot V1", "Rig spins at 120 deg/s."));
		modCategory.Mods.Add(BuildToggle("Spin Bot V2", "Rig spins at 300 deg/s."));
		modCategory.Mods.Add(BuildToggle("Spin Bot V3", "Rig spins at 560 deg/s."));
		modCategory.Mods.Add(BuildToggle("Spin Bot V4", "Rig spins at 820 deg/s."));
		modCategory.Mods.Add(BuildToggle("Spin Bot V5", "Rig spins at 1200 deg/s."));
		modCategory.Mods.Add(BuildToggle("Upside Down Spin Bot", "Flip + 400 deg/s yaw."));
		modCategory.Mods.Add(BuildToggle("Full Spin Bot", "3-axis tumble."));
		modCategory.Mods.Add(BuildToggle("Backflip (RJ)", "Press right grip: 360 backflip."));
		modCategory.Mods.Add(BuildToggle("Frontflip (LJ)", "Press left grip: 360 frontflip."));
		Categories.Add(modCategory);
		ModCategory modCategory2 = new ModCategory("Movement");
		string[] array = new string[29]
		{
			"Fly", "Speed Boost", "High Jump", "Long Arms", "Moon Gravity", "Rainbow Fur", "Joystick Fly", "Dash", "Zero Gravity", "Low Gravity",
			"Bouncy", "Slingshot Fly", "Feather Fall", "Trigger Fly", "Hand Fly", "WASD Fly", "Bark Fly", "Bird Fly", "Reverse Velocity", "High Gravity",
			"Ground Helper", "Bunny Hop", "Strafe", "Slide Control", "No Tag Freeze", "Drive", "Iron Man", "Flick Jump", "Up And Down"
		};
		string[] array2 = new string[29]
		{
			"Hold A to fly where you look.", "Raises velocity limit.", "Multiplies jump force.", "Extends arm reach.", "Low gravity.", "Rainbow fur.", "Fly with joystick.", "Dash forward.", "Zero gravity.", "Low gravity push.",
			"Bouncy body.", "Slingshot fly.", "Feather fall.", "Trigger fly.", "Hand fly.", "WASD fly.", "Bark fly.", "Bird fly.", "Reverse velocity.", "High gravity.",
			"Ground helper.", "Bunny hop.", "Strafe.", "Slide control.", "No tag freeze.", "Drive.", "Iron man.", "Flick jump.", "Up and down."
		};
		for (int num = 0; num < array.Length; num++)
		{
			string description = ((num < array2.Length) ? array2[num] : string.Empty);
			modCategory2.Mods.Add(BuildToggle(array[num], description));
		}
		modCategory2.Mods.Insert(0, new ModEntry("Fly Speed", "Speed for every fly mod and Super Monke.", ModRuntime.CycleFlySpeed, () => ModRuntime.FlySpeedLabel, Pulse));
		Categories.Add(modCategory2);
		ModCategory modCategoryPlus = new ModCategory("Movement+");
		modCategoryPlus.Mods.Add(BuildToggle("Platforms", "Hold grip: a platform appears under that hand."));
		modCategoryPlus.Mods.Add(BuildToggle("Noclip", "Walk through the map. Pair with a fly mod."));
		modCategoryPlus.Mods.Add(BuildToggle("Teleport Pointer", "Hold right grip to aim, pull trigger to teleport."));
		modCategoryPlus.Mods.Add(BuildToggle("Checkpoint", "X saves your spot, Y takes you back."));
		modCategoryPlus.Mods.Add(BuildToggle("Wall Walk", "Hold grip on a wall: gravity pulls you into it."));
		modCategoryPlus.Mods.Add(BuildToggle("Super Monke", "A flies where you look, B toggles gravity."));
		modCategoryPlus.Mods.Add(BuildToggle("Pull Mod", "Small speed kick every time a hand leaves a surface."));
		modCategoryPlus.Mods.Add(BuildToggle("Portal Gun", "Shoots blue/orange portals; walk into one to teleport to the other."));
		modCategoryPlus.Mods.Add(BuildToggle("Grip Jump", "Press B for a straight-up launch."));
		Categories.Add(modCategoryPlus);
		ModCategory funCategory = new ModCategory("Fun");
		funCategory.Mods.Add(new ModEntry("Intro Redo", "Replays the Sonder intro screen.", delegate
		{
			SonderIntroUI.Play();
		}, () => "REPLAY", Never));
		Categories.Add(funCategory);
		ModCategory pcCategory = new ModCategory("PC");
		pcCategory.Mods.Add(BuildToggle("PC Button Clicker", "Click in-world buttons with the mouse. PC only."));
		Categories.Add(pcCategory);
		ModCategory visualCategory = new ModCategory("Visual");
		string[] visualMods = new string[]
		{
			"Arrow Pointer ESP",
			"Beacon ESP",
			"Bone ESP",
			"Capsule ESP",
			"Chams",
			"Cylinder ESP",
			"Diamond ESP",
			"Distance Tracers",
			"Double Hand Tracers",
			"ESP",
			"Floor Marker ESP",
			"Full Player Info",
			"Grip Indicator",
			"Grip Tracker",
			"Hand Comets",
			"Hand ESP",
			"Head Sphere ESP",
			"Head Tracers",
			"Lightning Hands",
			"Modder Tracers",
			"Neon Outline",
			"Pulse Chams",
			"Pulse Tracers",
			"Ring ESP",
			"Shadow Clone",
			"Show Distance",
			"Show Join Date All",
			"Show Platform",
			"Show Velocity",
			"Skeleton ESP V2",
			"Sparkler Hands",
			"Sphere ESP",
			"Thick Tracers",
			"Thin ESP",
			"Tracers"
		};
		for (int vi = 0; vi < visualMods.Length; vi++)
		{
			visualCategory.Mods.Add(BuildToggle(visualMods[vi], "Through-wall visual."));
		}
		Categories.Add(visualCategory);
		ModCategory modCategory3 = new ModCategory("Menu Settings");
		modCategory3.Mods.Add(new ModEntry("Accent Theme", "Cycles the accent colour.", SonderSettings.CycleAccent, () => SonderSettings.AccentLabel, Pulse));
		modCategory3.Mods.Add(new ModEntry("Menu Scale", "Resizes the Sonder menu.", SonderSettings.CycleScale, () => SonderSettings.ScaleLabel, Pulse));
		modCategory3.Mods.Add(new ModEntry("Menu Opacity", "Dims or brightens the panel.", SonderSettings.CycleOpacity, () => SonderSettings.OpacityLabel, Pulse));
		modCategory3.Mods.Add(new ModEntry("Animation Speed", "Scales open and close motion.", SonderSettings.CycleAnimation, () => SonderSettings.AnimationLabel, Pulse));
		modCategory3.Mods.Add(new ModEntry("Pointer Size", "Thickness of the aim dot.", SonderSettings.CyclePointer, () => SonderSettings.PointerLabel, Pulse));
		modCategory3.Mods.Add(new ModEntry("UI Sounds", "Toggles menu click sounds.", SonderSettings.ToggleSounds, () => SonderSettings.SoundsLabel, SoundsOn));
		modCategory3.Mods.Add(new ModEntry("Discord Status", "Shows Sonder as your Discord activity.", SonderSettings.ToggleDiscord, () => SonderSettings.DiscordLabel, DiscordOn));
		modCategory3.Mods.Add(new ModEntry("PC First Person", "Shows your eyes on the PC screen instead of the shoulder camera.", MenuManager.TogglePcFirstPerson, () => MenuManager.PcFirstPerson ? "ON" : "OFF", () => MenuManager.PcFirstPerson));
		modCategory3.Mods.Add(new ModEntry("Stump Dock", "Parks the menu over the Stump map.", MenuManager.ToggleDock, DockLabel, Pulse));
		modCategory3.Mods.Add(new ModEntry("Reset All", "Resets every Sonder setting.", SonderSettings.ResetAll, ResetLabel, Never));
		ModCategory modCategory4 = new ModCategory("GunLib Settings");
		modCategory4.Mods.Add(new ModEntry("Gun Colour", "Cycles the gun beam colour.", SonderSettings.CycleGunColor, () => SonderSettings.GunColorLabel, Pulse));
		modCategory4.Mods.Add(new ModEntry("Gun Beam Width", "Thickness of the gun beam.", SonderSettings.CycleGunBeam, () => SonderSettings.GunBeamLabel, Pulse));
		modCategory4.Mods.Add(new ModEntry("Gun Ball Size", "Size of the aim reticle.", SonderSettings.CycleGunBall, () => SonderSettings.GunBallLabel, Pulse));
		modCategory4.Mods.Add(new ModEntry("Gun Lock-On", "Toggles player lock-on.", delegate
		{
			if ((Object)(object)GunRuntime.Instance != (Object)null)
			{
				GunRuntime.Instance.GunLockOn = !GunRuntime.Instance.GunLockOn;
			}
		}, () => (!((Object)(object)GunRuntime.Instance != (Object)null) || !GunRuntime.Instance.GunLockOn) ? "OFF" : "ON", () => (Object)(object)GunRuntime.Instance != (Object)null && GunRuntime.Instance.GunLockOn));
		modCategory4.Mods.Add(new ModEntry("Gun Smooth Pointer", "Smooths the gun pointer.", delegate
		{
			if ((Object)(object)GunRuntime.Instance != (Object)null)
			{
				GunRuntime.Instance.GunSmoothPointer = !GunRuntime.Instance.GunSmoothPointer;
			}
		}, () => (!((Object)(object)GunRuntime.Instance != (Object)null) || !GunRuntime.Instance.GunSmoothPointer) ? "OFF" : "ON", () => (Object)(object)GunRuntime.Instance != (Object)null && GunRuntime.Instance.GunSmoothPointer));
		modCategory4.Mods.Add(new ModEntry("Gun Small Pointer", "Smaller aim reticle.", delegate
		{
			if ((Object)(object)GunRuntime.Instance != (Object)null)
			{
				GunRuntime.Instance.GunSmallPointer = !GunRuntime.Instance.GunSmallPointer;
			}
		}, () => (!((Object)(object)GunRuntime.Instance != (Object)null) || !GunRuntime.Instance.GunSmallPointer) ? "OFF" : "ON", () => (Object)(object)GunRuntime.Instance != (Object)null && GunRuntime.Instance.GunSmallPointer));
		modCategory4.Mods.Add(new ModEntry("Gun Hide Pointer", "Hides the aim reticle.", delegate
		{
			if ((Object)(object)GunRuntime.Instance != (Object)null)
			{
				GunRuntime.Instance.GunHidePointer = !GunRuntime.Instance.GunHidePointer;
			}
		}, () => (!((Object)(object)GunRuntime.Instance != (Object)null) || !GunRuntime.Instance.GunHidePointer) ? "OFF" : "ON", () => (Object)(object)GunRuntime.Instance != (Object)null && GunRuntime.Instance.GunHidePointer));
		modCategory4.Mods.Add(new ModEntry("Gun Hide Line", "Hides the beam line.", delegate
		{
			if ((Object)(object)GunRuntime.Instance != (Object)null)
			{
				GunRuntime.Instance.GunHideLine = !GunRuntime.Instance.GunHideLine;
			}
		}, () => (!((Object)(object)GunRuntime.Instance != (Object)null) || !GunRuntime.Instance.GunHideLine) ? "OFF" : "ON", () => (Object)(object)GunRuntime.Instance != (Object)null && GunRuntime.Instance.GunHideLine));
		modCategory4.Mods.Add(new ModEntry("Gun Swap Hand", "Use left hand for gun.", delegate
		{
			if ((Object)(object)GunRuntime.Instance != (Object)null)
			{
				GunRuntime.Instance.GunSwapHand = !GunRuntime.Instance.GunSwapHand;
			}
		}, () => (!((Object)(object)GunRuntime.Instance != (Object)null) || !GunRuntime.Instance.GunSwapHand) ? "OFF" : "ON", () => (Object)(object)GunRuntime.Instance != (Object)null && GunRuntime.Instance.GunSwapHand));
		modCategory4.Mods.Add(new ModEntry("Gun Gripless", "Aim without holding grip.", delegate
		{
			if ((Object)(object)GunRuntime.Instance != (Object)null)
			{
				GunRuntime.Instance.GunGripless = !GunRuntime.Instance.GunGripless;
			}
		}, () => (!((Object)(object)GunRuntime.Instance != (Object)null) || !GunRuntime.Instance.GunGripless) ? "OFF" : "ON", () => (Object)(object)GunRuntime.Instance != (Object)null && GunRuntime.Instance.GunGripless));
		modCategory4.Mods.Add(new ModEntry("Gun Triggerless", "Lock without trigger press.", delegate
		{
			if ((Object)(object)GunRuntime.Instance != (Object)null)
			{
				GunRuntime.Instance.GunTriggerless = !GunRuntime.Instance.GunTriggerless;
			}
		}, () => (!((Object)(object)GunRuntime.Instance != (Object)null) || !GunRuntime.Instance.GunTriggerless) ? "OFF" : "ON", () => (Object)(object)GunRuntime.Instance != (Object)null && GunRuntime.Instance.GunTriggerless));
		modCategory4.Mods.Add(new ModEntry("Gun Hard Lock", "Keep lock after release.", delegate
		{
			if ((Object)(object)GunRuntime.Instance != (Object)null)
			{
				GunRuntime.Instance.GunHardLock = !GunRuntime.Instance.GunHardLock;
			}
		}, () => (!((Object)(object)GunRuntime.Instance != (Object)null) || !GunRuntime.Instance.GunHardLock) ? "OFF" : "ON", () => (Object)(object)GunRuntime.Instance != (Object)null && GunRuntime.Instance.GunHardLock));
		modCategory4.Mods.Add(new ModEntry("Gun Sounds", "Lock notification sounds.", delegate
		{
			if ((Object)(object)GunRuntime.Instance != (Object)null)
			{
				GunRuntime.Instance.GunSounds = !GunRuntime.Instance.GunSounds;
			}
		}, () => (!((Object)(object)GunRuntime.Instance != (Object)null) || !GunRuntime.Instance.GunSounds) ? "OFF" : "ON", () => (Object)(object)GunRuntime.Instance != (Object)null && GunRuntime.Instance.GunSounds));
		modCategory4.Mods.Add(new ModEntry("Gun Vibrations", "Haptics on lock.", delegate
		{
			if ((Object)(object)GunRuntime.Instance != (Object)null)
			{
				GunRuntime.Instance.GunVibrations = !GunRuntime.Instance.GunVibrations;
			}
		}, () => (!((Object)(object)GunRuntime.Instance != (Object)null) || !GunRuntime.Instance.GunVibrations) ? "OFF" : "ON", () => (Object)(object)GunRuntime.Instance != (Object)null && GunRuntime.Instance.GunVibrations));
		SettingsSubCategories.Clear();
		SettingsSubCategories.Add(modCategory3);
		SettingsSubCategories.Add(modCategory4);
		ModCategory item = new ModCategory("Settings");
		Categories.Add(item);
		ModCategory modCategory5 = new ModCategory("Overpowered");
		modCategory5.Mods.Add(new ModEntry("Log Spam All V6", "Spams logs and notifications.", delegate
		{
			if ((Object)(object)OverpoweredRuntime.Instance != (Object)null)
			{
				OverpoweredRuntime.Instance.LogSpam = !OverpoweredRuntime.Instance.LogSpam;
			}
		}, () => (!((Object)(object)OverpoweredRuntime.Instance != (Object)null) || !OverpoweredRuntime.Instance.LogSpam) ? "OFF" : "ON", () => (Object)(object)OverpoweredRuntime.Instance != (Object)null && OverpoweredRuntime.Instance.LogSpam));
		modCategory5.Mods.Add(new ModEntry("Break Movement All [M]", "Freezes movement for others in Ghost Reactor. Requires master.", delegate
		{
			if ((Object)(object)OverpoweredRuntime.Instance != (Object)null)
			{
				OverpoweredRuntime.Instance.BreakMovement = !OverpoweredRuntime.Instance.BreakMovement;
			}
		}, () => (!((Object)(object)OverpoweredRuntime.Instance != (Object)null) || !OverpoweredRuntime.Instance.BreakMovement) ? "OFF" : "ON", () => (Object)(object)OverpoweredRuntime.Instance != (Object)null && OverpoweredRuntime.Instance.BreakMovement));
		modCategory5.Mods.Add(BuildAction("Unbreak Movement All [GR]", "Fixes movement for everyone in Ghost Reactor.", OverpoweredRuntime.UnbreakMovementAll, "FIX"));
		modCategory5.Mods.Add(BuildToggle("Break Movement Gun [GR]", "Aim and trigger to break movement for the locked player in Ghost Reactor."));
		modCategory5.Mods.Add(BuildToggle("Unbreak Movement Gun [GR]", "Aim and trigger to fix movement for the locked player in Ghost Reactor."));
		modCategory5.Mods.Add(new ModEntry("Spawn Mines Lucy (M)", "Spawns mines.", delegate
		{
			if ((Object)(object)OverpoweredRuntime.Instance != (Object)null)
			{
				OverpoweredRuntime.Instance.MinesLucy = !OverpoweredRuntime.Instance.MinesLucy;
			}
		}, () => (!((Object)(object)OverpoweredRuntime.Instance != (Object)null) || !OverpoweredRuntime.Instance.MinesLucy) ? "OFF" : "ON", () => (Object)(object)OverpoweredRuntime.Instance != (Object)null && OverpoweredRuntime.Instance.MinesLucy));

		modCategory5.Mods.Add(BuildToggle("Kick Gun [GR]", "Ghost Reactor kick gun. Aim and fire to kick the locked player in GR."));
		modCategory5.Mods.Add(BuildAction("Kick All [GR]", "Kicks players in Ghost Reactor using Malachi's GR kick method.", OverpoweredRuntime.KickAllGR, "KICK"));
		modCategory5.Mods.Add(BuildToggle("Crash Gun [GR]", "Malachi crash gun. Lock and trigger a player in Ghost Reactor to crash them."));
		modCategory5.Mods.Add(BuildAction("Crash All [GR]", "Malachi crash all. Crashes players in Ghost Reactor.", OverpoweredRuntime.CrashAllGR, "CRASH"));
		modCategory5.Mods.Add(BuildToggle("Auto Try Get Zone Auth", "When enabled and you enter a map by yourself, tries to keep zone authority in that zone."));
		Categories.Add(modCategory5);
		ModCategory modCategory6 = new ModCategory("Guns");
		string[] array4 = new string[13]
		{
			"Rig Spaz Gun", "Confuse Player Gun", "Chase Player Gun", "Orbit Player Gun", "Lag Gun", "Snowball Cannon", "Bubble Blaster", "Banana Gun", "Paint Ball", "Snow Volley",
			"Bomb Toss", "Laser Pointer", "Rig Gun"
		};
		for (int num3 = 0; num3 < array4.Length; num3++)
		{
			modCategory6.Mods.Add(BuildToggle(array4[num3], "Gun mod."));
		}
		Categories.Add(modCategory6);
		ModCategory roomCategory = new ModCategory("Room");
		roomCategory.Mods.Add(new ModEntry("Room Code", "Click, type the code on your keyboard, Enter to join.", RoomRuntime.ToggleTyping, () => RoomRuntime.CodeLabel, () => RoomRuntime.Typing));
		roomCategory.Mods.Add(BuildAction("Join Room", "Joins the code above.", RoomRuntime.JoinRoom, "JOIN"));
		roomCategory.Mods.Add(new ModEntry("Game Mode", "Click to cycle the mode, then Create Public Room to play it.", RoomRuntime.NextGameMode, () => RoomRuntime.GameModeLabel, Pulse));
		roomCategory.Mods.Add(BuildAction("Join Random Room", "Joins a random public room in your map and game mode.", RoomRuntime.JoinRandomRoom, "JOIN"));
		roomCategory.Mods.Add(BuildAction("Create Public Room", "Joins or makes a public room in your current queue.", RoomRuntime.CreatePublicRoom, "RUN"));
		roomCategory.Mods.Add(BuildAction("Create Private Room", "Makes a private room with a random code.", RoomRuntime.CreatePrivateRoom, "RUN"));
		roomCategory.Mods.Add(BuildAction("Leave Room", "Disconnects you from the room.", RoomRuntime.LeaveRoom, "RUN"));
		roomCategory.Mods.Add(BuildAction("Copy Code", "Copies the current room code to clipboard.", RoomRuntime.CopyCode, "COPY"));
		roomCategory.Mods.Add(BuildAction("Paste Code", "Pastes a room code from clipboard.", RoomRuntime.PasteCode, "PASTE"));
		Categories.Add(roomCategory);
		ModCategory modCategory7 = new ModCategory("Super Infection");
		string[] array5 = new string[8] { "Tech Point Gun [M]", "Strange Wood Gun [M]", "Weird Gear Gun [M]", "Vibrating Spring Gun [M]", "Bouncy Sand Gun [M]", "Floppy Metal Gun [M]", "All Resources Gun [M]", "Destroy Resource Gun [M]" };
		for (int num4 = 0; num4 < array5.Length; num4++)
		{
			modCategory7.Mods.Add(BuildToggle(array5[num4], "Spawns resources where you aim. Requires master."));
		}
		modCategory7.Mods.Add(BuildAction("Spawn All Resources [M]", "Spawns one of each resource at your hand. Requires master.", SuperInfectionRuntime.SpawnAllAtHand, "RUN"));
		Categories.Add(modCategory7);
		ModCategory modCategory8 = new ModCategory("Ghost Reactor");
		modCategory8.Mods.Add(BuildToggle("GR Item Gun [Priv M]", "Spawns the picked Ghost Reactor item where you aim. Private room + master only, inside the reactor."));
		modCategory8.Mods.Add(new ModEntry("Next GR Item", "Picks which item the GR Item Gun spawns.", SpawnerRuntime.NextGRItem, () => SpawnerRuntime.SelectedGRItemName, Never));
		modCategory8.Mods.Add(BuildAction("Spawn All GR Items [Priv M]", "Spawns one of every Ghost Reactor item around your hand. Private room + master only, inside the reactor.", SpawnerRuntime.SpawnAllGRItemsAtHand, "RUN"));
		modCategory8.Mods.Add(BuildToggle("GR Destroy Gun [Priv M]", "Removes Ghost Reactor items where you aim. Private room + master only, inside the reactor."));
		Categories.Add(modCategory8);
		ModCategory modCategory9 = new ModCategory("Critters");
		modCategory9.Mods.Add(BuildToggle("Critter Gun [M]", "Spawns the picked critter where you aim. Requires master."));
		modCategory9.Mods.Add(new ModEntry("Next Critter", "Picks which critter the Critter Gun spawns.", SpawnerRuntime.NextCritter, () => SpawnerRuntime.SelectedCritterName, Never));
		modCategory9.Mods.Add(BuildToggle("Critter Food Gun [M]", "Spawns the picked food where you aim. Requires master."));
		modCategory9.Mods.Add(new ModEntry("Next Food", "Picks which food the Critter Food Gun spawns.", SpawnerRuntime.NextFood, () => SpawnerRuntime.SelectedFoodName, Never));
		modCategory9.Mods.Add(BuildToggle("Critter Item Gun [M]", "Spawns the picked Critters item where you aim. Requires master."));
		modCategory9.Mods.Add(new ModEntry("Next Critter Item", "Picks which item the Critter Item Gun spawns.", SpawnerRuntime.NextCritterItem, () => SpawnerRuntime.SelectedCritterItemName, Never));
		Categories.Add(modCategory9);
		ModCategory modCategory10 = new ModCategory("Monkey Blocks");
		modCategory10.Mods.Add(BuildToggle("Block Gun [M]", "Spawns the picked block where you aim. Requires master."));
		modCategory10.Mods.Add(new ModEntry("Next Block", "Picks which block the guns use. Only sets you own.", BlocksRuntime.NextBlock, () => BlocksRuntime.SelectedBlockName, Never));
		modCategory10.Mods.Add(BuildToggle("Shape Gun [M]", "Builds the picked shape where you aim. Requires master."));
		modCategory10.Mods.Add(new ModEntry("Next Shape", "Picks wall, tower, stairs or dome.", BlocksRuntime.NextShape, () => BlocksRuntime.SelectedShapeName, Never));
		modCategory10.Mods.Add(BuildToggle("Block Rain [M]", "Blocks appear in the air all around you. Requires master."));
		modCategory10.Mods.Add(BuildToggle("Destroy Block Gun [M]", "Removes blocks you spawned where you aim. Requires master."));
		modCategory10.Mods.Add(BuildAction("Clear My Blocks [M]", "Removes every block you spawned. Requires master.", BlocksRuntime.ClearMyBlocks, "RUN"));
		modCategory10.Mods.Add(BuildToggle("Block Platform [M]", "Hold left grip to make blocks under your hand. Requires master."));
		modCategory10.Mods.Add(BuildAction("Block Name [M]", "Writes your name in blocks in front of you. Requires master.", BlocksRuntime.WriteName, "RUN"));
		Categories.Add(modCategory10);
		ModCategory safetyCategory = new ModCategory("Safety");
		safetyCategory.Mods.Add(BuildToggle("Anti Kick", "Blocks kick attempts from modders."));
		safetyCategory.Mods.Add(BuildToggle("Anti Break Audio", "Blocks break audio payloads."));
		Categories.Add(safetyCategory);
		ModCategory gamemodeCategory = new ModCategory("Gamemode");
		gamemodeCategory.Mods.Add(BuildAction("Tag Self", "Tag yourself.", GamemodeRuntime.TagSelf, "TAG"));
		gamemodeCategory.Mods.Add(BuildAction("Tag All", "Tag everyone.", GamemodeRuntime.TagAll, "TAG"));
		gamemodeCategory.Mods.Add(BuildToggle("Tag All Except", "Lock a target with gun to tag everyone except them."));
		gamemodeCategory.Mods.Add(BuildAction("Tag Others (M)", "Tag all others. Master.", GamemodeRuntime.TagOthersM, "TAG"));
		gamemodeCategory.Mods.Add(BuildToggle("Tag Gun", "Lock and tag the target."));
		gamemodeCategory.Mods.Add(BuildToggle("Tag Aura", "Tag nearby players."));
		gamemodeCategory.Mods.Add(BuildAction("UnTag Self (M)", "Untag yourself. Master.", GamemodeRuntime.UnTagSelfM, "UNTAG"));
		gamemodeCategory.Mods.Add(BuildAction("UnTag All (M)", "Untag everyone. Master.", GamemodeRuntime.UnTagAllM, "UNTAG"));
		gamemodeCategory.Mods.Add(BuildAction("UnTag Others (M)", "Untag all others. Master.", GamemodeRuntime.UnTagOthersMAction, "UNTAG"));
		gamemodeCategory.Mods.Add(BuildToggle("UnTag Gun (M)", "Lock and untag target. Master."));
		gamemodeCategory.Mods.Add(BuildToggle("UnTag Aura (M)", "Untag nearby. Master."));
		gamemodeCategory.Mods.Add(BuildAction("Mat Self (M)", "Spam tag/untag self. Master.", GamemodeRuntime.MatSelfM, "RUN"));
		gamemodeCategory.Mods.Add(BuildAction("Mat All V1 (M)", "Spam tag/untag others. Master.", GamemodeRuntime.MatAllV1M, "RUN"));
		gamemodeCategory.Mods.Add(BuildAction("Mat All V2 (M)", "Spam tag/untag others. Master.", GamemodeRuntime.MatAllV2M, "RUN"));
		gamemodeCategory.Mods.Add(BuildToggle("Mat Gun (M)", "Lock and mat target. Master."));
		gamemodeCategory.Mods.Add(BuildToggle("Mat Aura (M)", "Mat nearby. Master."));
		gamemodeCategory.Mods.Add(BuildAction("Give Mat All (M)", "Give mat cycle to all. Master.", GamemodeRuntime.GiveMatAllM, "RUN"));
		gamemodeCategory.Mods.Add(BuildToggle("Flick Tag Gun", "Flick/tag with gun lock."));
		gamemodeCategory.Mods.Add(BuildAction("Get Guardian", "Become guardian / tap idol.", GamemodeRuntime.GetGuardian, "GET"));
		gamemodeCategory.Mods.Add(BuildAction("Undo Guardian", "Restore rig after guardian grab.", GamemodeRuntime.UndoGuardian, "UNDO"));
		gamemodeCategory.Mods.Add(BuildAction("Guardian Fling All", "Fling everyone as guardian.", GamemodeRuntime.GuardianFlingAll, "RUN"));
		gamemodeCategory.Mods.Add(BuildToggle("Guardian Fling Gun", "Fling locked target as guardian."));
		gamemodeCategory.Mods.Add(BuildAction("Guardian Spaz All", "Spaz-fling everyone as guardian.", GamemodeRuntime.GuardianSpazAll, "RUN"));
		gamemodeCategory.Mods.Add(BuildToggle("Guardian Spaz Gun", "Spaz-fling locked target as guardian."));
		gamemodeCategory.Mods.Add(BuildAction("Guardian Orbit All", "Orbit everyone as guardian.", GamemodeRuntime.GuardianOrbitAll, "RUN"));
		gamemodeCategory.Mods.Add(BuildToggle("Guardian Orbit Gun", "Orbit locked target as guardian."));
		gamemodeCategory.Mods.Add(BuildAction("Give Guardian Fling All", "Fling non-guardian others.", GamemodeRuntime.GiveGuardianFlingAll, "RUN"));
		gamemodeCategory.Mods.Add(BuildAction("Give Guardian Spaz All", "Spaz non-guardian others.", GamemodeRuntime.GiveGuardianSpazAll, "RUN"));
		gamemodeCategory.Mods.Add(BuildAction("Give Guardian Orbit All", "Orbit non-guardian others.", GamemodeRuntime.GiveGuardianOrbitAll, "RUN"));
		gamemodeCategory.Mods.Add(BuildToggle("Guardian To Point All", "Throw all to locked point as guardian."));
		gamemodeCategory.Mods.Add(BuildToggle("PaintBrawl Hard AimBot", "Hard aim assist in PaintBrawl."));
		gamemodeCategory.Mods.Add(BuildToggle("PaintBrawl Soft AimBot", "Soft aim assist in PaintBrawl."));
		gamemodeCategory.Mods.Add(BuildAction("No PaintBrawl Cooldown (M)", "Toggle PaintBrawl cooldowns. Master.", GamemodeRuntime.NoPaintBrawlCooldownToggle, "TOG"));
		gamemodeCategory.Mods.Add(BuildAction("PaintBrawl Teams (M)", "Team mode helper. Master.", GamemodeRuntime.PaintBrawlTeamsM, "RUN"));
		gamemodeCategory.Mods.Add(BuildAction("Randomize Teams (M)", "Randomize teams. Master.", GamemodeRuntime.RandomizeTeamsM, "RUN"));
		gamemodeCategory.Mods.Add(BuildAction("Set Orange Team All (M)", "Set all orange. Master.", delegate { GamemodeRuntime.SetTeamAllM(1); }, "SET"));
		gamemodeCategory.Mods.Add(BuildToggle("Set Orange Team Gun (M)", "Set locked target orange. Master."));
		gamemodeCategory.Mods.Add(BuildAction("Set Orange Team Self (M)", "Set self orange. Master.", delegate { GamemodeRuntime.SetTeamSelfM(1); }, "SET"));
		gamemodeCategory.Mods.Add(BuildAction("Set Blue Team All (M)", "Set all blue. Master.", delegate { GamemodeRuntime.SetTeamAllM(0); }, "SET"));
		gamemodeCategory.Mods.Add(BuildToggle("Set Blue Team Gun (M)", "Set locked target blue. Master."));
		gamemodeCategory.Mods.Add(BuildAction("Set Blue Team Self (M)", "Set self blue. Master.", delegate { GamemodeRuntime.SetTeamSelfM(0); }, "SET"));
		gamemodeCategory.Mods.Add(BuildAction("Set No Team All (M)", "Set all no team. Master.", delegate { GamemodeRuntime.SetTeamAllM(2); }, "SET"));
		gamemodeCategory.Mods.Add(BuildToggle("Set No Team Gun (M)", "Set locked target no team. Master."));
		gamemodeCategory.Mods.Add(BuildAction("Set No Team Self (M)", "Set self no team. Master.", delegate { GamemodeRuntime.SetTeamSelfM(2); }, "SET"));
		gamemodeCategory.Mods.Add(BuildAction("Set Stunned All (M)", "Stun all. Master.", delegate { GamemodeRuntime.SetTeamAllM(67); }, "SET"));
		gamemodeCategory.Mods.Add(BuildToggle("Set Stunned Gun (M)", "Stun locked target. Master."));
		gamemodeCategory.Mods.Add(BuildAction("Set Stunned Self (M)", "Stun self. Master.", delegate { GamemodeRuntime.SetTeamSelfM(67); }, "SET"));
		gamemodeCategory.Mods.Add(BuildAction("Set Hit All (M)", "Set hit all. Master.", delegate { GamemodeRuntime.SetTeamAllM(69); }, "SET"));
		gamemodeCategory.Mods.Add(BuildToggle("Set Hit Gun (M)", "Set hit locked target. Master."));
		gamemodeCategory.Mods.Add(BuildAction("Set Hit Self (M)", "Set hit self. Master.", delegate { GamemodeRuntime.SetTeamSelfM(69); }, "SET"));
		gamemodeCategory.Mods.Add(BuildAction("GodMode All (M)", "God mode all. Master.", delegate { GamemodeRuntime.GodModeAllM(true); }, "SET"));
		gamemodeCategory.Mods.Add(BuildToggle("GodMode Gun (M)", "God mode locked target. Master."));
		gamemodeCategory.Mods.Add(BuildAction("GodMode Self (M)", "God mode self. Master.", delegate { GamemodeRuntime.GodModeSelfM(true); }, "SET"));
		gamemodeCategory.Mods.Add(BuildAction("UnGodMode All (M)", "Remove god mode all. Master.", delegate { GamemodeRuntime.GodModeAllM(false); }, "SET"));
		gamemodeCategory.Mods.Add(BuildToggle("UnGodMode Gun (M)", "Remove god mode locked target. Master."));
		gamemodeCategory.Mods.Add(BuildAction("UnGodMode Self (M)", "Remove god mode self. Master.", delegate { GamemodeRuntime.GodModeSelfM(false); }, "SET"));
		Categories.Add(gamemodeCategory);
		CurrentCategoryIndex = 0;
		InHome = true;
		LoadFavorites();
	}

	public static void NextCategory()
	{
		if (Categories.Count != 0)
		{
			CurrentCategoryIndex = (CurrentCategoryIndex + 1) % Categories.Count;
		}
	}

	public static void PreviousCategory()
	{
		if (Categories.Count != 0)
		{
			CurrentCategoryIndex--;
			if (CurrentCategoryIndex < 0)
			{
				CurrentCategoryIndex = Categories.Count - 1;
			}
		}
	}

	public static void SetCategory(int index)
	{
		if (Categories.Count != 0)
		{
			if (index < 0)
			{
				index = 0;
			}
			if (index >= Categories.Count)
			{
				index = Categories.Count - 1;
			}
			CurrentCategoryIndex = index;
		}
	}

	public static void SetModPage(int page)
	{
		int modPageCount = ModPageCount;
		if (modPageCount > 0)
		{
			page %= modPageCount;
			if (page < 0)
			{
				page += modPageCount;
			}
			ModPage = page;
		}
	}

	public static void ShowHome()
	{
		RebuildDynamicCategories();
		InHome = true;
		InSettingsHome = false;
		SettingsSubIndex = -1;
	}

	public static void ShowMods()
	{
		RebuildDynamicCategories();
		ModPage = 0;
		InHome = false;
		if (CurrentCategory != null && string.Equals(CurrentCategory.Name, "Settings", StringComparison.OrdinalIgnoreCase))
		{
			InSettingsHome = true;
			SettingsSubIndex = -1;
		}
		else
		{
			InSettingsHome = false;
			SettingsSubIndex = -1;
		}
	}

	public static void ShowSettingsSub(int index)
	{
		if (index >= 0 && index < SettingsSubCategories.Count)
		{
			RebuildDynamicCategories();
			SettingsSubIndex = index;
			ModPage = 0;
			InSettingsHome = true;
			InHome = false;
		}
	}

	private static void RebuildDynamicCategories()
	{
		// Remove auto-generated Favorite/Enabled categories so only real categories show.
		for (int i = Categories.Count - 1; i >= 0; i--)
		{
			if (string.Equals(Categories[i].Name, "Favorite", StringComparison.OrdinalIgnoreCase) ||
			    string.Equals(Categories[i].Name, "Enabled", StringComparison.OrdinalIgnoreCase))
			{
				Categories.RemoveAt(i);
			}
		}
	}

	private static ModCategory FindCategory(string name)
	{
		for (int i = 0; i < Categories.Count; i++)
		{
			if (string.Equals(Categories[i].Name, name, StringComparison.OrdinalIgnoreCase))
			{
				return Categories[i];
			}
		}
		return null;
	}

	public static void SaveFavorites()
	{
		try
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < Categories.Count; i++)
			{
				ModCategory modCategory = Categories[i];
				for (int j = 0; j < modCategory.Mods.Count; j++)
				{
					ModEntry modEntry = modCategory.Mods[j];
					if (modEntry.Favorite)
					{
						stringBuilder.AppendLine(modEntry.Title);
					}
				}
			}
			File.WriteAllText(FavoritesPath(), stringBuilder.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
		}
		catch (Exception ex)
		{
			RuntimeLog.Write("favorites save failed: " + ex.Message);
		}
	}

	public static void LoadFavorites()
	{
		try
		{
			string path = FavoritesPath();
			if (!File.Exists(path))
			{
				return;
			}
			HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			string[] array = File.ReadAllLines(path);
			for (int i = 0; i < array.Length; i++)
			{
				string text = array[i].Trim();
				if (text.Length > 0)
				{
					hashSet.Add(text);
				}
			}
			for (int j = 0; j < Categories.Count; j++)
			{
				ModCategory modCategory = Categories[j];
				for (int k = 0; k < modCategory.Mods.Count; k++)
				{
					ModEntry modEntry = modCategory.Mods[k];
					if (hashSet.Contains(modEntry.Title))
					{
						modEntry.Favorite = true;
					}
				}
			}
			RuntimeLog.Write("favorites loaded: " + hashSet.Count);
		}
		catch (Exception ex)
		{
			RuntimeLog.Write("favorites load failed: " + ex.Message);
		}
	}

	private static string FavoritesPath()
	{
		return Path.Combine(Paths.ConfigPath, "Sonder.favorites");
	}

	public static void NotifyChanged()
	{
		Changed?.Invoke();
	}

	public static ModEntry Find(string title)
	{
		for (int i = 0; i < Categories.Count; i++)
		{
			List<ModEntry> mods = Categories[i].Mods;
			for (int j = 0; j < mods.Count; j++)
			{
				if (string.Equals(mods[j].Title, title, StringComparison.OrdinalIgnoreCase))
				{
					return mods[j];
				}
			}
		}
		return null;
	}

	public static bool SetEnabled(string title, bool enabled)
	{
		ModEntry modEntry = Find(title);
		if (modEntry == null || modEntry.IsAction)
		{
			return false;
		}
		modEntry.SetEnabled(enabled);
		NotifyChanged();
		return true;
	}

	public static void DisableAll()
	{
		for (int i = 0; i < Categories.Count; i++)
		{
			ModCategory modCategory = Categories[i];
			if (modCategory.Name == "Settings")
			{
				continue;
			}
			for (int j = 0; j < modCategory.Mods.Count; j++)
			{
				ModEntry modEntry = modCategory.Mods[j];
				if (!modEntry.IsAction && modEntry.Enabled)
				{
					modEntry.SetEnabled(enabled: false);
				}
			}
		}
		NotifyChanged();
	}

	public static void ApplySet(IEnumerable<string> titles)
	{
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		if (titles != null)
		{
			foreach (string title in titles)
			{
				if (!string.IsNullOrEmpty(title))
				{
					hashSet.Add(title);
				}
			}
		}
		for (int i = 0; i < Categories.Count; i++)
		{
			ModCategory modCategory = Categories[i];
			if (modCategory.Name == "Settings")
			{
				continue;
			}
			for (int j = 0; j < modCategory.Mods.Count; j++)
			{
				ModEntry modEntry = modCategory.Mods[j];
				if (!modEntry.IsAction)
				{
					modEntry.SetEnabled(hashSet.Contains(modEntry.Title));
				}
			}
		}
		NotifyChanged();
	}

	public static List<string> ActiveTitles()
	{
		List<string> list = new List<string>();
		for (int i = 0; i < Categories.Count; i++)
		{
			ModCategory modCategory = Categories[i];
			for (int j = 0; j < modCategory.Mods.Count; j++)
			{
				ModEntry modEntry = modCategory.Mods[j];
				if (!modEntry.IsAction && modEntry.Enabled)
				{
					list.Add(modEntry.Title);
				}
			}
		}
		return list;
	}

	public static void SaveBinds(string reason = "manual")
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("# Sonder binds");
		stringBuilder.AppendLine("# " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + reason);
		for (int i = 0; i < Categories.Count; i++)
		{
			ModCategory modCategory = Categories[i];
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("[" + modCategory.Name + "]");
			for (int j = 0; j < modCategory.Mods.Count; j++)
			{
				ModEntry modEntry = modCategory.Mods[j];
				if (!modEntry.IsAction && modEntry.Enabled)
				{
					stringBuilder.AppendLine(modEntry.Title);
				}
			}
		}
		try
		{
			File.WriteAllText(Path.Combine(Paths.ConfigPath, "Sonder.binds"), stringBuilder.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
			RuntimeLog.Write("binds saved: " + ActiveTitles().Count + " active");
		}
		catch (Exception ex)
		{
			RuntimeLog.Write("binds save failed: " + ex.Message);
		}
	}

	private static ModEntry BuildToggle(string title, string description)
	{
		ModRegistry.TryGet(title, out var handler);
		return new ModEntry(title, description, handler);
	}

	private static ModEntry BuildAction(string title, string description, Action action, string state)
	{
		return new ModEntry(title, description, action, () => state, Never);
	}

	private static bool Pulse()
	{
		return true;
	}

	private static bool Never()
	{
		return false;
	}

	private static bool SoundsOn()
	{
		return SonderSettings.SoundsOn;
	}

	private static bool DiscordOn()
	{
		return SonderSettings.DiscordOn;
	}

	private static bool HumOn()
	{
		return SonderSettings.HumOn;
	}

	private static bool MuteOn()
	{
		return SonderSettings.Muted;
	}

	private static string ResetLabel()
	{
		return "RESET";
	}

	private static string DockLabel()
	{
		if (!MenuManager.Docked)
		{
			return "OFF";
		}
		return "DOCKED";
	}

	private static string SaveLabel()
	{
		return "SAVE";
	}

	private static string LoadLabel()
	{
		return "LOAD";
	}

	private static string ClearLabel()
	{
		return "CLEAR";
	}

	private static ModCategory BuildCategory(string name, ModEntry[] mods)
	{
		ModCategory modCategory = new ModCategory(name);
		foreach (ModEntry modEntry in mods)
		{
			ModRegistry.TryGet(modEntry.Title, out var handler);
			modCategory.Mods.Add(new ModEntry(modEntry.Title, modEntry.Description, handler));
		}
		return modCategory;
	}
}
