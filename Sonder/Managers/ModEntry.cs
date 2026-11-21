using System;

namespace Sonder.Managers;

public sealed class ModEntry
{
	public readonly string Title;

	public readonly string Description;

	private readonly Action<bool> _onToggle;

	private readonly Action _onActivate;

	private readonly Func<string> _stateLabel;

	private readonly Func<bool> _highlight;

	public bool Enabled { get; private set; }

	public bool Favorite { get; internal set; }

	public bool IsAction => _onActivate != null;

	public string State
	{
		get
		{
			if (_stateLabel == null)
			{
				if (!Enabled)
				{
					return "OFF";
				}
				return "ON";
			}
			return _stateLabel();
		}
	}

	public bool Highlighted
	{
		get
		{
			if (_highlight == null)
			{
				return Enabled;
			}
			return _highlight();
		}
	}

	public void ToggleFavorite()
	{
		Favorite = !Favorite;
		ModManager.SaveFavorites();
	}

	public ModEntry(string title, string description, Action<bool> onToggle)
	{
		Title = title;
		Description = description;
		_onToggle = onToggle;
	}

	public ModEntry(string title, string description, Action onActivate, Func<string> stateLabel, Func<bool> highlight)
	{
		Title = title;
		Description = description;
		_onActivate = onActivate;
		_stateLabel = stateLabel;
		_highlight = highlight;
	}

	public void Toggle()
	{
		if (_onActivate != null)
		{
			_onActivate();
			NotificationRuntime.Show(Title + " activated");
		}
		else
		{
			SetEnabled(!Enabled);
			NotificationRuntime.Show(Title + (Enabled ? " enabled" : " disabled"));
			ModManager.NotifyChanged();
		}
	}

	public void SetEnabled(bool enabled)
	{
		if (Enabled != enabled)
		{
			Enabled = enabled;
			if (_onToggle != null)
			{
				_onToggle(enabled);
			}
		}
	}

	public void Trigger()
	{
		if (_onActivate != null)
		{
			_onActivate();
		}
		else
		{
			Toggle();
		}
	}
}
