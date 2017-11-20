using System;

namespace Logging
{
	public class LoggerMessage
	{
		public DateTime CreatedOn { get; private set; }
		public LoggerPriorities Priority { get; set; }
		public string Tag { get; set; }
		public string Text { get; set; }

		public LoggerMessage()
		{
			this.CreatedOn = DateTime.UtcNow;
			this.Priority = LoggerPriorities.Verbose;
		}
	}
}