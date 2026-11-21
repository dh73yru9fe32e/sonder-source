using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Sonder.Managers;

public static class ModRegistry
{
	private static readonly Dictionary<string, Action<bool>> Handlers = Build();

	public static bool TryGet(string title, out Action<bool> handler)
	{
		return Handlers.TryGetValue(title, out handler);
	}

	private static Dictionary<string, Action<bool>> Build()
	{
		Dictionary<string, Action<bool>> dictionary = new Dictionary<string, Action<bool>>();
		dictionary["Speed Boost"] = delegate(bool on)
		{
			Set(delegate(ModRuntime runtime)
			{
				runtime.SpeedBoost = on;
			});
		};
		dictionary["High Jump"] = delegate(bool on)
		{
			Set(delegate(ModRuntime runtime)
			{
				runtime.HighJump = on;
			});
		};
		dictionary["Long Arms"] = delegate(bool on)
		{
			Set(delegate(ModRuntime runtime)
			{
				runtime.LongArms = on;
			});
		};
		dictionary["Moon Gravity"] = delegate(bool on)
		{
			Set(delegate(ModRuntime runtime)
			{
				runtime.MoonGravity = on;
			});
		};
		dictionary["Fly"] = delegate(bool on)
		{
			Set(delegate(ModRuntime runtime)
			{
				runtime.Fly = on;
			});
		};
		dictionary["Rainbow Fur"] = delegate(bool on)
		{
			Set(delegate(ModRuntime runtime)
			{
				runtime.RainbowFur = on;
			});
		};
		dictionary["Joystick Fly"] = delegate(bool on)
		{
			Set(delegate(ModRuntime runtime)
			{
				runtime.JoystickFly = on;
			});
		};
		dictionary["Dash"] = delegate(bool on)
		{
			Set(delegate(ModRuntime runtime)
			{
				runtime.Dash = on;
			});
		};
		dictionary["Zero Gravity"] = delegate(bool on)
		{
			Set(delegate(ModRuntime runtime)
			{
				runtime.ZeroGravity = on;
			});
		};
		dictionary["Low Gravity"] = delegate(bool on)
		{
			Set(delegate(ModRuntime runtime)
			{
				runtime.LowGravity = on;
			});
		};
		dictionary["Bouncy"] = delegate(bool on)
		{
			Set(delegate(ModRuntime runtime)
			{
				runtime.Bouncy = on;
			});
		};
		dictionary["Slingshot Fly"] = delegate(bool on)
		{
			Set(delegate(ModRuntime runtime)
			{
				runtime.SlingshotFly = on;
			});
		};
		dictionary["Trigger Fly"] = delegate(bool on)
		{
			Set(delegate(ModRuntime runtime)
			{
				runtime.TriggerFly = on;
			});
		};
		dictionary["Hand Fly"] = delegate(bool on)
		{
			Set(delegate(ModRuntime runtime)
			{
				runtime.HandFly = on;
			});
		};
		dictionary["WASD Fly"] = delegate(bool on)
		{
			Set(delegate(ModRuntime runtime)
			{
				runtime.WasdFly = on;
			});
		};
		dictionary["Bark Fly"] = delegate(bool on)
		{
			Set(delegate(ModRuntime runtime)
			{
				runtime.BarkFly = on;
			});
		};
		dictionary["Bird Fly"] = delegate(bool on)
		{
			Set(delegate(ModRuntime runtime)
			{
				runtime.BirdFly = on;
			});
		};
		dictionary["Reverse Velocity"] = delegate(bool on)
		{
			Set(delegate(ModRuntime runtime)
			{
				runtime.ReverseVelocity = on;
			});
		};
		dictionary["High Gravity"] = delegate(bool on)
		{
			Set(delegate(ModRuntime runtime)
			{
				runtime.HighGravity = on;
			});
		};
		dictionary["Ground Helper"] = delegate(bool on)
		{
			Set(delegate(ModRuntime runtime)
			{
				runtime.GroundHelper = on;
			});
		};
		dictionary["Bunny Hop"] = delegate(bool on)
		{
			Set(delegate(ModRuntime runtime)
			{
				runtime.BunnyHop = on;
			});
		};
		dictionary["Strafe"] = delegate(bool on)
		{
			Set(delegate(ModRuntime runtime)
			{
				runtime.Strafe = on;
			});
		};
		dictionary["Slide Control"] = delegate(bool on)
		{
			Set(delegate(ModRuntime runtime)
			{
				runtime.SlideControl = on;
			});
		};
		dictionary["No Tag Freeze"] = delegate(bool on)
		{
			Set(delegate(ModRuntime runtime)
			{
				runtime.NoTagFreeze = on;
			});
		};
		dictionary["Drive"] = delegate(bool on)
		{
			Set(delegate(ModRuntime runtime)
			{
				runtime.Drive = on;
			});
		};
		dictionary["Iron Man"] = delegate(bool on)
		{
			Set(delegate(ModRuntime runtime)
			{
				runtime.IronMan = on;
			});
		};
		dictionary["Flick Jump"] = delegate(bool on)
		{
			Set(delegate(ModRuntime runtime)
			{
				runtime.FlickJump = on;
			});
		};
		dictionary["Up And Down"] = delegate(bool on)
		{
			Set(delegate(ModRuntime runtime)
			{
				runtime.UpAndDown = on;
			});
		};
		dictionary["No Finger"] = delegate(bool on)
		{
			SetSafety(delegate(SafetyRuntime runtime)
			{
				runtime.NoFinger = on;
			});
		};
		dictionary["Fake Oculus Menu"] = delegate(bool on)
		{
			SetSafety(delegate(SafetyRuntime runtime)
			{
				runtime.FakeOculusMenu = on;
			});
		};
		dictionary["Fake Report Menu"] = delegate(bool on)
		{
			SetSafety(delegate(SafetyRuntime runtime)
			{
				runtime.FakeReportMenu = on;
			});
		};
		dictionary["Fake Broken Controller"] = delegate(bool on)
		{
			SetSafety(delegate(SafetyRuntime runtime)
			{
				runtime.FakeBrokenController = on;
			});
		};
		dictionary["Fake Power Off"] = delegate(bool on)
		{
			SetSafety(delegate(SafetyRuntime runtime)
			{
				runtime.FakePowerOff = on;
			});
		};
		dictionary["Fake Valve Tracking"] = delegate(bool on)
		{
			SetSafety(delegate(SafetyRuntime runtime)
			{
				runtime.FakeValveTracking = on;
			});
		};
		dictionary["Anti Report (Disconnect)"] = delegate(bool on)
		{
			SetSafety(delegate(SafetyRuntime runtime)
			{
				runtime.AntiReportDisconnect = on;
			});
		};
		dictionary["Anti Report (Notify)"] = delegate(bool on)
		{
			SetSafety(delegate(SafetyRuntime runtime)
			{
				runtime.AntiReportNotify = on;
			});
		};
		dictionary["Anti Report (Oculus)"] = delegate(bool on)
		{
			SetSafety(delegate(SafetyRuntime runtime)
			{
				runtime.AntiReportOculus = on;
			});
		};
		dictionary["Anti Kick"] = delegate(bool on)
		{
			SetSafety(delegate(SafetyRuntime runtime)
			{
				runtime.AntiKick = on;
			});
		};
		dictionary["Anti Break Audio"] = delegate(bool on)
		{
			SetSafety(delegate(SafetyRuntime runtime)
			{
				runtime.AntiBreakAudio = on;
			});
		};
		dictionary["Auto Clear Cache"] = delegate(bool on)
		{
			SetSafety(delegate(SafetyRuntime runtime)
			{
				runtime.AutoClearCache = on;
			});
		};
		dictionary["Hide Gamemode Button"] = delegate(bool on)
		{
			SetSafety(delegate(SafetyRuntime runtime)
			{
				runtime.HideGamemodeButton = on;
			});
		};
		dictionary["Feather Fall"] = delegate(bool on)
		{
			Set(delegate(ModRuntime runtime)
			{
				runtime.FeatherFall = on;
			});
		};
		dictionary["Upside Down Head"] = delegate(bool on)
		{
			SetFun(delegate(FunRuntime runtime)
			{
				runtime.UpsideDownHead = on;
			});
		};
		dictionary["Broken Neck"] = delegate(bool on)
		{
			SetFun(delegate(FunRuntime runtime)
			{
				runtime.BrokenNeck = on;
			});
		};
		dictionary["Backwards Head"] = delegate(bool on)
		{
			SetFun(delegate(FunRuntime runtime)
			{
				runtime.BackwardsHead = on;
			});
		};
		dictionary["Sideways Head"] = delegate(bool on)
		{
			SetFun(delegate(FunRuntime runtime)
			{
				runtime.SidewaysHead = on;
			});
		};
		dictionary["Head Bang"] = delegate(bool on)
		{
			SetFun(delegate(FunRuntime runtime)
			{
				runtime.HeadBang = on;
			});
		};
		dictionary["Spin Head"] = delegate(bool on)
		{
			SetFun(delegate(FunRuntime runtime)
			{
				runtime.SpinHead = on;
			});
		};
		dictionary["Spaz Head"] = delegate(bool on)
		{
			SetFun(delegate(FunRuntime runtime)
			{
				runtime.SpazHead = on;
			});
		};
		dictionary["Fix Head"] = delegate(bool on)
		{
			if (on)
			{
				FunRuntime instance = FunRuntime.Instance;
				if ((Object)(object)instance != (Object)null)
				{
					instance.FixHeadNow();
				}
			}
		};
		dictionary["Flip Hands"] = delegate(bool on)
		{
			SetFun(delegate(FunRuntime runtime)
			{
				runtime.FlipHands = on;
			});
		};
		dictionary["Auto Clicker"] = delegate(bool on)
		{
			SetFun(delegate(FunRuntime runtime)
			{
				runtime.AutoClicker = on;
			});
		};
		dictionary["Loud Hand Taps"] = delegate(bool on)
		{
			SetFun(delegate(FunRuntime runtime)
			{
				runtime.LoudHandTaps = on;
			});
		};
		dictionary["Silent Hand Taps"] = delegate(bool on)
		{
			SetFun(delegate(FunRuntime runtime)
			{
				runtime.SilentHandTaps = on;
			});
		};
		dictionary["Silent Taps On Tag"] = delegate(bool on)
		{
			SetFun(delegate(FunRuntime runtime)
			{
				runtime.SilentTapsOnTag = on;
			});
		};
		dictionary["Freecam"] = delegate(bool on)
		{
			SetFun(delegate(FunRuntime runtime)
			{
				runtime.Freecam = on;
			});
		};
		dictionary["Third Person"] = delegate(bool on)
		{
			SetFun(delegate(FunRuntime runtime)
			{
				runtime.ThirdPersonCamera = on;
			});
		};
		dictionary["Flip Camera"] = delegate(bool on)
		{
			SetFun(delegate(FunRuntime runtime)
			{
				runtime.FlipCamera = on;
			});
		};
		dictionary["Nausea"] = delegate(bool on)
		{
			SetFun(delegate(FunRuntime runtime)
			{
				runtime.Nausea = on;
			});
		};
		dictionary["Mute DJ Sets"] = delegate(bool on)
		{
			SetFun(delegate(FunRuntime runtime)
			{
				runtime.MuteDJSets = on;
			});
		};
		dictionary["Max Quest Score"] = delegate(bool on)
		{
			SetFun(delegate(FunRuntime runtime)
			{
				runtime.MaxQuestScore = on;
			});
		};
		dictionary["Quest Noises"] = delegate(bool on)
		{
			SetFun(delegate(FunRuntime runtime)
			{
				runtime.QuestNoises = on;
			});
		};
		dictionary["Golden Name Tag"] = delegate(bool on)
		{
			SetFun(delegate(FunRuntime runtime)
			{
				runtime.GoldenNameTag = on;
			});
		};
		dictionary["Flash Name Tag"] = delegate(bool on)
		{
			SetFun(delegate(FunRuntime runtime)
			{
				runtime.FlashNameTag = on;
			});
		};
		dictionary["Spawn Hoverboard"] = delegate(bool on)
		{
			SetFun(delegate(FunRuntime runtime)
			{
				runtime.SpawnHoverboard = on;
			});
		};
		dictionary["Sticky Hands"] = delegate(bool on)
		{
			SetAdvantage(delegate(AdvantageRuntime runtime)
			{
				runtime.StickyHands = on;
			});
		};
		dictionary["Iron Legs"] = delegate(bool on)
		{
			SetAdvantage(delegate(AdvantageRuntime runtime)
			{
				runtime.IronLegs = on;
			});
		};
		dictionary["Sturdy Grip"] = delegate(bool on)
		{
			SetAdvantage(delegate(AdvantageRuntime runtime)
			{
				runtime.SturdyGrip = on;
			});
		};
		dictionary["Low Drag"] = delegate(bool on)
		{
			SetAdvantage(delegate(AdvantageRuntime runtime)
			{
				runtime.LowDrag = on;
			});
		};
		dictionary["Watchlist"] = delegate(bool on)
		{
			SetDetection(delegate(DetectionRuntime runtime)
			{
				runtime.Watchlist = on;
			});
		};
		dictionary["Alert Ping"] = delegate(bool on)
		{
			SetDetection(delegate(DetectionRuntime runtime)
			{
				runtime.AlertPing = on;
			});
		};
		dictionary["Name Tags"] = delegate(bool on)
		{
			SetDetection(delegate(DetectionRuntime runtime)
			{
				runtime.NameTags = on;
			});
		};
		dictionary["Distance Readout"] = delegate(bool on)
		{
			SetDetection(delegate(DetectionRuntime runtime)
			{
				runtime.DistanceReadout = on;
			});
		};
		dictionary["Join Alerts"] = delegate(bool on)
		{
			SetDetection(delegate(DetectionRuntime runtime)
			{
				runtime.JoinAlerts = on;
			});
		};
		dictionary["Room Scan"] = delegate(bool on)
		{
			SetDetection(delegate(DetectionRuntime runtime)
			{
				runtime.RoomScan = on;
			});
		};
		dictionary["Ping History"] = delegate(bool on)
		{
			SetDetection(delegate(DetectionRuntime runtime)
			{
				runtime.PingHistory = on;
			});
		};
		dictionary["Alt Alert"] = delegate(bool on)
		{
			SetDetection(delegate(DetectionRuntime runtime)
			{
				runtime.AltAlert = on;
			});
		};
		dictionary["Glide Mode"] = delegate(bool on)
		{
			SetExperimental(delegate(ExperimentalRuntime runtime)
			{
				runtime.GlideMode = on;
			});
		};
		dictionary["No Gravity Pockets"] = delegate(bool on)
		{
			SetExperimental(delegate(ExperimentalRuntime runtime)
			{
				runtime.NoGravityPockets = on;
			});
		};
		dictionary["Ice Floor"] = delegate(bool on)
		{
			SetExperimental(delegate(ExperimentalRuntime runtime)
			{
				runtime.IceFloor = on;
			});
		};
		dictionary["Rubber Body"] = delegate(bool on)
		{
			SetExperimental(delegate(ExperimentalRuntime runtime)
			{
				runtime.RubberBody = on;
			});
		};
		dictionary["Chaos Wheel"] = delegate(bool on)
		{
			SetExperimental(delegate(ExperimentalRuntime runtime)
			{
				runtime.ChaosWheel = on;
			});
		};
		dictionary["Mirror Mode"] = delegate(bool on)
		{
			SetExperimental(delegate(ExperimentalRuntime runtime)
			{
				runtime.MirrorMode = on;
			});
		};
		dictionary["Slow World"] = delegate(bool on)
		{
			SetExperimental(delegate(ExperimentalRuntime runtime)
			{
				runtime.SlowWorld = on;
			});
		};
		dictionary["Phase Shift"] = delegate(bool on)
		{
			SetExperimental(delegate(ExperimentalRuntime runtime)
			{
				runtime.PhaseShift = on;
			});
		};
		dictionary["Safe Spot"] = delegate(bool on)
		{
			SetImportant(delegate(ImportantRuntime runtime)
			{
				runtime.SafeSpot = on;
			});
		};
		dictionary["Respawn Lock"] = delegate(bool on)
		{
			SetImportant(delegate(ImportantRuntime runtime)
			{
				runtime.RespawnLock = on;
			});
		};
		dictionary["Anti Fall"] = delegate(bool on)
		{
			SetImportant(delegate(ImportantRuntime runtime)
			{
				runtime.AntiFall = on;
			});
		};
		dictionary["Backup Binds"] = delegate(bool on)
		{
			SetImportant(delegate(ImportantRuntime runtime)
			{
				runtime.BackupBinds = on;
			});
		};
		dictionary["Auto Save"] = delegate(bool on)
		{
			SetImportant(delegate(ImportantRuntime runtime)
			{
				runtime.AutoSave = on;
			});
		};
		dictionary["Panic Bind"] = delegate(bool on)
		{
			SetImportant(delegate(ImportantRuntime runtime)
			{
				runtime.PanicBind = on;
			});
		};
		dictionary["Fling Touch"] = delegate(bool on)
		{
			SetOverpowered(delegate(OverpoweredRuntime runtime)
			{
				runtime.FlingTouch = on;
			});
		};
		dictionary["Size Shift"] = delegate(bool on)
		{
			SetOverpowered(delegate(OverpoweredRuntime runtime)
			{
				runtime.SizeShift = on;
			});
		};
		dictionary["Time Slow"] = delegate(bool on)
		{
			SetOverpowered(delegate(OverpoweredRuntime runtime)
			{
				runtime.TimeSlow = on;
			});
		};
		dictionary["Reality Skip"] = delegate(bool on)
		{
			SetOverpowered(delegate(OverpoweredRuntime runtime)
			{
				runtime.RealitySkip = on;
			});
		};
		dictionary["Magnet Hands"] = delegate(bool on)
		{
			SetOverpowered(delegate(OverpoweredRuntime runtime)
			{
				runtime.MagnetHands = on;
			});
		};
		dictionary["Force Push"] = delegate(bool on)
		{
			SetOverpowered(delegate(OverpoweredRuntime runtime)
			{
				runtime.ForcePush = on;
			});
		};
		dictionary["Force Grab Gun"] = delegate(bool on)
		{
			SetOverpowered(delegate(OverpoweredRuntime runtime)
			{
				runtime.ForceGrabGunEnabled = on;
			});
		};
		dictionary["Kick Gun [GR]"] = delegate(bool on)
		{
			SetOverpowered(delegate(OverpoweredRuntime runtime)
			{
				runtime.KickGunGR = on;
			});
		};
		dictionary["Crash Gun [GR]"] = delegate(bool on)
		{
			SetOverpowered(delegate(OverpoweredRuntime runtime)
			{
				runtime.CrashGun = on;
			});
		};
		dictionary["Break Movement Gun [GR]"] = delegate(bool on)
		{
			SetOverpowered(delegate(OverpoweredRuntime runtime)
			{
				runtime.BreakMovementGun = on;
			});
		};
		dictionary["Unbreak Movement Gun [GR]"] = delegate(bool on)
		{
			SetOverpowered(delegate(OverpoweredRuntime runtime)
			{
				runtime.UnbreakMovementGun = on;
			});
		};
		dictionary["Auto Try Get Zone Auth"] = delegate(bool on)
		{
			SetOverpowered(delegate(OverpoweredRuntime runtime)
			{
				runtime.AutoZoneAuth = on;
			});
		};
		AddResourceGun(dictionary, "Tech Point Gun [M]", SIResource.ResourceType.TechPoint);
		AddResourceGun(dictionary, "Strange Wood Gun [M]", SIResource.ResourceType.StrangeWood);
		AddResourceGun(dictionary, "Weird Gear Gun [M]", SIResource.ResourceType.WeirdGear);
		AddResourceGun(dictionary, "Vibrating Spring Gun [M]", SIResource.ResourceType.VibratingSpring);
		AddResourceGun(dictionary, "Bouncy Sand Gun [M]", SIResource.ResourceType.BouncySand);
		AddResourceGun(dictionary, "Floppy Metal Gun [M]", SIResource.ResourceType.FloppyMetal);
		dictionary["Block Gun [M]"] = delegate(bool on)
		{
			SetBlocks(delegate(BlocksRuntime runtime)
			{
				runtime.BlockGun = on;
			});
		};
		dictionary["Shape Gun [M]"] = delegate(bool on)
		{
			SetBlocks(delegate(BlocksRuntime runtime)
			{
				runtime.ShapeGun = on;
			});
		};
		dictionary["Block Rain [M]"] = delegate(bool on)
		{
			SetBlocks(delegate(BlocksRuntime runtime)
			{
				runtime.BlockRain = on;
			});
		};
		dictionary["Destroy Block Gun [M]"] = delegate(bool on)
		{
			SetBlocks(delegate(BlocksRuntime runtime)
			{
				runtime.DestroyBlockGun = on;
			});
		};
		dictionary["Block Platform [M]"] = delegate(bool on)
		{
			SetBlocks(delegate(BlocksRuntime runtime)
			{
				runtime.BlockPlatform = on;
			});
		};
		dictionary["GR Item Gun [Priv M]"] = delegate(bool on)
		{
			SetSpawner(delegate(SpawnerRuntime runtime)
			{
				runtime.GRItemGun = on;
			});
		};
		dictionary["GR Destroy Gun [Priv M]"] = delegate(bool on)
		{
			SetSpawner(delegate(SpawnerRuntime runtime)
			{
				runtime.GRDestroyGun = on;
			});
		};
		dictionary["Critter Gun [M]"] = delegate(bool on)
		{
			SetSpawner(delegate(SpawnerRuntime runtime)
			{
				runtime.CritterGun = on;
			});
		};
		dictionary["Critter Food Gun [M]"] = delegate(bool on)
		{
			SetSpawner(delegate(SpawnerRuntime runtime)
			{
				runtime.CritterFoodGun = on;
			});
		};
		dictionary["Critter Item Gun [M]"] = delegate(bool on)
		{
			SetSpawner(delegate(SpawnerRuntime runtime)
			{
				runtime.CritterItemGun = on;
			});
		};
		dictionary["All Resources Gun [M]"] = delegate(bool on)
		{
			SetSuperInfection(delegate(SuperInfectionRuntime runtime)
			{
				runtime.AllResourcesGun = on;
			});
		};
		dictionary["Destroy Resource Gun [M]"] = delegate(bool on)
		{
			SetSuperInfection(delegate(SuperInfectionRuntime runtime)
			{
				runtime.DestroyResourceGun = on;
			});
		};
		dictionary["Rig Spaz Gun"] = delegate(bool on)
		{
			SetRigGuns(delegate(RigGunRuntime runtime)
			{
				runtime.RigSpazGun = on;
			});
		};
		dictionary["Confuse Player Gun"] = delegate(bool on)
		{
			SetRigGuns(delegate(RigGunRuntime runtime)
			{
				runtime.ConfusePlayerGun = on;
			});
		};
		dictionary["Chase Player Gun"] = delegate(bool on)
		{
			SetRigGuns(delegate(RigGunRuntime runtime)
			{
				runtime.ChasePlayerGun = on;
			});
		};
		dictionary["Orbit Player Gun"] = delegate(bool on)
		{
			SetRigGuns(delegate(RigGunRuntime runtime)
			{
				runtime.OrbitPlayerGun = on;
			});
		};
		dictionary["Lag Gun"] = delegate(bool on)
		{
			SetProjectile(delegate(ProjectileRuntime runtime)
			{
				runtime.LagGun = on;
			});
		};
		dictionary["Snowball Cannon"] = delegate(bool on)
		{
			SetProjectile(delegate(ProjectileRuntime runtime)
			{
				runtime.SnowballCannon = on;
			});
		};
		dictionary["Bubble Blaster"] = delegate(bool on)
		{
			SetProjectile(delegate(ProjectileRuntime runtime)
			{
				runtime.BubbleBlaster = on;
			});
		};
		dictionary["Banana Gun"] = delegate(bool on)
		{
			SetProjectile(delegate(ProjectileRuntime runtime)
			{
				runtime.BananaGun = on;
			});
		};
		dictionary["Paint Ball"] = delegate(bool on)
		{
			SetProjectile(delegate(ProjectileRuntime runtime)
			{
				runtime.PaintBall = on;
			});
		};
		dictionary["Snow Volley"] = delegate(bool on)
		{
			SetProjectile(delegate(ProjectileRuntime runtime)
			{
				runtime.SnowVolley = on;
			});
		};
		dictionary["Bomb Toss"] = delegate(bool on)
		{
			SetProjectile(delegate(ProjectileRuntime runtime)
			{
				runtime.BombToss = on;
			});
		};
		dictionary["Laser Pointer"] = delegate(bool on)
		{
			SetProjectile(delegate(ProjectileRuntime runtime)
			{
				runtime.LaserPointer = on;
			});
		};
		dictionary["Tag Horn"] = delegate(bool on)
		{
			SetSound(delegate(SoundRuntime runtime)
			{
				runtime.TagHorn = on;
			});
		};
		dictionary["Ambient Drip"] = delegate(bool on)
		{
			SetSound(delegate(SoundRuntime runtime)
			{
				runtime.AmbientDrip = on;
			});
		};
		dictionary["Voice Changer"] = delegate(bool on)
		{
			SetSound(delegate(SoundRuntime runtime)
			{
				runtime.VoiceChanger = on;
			});
		};
		dictionary["Mute Others"] = delegate(bool on)
		{
			SetSound(delegate(SoundRuntime runtime)
			{
				runtime.MuteOthers = on;
			});
		};
		dictionary["Tag Echo"] = delegate(bool on)
		{
			SetSound(delegate(SoundRuntime runtime)
			{
				runtime.TagEcho = on;
			});
		};
		dictionary["ESP Dots"] = delegate(bool on)
		{
			SetVisual(delegate(VisualRuntime runtime)
			{
				runtime.ESP = on;
			});
		};
		dictionary["ESP"] = delegate(bool on)
		{
			SetVisual(delegate(VisualRuntime runtime)
			{
				runtime.ESP = on;
			});
		};
		dictionary["Tracer Lines"] = delegate(bool on)
		{
			SetVisual(delegate(VisualRuntime runtime)
			{
				runtime.Tracers = on;
			});
		};
		dictionary["Tracers"] = delegate(bool on)
		{
			SetVisual(delegate(VisualRuntime runtime)
			{
				runtime.Tracers = on;
			});
		};
		dictionary["Sky Replace"] = delegate(bool on)
		{
			SetVisual(delegate(VisualRuntime runtime)
			{
				runtime.SkyReplacement = on;
			});
		};
		dictionary["FPS Overlay"] = delegate(bool on)
		{
			SetVisual(delegate(VisualRuntime runtime)
			{
				runtime.EnlargeFps = on;
			});
		};
		dictionary["Player Outline"] = delegate(bool on)
		{
			SetVisual(delegate(VisualRuntime runtime)
			{
				runtime.Outline = on;
			});
		};
		dictionary["Night Vision"] = delegate(bool on)
		{
			SetVisual(delegate(VisualRuntime runtime)
			{
				runtime.NightVision = on;
			});
		};
		dictionary["Trail Fade"] = delegate(bool on)
		{
			SetVisual(delegate(VisualRuntime runtime)
			{
				runtime.Trails = on;
			});
		};
		dictionary["Arrow Pointer ESP"] = delegate(bool on) { SetVisual(r => r.ArrowPointerESP = on); };
		dictionary["Beacon ESP"] = delegate(bool on) { SetVisual(r => r.BeaconESP = on); };
		dictionary["Bone ESP"] = delegate(bool on) { SetVisual(r => r.BoneESP = on); };
		dictionary["Capsule ESP"] = delegate(bool on) { SetVisual(r => r.CapsuleESP = on); };
		dictionary["Chams"] = delegate(bool on) { SetVisual(r => r.Chams = on); };
		dictionary["Cylinder ESP"] = delegate(bool on) { SetVisual(r => r.CylinderESP = on); };
		dictionary["Diamond ESP"] = delegate(bool on) { SetVisual(r => r.DiamondESP = on); };
		dictionary["Distance Tracers"] = delegate(bool on) { SetVisual(r => r.DistanceTracers = on); };
		dictionary["Double Hand Tracers"] = delegate(bool on) { SetVisual(r => r.DoubleHandTracers = on); };
		dictionary["Floor Marker ESP"] = delegate(bool on) { SetVisual(r => r.FloorMarkerESP = on); };
		dictionary["Full Player Info"] = delegate(bool on) { SetVisual(r => r.FullPlayerInfo = on); };
		dictionary["Grip Indicator"] = delegate(bool on) { SetVisual(r => r.GripIndicator = on); };
		dictionary["Grip Tracker"] = delegate(bool on) { SetVisual(r => r.GripTracker = on); };
		dictionary["Hand Comets"] = delegate(bool on) { SetVisual(r => r.HandComets = on); };
		dictionary["Hand ESP"] = delegate(bool on) { SetVisual(r => r.HandESP = on); };
		dictionary["Head Sphere ESP"] = delegate(bool on) { SetVisual(r => r.HeadSphereESP = on); };
		dictionary["Head Tracers"] = delegate(bool on) { SetVisual(r => r.HeadTracers = on); };
		dictionary["Lightning Hands"] = delegate(bool on) { SetVisual(r => r.LightningHands = on); };
		dictionary["Modder Tracers"] = delegate(bool on) { SetVisual(r => r.ModderTracers = on); };
		dictionary["Neon Outline"] = delegate(bool on) { SetVisual(r => r.NeonOutline = on); };
		dictionary["Pulse Chams"] = delegate(bool on) { SetVisual(r => r.PulseChams = on); };
		dictionary["Pulse Tracers"] = delegate(bool on) { SetVisual(r => r.PulseTracers = on); };
		dictionary["Ring ESP"] = delegate(bool on) { SetVisual(r => r.RingESP = on); };
		dictionary["Shadow Clone"] = delegate(bool on) { SetVisual(r => r.ShadowClone = on); };
		dictionary["Show Distance"] = delegate(bool on) { SetVisual(r => r.ShowDistance = on); };
		dictionary["Show Join Date All"] = delegate(bool on) { SetVisual(r => r.ShowJoinDateAll = on); };
		dictionary["Show Platform"] = delegate(bool on) { SetVisual(r => r.ShowPlatform = on); };
		dictionary["Show Velocity"] = delegate(bool on) { SetVisual(r => r.ShowVelocity = on); };
		dictionary["Skeleton ESP V2"] = delegate(bool on) { SetVisual(r => r.SkeletonESPV2 = on); };
		dictionary["Sparkler Hands"] = delegate(bool on) { SetVisual(r => r.SparklerHands = on); };
		dictionary["Sphere ESP"] = delegate(bool on) { SetVisual(r => r.SphereESP = on); };
		dictionary["Thick Tracers"] = delegate(bool on) { SetVisual(r => r.ThickTracers = on); };
		dictionary["Thin ESP"] = delegate(bool on) { SetVisual(r => r.ThinESP = on); };

		// Gamemode
		dictionary["Tag Gun"] = delegate(bool on) { SetGamemode(r => r.TagGun = on); };
		dictionary["Tag All Except"] = delegate(bool on) { SetGamemode(r => r.TagAllExceptGun = on); };
		dictionary["Tag Aura"] = delegate(bool on) { SetGamemode(r => r.TagAura = on); };
		dictionary["UnTag Gun (M)"] = delegate(bool on) { SetGamemode(r => r.UnTagGunM = on); };
		dictionary["UnTag Aura (M)"] = delegate(bool on) { SetGamemode(r => r.UnTagAuraM = on); };
		dictionary["UnTag Others (M)"] = delegate(bool on) { SetGamemode(r => r.UnTagOthersM = on); };
		dictionary["Mat Gun (M)"] = delegate(bool on) { SetGamemode(r => r.MatGunM = on); };
		dictionary["Mat Aura (M)"] = delegate(bool on) { SetGamemode(r => r.MatAuraM = on); };
		dictionary["Flick Tag Gun"] = delegate(bool on) { SetGamemode(r => r.FlickTagGun = on); };
		dictionary["Guardian Fling Gun"] = delegate(bool on) { SetGamemode(r => r.GuardianFlingGun = on); };
		dictionary["Guardian Spaz Gun"] = delegate(bool on) { SetGamemode(r => r.GuardianSpazGun = on); };
		dictionary["Guardian Orbit Gun"] = delegate(bool on) { SetGamemode(r => r.GuardianOrbitGun = on); };
		dictionary["Guardian To Point All"] = delegate(bool on) { SetGamemode(r => r.GuardianToPointAll = on); };
		dictionary["PaintBrawl Hard AimBot"] = delegate(bool on) { SetGamemode(r => r.PaintBrawlHardAimbot = on); };
		dictionary["PaintBrawl Soft AimBot"] = delegate(bool on) { SetGamemode(r => r.PaintBrawlSoftAimbot = on); };
		dictionary["Set Orange Team Gun (M)"] = delegate(bool on) { SetGamemode(r => r.SetOrangeTeamGunM = on); };
		dictionary["Set Blue Team Gun (M)"] = delegate(bool on) { SetGamemode(r => r.SetBlueTeamGunM = on); };
		dictionary["Set No Team Gun (M)"] = delegate(bool on) { SetGamemode(r => r.SetNoTeamGunM = on); };
		dictionary["Set Stunned Gun (M)"] = delegate(bool on) { SetGamemode(r => r.SetStunnedGunM = on); };
		dictionary["Set Hit Gun (M)"] = delegate(bool on) { SetGamemode(r => r.SetHitGunM = on); };
		dictionary["GodMode Gun (M)"] = delegate(bool on) { SetGamemode(r => r.GodModeGunM = on); };
		dictionary["UnGodMode Gun (M)"] = delegate(bool on) { SetGamemode(r => r.UnGodModeGunM = on); };

		dictionary["Ghost Monke (A)"] = delegate(bool on)
		{
			RigBank.ghostMonke = on;
		};
		dictionary["Invis Monke (X)"] = delegate(bool on)
		{
			RigBank.invisMonke = on;
		};
		dictionary["Spaz Monke (B)"] = delegate(bool on)
		{
			RigBank.spazMonke = on;
		};
		dictionary["Wacky Monke (B)"] = delegate(bool on)
		{
			RigBank.wackyMonke = on;
		};
		dictionary["Rig Gun"] = delegate(bool on)
		{
			RigBank.rigGun = on;
		};
		dictionary["Confuse Player Gun"] = delegate(bool on)
		{
			RigBank.confuseGun = on;
		};
		dictionary["Chase Player Gun"] = delegate(bool on)
		{
			RigBank.chaseGun = on;
		};
		dictionary["Fast Chase Player Gun"] = delegate(bool on)
		{
			RigBank.fastChaseGun = on;
		};
		dictionary["Orbit Player Gun"] = delegate(bool on)
		{
			RigBank.orbitGun = on;
		};
		dictionary["Grab Rig"] = delegate(bool on)
		{
			RigBank.grabRig = on;
		};
		dictionary["Fake Lag"] = delegate(bool on)
		{
			RigBank.fakeLag = on;
		};
		dictionary["Fast Fake Lag"] = delegate(bool on)
		{
			RigBank.fastFakeLag = on;
		};
		dictionary["Head Spin V1"] = delegate(bool on)
		{
			if (on)
			{
				RigBank.headSpinTier = 1;
			}
			else if (RigBank.headSpinTier == 1)
			{
				RigBank.headSpinTier = 0;
			}
		};
		dictionary["Head Spin V2"] = delegate(bool on)
		{
			if (on)
			{
				RigBank.headSpinTier = 2;
			}
			else if (RigBank.headSpinTier == 2)
			{
				RigBank.headSpinTier = 0;
			}
		};
		dictionary["Head Spin V3"] = delegate(bool on)
		{
			if (on)
			{
				RigBank.headSpinTier = 3;
			}
			else if (RigBank.headSpinTier == 3)
			{
				RigBank.headSpinTier = 0;
			}
		};
		dictionary["Fake Grey Screen (RJ)"] = delegate(bool on)
		{
			RigBank.fakeGrey = on;
		};
		dictionary["Backwards Head"] = delegate(bool on)
		{
			RigBank.backwardsHead = on;
		};
		dictionary["Backwards Body"] = delegate(bool on)
		{
			RigBank.backwardsBody = on;
		};
		dictionary["Backwards Rig"] = delegate(bool on)
		{
			RigBank.backwardsRig = on;
		};
		dictionary["Upside Down Head"] = delegate(bool on)
		{
			RigBank.upsideDownHead = on;
		};
		dictionary["Upside Down Body"] = delegate(bool on)
		{
			RigBank.upsideDownBody = on;
		};
		dictionary["Upside Down Rig"] = delegate(bool on)
		{
			RigBank.upsideDownRig = on;
		};
		dictionary["Forwards Body"] = delegate(bool on)
		{
			RigBank.forwardsBody = on;
		};
		dictionary["Forwards Rig"] = delegate(bool on)
		{
			RigBank.forwardsRig = on;
		};
		dictionary["Crawling Body"] = delegate(bool on)
		{
			RigBank.crawlingBody = on;
		};
		dictionary["Crawling Rig"] = delegate(bool on)
		{
			RigBank.crawlingRig = on;
		};
		dictionary["Spaz Head"] = delegate(bool on)
		{
			RigBank.spazHead = on;
		};
		dictionary["Spaz Body"] = delegate(bool on)
		{
			RigBank.spazBody = on;
		};
		dictionary["Spaz Rig"] = delegate(bool on)
		{
			RigBank.spazRig = on;
		};
		dictionary["Fake Full Body Tracking"] = delegate(bool on)
		{
			RigBank.fakeFBT = on;
		};
		dictionary["Spin Bot V1"] = delegate(bool on)
		{
			if (on)
			{
				RigBank.spinBotTier = 1;
			}
			else if (RigBank.spinBotTier == 1)
			{
				RigBank.spinBotTier = 0;
			}
		};
		dictionary["Spin Bot V2"] = delegate(bool on)
		{
			if (on)
			{
				RigBank.spinBotTier = 2;
			}
			else if (RigBank.spinBotTier == 2)
			{
				RigBank.spinBotTier = 0;
			}
		};
		dictionary["Spin Bot V3"] = delegate(bool on)
		{
			if (on)
			{
				RigBank.spinBotTier = 3;
			}
			else if (RigBank.spinBotTier == 3)
			{
				RigBank.spinBotTier = 0;
			}
		};
		dictionary["Spin Bot V4"] = delegate(bool on)
		{
			if (on)
			{
				RigBank.spinBotTier = 4;
			}
			else if (RigBank.spinBotTier == 4)
			{
				RigBank.spinBotTier = 0;
			}
		};
		dictionary["Spin Bot V5"] = delegate(bool on)
		{
			if (on)
			{
				RigBank.spinBotTier = 5;
			}
			else if (RigBank.spinBotTier == 5)
			{
				RigBank.spinBotTier = 0;
			}
		};
		dictionary["Upside Down Spin Bot"] = delegate(bool on)
		{
			RigBank.upsideDownSpinBot = on;
		};
		dictionary["Full Spin Bot"] = delegate(bool on)
		{
			RigBank.fullSpinBot = on;
		};
		dictionary["Backflip (RJ)"] = delegate(bool on)
		{
			RigBank.backflipOn = on;
		};
		dictionary["Frontflip (LJ)"] = delegate(bool on)
		{
			RigBank.frontflipOn = on;
		};
		dictionary["PC Button Clicker"] = delegate(bool on)
		{
			SetPc(delegate(PcRuntime runtime)
			{
				runtime.ClickerEnabled = on;
			});
		};
		dictionary["Platforms"] = delegate(bool on)
		{
			SetMovementPlus(delegate(MovementPlusRuntime runtime)
			{
				runtime.Platforms = on;
			});
		};
		dictionary["Noclip"] = delegate(bool on)
		{
			SetMovementPlus(delegate(MovementPlusRuntime runtime)
			{
				runtime.Noclip = on;
			});
		};
		dictionary["Teleport Pointer"] = delegate(bool on)
		{
			SetMovementPlus(delegate(MovementPlusRuntime runtime)
			{
				runtime.TeleportPointer = on;
			});
		};
		dictionary["Checkpoint"] = delegate(bool on)
		{
			SetMovementPlus(delegate(MovementPlusRuntime runtime)
			{
				runtime.Checkpoint = on;
			});
		};
		dictionary["Wall Walk"] = delegate(bool on)
		{
			SetMovementPlus(delegate(MovementPlusRuntime runtime)
			{
				runtime.WallWalk = on;
			});
		};
		dictionary["Super Monke"] = delegate(bool on)
		{
			SetMovementPlus(delegate(MovementPlusRuntime runtime)
			{
				runtime.SuperMonke = on;
			});
		};
		dictionary["Pull Mod"] = delegate(bool on)
		{
			SetMovementPlus(delegate(MovementPlusRuntime runtime)
			{
				runtime.PullMod = on;
			});
		};
		dictionary["Grip Jump"] = delegate(bool on)
		{
			SetMovementPlus(delegate(MovementPlusRuntime runtime)
			{
				runtime.GripJump = on;
			});
		};
		return dictionary;
	}

