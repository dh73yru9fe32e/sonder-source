using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Object = UnityEngine.Object;
using UnityEngine.Rendering;

namespace Taurus;

public sealed class GunLib : IDisposable
{
	public readonly struct BodyTarget(Transform body, float radius, Transform root = null)
	{
		public readonly Transform Body = body;

		public readonly float Radius = radius;

		public readonly Transform Root = root ?? body;
	}

	// Only things you can see and stand on: Default, MeshBakerAtlas, GorillaObject, GorillaInteractable,
	// GorillaThrowable, BuilderProp, NoMirror, RopeSwing, Prop, Bake. Triggers, boundaries, body capsules,
	// hand sockets and cosmetics are invisible and used to stop the beam in mid-air.
	private const int SolidMask = (1 << 0) | (1 << 6) | (1 << 9) | (1 << 18) | (1 << 23) | (1 << 26) | (1 << 27) | (1 << 29) | (1 << 30) | (1 << 31);

	private readonly GameObject root;

	private readonly LineRenderer line;

	private readonly GameObject ball;

	private readonly Renderer ballRenderer;

	private readonly Mesh reticleMesh;

	private Color previousLeft;

	private Color previousRight;

	private bool colored;

	private readonly MaterialPropertyBlock colors;

	private bool wasTriggered;

	private RaycastHit[] hits;

	[CompilerGenerated]
	private Vector3 _003CTargetPoint_003Ek__BackingField;

	[CompilerGenerated]
	private Vector3 _003CTargetNormal_003Ek__BackingField;

	public Transform LockedBody { get; private set; }

