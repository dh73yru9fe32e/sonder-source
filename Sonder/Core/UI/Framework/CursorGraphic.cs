using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;
using UnityEngine.UI;

namespace Sonder.Core.UI.Framework;

public sealed class CursorGraphic : MaskableGraphic
{
	protected override void OnPopulateMesh(VertexHelper vh)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		vh.Clear();
		Rect pixelAdjustedRect = ((Graphic)this).GetPixelAdjustedRect();
		float width = pixelAdjustedRect.width;
		float height = pixelAdjustedRect.height;
		Vector2[] array = new Vector2[7]
		{
			new Vector2(pixelAdjustedRect.xMin, pixelAdjustedRect.yMax),
			new Vector2(pixelAdjustedRect.xMin, pixelAdjustedRect.yMin),
			new Vector2(pixelAdjustedRect.xMin + width * 0.3f, pixelAdjustedRect.yMin + height * 0.24f),
			new Vector2(pixelAdjustedRect.xMin + width * 0.4f, pixelAdjustedRect.yMin + height * 0.02f),
			new Vector2(pixelAdjustedRect.xMin + width * 0.57f, pixelAdjustedRect.yMin + height * 0.08f),
			new Vector2(pixelAdjustedRect.xMin + width * 0.48f, pixelAdjustedRect.yMin + height * 0.32f),
			new Vector2(pixelAdjustedRect.xMax, pixelAdjustedRect.yMin + height * 0.38f)
		};
		vh.AddVert((Vector2)array[0], (Color32)((Graphic)this).color, (Vector4)Vector2.zero);
		vh.AddVert((Vector2)array[1], (Color32)((Graphic)this).color, (Vector4)Vector2.zero);
		vh.AddVert((Vector2)array[2], (Color32)((Graphic)this).color, (Vector4)Vector2.zero);
		vh.AddVert((Vector2)array[3], (Color32)((Graphic)this).color, (Vector4)Vector2.zero);
		vh.AddVert((Vector2)array[4], (Color32)((Graphic)this).color, (Vector4)Vector2.zero);
		vh.AddVert((Vector2)array[5], (Color32)((Graphic)this).color, (Vector4)Vector2.zero);
		vh.AddVert((Vector2)array[6], (Color32)((Graphic)this).color, (Vector4)Vector2.zero);
		for (int i = 1; i < array.Length - 1; i++)
		{
			vh.AddTriangle(0, i, i + 1);
		}
	}
}




