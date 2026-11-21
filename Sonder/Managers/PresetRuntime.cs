using Sonder.Core;

namespace Sonder.Managers;

public static class PresetRuntime
{
	private static readonly string[] MovementPack = new string[9] { "Fly", "Speed Boost", "High Jump", "Dash", "Low Gravity", "Bouncy", "Feather Fall", "Slide Control", "Ground Helper" };

	private static readonly string[] ChaosPack = new string[15]
	{
		"Rainbow Fur", "Spin Head", "Spaz Head", "Head Bang", "Flip Hands", "Auto Clicker", "Loud Hand Taps", "Nausea", "Spawn Hoverboard", "ESP Dots",
		"Tracer Lines", "Player Outline", "Trail Fade", "Tag Horn", "Ambient Drip"
	};

	private static readonly string[] StealthPack = new string[7] { "Low Drag", "Sticky Hands", "Sturdy Grip", "Iron Legs", "Anti Fall", "Respawn Lock", "Silent Hand Taps" };

	private static readonly string[] SpeedRunPack = new string[8] { "Speed Boost", "High Jump", "Bunny Hop", "Slide Control", "Dash", "Strafe", "Feather Fall", "Low Drag" };

	private static readonly string[] BuilderPack = new string[7] { "Fly", "Long Arms", "Joystick Fly", "WASD Fly", "Ground Helper", "Slide Control", "Anti Fall" };

	private static readonly string[] WreckingBallPack = new string[7] { "Fling Touch", "Force Push", "Magnet Hands", "Bouncy", "High Gravity", "Speed Boost", "Size Shift" };

	public static void ApplyDefault()
	{
		ModManager.ApplySet(null);
		SonderAudio.Alert(0.5f);
		RuntimeLog.Write("preset: default");
	}

	public static void ApplyMovement()
	{
		Apply(MovementPack, "movement");
	}

	public static void ApplyChaos()
	{
		Apply(ChaosPack, "chaos");
	}

	public static void ApplyStealth()
	{
		Apply(StealthPack, "stealth");
	}

	public static void ApplySpeedRun()
	{
		Apply(SpeedRunPack, "speed run");
	}

	public static void ApplyBuilder()
	{
		Apply(BuilderPack, "builder");
	}

	public static void ApplyWreckingBall()
	{
		Apply(WreckingBallPack, "wrecking ball");
	}

	public static void SaveCurrent()
	{
		ModManager.SaveBinds("manual save");
		SonderAudio.Alert(0.5f);
	}

	private static void Apply(string[] titles, string name)
	{
		ModManager.ApplySet(titles);
		SonderAudio.Alert(0.5f);
		RuntimeLog.Write("preset: " + name);
	}
}
