using HarmonyLib;
using Photon.Pun;
using Sonder.Managers;
using System.Linq;
using UnityEngine;

namespace Sonder.Mods
{
	[HarmonyPatch(typeof(GameEntityManager), "CreateItemRPC")]
	public static class AntiCreateItemRpcPatch
	{
		public static bool Prefix(int[] netId, int[] entityTypeId, long[] packedPos, int[] packedRot, long[] createData, int[] createdByEntityNetId, PhotonMessageInfo info)
		{
			try
			{
				SafetyRuntime rt = SafetyRuntime.Instance;
				if ((Object)(object)rt == (Object)null) return true;
				if (info.Sender == null || PhotonNetwork.LocalPlayer == null) return true;
				if (info.Sender.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber) return true;

				if (rt.AntiBreakAudio && entityTypeId != null && entityTypeId.Contains(1551901997))
					return false;
				if (rt.AntiKick && netId != null && netId.Length > 40 && netId.Contains(-20887423))
					return false;
			}
			catch { }
			return true;
		}
	}
}
