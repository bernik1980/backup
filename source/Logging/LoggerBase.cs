using System.Text.RegularExpressions;

namespace Logging
{
	/// <summary>
	/// Base class for all loggers.
	/// </summary>
	public abstract class LoggerBase
	{
		private LoggerPriorities _priorities;

		public LoggerBase(LoggerPriorities priorities)
		{
			_priorities = priorities;
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