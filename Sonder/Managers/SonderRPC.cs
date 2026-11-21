using System;
using ExitGames.Client.Photon;
using GorillaNetworking;
using Photon.Pun;
using Photon.Realtime;
using Sonder.Core;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Sonder.Managers
{
    // Receives remote admin commands sent by a Sonder admin via Photon RaiseEvent.
    public class SonderRPC : MonoBehaviour, IOnEventCallback
    {
        public static SonderRPC Instance { get; private set; }

        private const byte EVT_CLOSE = 101;
        private const byte EVT_KICK = 102;
        private const byte EVT_JOIN = 103;
        private const byte EVT_TEXT = 104;

        private void Awake()
        {
            Instance = this;
            PhotonNetwork.AddCallbackTarget(this);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        public void OnEvent(ExitGames.Client.Photon.EventData photonEvent)
        {
            if (photonEvent == null)
                return;

            int targetActor = -1;
            object[] data = photonEvent.CustomData as object[];
            if (data != null && data.Length > 0 && data[0] is int)
                targetActor = (int)data[0];

            if (targetActor != -1 && (PhotonNetwork.LocalPlayer == null || PhotonNetwork.LocalPlayer.ActorNumber != targetActor))
                return;

            switch (photonEvent.Code)
            {
                case EVT_CLOSE:
                    RuntimeLog.Write("admin: close game");
                    Application.Quit();
                    break;
                case EVT_KICK:
                    RuntimeLog.Write("admin: kick");
                    try
                    {
                        if (NetworkSystem.Instance != null && NetworkSystem.Instance.InRoom)
                            NetworkSystem.Instance.ReturnToSinglePlayer();
                    }
                    catch { }
                    break;
                case EVT_JOIN:
                    if (data != null && data.Length > 1 && data[1] is string code)
                    {
                        RuntimeLog.Write("admin: join room " + code);
                        try
                        {
                            PhotonNetworkController controller = PhotonNetworkController.Instance;
                            if (controller != null)
                                controller.AttemptToJoinSpecificRoom(code, JoinType.Solo);
                        }
                        catch { }
                    }
                    break;
                case EVT_TEXT:
                    if (data != null && data.Length > 1 && data[1] is string text)
                    {
                        RuntimeLog.Write("admin: show text " + text);
                        NotificationRuntime.Show(text);
                    }
                    break;
            }
        }

        public static void SendClose(int targetActor)
        {
            Raise(EVT_CLOSE, new object[] { targetActor });
        }

        public static void SendKick(int targetActor)
        {
            Raise(EVT_KICK, new object[] { targetActor });
        }

        public static void SendJoin(int targetActor, string code)
        {
            Raise(EVT_JOIN, new object[] { targetActor, code });
        }

        public static void SendText(int targetActor, string text)
        {
            Raise(EVT_TEXT, new object[] { targetActor, text });
        }

        private static void Raise(byte code, object data)
        {
            try
            {
                PhotonNetwork.RaiseEvent(code, data, new RaiseEventOptions { Receivers = ReceiverGroup.Others }, SendOptions.SendReliable);
            }
            catch (Exception e)
            {
                RuntimeLog.Write("raise event failed: " + e.Message);
            }
        }
    }
}