	private static void SetPc(Action<PcRuntime> apply)
	{
		PcRuntime instance = PcRuntime.Instance;
		if (!((Object)(object)instance == (Object)null))
		{
			apply(instance);
		}
	}

	private static void SetMovementPlus(Action<MovementPlusRuntime> apply)
	{
		MovementPlusRuntime instance = MovementPlusRuntime.Instance;
		if (!((Object)(object)instance == (Object)null))
		{
			apply(instance);
		}
	}

	private static void Set(Action<ModRuntime> apply)
	{
		ModRuntime instance = ModRuntime.Instance;
		if (!((Object)(object)instance == (Object)null))
		{
			apply(instance);
		}
	}

	private static void SetSafety(Action<SafetyRuntime> apply)
	{
		SafetyRuntime instance = SafetyRuntime.Instance;
		if (!((Object)(object)instance == (Object)null))
		{
			apply(instance);
		}
	}

	private static void SetFun(Action<FunRuntime> apply)
	{
		FunRuntime instance = FunRuntime.Instance;
		if (!((Object)(object)instance == (Object)null))
		{
			apply(instance);
		}
	}

	private static void SetAdvantage(Action<AdvantageRuntime> apply)
	{
		AdvantageRuntime instance = AdvantageRuntime.Instance;
		if (!((Object)(object)instance == (Object)null))
		{
			apply(instance);
		}
	}

