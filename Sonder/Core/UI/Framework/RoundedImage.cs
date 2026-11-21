using TMPro;
using System;
using UnityEngine;
using Object = UnityEngine.Object;
using UnityEngine.UI;

namespace Sonder.Core.UI.Framework;

public class RoundedImage : MaskableGraphic
{
	public float TopLeft = 10f;

	public float TopRight = 10f;

	public float BottomLeft = 10f;

	public float BottomRight = 10f;

	public int Segments = 48;

	public bool UseGradient = false;

	public Color TopColor = Color.white;

	public Color BottomColor = Color.white;

	protected RoundedImage()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		useLegacyMeshGeneration = false;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		((Graphic)this).SetAllDirty();
	}

	protected override void OnPopulateMesh(VertexHelper vh)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		vh.Clear();
		Rect pixelAdjustedRect = ((Graphic)this).GetPixelAdjustedRect();
		if (!(pixelAdjustedRect.width < 1f) && !(pixelAdjustedRect.height < 1f))
		{
			Vector2 val = new Vector2(pixelAdjustedRect.x, pixelAdjustedRect.y);
			Vector2 val2 = new Vector2(pixelAdjustedRect.x + pixelAdjustedRect.width, pixelAdjustedRect.y + pixelAdjustedRect.height);
			Color32 val3 = (Color32)((Graphic)this).color;
			float num = Mathf.Min(pixelAdjustedRect.width, pixelAdjustedRect.height) * 0.5f;
			float num2 = Mathf.Clamp(TopLeft, 0f, num);
			float num3 = Mathf.Clamp(TopRight, 0f, num);
			float num4 = Mathf.Clamp(BottomLeft, 0f, num);
			float num5 = Mathf.Clamp(BottomRight, 0f, num);
			vh.AddVert((Vector2)pixelAdjustedRect.center, val3, (Vector4)new Vector2(0.5f, 0.5f));
			AddCornerArc(vh, new Vector2(val.x + num2, val2.y - num2), num2, 180f, 90f, val3);
			AddCornerArc(vh, new Vector2(val2.x - num3, val2.y - num3), num3, 90f, 0f, val3);
			AddCornerArc(vh, new Vector2(val2.x - num5, val.y + num5), num5, 360f, 270f, val3);
			AddCornerArc(vh, new Vector2(val.x + num4, val.y + num4), num4, 270f, 180f, val3);
			int num6 = Segments * 4;
			for (int i = 0; i < num6; i++)
			{
				int num7 = 1 + (i + 1) % num6;
				vh.AddTriangle(0, 1 + i, num7);
			}
			if (UseGradient)
			{
				ApplyGradient(vh, pixelAdjustedRect);
			}
		}
	}

	private void AddCornerArc(VertexHelper vh, Vector2 center, float radius, float startDeg, float endDeg, Color32 c)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < Segments; i++)
		{
			float num = (float)i / (float)Segments;
			float num2 = Mathf.Lerp(startDeg, endDeg, num) * ((float)Math.PI / 180f);
			Vector2 val = center + new Vector2(Mathf.Cos(num2), Mathf.Sin(num2)) * radius;
			vh.AddVert((Vector2)val, c, (Vector4)Vector2.zero);
		}
	}

	private void ApplyGradient(VertexHelper vh, Rect r)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		UIVertex val = default;
		float num = Mathf.Max(r.height, 1f);
		for (int i = 0; i < vh.currentVertCount; i++)
		{
			vh.PopulateUIVertex(ref val, i);
			float num2 = Mathf.Clamp01((val.position.y - r.y) / num);
			Color val2 = Color.Lerp(BottomColor, TopColor, num2) * ((Graphic)this).color;
			val2.a = ((Graphic)this).color.a;
			val.color = (Color32)val2;
			vh.SetUIVertex(val, i);
		}
	}
}




