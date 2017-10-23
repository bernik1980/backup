using System.Collections.Generic;

namespace Logging
{
	public class LoggerMultiple : LoggerBase
	{
		private IEnumerable<LoggerBase> _loggers;

		public LoggerMultiple(IEnumerable<LoggerBase> loggers)
		{
			_loggers = loggers;
		}

		public override void Log(string text)
		{
			foreach (var logger in _loggers)
			{
				logger.Log(text);
			}
		}
	}
}