	private static void SetDetection(Action<DetectionRuntime> apply)
	{
		DetectionRuntime instance = DetectionRuntime.Instance;
		if (!((Object)(object)instance == (Object)null))
		{
			apply(instance);
		}
	}

	private static void SetExperimental(Action<ExperimentalRuntime> apply)
	{
		ExperimentalRuntime instance = ExperimentalRuntime.Instance;
		if (!((Object)(object)instance == (Object)null))
		{
			apply(instance);
		}
	}

	private static void SetImportant(Action<ImportantRuntime> apply)
	{
		ImportantRuntime instance = ImportantRuntime.Instance;
		if (!((Object)(object)instance == (Object)null))
		{
			apply(instance);
		}
	}

	private static void SetOverpowered(Action<OverpoweredRuntime> apply)
	{
		OverpoweredRuntime instance = OverpoweredRuntime.Instance;
		if (!((Object)(object)instance == (Object)null))
		{
			apply(instance);
		}
	}

	private static void SetSuperInfection(Action<SuperInfectionRuntime> apply)
	{
		SuperInfectionRuntime instance = SuperInfectionRuntime.Instance;
		if (!((Object)(object)instance == (Object)null))
		{
			apply(instance);
		}
	}

	private static void SetBlocks(Action<BlocksRuntime> apply)
	{
		BlocksRuntime instance = BlocksRuntime.Instance;
		if (!((Object)(object)instance == (Object)null))
		{
			apply(instance);
		}
	}

