using UnityEngine;
using Object = UnityEngine.Object;
using System;

namespace Sonder.Core.UI.Framework;

public class ButtonData
{
	public string Name { get; set; }

	public string Category { get; set; }

	public ButtonType Type { get; set; }

	public bool Enabled { get; set; }

	public int CurrentIndex { get; set; }

	public string[] Options { get; set; }

	public Color? CustomColor { get; set; }

	public Action OnEnable { get; set; }

	public Action OnDisable { get; set; }

	public Action OnClick { get; set; }

	public Action<int, string> OnIncrementalChange { get; set; }

	public ButtonData(string name, string category)
	{
		Name = name;
		Category = category;
		Type = ButtonType.Action;
	}

	public ButtonData(string name, string category, Action onEnable, Action onDisable, Color? color = null)
	{
		Name = name;
		Category = category;
		Type = ButtonType.Toggle;
		OnEnable = onEnable;
		OnDisable = onDisable;
		CustomColor = color;
	}

	public ButtonData(string name, string category, Action onClick, Color? color = null)
	{
		Name = name;
		Category = category;
		Type = ButtonType.Action;
		OnClick = onClick;
		CustomColor = color;
	}

	public ButtonData(string name, string category, string[] options, Action<int, string> onChange)
	{
		Name = name;
		Category = category;
		Type = ButtonType.Incremental;
		Options = options;
		OnIncrementalChange = onChange;
		CurrentIndex = 0;
	}
}



