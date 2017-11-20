using System;
using System.Collections.Generic;

namespace Logging
{
	public class LoggerMultiple : LoggerBase
	{
		private IEnumerable<LoggerBase> _loggers;

		public LoggerMultiple(LoggerPriorities priorities) : base(priorities)
		{
			throw new NotSupportedException("LoggerMultiple can not be initilized with priorities, since its not logging itself.");
		}

		public LoggerMultiple(IEnumerable<LoggerBase> loggers) : base(LoggerPriorities.All)
		{
			_loggers = loggers;
		}

		protected override void LogInternal(LoggerMessage message)
		{
			foreach (var logger in _loggers)
			{
				logger.Log(message);
			}
		}
	}
}