	private static void SetSpawner(Action<SpawnerRuntime> apply)
	{
		SpawnerRuntime instance = SpawnerRuntime.Instance;
		if (!((Object)(object)instance == (Object)null))
		{
			apply(instance);
		}
	}

	private static void AddResourceGun(Dictionary<string, Action<bool>> dictionary, string title, SIResource.ResourceType type)
	{
		dictionary[title] = delegate(bool on)
		{
			SetSuperInfection(delegate(SuperInfectionRuntime runtime)
			{
				runtime.SetGun(type, on);
			});
		};
	}

	private static void SetRigGuns(Action<RigGunRuntime> apply)
	{
		RigGunRuntime instance = RigGunRuntime.Instance;
		if (!((Object)(object)instance == (Object)null))
		{
			apply(instance);
		}
	}

	private static void SetProjectile(Action<ProjectileRuntime> apply)
	{
		ProjectileRuntime instance = ProjectileRuntime.Instance;
		if (!((Object)(object)instance == (Object)null))
		{
			apply(instance);
		}
	}

	private static void SetSound(Action<SoundRuntime> apply)
	{
		SoundRuntime instance = SoundRuntime.Instance;
		if (!((Object)(object)instance == (Object)null))
		{
			apply(instance);
		}
	}

	private static void SetVisual(Action<VisualRuntime> apply)
	{
		VisualRuntime instance = VisualRuntime.Instance;
		if (!((Object)(object)instance == (Object)null))
		{
			apply(instance);
		}
	}

	private static void SetGamemode(Action<GamemodeRuntime> apply)
	{
		GamemodeRuntime instance = GamemodeRuntime.Instance;
		if (!((Object)(object)instance == (Object)null))
		{
			apply(instance);
		}
	}
}
