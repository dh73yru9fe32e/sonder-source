using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using Photon.Pun;
using Sonder.Managers;
using UnityEngine;

namespace Sonder.Core;

// Talks to the Discord desktop client over its local IPC pipe, so no extra DLLs ship with the menu.
public class DiscordPresence : MonoBehaviour
{
	// Application ID from https://discord.com/developers/applications. The app's name is what shows as "Playing ...".
	private const string ClientId = "1566119965235609691";

	// Art asset key uploaded under Rich Presence > Art Assets (or a direct https image URL).
	private const string LargeImage = "sonder";

	// Optional button on the profile card. Leave empty to hide it.
	private const string ButtonLabel = "Join Discord";

	private const string ButtonUrl = "https://discord.gg/V75WZnvHG";

	private const string Version = "1.0.0";

	private const float RefreshInterval = 5f;

	private const int RetryMilliseconds = 15000;

	private readonly object _lock = new object();

	private readonly long _startedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

	private Thread _worker;

	private volatile bool _running;

	private string _pendingActivity;

	private bool _hasPending;

	private string _lastSnapshot;

	private float _nextRefresh;

	private NamedPipeClientStream _pipe;

	private int _pid;

	private void Start()
	{
		if (ClientId.Length == 0 || !char.IsDigit(ClientId[0]))
		{
			RuntimeLog.Write("discord: no client id, presence disabled");
			((Behaviour)this).enabled = false;
			return;
		}
		_pid = Process.GetCurrentProcess().Id;
		_running = true;
		_worker = new Thread(WorkerLoop)
		{
			IsBackground = true,
			Name = "Sonder.Discord"
		};
		_worker.Start();
	}

	private void Update()
	{
		if (Time.unscaledTime < _nextRefresh)
		{
			return;
		}
		_nextRefresh = Time.unscaledTime + RefreshInterval;
		string text = SonderSettings.DiscordOn ? BuildActivity() : null;
		if (text == _lastSnapshot)
		{
			return;
		}
		_lastSnapshot = text;
		lock (_lock)
		{
			_pendingActivity = text;
			_hasPending = true;
		}
	}

	private void OnDestroy()
	{
		Shutdown();
	}

	private void OnApplicationQuit()
	{
		Shutdown();
	}

	private void Shutdown()
	{
		_running = false;
		Disconnect();
	}

	private string BuildActivity()
	{
		int count = 0;
		try
		{
			count = ModManager.ActiveTitles().Count;
		}
		catch
		{
		}
		string details = count == 0 ? "Browsing the menu" : count == 1 ? "1 mod enabled" : count + " mods enabled";
		string state;
		try
		{
			// Player count only - never the room code, so nobody can follow you in from your profile.
			state = PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom != null
				? "In a room (" + PhotonNetwork.CurrentRoom.PlayerCount + " of " + PhotonNetwork.CurrentRoom.MaxPlayers + ")"
				: "In the lobby";
		}
		catch
		{
			state = "In Gorilla Tag";
		}
		StringBuilder sb = new StringBuilder();
		sb.Append("{\"details\":").Append(Json(details));
		sb.Append(",\"state\":").Append(Json(state));
		sb.Append(",\"timestamps\":{\"start\":").Append(_startedAt).Append('}');
		sb.Append(",\"assets\":{\"large_image\":").Append(Json(LargeImage));
		sb.Append(",\"large_text\":").Append(Json("Sonder v" + Version)).Append('}');
		if (ButtonLabel.Length > 0 && ButtonUrl.Length > 0)
		{
			sb.Append(",\"buttons\":[{\"label\":").Append(Json(ButtonLabel));
			sb.Append(",\"url\":").Append(Json(ButtonUrl)).Append("}]");
		}
		sb.Append('}');
		return sb.ToString();
	}

	private void WorkerLoop()
	{
		while (_running)
		{
			try
			{
				if (_pipe == null && !Connect())
				{
					Thread.Sleep(RetryMilliseconds);
					continue;
				}
				string activity;
				bool send;
				lock (_lock)
				{
					activity = _pendingActivity;
					send = _hasPending;
					_hasPending = false;
				}
				if (send)
				{
					string args = "{\"pid\":" + _pid + ",\"activity\":" + (activity ?? "null") + "}";
					Send(1, "{\"cmd\":\"SET_ACTIVITY\",\"args\":" + args + ",\"nonce\":\"" + Guid.NewGuid().ToString("N") + "\"}");
					Receive();
				}
				Thread.Sleep(500);
			}
			catch (Exception ex)
			{
				RuntimeLog.Write("discord: connection lost: " + ex.Message);
				Disconnect();
				lock (_lock)
				{
					// Resend the latest state once we reconnect.
					_pendingActivity = _lastSnapshot;
					_hasPending = true;
				}
				Thread.Sleep(RetryMilliseconds);
			}
		}
	}

	private bool Connect()
	{
		for (int i = 0; i < 10; i++)
		{
			NamedPipeClientStream pipe = new NamedPipeClientStream(".", "discord-ipc-" + i, PipeDirection.InOut, PipeOptions.None);
			try
			{
				pipe.Connect(200);
				_pipe = pipe;
				Send(0, "{\"v\":1,\"client_id\":" + Json(ClientId) + "}");
				Receive();
				RuntimeLog.Write("discord: connected on pipe " + i);
				return true;
			}
			catch
			{
				_pipe = null;
				pipe.Dispose();
			}
		}
		return false;
	}

	private void Disconnect()
	{
		try
		{
			_pipe?.Dispose();
		}
		catch
		{
		}
		_pipe = null;
	}

	private void Send(int opcode, string json)
	{
		byte[] body = Encoding.UTF8.GetBytes(json);
		byte[] frame = new byte[8 + body.Length];
		BitConverter.GetBytes(opcode).CopyTo(frame, 0);
		BitConverter.GetBytes(body.Length).CopyTo(frame, 4);
		body.CopyTo(frame, 8);
		_pipe.Write(frame, 0, frame.Length);
		_pipe.Flush();
	}

	private string Receive()
	{
		byte[] header = ReadExact(8);
		int opcode = BitConverter.ToInt32(header, 0);
		int length = BitConverter.ToInt32(header, 4);
		string json = Encoding.UTF8.GetString(ReadExact(length));
		if (opcode == 2)
		{
			throw new IOException("discord closed the connection: " + json);
		}
		return json;
	}

	private byte[] ReadExact(int count)
	{
		byte[] buffer = new byte[count];
		int offset = 0;
		while (offset < count)
		{
			int read = _pipe.Read(buffer, offset, count - offset);
			if (read <= 0)
			{
				throw new IOException("pipe closed");
			}
			offset += read;
		}
		return buffer;
	}

	private static string Json(string value)
	{
		StringBuilder sb = new StringBuilder("\"");
		foreach (char c in value)
		{
			switch (c)
			{
			case '"':
				sb.Append("\\\"");
				break;
			case '\\':
				sb.Append("\\\\");
				break;
			default:
				if (c < ' ')
				{
					sb.Append("\\u").Append(((int)c).ToString("x4"));
				}
				else
				{
					sb.Append(c);
				}
				break;
			}
		}
		return sb.Append('"').ToString();
	}
}
