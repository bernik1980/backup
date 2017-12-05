using System.Diagnostics;
using System.Reflection;

namespace Logging
{
	public class LoggerEventLog : LoggerBase
	{
		private string _source;

		#region Initialization
		public LoggerEventLog(Configurations.Logger config) : base(config)
		{
			_source = config.Settings;

			// if no source has been specified or if it is not available
			if (string.IsNullOrEmpty(_source) || !EventLog.SourceExists(_source))
			{
				_source = Assembly.GetExecutingAssembly().GetName().Name + "test";

				if (!EventLog.SourceExists(_source))
				{
					EventLog.CreateEventSource(_source, "Application");
				}
			}
		}
		#endregion

		#region LoggerBase
		protected override void LogInternal(LoggerMessage message)
		{
			var type = EventLogEntryType.Information;
			if (message.Priority == LoggerPriorities.Error)
			{
				type = EventLogEntryType.Error;
			}

			EventLog.WriteEntry(_source, message.Text, type);
		}
		#endregion
	}
}