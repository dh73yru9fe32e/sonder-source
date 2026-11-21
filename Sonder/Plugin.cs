using BepInEx;
using HarmonyLib;
using Sonder.Managers;
using UnityEngine;

namespace Sonder;

[BepInPlugin("com.sonder.menu", "Sonder", "1.0.3")]
public class Plugin : BaseUnityPlugin
{
	private static bool _patched;

	private void Awake()
	{
		if (!_patched)
		{
			try
			{
				new Harmony("com.sonder.menu").PatchAll();
			}
			catch (System.Exception ex)
			{
				Debug.Log("[Sonder] PatchAll failed: " + ex);
			}
			_patched = true;
		}
		MenuManager.Initialize();
		BoardRuntime.Initialize();
	}
}
