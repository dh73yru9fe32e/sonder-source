using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using Object = UnityEngine.Object;

namespace Sonder.Menu;

public static class FontLoader
{
	private static TMP_FontAsset _font;

	public static TMP_FontAsset Get()
	{
		if ((Object)(object)_font != (Object)null)
		{
			return _font;
		}
		// 1. Prefer a bundled Open Sauce Bold font (shipped as an embedded resource).
		try
		{
			using Stream stream = typeof(FontLoader).Assembly.GetManifestResourceStream("Sonder.OpenSauceBold.ttf");
			if (stream != null)
			{
				string tempPath = Path.Combine(Path.GetTempPath(), "Sonder.OpenSauceBold.ttf");
				using (FileStream fs = new FileStream(tempPath, FileMode.Create, FileAccess.Write))
				{
					stream.CopyTo(fs);
				}
				TMP_FontAsset created = TMP_FontAsset.CreateFontAsset(tempPath, 0, 90, 9, GlyphRenderMode.SDFAA, 1024, 1024);
				if ((Object)(object)created != (Object)null)
				{
					created.name = "SonderOpenSauceBold";
					_font = created;
					return _font;
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning((object)("[Sonder] Open Sauce Bold load failed: " + ex.Message));
		}
		// 2. Fall back to bundled Montserrat.
		try
		{
			using Stream stream = typeof(FontLoader).Assembly.GetManifestResourceStream("Sonder.Montserrat.ttf");
			if (stream != null)
			{
				string tempPath = Path.Combine(Path.GetTempPath(), "Sonder.Montserrat.ttf");
				using (FileStream fs = new FileStream(tempPath, FileMode.Create, FileAccess.Write))
				{
					stream.CopyTo(fs);
				}
				TMP_FontAsset created = TMP_FontAsset.CreateFontAsset(tempPath, 0, 90, 9, GlyphRenderMode.SDFAA, 1024, 1024);
				if ((Object)(object)created != (Object)null)
				{
					created.name = "SonderMontserrat";
					_font = created;
					return _font;
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning((object)("[Sonder] Montserrat load failed: " + ex.Message));
		}
		// 2. Fall back to a Montserrat asset already present in the game resources.
		try
		{
			TMP_FontAsset[] array = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
			foreach (TMP_FontAsset val in array)
			{
				if (!((Object)(object)val == (Object)null))
				{
					string name = ((Object)val).name;
					if (name.IndexOf("montserrat", StringComparison.OrdinalIgnoreCase) >= 0)
					{
						_font = val;
						return _font;
					}
				}
			}
			// 3. Previous defaults.
			foreach (TMP_FontAsset val2 in array)
			{
				if (!((Object)(object)val2 == (Object)null))
				{
					string name = ((Object)val2).name;
					if (name.IndexOf("gorilla", StringComparison.OrdinalIgnoreCase) >= 0 || name.IndexOf("pixel", StringComparison.OrdinalIgnoreCase) >= 0)
					{
						_font = val2;
						return _font;
					}
				}
			}
			TextMeshProUGUI[] array2 = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();
			foreach (TextMeshProUGUI val3 in array2)
			{
				if ((Object)(object)val3 != (Object)null && (Object)(object)((TMP_Text)val3).font != (Object)null)
				{
					_font = ((TMP_Text)val3).font;
					return _font;
				}
			}
			if ((Object)(object)TMP_Settings.defaultFontAsset != (Object)null)
			{
				_font = TMP_Settings.defaultFontAsset;
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning((object)("[Sonder] Font scan failed: " + ex.Message));
		}
		return _font;
	}
}