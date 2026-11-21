using System.Collections.Generic;
using System.Reflection;
using GorillaTagScripts;
using Photon.Pun;
using Sonder.Core;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Sonder.Managers;

// Master-only Monkey Blocks building tools. Blocks come only from sets the local player owns.
public class BlocksRuntime : MonoBehaviour
{
	private struct BlockInfo
	{
		public int Type;

		public int Material;

		public string Name;

		public float Size;
	}

	private struct PendingBlock
	{
		public int Type;

		public int Material;

		public Vector3 Position;

		public Quaternion Rotation;

		public float Lifetime;
	}

	private const float SpawnInterval = 0.025f;

	private const float GunDelay = 0.2f;

	private const float PlatformDelay = 0.15f;

	private const float PlatformLifetime = 3f;

	private const int MaxPerShape = 80;

	private static readonly FieldInfo NextPieceIdField = typeof(BuilderTable).GetField("nextPieceId", BindingFlags.Instance | BindingFlags.NonPublic);

	private static readonly string[] ShapeNames = new string[4] { "WALL", "TOWER", "STAIRS", "DOME" };

	// 5x5 pixel font, top row first.
	private static readonly Dictionary<char, string[]> Font = new Dictionary<char, string[]>
	{
		['A'] = new string[5] { ".###.", "#...#", "#####", "#...#", "#...#" },
		['B'] = new string[5] { "####.", "#...#", "####.", "#...#", "####." },
		['C'] = new string[5] { ".####", "#....", "#....", "#....", ".####" },
		['D'] = new string[5] { "####.", "#...#", "#...#", "#...#", "####." },
		['E'] = new string[5] { "#####", "#....", "####.", "#....", "#####" },
		['F'] = new string[5] { "#####", "#....", "####.", "#....", "#...." },
		['G'] = new string[5] { ".####", "#....", "#..##", "#...#", ".###." },
		['H'] = new string[5] { "#...#", "#...#", "#####", "#...#", "#...#" },
		['I'] = new string[5] { "#####", "..#..", "..#..", "..#..", "#####" },
		['J'] = new string[5] { "..###", "...#.", "...#.", "#..#.", ".##.." },
		['K'] = new string[5] { "#...#", "#..#.", "###..", "#..#.", "#...#" },
		['L'] = new string[5] { "#....", "#....", "#....", "#....", "#####" },
		['M'] = new string[5] { "#...#", "##.##", "#.#.#", "#...#", "#...#" },
		['N'] = new string[5] { "#...#", "##..#", "#.#.#", "#..##", "#...#" },
		['O'] = new string[5] { ".###.", "#...#", "#...#", "#...#", ".###." },
		['P'] = new string[5] { "####.", "#...#", "####.", "#....", "#...." },
		['Q'] = new string[5] { ".###.", "#...#", "#.#.#", "#..#.", ".##.#" },
		['R'] = new string[5] { "####.", "#...#", "####.", "#..#.", "#...#" },
		['S'] = new string[5] { ".####", "#....", ".###.", "....#", "####." },
		['T'] = new string[5] { "#####", "..#..", "..#..", "..#..", "..#.." },
		['U'] = new string[5] { "#...#", "#...#", "#...#", "#...#", ".###." },
		['V'] = new string[5] { "#...#", "#...#", "#...#", ".#.#.", "..#.." },
		['W'] = new string[5] { "#...#", "#...#", "#.#.#", "##.##", "#...#" },
		['X'] = new string[5] { "#...#", ".#.#.", "..#..", ".#.#.", "#...#" },
		['Y'] = new string[5] { "#...#", ".#.#.", "..#..", "..#..", "..#.." },
		['Z'] = new string[5] { "#####", "...#.", "..#..", ".#...", "#####" },
		['0'] = new string[5] { ".###.", "#..##", "#.#.#", "##..#", ".###." },
		['1'] = new string[5] { "..#..", ".##..", "..#..", "..#..", ".###." },
		['2'] = new string[5] { "####.", "....#", ".###.", "#....", "#####" },
		['3'] = new string[5] { "####.", "....#", ".###.", "....#", "####." },
		['4'] = new string[5] { "#...#", "#...#", "#####", "....#", "....#" },
		['5'] = new string[5] { "#####", "#....", "####.", "....#", "####." },
		['6'] = new string[5] { ".###.", "#....", "####.", "#...#", ".###." },
		['7'] = new string[5] { "#####", "....#", "...#.", "..#..", "..#.." },
		['8'] = new string[5] { ".###.", "#...#", ".###.", "#...#", ".###." },
		['9'] = new string[5] { ".###.", "#...#", ".####", "....#", ".###." }
	};