	public Vector3 TargetPoint
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _003CTargetPoint_003Ek__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_003CTargetPoint_003Ek__BackingField = value;
		}
	}

	public float BeamStartWidth { get; set; }

	public float BeamEndWidth { get; set; }

	public float BallScale { get; set; }

	public bool LineVisible { get; set; }

	public bool BallVisible { get; set; }

	public Vector3 TargetNormal
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _003CTargetNormal_003Ek__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_003CTargetNormal_003Ek__BackingField = value;
		}
	}

	public bool IsVisible
	{
		get
		{
			if (root.activeSelf)
			{
				return ball.activeSelf;
			}
			return false;
		}
	}

	public bool HasHit { get; private set; }

	public Vector3 VisualOrigin
	{
		get
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			if (IsVisible)
			{
				return line.GetPosition(0);
			}
			return Vector3.zero;
		}
	}

	public bool IsPressed => wasTriggered;

	public event Action<Transform> Triggered;

	public event Action<Vector3> PointTriggered;

	public GunLib(Material lineMaterial, Material ballMaterial)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Expected O, but got Unknown
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		colors = new MaterialPropertyBlock();
		hits = (RaycastHit[])(object)new RaycastHit[64];
		BeamStartWidth = 0.009f;
		BeamEndWidth = 0.002f;
		BallScale = 0.14f;
		LineVisible = true;
		BallVisible = true;
		root = new GameObject("TaurusGun");
		root.layer = 2;
		if (Application.isPlaying)
		{
			Object.DontDestroyOnLoad((Object)(object)root);
		}
		line = root.AddComponent<LineRenderer>();
		((Renderer)line).sharedMaterial = lineMaterial;
		line.positionCount = 2;
		line.startWidth = 0.009f;
		line.endWidth = 0.002f;
		line.numCapVertices = 4;
		line.useWorldSpace = true;
		line.alignment = (LineAlignment)0;
		((Renderer)line).shadowCastingMode = (ShadowCastingMode)0;
		((Renderer)line).receiveShadows = false;
		ball = new GameObject("TaurusDiamondReticle", new Type[2]
		{
			typeof(MeshFilter),
			typeof(MeshRenderer)
		});
		ball.layer = 2;
		ball.transform.SetParent(root.transform, false);
		ball.transform.localScale = Vector3.one * 0.14f;
		reticleMesh = CreateReticle();
		ball.GetComponent<MeshFilter>().sharedMesh = reticleMesh;
		ballRenderer = ball.GetComponent<Renderer>();
		ballRenderer.sharedMaterial = ballMaterial;
		ballRenderer.shadowCastingMode = (ShadowCastingMode)0;
		ballRenderer.receiveShadows = false;
		root.SetActive(false);
	}

	// Moves the start of the drawn beam without re-aiming. Called late in the frame, after the hand has
	// been posed, so the beam stays attached to the fingers instead of trailing a frame behind.
	public void SetVisualOrigin(Vector3 origin)
	{
		if (!root.activeSelf || !Finite(origin))
		{
			return;
		}
		Vector3 end = line.GetPosition(line.positionCount - 1);
		for (int i = 0; i < line.positionCount - 1; i++)
		{
			line.SetPosition(i, Vector3.Lerp(origin, end, (float)i / (float)(line.positionCount - 1)));
		}
	}

	public void ShowRemote(Vector3 start, Vector3 end, bool locked)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		root.SetActive(true);
		ball.SetActive(true);
		for (int i = 0; i < line.positionCount; i++)
		{
			line.SetPosition(i, Vector3.Lerp(start, end, (float)i / (float)(line.positionCount - 1)));
		}
		ball.transform.position = end;
		Camera main = Camera.main;
		if ((Object)(object)main != (Object)null)
		{
			ball.transform.rotation = ((Component)main).transform.rotation;
		}
		ball.transform.localScale = Vector3.one * (locked ? 0.115f : 0.14f);
		if (locked)
		{
			ball.transform.Rotate(Vector3.forward, 45f, (Space)1);
		}
	}

	public void Update(Transform hand, bool aiming, bool trigger, IReadOnlyList<BodyTarget> targets, Transform localRig = null, Transform localPlayer = null)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)hand == (Object)null)
		{
			Hide();
		}
		else
		{
			Update(hand.position + hand.forward * 0.25f, hand.forward, aiming, trigger, targets, localRig, null, localPlayer);
		}
	}

	public void Update(Vector3 origin, Vector3 direction, bool aiming, bool trigger, IReadOnlyList<BodyTarget> targets, Transform localRig = null, Vector3? visualOrigin = null, Transform localPlayer = null)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0309: Unknown result type (might be due to invalid IL or missing references)
		//IL_030c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0479: Unknown result type (might be due to invalid IL or missing references)
		//IL_0474: Unknown result type (might be due to invalid IL or missing references)
		//IL_0491: Unknown result type (might be due to invalid IL or missing references)
		//IL_048e: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0380: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_050f: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0501: Unknown result type (might be due to invalid IL or missing references)
		//IL_0524: Unknown result type (might be due to invalid IL or missing references)
		//IL_054b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0560: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0590: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_059e: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_05cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0611: Unknown result type (might be due to invalid IL or missing references)
		if (!aiming || !Finite(origin) || !Finite(direction) || direction.sqrMagnitude < 0.0001f || float.IsInfinity(direction.sqrMagnitude))
		{
			Hide();
			return;
		}
		direction.Normalize();
		targets = targets ?? Array.Empty<BodyTarget>();
		root.SetActive(true);
		((Renderer)line).enabled = LineVisible;
		line.startWidth = BeamStartWidth;
		line.endWidth = BeamEndWidth;
		if (!trigger || (Object)(object)LockedBody == (Object)null || !((Component)LockedBody).gameObject.activeInHierarchy)
		{
			LockedBody = null;
		}
		if ((Object)(object)LockedBody != (Object)null)
		{
			bool flag = false;
			foreach (BodyTarget target in targets)
			{
				if ((Object)(object)target.Body == (Object)(object)LockedBody && Finite(target.Body.position))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				LockedBody = null;
			}
		}
		if ((Object)(object)LockedBody != (Object)null)
		{
			Vector3 val = LockedBody.position - origin;
			direction = val.normalized;
		}
		float num = 100f;
		Vector3 val2 = origin + direction * 5f;
		bool flag2 = false;
		Vector3 val3 = Vector3.up;
		int num2 = Physics.RaycastNonAlloc(origin, direction, hits, 100f, SolidMask, QueryTriggerInteraction.Ignore);
		while (num2 == hits.Length && hits.Length < 4096)
		{
			Array.Resize(ref hits, hits.Length * 2);
			num2 = Physics.RaycastNonAlloc(origin, direction, hits, 100f, SolidMask, QueryTriggerInteraction.Ignore);
		}
		RaycastHit[] array = hits;
		if (num2 == hits.Length)
		{
			array = Physics.RaycastAll(origin, direction, 100f, SolidMask, QueryTriggerInteraction.Ignore);
			num2 = array.Length;
		}
		for (int i = 0; i < num2; i++)
		{
			Transform transform = ((Component)array[i].collider).transform;
			if ((!((Object)(object)localRig == (Object)null) && transform.IsChildOf(localRig)) || (!((Object)(object)localPlayer == (Object)null) && transform.IsChildOf(localPlayer)))
			{
				continue;
			}
			bool flag3 = false;
			foreach (BodyTarget target2 in targets)
			{
				if ((Object)(object)target2.Root != (Object)null && transform.IsChildOf(target2.Root))
				{
					flag3 = true;
					break;
				}
			}
			if (!flag3 && array[i].distance < num)
			{
				num = array[i].distance;
				val2 = array[i].point;
				val3 = array[i].normal;
				flag2 = true;
			}
		}
		Transform val4 = LockedBody;
		if ((Object)(object)val4 != (Object)null && Vector3.Distance(origin, val4.position) - 0.3f > num)
		{
			Transform val5 = (LockedBody = null);
			val4 = val5;
		}
		if ((Object)(object)val4 == (Object)null)
		{
			foreach (BodyTarget target3 in targets)
			{
				if (!((Object)(object)target3.Body != (Object)null) || !((Component)target3.Body).gameObject.activeInHierarchy || !Finite(target3.Body.position) || float.IsNaN(target3.Radius) || float.IsInfinity(target3.Radius) || !(target3.Radius > 0f))
				{
					continue;
				}
				Vector3 val7 = target3.Body.position - origin;
				float num3 = Vector3.Dot(val7, direction);
				float num4 = Mathf.Max(0f, val7.sqrMagnitude - num3 * num3);
				float num5 = target3.Radius * target3.Radius;
				if (num3 >= 0f && num4 <= num5)
				{
					float num6 = Mathf.Max(0f, num3 - Mathf.Sqrt(num5 - num4));
					if (num6 < num)
					{
						num = num6;
						val4 = target3.Body;
					}
				}
			}
			if (trigger)
			{
				LockedBody = val4;
			}
		}
		TargetPoint = (((Object)(object)val4 != (Object)null) ? val4.position : val2);
		TargetNormal = (((Object)(object)val4 != (Object)null) ? Vector3.up : val3);
		HasHit = ((Object)(object)val4 != (Object)null) | flag2;
		ball.SetActive(BallVisible);
		if (BallVisible)
		{
			ball.transform.position = TargetPoint;
		}
		Camera main = Camera.main;
		ball.transform.rotation = (((Object)(object)main != (Object)null) ? ((Component)main).transform.rotation : Quaternion.FromToRotation(Vector3.forward, -direction));
		ball.transform.localScale = Vector3.one * (((Object)(object)LockedBody != (Object)null) ? (BallScale * 0.82f) : BallScale);
		ball.transform.Rotate(Vector3.forward, ((Object)(object)LockedBody != (Object)null) ? 45f : 0f, (Space)1);
		Vector3 val8 = ((!visualOrigin.HasValue || !Finite(visualOrigin.Value)) ? origin : visualOrigin.Value);
		for (int j = 0; j < line.positionCount; j++)
		{
			line.SetPosition(j, Vector3.Lerp(val8, TargetPoint, (float)j / (float)(line.positionCount - 1)));
		}
		if (trigger && !wasTriggered && HasHit)
		{
			PointTriggered?.Invoke(TargetPoint);
			if ((Object)(object)val4 != (Object)null)
			{
				Triggered?.Invoke(val4);
			}
		}
		wasTriggered = trigger;
	}

	public void Hide()
	{
		root.SetActive(false);
		LockedBody = null;
		HasHit = false;
		wasTriggered = false;
	}

	public void SetColors(Color left, Color right)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if (!colored || !(left == previousLeft) || !(right == previousRight))
		{
			colored = true;
			previousLeft = left;
			previousRight = right;
			colors.SetColor("_LeftColor", left);
			colors.SetColor("_RightColor", right);
			ballRenderer.SetPropertyBlock(colors);
			colors.SetFloat("_UseColors", 1f);
			((Renderer)line).SetPropertyBlock(colors);
			line.startColor = left;
			line.endColor = right;
		}
	}

	private static bool Finite(Vector3 v)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (!float.IsNaN(v.x) && !float.IsNaN(v.y) && !float.IsNaN(v.z) && !float.IsInfinity(v.x) && !float.IsInfinity(v.y))
		{
			return !float.IsInfinity(v.z);
		}
		return false;
	}

	private static Mesh CreateReticle()
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Expected O, but got Unknown
		List<Vector3> vertices = new List<Vector3>();
		List<int> triangles = new List<int>();
		for (int i = 0; i < 4; i++)
		{
			float num = (float)i * (float)Math.PI * 0.5f;
			Vector2 val = new Vector2(Mathf.Cos(num), Mathf.Sin(num)) * 0.5f;
			Vector2 val2 = new Vector2(Mathf.Cos(num + (float)Math.PI / 2f), Mathf.Sin(num + (float)Math.PI / 2f)) * 0.5f;
			Segment(val, Vector2.Lerp(val, val2, 0.36f));
			Segment(Vector2.Lerp(val, val2, 0.64f), val2);
		}
		Mesh val3 = new Mesh
		{
			name = "Taurus segmented diamond",
			vertices = vertices.ToArray(),
			triangles = triangles.ToArray()
		};
		val3.RecalculateBounds();
		val3.RecalculateNormals();
		return val3;
		void Segment(Vector2 a, Vector2 b)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_0088: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			//IL_008f: Unknown result type (might be due to invalid IL or missing references)
			Vector2 val4 = new Vector2(0f - (b - a).y, (b - a).x);
			Vector2 val5 = val4.normalized * 0.035f;
			int count = vertices.Count;
			vertices.Add((a + val5));
			vertices.Add((b + val5));
			vertices.Add((b - val5));
			vertices.Add((a - val5));
			triangles.AddRange(new int[12]
			{
				count,
				count + 1,
				count + 2,
				count,
				count + 2,
				count + 3,
				count + 2,
				count + 1,
				count,
				count + 3,
				count + 2,
				count
			});
		}
	}

	public void Dispose()
	{
		if (Application.isPlaying)
		{
			Object.Destroy((Object)(object)root);
			Object.Destroy((Object)(object)reticleMesh);
		}
		else
		{
			Object.DestroyImmediate((Object)(object)root);
			Object.DestroyImmediate((Object)(object)reticleMesh);
		}
	}
}
