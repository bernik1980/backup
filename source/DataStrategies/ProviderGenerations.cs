using Logging;
using System;
using System.Collections.Generic;

namespace DataStrategies
{
	/// <summary>
	/// The generation provider handles dayly, weekly and montly backups.
	/// There will be dayly backups for the last 7 days, weekly backups for the last month and unlimited monthly backups.
	/// </summary>
	internal class ProviderGenerations : ProviderBase
	{
		#region Initialization
		public ProviderGenerations(Configurations.DataStrategy config, DataTargets.ProviderBase target, LoggerBase logger) : base(config, target, logger)
		{
		}
		#endregion

		#region ProviderBase
		public override void Save(IEnumerable<string> files)
		{
			// save all files via the related DataSource
			_target.Save(_timestamp.ToString("yyyy-MM-dd"), files);

			_logger.Log(_target.Config.Name, LoggerPriorities.Info, "Applying strategy with {0}.", _config.Provider);

			// if not on monday, we delete the backup from a week ago.
			// the monday backup will be kept for weekly backups
			if (_timestamp.DayOfWeek != DayOfWeek.Monday)
			{
				// delete the backup from a week ago
				_target.DeleteDirectory(_timestamp.AddDays(-7).ToString("yyyy-MM-dd"));
			}
			else
			{
				// delete weekly backup of last month
				var date = _timestamp.AddDays(-4 * 7);

				// but only, if it is not the first monday
				// this will be kept as monthly backup
				if (date.Month == date.AddDays(-7).Month)
				{
					_target.DeleteDirectory(date.ToString("yyyy-MM-dd"));
				}
			}
		}
		#endregion
	}
}