using Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ValueObjects;

namespace DataSources
{
	/// <summary>
	/// The file provider backups a file or the content of a directory.
	/// The files will not be copied to the temp directory, but directly handled from the source.
	/// </summary>
	internal class ProviderFile : ProviderBase
	{
		#region Initialization
		public ProviderFile(Configurations.DataSource config, LoggerBase logger) : base(config, logger)
		{
		}
		#endregion

		#region ProviderBase
		protected override List<string> GetSources()
		{
			if (_config == null)
			{
				return null;
			}

			if (!File.Exists(_config.Source) && !Directory.Exists(_config.Source))
			{
				_logger.Log(_config.Name, LoggerPriorities.Error, "Could not get source ›{0}‹ Error: file/directory not found.", _config.Source);
				return null;
			}

			var files = new string[] { _config.Source }.ToList();

			_logger.Log(_config.Name, LoggerPriorities.Info, "Created {0} backup{1:'s';'s';''}.", files.Count, files.Count - 1);

			return files;
		}

		internal override IEnumerable<BackupFile> Load(string directory)
		{
			var files = this.GetSourcesFiltered();

			return files != null && files.Count > 0 ? new BackupFile[] { new BackupFile(files[0]) { CreatedOn = DateTime.UtcNow } } : null;
		}
		#endregion
	}
}