using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Sonder.Core;

public static class SonderSprite
{
	public const int Size = 64;

	private static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

	public static Sprite Fill(int radius, float bottomShade)
	{
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		if (bottomShade < 0f)
		{
			bottomShade = 0f;
		}
		if (bottomShade > 1f)
		{
			bottomShade = 1f;
		}
		string key = "fill:" + radius + ":" + Mathf.RoundToInt(bottomShade * 100f);
		if (Cache.TryGetValue(key, out var value) && (Object)(object)value != (Object)null)
		{
			return value;
		}
		Color[] array = (Color[])(object)new Color[4096];
		for (int i = 0; i < 64; i++)
		{
			float num = (float)i / 63f;
			float num2 = Mathf.Lerp(bottomShade, 1f, num);
			for (int j = 0; j < 64; j++)
			{
				float num3 = RoundMask(j, i, 64, 64, radius);
				array[i * 64 + j] = new Color(num2, num2, num2, num3);
			}
		}
		return Register(key, Build(array, radius));
	}

	public static Sprite Flat(int radius)
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		string key = "flat:" + radius;
		if (Cache.TryGetValue(key, out var value) && (Object)(object)value != (Object)null)
		{
			return value;
		}
		Color[] array = (Color[])(object)new Color[4096];
		for (int i = 0; i < 64; i++)
		{
			for (int j = 0; j < 64; j++)
			{
				float num = RoundMask(j, i, 64, 64, radius);
				array[i * 64 + j] = new Color(1f, 1f, 1f, num);
			}
		}
		return Register(key, Build(array, radius));
	}

	public static Sprite Border(int radius, int width)
	{
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		if (width < 1)
		{
			width = 1;
		}
		string key = "ring:" + radius + ":" + width;
		if (Cache.TryGetValue(key, out var value) && (Object)(object)value != (Object)null)
		{
			return value;
		}
		float radius2 = Mathf.Max(0f, (float)(radius - width));
		Color[] array = (Color[])(object)new Color[4096];
		for (int i = 0; i < 64; i++)
		{
			for (int j = 0; j < 64; j++)
			{
				float num = RoundMask(j, i, 64, 64, radius);
				float num2 = RoundMask(j, i, 64, 64, radius2);
				float num3 = Mathf.Clamp01(num - num2);
				array[i * 64 + j] = new Color(1f, 1f, 1f, num3);
			}
		}
		return Register(key, Build(array, radius));
	}

	public static void Clear()
	{
		Cache.Clear();
	}

	private static Sprite Register(string key, Sprite sprite)
	{
		Cache[key] = sprite;
		return sprite;
	}

	private static Sprite Build(Color[] pixels, float radius)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Expected O, but got Unknown
		Texture2D val = new Texture2D(64, 64, (TextureFormat)4, false)
		{
			filterMode = (FilterMode)1,
			wrapMode = (TextureWrapMode)1
		};
		val.SetPixels(pixels);
		val.Apply(false, false);
		float num = Mathf.Max(1f, Mathf.Min(radius, 31f));
		return Sprite.Create(val, new Rect(0f, 0f, 64f, 64f), new Vector2(0.5f, 0.5f), 100f, 0u, (SpriteMeshType)0, new Vector4(num, num, num, num));
	}

	private static float RoundMask(int x, int y, int width, int height, float radius)
	{
		if (radius < 0.5f)
		{
			return 1f;
		}
		float num = (float)x + 0.5f;
		float num2 = (float)y + 0.5f;
		float num3 = Mathf.Clamp(num, radius, (float)width - radius);
		float num4 = Mathf.Clamp(num2, radius, (float)height - radius);
		float num5 = num - num3;
		float num6 = num2 - num4;
		float num7 = Mathf.Sqrt(num5 * num5 + num6 * num6);
		return Mathf.Clamp01(radius - num7 + 0.5f);
	}
}
