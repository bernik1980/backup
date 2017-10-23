using System;
using System.Collections.Generic;
using System.IO;
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
		public ProviderFile(Configurations.DataSource config) : base(config)
		{
		}
		#endregion

		#region ProviderBase
		internal override IEnumerable<BackupFile> Load(string directory)
		{
			if (!File.Exists(_config.Source) && !Directory.Exists(_config.Source))
			{
				return null;
			}

			// TODO: Apply _includes and _excludes

			return new BackupFile[] { new BackupFile(_config.Source) { CreatedOn = DateTime.UtcNow } };
		}
		#endregion
	}
}