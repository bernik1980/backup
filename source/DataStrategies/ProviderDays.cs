using System;
using System.Collections.Generic;

namespace DataStrategies
{
	/// <summary>
	/// The days provider handles dayly backups.
	/// If the Revisions(-count) is set via config, only this number of recent days will be kept
	/// </summary>
	internal class ProviderDays : ProviderBase
	{
		#region Initialization
		public ProviderDays(Configurations.DataStrategy config, DataTargets.ProviderBase target) : base(config, target)
		{
		}
		#endregion

		#region ProviderBase
		public override void Save(IEnumerable<string> files)
		{
			// save all files via the related DataSource
			_target.Save(_timestamp.ToString("yyyy-MM-dd"), files);

			if (_config.Revisions <= 0)
			{
				return;
			}

			// get the first date out of revisions range
			var date = _timestamp.AddDays(-_config.Revisions);

			// delete the directory for this date
			// we do not check every other directory, since we assume dayly backups
			try
			{
				_target.DeleteDirectory(date.ToString("yyyy-MM-dd"));
			}
			catch (Exception ex)
			{
				Program.Logger.Log("Could not delete old revision. Date: {0:yyyy-MM-dd}, Error: {1}.", date, ex.ToString());
			}
		}
		#endregion
	}
}