using System.Text.RegularExpressions;

namespace Logging
{
	/// <summary>
	/// Base class for all loggers.
	/// </summary>
	public abstract class LoggerBase
	{
		protected Configurations.Logger _config;
		protected LoggerPriorities _priorities;

		public LoggerBase(Configurations.Logger config)
		{
			_config = config;

			_priorities = LoggerPriorities.None;

			// parse priorities
			if (_config != null && !string.IsNullOrEmpty(_config.Priorities))
			{
				_priorities = LoggerPriorities.None;

				var configPriorities = _config.Priorities.ToLower().Split(',');
				foreach (var configPriority in configPriorities)
				{
					switch (configPriority.Trim())
					{
						case "verbose":
							_priorities |= LoggerPriorities.Verbose;
							break;
						case "info":
							_priorities |= LoggerPriorities.Info;
							break;
						case "error":
							_priorities |= LoggerPriorities.Error;
							break;
					}
				}
			}

			// if no priorities were found, log all
			if (_priorities == LoggerPriorities.None)
			{
				_priorities = LoggerPriorities.All;
			}
		}

		public void Log(object tag, LoggerPriorities priority, string format, params object[] args)
		{
			if (format != null)
			{
				format = Regex.Replace(format, " ({[^{}]+})", " ›$1‹");
			}

			this.Log(tag, priority, args != null ? string.Format(format, args) : format);
		}

		public void Log(object tag, LoggerPriorities priority, string text)
		{
			this.Log(new LoggerMessage
			{
				Priority = priority,
				Tag = tag != null ? tag.ToString() : string.Empty,
				Text = text
			});
		}

		public void Log(LoggerMessage message)
		{
			if ((_priorities & message.Priority) == LoggerPriorities.None)
			{
				return;
			}

			this.LogInternal(message);
		}

		protected abstract void LogInternal(LoggerMessage message);
	}
}