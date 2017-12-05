using System;
using System.Collections.Generic;

namespace Logging
{
	public class LoggerMultiple : LoggerBase
	{
		private IEnumerable<LoggerBase> _loggers;

		public LoggerMultiple(Configurations.Logger config) : base(config)
		{
			throw new NotSupportedException("LoggerMultiple can not be initilized with a configuration, since its not logging itself.");
		}

		public LoggerMultiple(IEnumerable<LoggerBase> loggers) : base(null)
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