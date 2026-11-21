using System.Collections.Generic;

namespace Sonder.Managers;

public sealed class ModCategory
{
	public readonly string Name;

	public readonly List<ModEntry> Mods = new List<ModEntry>();

	public ModCategory(string name)
	{
		Name = name;
	}
}
