using UnityEngine;
using Object = UnityEngine.Object;

namespace Sonder.Core;

public static class SonderTheme
{
	private static readonly Color PanelBase;

	private static readonly Color PanelSinkBase;

	private static readonly Color RowBase;

	private static readonly Color RowHoverBase;

	private static readonly Color RowActiveBase;

	private static readonly Color RowActiveHoverBase;

	private static readonly Color TitleBase;

	private static readonly Color DescriptionBase;

	private static readonly Color OffStateBase;

	private static readonly Color DangerBase;

	private static readonly Color DangerHoverBase;

	private static readonly Color DangerTitleBase;

	private static readonly Color BorderBase;

	private static readonly Color ShadowBase;

	public const float RowGradient = 0.76f;

	public const float PanelGradient = 0.62f;

	public const int BorderWidth = 1;

	public static Color Panel
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			return WithAlpha(PanelBase, SonderSettings.PanelAlpha);
		}
	}

	public static Color PanelSink
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			return WithAlpha(PanelSinkBase, SonderSettings.PanelAlpha);
		}
	}

	public static Color Row
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			return WithAlpha(RowBase, 0.16f);
		}
	}

	public static Color RowHover
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			return WithAlpha(RowHoverBase, 0.30f);
		}
	}

	public static Color RowActive
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			return WithAlpha(RowActiveBase, 0.24f);
		}
	}

	public static Color RowActiveHover
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			return WithAlpha(RowActiveHoverBase, 0.38f);
		}
	}

	public static Color Title
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			return TitleBase;
		}
	}

	public static Color Description
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			return DescriptionBase;
		}
	}

	public static Color OffState
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			return OffStateBase;
		}
	}

	public static Color Danger
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			return WithAlpha(DangerBase, 0.16f);
		}
	}

	public static Color DangerHover
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			return WithAlpha(DangerHoverBase, 0.30f);
		}
	}

	public static Color DangerTitle
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			return DangerTitleBase;
		}
	}

	public static Color Border
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			return WithAlpha(BorderBase, 0.92f);
		}
	}

	public static Color BorderHover
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			return WithAlpha(BorderBase, 1f);
		}
	}

	public static Color Glow
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			Color sheen = Color.white;
			sheen.a = 0.12f * SonderSettings.PanelAlpha;
			return sheen;
		}
	}

	public static Color Shadow
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			return WithAlpha(ShadowBase, 0.45f * SonderSettings.PanelAlpha);
		}
	}

	public static Color WithAlpha(Color color, float alpha)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		color.a = alpha;
		return color;
	}

	static SonderTheme()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		PanelBase = new Color(1f, 1f, 1f, 1f);
		PanelSinkBase = new Color(0.96f, 0.98f, 1f, 1f);
		RowBase = new Color(1f, 1f, 1f, 1f);
		RowHoverBase = new Color(1f, 1f, 1f, 1f);
		RowActiveBase = new Color(0.94f, 0.98f, 1f, 1f);
		RowActiveHoverBase = new Color(0.88f, 0.96f, 1f, 1f);
		TitleBase = new Color(0.035f, 0.04f, 0.045f, 1f);
		DescriptionBase = new Color(0.16f, 0.18f, 0.19f, 1f);
		OffStateBase = new Color(0.34f, 0.36f, 0.37f, 1f);
		DangerBase = new Color(1f, 1f, 1f, 1f);
		DangerHoverBase = new Color(0.94f, 0.97f, 1f, 1f);
		DangerTitleBase = new Color(0.06f, 0.07f, 0.08f, 1f);
		BorderBase = new Color(0f, 0f, 0f, 1f);
		ShadowBase = new Color(0f, 0f, 0f, 1f);
	}
}
