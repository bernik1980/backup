using System;

namespace Logging
{
	/// <summary>
	/// Logs to the console or and cmd output.
	/// </summary>
	public class LoggerConsole : LoggerBase
	{
		public override void Log(string text)
		{
			Console.WriteLine(text);
		}
	}
}