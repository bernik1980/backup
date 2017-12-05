using System;

namespace Logging
{
	/// <summary>
	/// Logs to the console or and cmd output.
	/// </summary>
	public class LoggerConsole : LoggerBase
	{
		public LoggerConsole(Configurations.Logger config) : base(config)
		{
			Console.BackgroundColor = ConsoleColor.Black;
		}

		protected override void LogInternal(LoggerMessage message)
		{
			if (!Environment.UserInteractive)
			{
				return;
			}

			switch (message.Priority)
			{
				case LoggerPriorities.Error:
					Console.ForegroundColor = ConsoleColor.Red;
					break;
				case LoggerPriorities.Verbose:
					Console.ForegroundColor = ConsoleColor.Gray;
					break;
				default:
					Console.ForegroundColor = ConsoleColor.White;
					break;
			}

			Console.WriteLine(string.Format("{1:HH:mm:ss} {0} ({2}): {3}", message.Tag, message.CreatedOn, message.Priority.ToString(), message.Text));
		}
	}
}