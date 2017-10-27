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
		protected override List<string> GetSources()
		{
			var files = new List<string>();

			if (File.Exists(_config.Source) || Directory.Exists(_config.Source))
			{
				files.Add(_config.Source);
			}

			return files;
		}

		internal override IEnumerable<BackupFile> Load(string directory)
		{
			var files = this.GetSourcesFiltered();

			return files.Count > 0 ? new BackupFile[] { new BackupFile(files[0]) { CreatedOn = DateTime.UtcNow } } : null;
		}
		#endregion
	}
}