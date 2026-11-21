using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Sonder.Core;

public static class SonderIcon
{
	private const string ResourceName = "Sonder.NewIcon.png";

	private static Sprite _sprite;
	private static bool _failed;

	public static Sprite Get()
	{
		if ((Object)(object)_sprite != (Object)null || _failed)
		{
			return _sprite;
		}
		try
		{
			using Stream stream = typeof(SonderIcon).Assembly.GetManifestResourceStream(ResourceName);
			if (stream == null)
			{
				_failed = true;
				RuntimeLog.Write("icon: embedded resource missing");
				return null;
			}
			byte[] bytes = new byte[stream.Length];
			stream.Read(bytes, 0, bytes.Length);
			Texture2D val = new Texture2D(2, 2, TextureFormat.RGBA32, false)
			{
				name = "SonderIcon",
				filterMode = FilterMode.Bilinear,
				wrapMode = TextureWrapMode.Clamp
			};
			Type conversionType = Type.GetType("UnityEngine.ImageConversion, UnityEngine.ImageConversionModule");
			MethodInfo loadMethod = conversionType != null ? conversionType.GetMethod("LoadImage", new Type[] { typeof(Texture2D), typeof(byte[]) }) : null;
			bool loaded = false;
			if (loadMethod != null)
			{
				loaded = (bool)loadMethod.Invoke(null, new object[] { val, bytes });
			}
			if (!loaded)
			{
				_failed = true;
				RuntimeLog.Write("icon: png decode failed");
				return null;
			}
			_sprite = Sprite.Create(val, new Rect(0f, 0f, val.width, val.height), new Vector2(0.5f, 0.5f), 100f, 0u, SpriteMeshType.FullRect);
			((Object)_sprite).name = "SonderIcon";
		}
		catch (Exception ex)
		{
			_failed = true;
			RuntimeLog.Write("icon: " + ex.Message);
		}
		return _sprite;
	}
}