	private readonly List<BlockInfo> _blocks = new List<BlockInfo>();

	private readonly Queue<PendingBlock> _queue = new Queue<PendingBlock>();

	private readonly List<int> _myBlocks = new List<int>();

	private readonly Dictionary<int, float> _expiry = new Dictionary<int, float>();

	private readonly List<int> _expired = new List<int>();

	private int _blockIndex;

	private int _shapeIndex;

	private float _nextQueue;

	private float _nextGun;

	private float _nextPlatform;

	private bool _warned;

	public static BlocksRuntime Instance { get; private set; }

	public bool BlockGun { get; set; }

	public bool ShapeGun { get; set; }

	public bool BlockRain { get; set; }

	public bool DestroyBlockGun { get; set; }

	public bool BlockPlatform { get; set; }

	public static string SelectedBlockName
	{
		get
		{
			if ((Object)(object)Instance == (Object)null || !Instance.LoadBlocks())
			{
				return "NONE";
			}
			return Instance._blocks[Instance._blockIndex].Name;
		}
	}

	public static string SelectedShapeName
	{
		get
		{
			if ((Object)(object)Instance == (Object)null)
			{
				return "NONE";
			}
			return ShapeNames[Instance._shapeIndex];
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
	}

	private void Update()
	{
		ProcessQueue();
		ProcessExpiry();
		if (BlockPlatform)
		{
			UpdatePlatform();
		}
		if (BlockRain)
		{
			UpdateRain();
		}
		if ((!BlockGun && !ShapeGun && !DestroyBlockGun) || (Object)(object)GunRuntime.Instance == (Object)null)
		{
			return;
		}
		if (!GunRuntime.GetGunInput(trigger: true))
		{
			_warned = false;
			return;
		}
		if (Time.time < _nextGun)
		{
			return;
		}
		_nextGun = Time.time + GunDelay;
		Vector3 point = GunRuntime.RenderGun().Ray.point;
		if (point == Vector3.zero)
		{
			return;
		}
		if (DestroyBlockGun)
		{
			DestroyNear(point);
		}
		if (!LoadBlocks())
		{
			return;
		}
		BlockInfo block = _blocks[_blockIndex];
		if (BlockGun)
		{
			Enqueue(block, point + Vector3.up * block.Size * 0.5f, Quaternion.identity, 0f);
		}
		if (ShapeGun)
		{
			// A shape per trigger press, not per frame held.
			_nextGun = Time.time + 1f;
			BuildShape(block, point);
		}
	}

	// ---------- Menu actions ----------

	public static void NextBlock()
	{
		if (!((Object)(object)Instance == (Object)null) && Instance.LoadBlocks())
		{
			Instance._blockIndex = (Instance._blockIndex + 1) % Instance._blocks.Count;
			NotificationRuntime.Show("Block: " + SelectedBlockName);
		}
	}

	public static void NextShape()
	{
		if (!((Object)(object)Instance == (Object)null))
		{
			Instance._shapeIndex = (Instance._shapeIndex + 1) % ShapeNames.Length;
			NotificationRuntime.Show("Shape: " + SelectedShapeName);
		}
	}

	public static void ClearMyBlocks()
	{
		if ((Object)(object)Instance == (Object)null)
		{
			return;
		}
		Instance._warned = false;
		Instance._queue.Clear();
		BuilderTable table = Instance.MasterTable();
		if ((Object)(object)table == (Object)null)
		{
			return;
		}
		foreach (int id in Instance._myBlocks)
		{
			BuilderPiece piece = table.GetPiece(id);
			if ((Object)(object)piece != (Object)null)
			{
				table.RequestRecyclePiece(piece, false, -1);
			}
		}
		NotificationRuntime.Show("Removed " + Instance._myBlocks.Count + " blocks");
		Instance._myBlocks.Clear();
		Instance._expiry.Clear();
	}

	public static void WriteName()
	{
		if ((Object)(object)Instance == (Object)null || (Object)(object)GorillaTagger.Instance == (Object)null)
		{
			return;
		}
		Instance._warned = false;
		if (!Instance.LoadBlocks() || (Object)(object)Instance.MasterTable() == (Object)null)
		{
			return;
		}
		string name = (PhotonNetwork.LocalPlayer != null) ? PhotonNetwork.LocalPlayer.NickName : "SONDER";
		Instance.BuildText(Instance._blocks[Instance._blockIndex], string.IsNullOrEmpty(name) ? "SONDER" : name.ToUpperInvariant());
	}

	// ---------- Building ----------

	private void BuildShape(BlockInfo block, Vector3 origin)
	{
		if ((Object)(object)MasterTable() == (Object)null)
		{
			return;
		}
		float s = block.Size;
		Vector3 forward = FlatForward();
		Vector3 right = Vector3.Cross(Vector3.up, forward);
		Quaternion rotation = Quaternion.LookRotation(forward, Vector3.up);
		List<Vector3> points = new List<Vector3>();
		switch (_shapeIndex)
		{
		case 0:
			for (int y = 0; y < 5; y++)
			{
				for (int x = -3; x <= 3; x++)
				{
					points.Add(origin + right * (x * s) + Vector3.up * (y * s + s * 0.5f));
				}
			}
			break;
		case 1:
			for (int y = 0; y < 15; y++)
			{
				points.Add(origin + Vector3.up * (y * s + s * 0.5f));
			}
			break;
		case 2:
			for (int step = 0; step < 8; step++)
			{
				for (int x = -1; x <= 1; x++)
				{
					points.Add(origin + forward * (step * s) + right * (x * s) + Vector3.up * (step * s + s * 0.5f));
				}
			}
			break;
		default:
		{
			float radius = s * 4f;
			for (int ring = 0; ring < 4; ring++)
			{
				float angle = ring * 22.5f * Mathf.Deg2Rad;
				float ringRadius = Mathf.Cos(angle) * radius;
				float height = Mathf.Sin(angle) * radius;
				int count = Mathf.Max(1, Mathf.RoundToInt(2f * Mathf.PI * ringRadius / s));
				for (int i = 0; i < count; i++)
				{
					float a = 2f * Mathf.PI * i / count;
					points.Add(origin + new Vector3(Mathf.Cos(a) * ringRadius, height + s * 0.5f, Mathf.Sin(a) * ringRadius));
				}
			}
			points.Add(origin + Vector3.up * (radius + s * 0.5f));
			break;
		}
		}
		for (int i = 0; i < points.Count && i < MaxPerShape; i++)
		{
			Enqueue(block, points[i], rotation, 0f);
		}
	}

	private void BuildText(BlockInfo block, string text)
	{
		Transform head = ((Component)GorillaTagger.Instance.headCollider).transform;
		Vector3 forward = FlatForward();
		Vector3 right = Vector3.Cross(Vector3.up, forward);
		Quaternion rotation = Quaternion.LookRotation(forward, Vector3.up);
		float s = block.Size;
		int columns = text.Length * 6 - 1;
		Vector3 start = head.position + forward * 1.5f - right * (columns * s * 0.5f) + Vector3.up * (s * 2f);
		int queued = 0;
		for (int c = 0; c < text.Length; c++)
		{
			if (!Font.TryGetValue(text[c], out var rows))
			{
				continue;
			}
			for (int row = 0; row < rows.Length; row++)
			{
				for (int col = 0; col < rows[row].Length; col++)
				{
					if (rows[row][col] == '#' && queued < MaxPerShape * 3)
					{
						Enqueue(block, start + right * ((c * 6 + col) * s) - Vector3.up * (row * s), rotation, 0f);
						queued++;
					}
				}
			}
		}
	}

	private void UpdateRain()
	{
		if (Time.time < _nextPlatform || !LoadBlocks() || (Object)(object)GorillaTagger.Instance == (Object)null)
		{
			return;
		}
		_nextPlatform = Time.time + PlatformDelay;
		Vector3 center = ((Component)GorillaTagger.Instance.bodyCollider).transform.position;
		Vector2 offset = Random.insideUnitCircle * 4f;
		Vector3 position = center + new Vector3(offset.x, Random.Range(2f, 4f), offset.y);
		Enqueue(_blocks[Random.Range(0, _blocks.Count)], position, Random.rotation, 0f);
	}

	private void UpdatePlatform()
	{
		ControllerInputPoller input = ControllerInputPoller.instance;
		if ((Object)(object)input == (Object)null || input.leftControllerGripFloat < 0.5f || Time.time < _nextPlatform || !LoadBlocks())
		{
			return;
		}
		_nextPlatform = Time.time + PlatformDelay;
		BlockInfo block = _blocks[_blockIndex];
		Vector3 hand = GorillaTagger.Instance.leftHandTransform.position;
		Enqueue(block, hand - Vector3.up * block.Size, Quaternion.identity, PlatformLifetime);
	}

	// ---------- Spawning ----------

	private void Enqueue(BlockInfo block, Vector3 position, Quaternion rotation, float lifetime)
	{
		_queue.Enqueue(new PendingBlock
		{
			Type = block.Type,
			Material = block.Material,
			Position = position,
			Rotation = rotation,
			Lifetime = lifetime
		});
	}

	// The game drops shelf spawns past 100 a second, so spread them out.
	private void ProcessQueue()
	{
		if (_queue.Count == 0 || Time.time < _nextQueue)
		{
			return;
		}
		_nextQueue = Time.time + SpawnInterval;
		BuilderTable table = MasterTable();
		if ((Object)(object)table == (Object)null)
		{
			_queue.Clear();
			return;
		}
		PendingBlock pending = _queue.Dequeue();
		if (!table.ValidatePieceWorldTransform(pending.Position, pending.Rotation))
		{
			Warn("Too far from the build area");
			return;
		}
		if (table.dispenserShelves.Count == 0)
		{
			Warn("No block shelves found");
			_queue.Clear();
			return;
		}
		int before = (NextPieceIdField != null) ? ((int)NextPieceIdField.GetValue(table)) : (-1);
		table.RequestCreateDispenserShelfPiece(pending.Type, pending.Position, pending.Rotation, pending.Material, 0);
		int after = (NextPieceIdField != null) ? ((int)NextPieceIdField.GetValue(table)) : (-1);
		if (before == after)
		{
			Warn("Block limit reached");
			_queue.Clear();
			return;
		}
		_myBlocks.Add(before);
		if (pending.Lifetime > 0f)
		{
			_expiry[before] = Time.time + pending.Lifetime;
		}
		else
		{
			// Make spawned blocks obey gravity so they fall instead of hanging in the air.
			BuilderPiece spawned = ((Object)(object)table != (Object)null) ? table.GetPiece(before) : null;
			if ((Object)(object)spawned != (Object)null)
			{
				foreach (Rigidbody rb in spawned.GetComponentsInChildren<Rigidbody>(true))
				{
					rb.isKinematic = false;
					rb.useGravity = true;
				}
			}
		}
	}

	private void ProcessExpiry()
	{
		if (_expiry.Count == 0)
		{
			return;
		}
		BuilderTable table = MasterTable();
		_expired.Clear();
		foreach (KeyValuePair<int, float> item in _expiry)
		{
			if (Time.time >= item.Value)
			{
				_expired.Add(item.Key);
			}
		}
		foreach (int id in _expired)
		{
			_expiry.Remove(id);
			_myBlocks.Remove(id);
			BuilderPiece piece = ((Object)(object)table != (Object)null) ? table.GetPiece(id) : null;
			if ((Object)(object)piece != (Object)null)
			{
				table.RequestRecyclePiece(piece, false, -1);
			}
		}
	}

	// Only removes blocks this mod made, so nobody else's builds get wiped.
	private void DestroyNear(Vector3 point)
	{
		BuilderTable table = MasterTable();
		if ((Object)(object)table == (Object)null)
		{
			return;
		}
		foreach (Collider hit in Physics.OverlapSphere(point, 0.3f, -1, QueryTriggerInteraction.Collide))
		{
			BuilderPiece piece = ((Component)hit).GetComponentInParent<BuilderPiece>();
			if ((Object)(object)piece != (Object)null && _myBlocks.Remove(piece.pieceId))
			{
				_expiry.Remove(piece.pieceId);
				table.RequestRecyclePiece(piece, true, -1);
			}
		}
	}

	private BuilderTable MasterTable()
	{
		if (!BuilderTable.TryGetBuilderTableForZone(GTZone.monkeBlocks, out var table) || (Object)(object)table == (Object)null)
		{
			Warn("Not in Monkey Blocks");
			return null;
		}
		if (PhotonNetwork.InRoom && !PhotonNetwork.IsMasterClient)
		{
			Warn("Need master");
			return null;
		}
		if (!table.isTableMutable || table.GetTableState() != BuilderTable.TableState.Ready)
		{
			Warn("Blocks not ready");
			return null;
		}
		return table;
	}

	private bool LoadBlocks()
	{
		if (_blocks.Count > 0)
		{
			return true;
		}
		BuilderSetManager sets = BuilderSetManager.instance;
		if ((Object)(object)sets == (Object)null || sets.GetUnlockedPieceSets() == null)
		{
			return false;
		}
		HashSet<int> seen = new HashSet<int>();
		foreach (BuilderPieceSet set in sets.GetUnlockedPieceSets())
		{
			if ((Object)(object)set == (Object)null || set.subsets == null)
			{
				continue;
			}
			int material = string.IsNullOrEmpty(set.materialId) ? (-1) : set.materialId.GetHashCode();
			foreach (BuilderPieceSet.BuilderPieceSubset subset in set.subsets)
			{
				if (subset == null || subset.pieceInfos == null)
				{
					continue;
				}
				foreach (BuilderPieceSet.PieceInfo info in subset.pieceInfos)
				{
					if ((Object)(object)info.piecePrefab == (Object)null)
					{
						continue;
					}
					int type = ((Object)info.piecePrefab).name.GetStaticHash();
					if (seen.Add(type))
					{
						_blocks.Add(new BlockInfo
						{
							Type = type,
							Material = material,
							Name = CleanName(((Object)info.piecePrefab).name),
							Size = MeasureBlock(info.piecePrefab)
						});
					}
				}
			}
		}
		_blockIndex = 0;
		return _blocks.Count > 0;
	}

	private static float MeasureBlock(BuilderPiece prefab)
	{
		float size = 0f;
		foreach (BoxCollider box in ((Component)prefab).GetComponentsInChildren<BoxCollider>(true))
		{
			Vector3 scaled = Vector3.Scale(box.size, ((Component)box).transform.lossyScale);
			size = Mathf.Max(size, Mathf.Max(scaled.x, Mathf.Max(scaled.y, scaled.z)));
		}
		return (size > 0.01f && size < 2f) ? size : 0.1f;
	}

	private static Vector3 FlatForward()
	{
		Transform head = ((Object)(object)GorillaTagger.Instance != (Object)null) ? ((Component)GorillaTagger.Instance.headCollider).transform : null;
		Vector3 forward = ((Object)(object)head != (Object)null) ? head.forward : Vector3.forward;
		forward.y = 0f;
		return (forward.sqrMagnitude < 0.001f) ? Vector3.forward : forward.normalized;
	}

	private static string CleanName(string name)
	{
		return name.Replace("(Clone)", "").Replace("_", " ").Trim().ToUpperInvariant();
	}

	private void Warn(string message)
	{
		if (!_warned)
		{
			_warned = true;
			RuntimeLog.Write("blocks: " + message);
			NotificationRuntime.Show(message);
		}
	}
}

