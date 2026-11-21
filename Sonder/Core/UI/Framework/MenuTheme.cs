using TMPro;
using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using Object = UnityEngine.Object;
using UnityEngine.UI;

namespace Sonder.Core.UI.Framework;

public class MenuTheme
{
	public string Name;

	public Color Top;

	public Color Bottom;

	public Color Background;

	public Color BackgroundBottom;

	public Color Button;

	public Color ButtonPressed;

	public Color ButtonText;

	public Color TitleText;

	public Color Accent;

	public Color Prompt;

	public bool HasLiquidGlass;

	public static readonly MenuTheme Satin;

	public static readonly MenuTheme SnowySky;

	public static readonly MenuTheme SteelFade;

	public static readonly MenuTheme Sundae;

	public static readonly MenuTheme Sunkist;

	public static readonly MenuTheme Water;

	public static readonly MenuTheme Legacy;

	public static readonly MenuTheme Winter;

	public static readonly MenuTheme Peony;

	public static readonly MenuTheme Shadow;

	public static readonly MenuTheme Wood;

	public static readonly MenuTheme Nord;

	public static readonly MenuTheme LiquidGlass;

	public static List<MenuTheme> All;

	public static MenuTheme Current;

	public MenuTheme(string name, string top, string bottom)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		Name = name;
		Top = Hex(top);
		Bottom = Hex(bottom);
		Accent = Color.Lerp(Top, Bottom, 0.35f);
		Background = Color.Lerp(Hex("0E0E12"), Accent, 0.05f);
		BackgroundBottom = Hex("0A0A0D");
		Button = Color.Lerp(Hex("1A1A21"), Accent, 0.1f);
		ButtonPressed = Color.Lerp(Hex("272731"), Accent, 0.28f);
		ButtonText = Hex("E9E9F2");
		TitleText = Color.white;
		Prompt = Hex("1B1B24");
	}

	private static Color Hex(string hex)
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		float num = (float)int.Parse(hex.Substring(0, 2), NumberStyles.HexNumber) / 255f;
		float num2 = (float)int.Parse(hex.Substring(2, 2), NumberStyles.HexNumber) / 255f;
		float num3 = (float)int.Parse(hex.Substring(4, 2), NumberStyles.HexNumber) / 255f;
		return new Color(num, num2, num3, 1f);
	}

	static MenuTheme()
	{
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Unknown result type (might be due to invalid IL or missing references)
		//IL_0274: Unknown result type (might be due to invalid IL or missing references)
		//IL_0279: Unknown result type (might be due to invalid IL or missing references)
		//IL_0297: Unknown result type (might be due to invalid IL or missing references)
		//IL_029c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e2: Unknown result type (might be due to invalid IL or missing references)
		Satin = new MenuTheme("Satin", "9A3F57", "5E1F34");
		SnowySky = new MenuTheme("Snowy Sky", "F5A35C", "FBE3B8");
		SteelFade = new MenuTheme("Steel Fade", "5FB8E8", "2A6FB8");
		Sundae = new MenuTheme("Sundae", "C86B9A", "6B3A7A");
		Sunkist = new MenuTheme("Sunkist", "FF9A3C", "FFD27A");
		Water = new MenuTheme("Water", "7FE3F0", "3FA7D6");
		Legacy = new MenuTheme("Legacy", "A9D9FF", "6FA8E0");
		Winter = new MenuTheme("Winter", "F4F6FF", "CFD8F0");
		Peony = new MenuTheme("Peony", "E9A8F0", "A56BD6");
		Shadow = new MenuTheme("Shadow", "4A5BC9", "1E2A6B");
		Wood = new MenuTheme("Wood", "9C8E5A", "5A4B2A");
		Nord = new MenuTheme("Nord", "88C0D0", "5E81AC");
		LiquidGlass = new MenuTheme("Liquid Glass", "B9F4FF", "8F9BFF");
		All = new List<MenuTheme>
		{
			Satin, SnowySky, SteelFade, Sundae, Sunkist, Water, Legacy, Winter, Peony, Shadow,
			Wood, Nord, LiquidGlass
		};
		Current = Shadow;
		LiquidGlass.HasLiquidGlass = true;
		LiquidGlass.Accent = Hex("A7E9FF");
		LiquidGlass.Background = new Color(0.055f, 0.09f, 0.15f, 0.82f);
		LiquidGlass.BackgroundBottom = new Color(0.08f, 0.14f, 0.23f, 0.76f);
		LiquidGlass.Button = new Color(0.54f, 0.69f, 0.84f, 0.17f);
		LiquidGlass.ButtonPressed = new Color(0.58f, 0.82f, 1f, 0.32f);
		LiquidGlass.ButtonText = Hex("F2FAFF");
		LiquidGlass.TitleText = Color.white;
		LiquidGlass.Prompt = new Color(0.7f, 0.83f, 0.96f, 0.18f);
	}

	public static MenuTheme GetByName(string name)
	{
		foreach (MenuTheme item in All)
		{
			if (item.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
			{
				return item;
			}
		}
		return Shadow;
	}

	public static Color Lighten(Color color, float amount)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		return new Color(Mathf.Clamp01(color.r + amount), Mathf.Clamp01(color.g + amount), Mathf.Clamp01(color.b + amount), color.a);
	}
}



