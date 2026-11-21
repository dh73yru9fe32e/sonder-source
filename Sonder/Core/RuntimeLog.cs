using System;
using System.IO;

namespace Sonder.Core;

internal static class RuntimeLog
{
	public static string FilePath { get; } = Path.Combine(Path.GetTempPath(), "Sonder.runtime.log");

	public static void Reset()
	{
		try
		{
			File.WriteAllText(FilePath, DateTime.Now.ToString("HH:mm:ss.fff") + "  session start" + Environment.NewLine);
		}
		catch
		{
		}
	}

	public static void Write(string message)
	{
		try
		{
			File.AppendAllText(FilePath, DateTime.Now.ToString("HH:mm:ss.fff") + "  " + message + Environment.NewLine);
		}
		catch
		{
		}
	}
}
