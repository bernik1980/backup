using System;
using System.Collections.Generic;
using System.IO;

namespace DataTargets
{
	/// <summary>
	/// The directory provider saves the files to a specified directory.
	/// The "target" property of the configuration is the path to this directory.
	/// </summary>
	internal class ProviderDirectory : ProviderBase
	{
		#region Initialization
		internal ProviderDirectory(Configurations.DataTarget config)
			: base(config)
		{
		}
		#endregion

		#region Functionality
		internal override List<string> Save(string directory, IEnumerable<string> files)
		{
			var directoryBackup = _config.Target;
			directoryBackup = Path.Combine(directoryBackup, directory);

			// check if the backup directory needs to be created.
			if (!Directory.Exists(directoryBackup))
			{
				try
				{
					Directory.CreateDirectory(directoryBackup);
				}
				catch (Exception ex)
				{
					Program.Logger.Log(string.Format("Can not create backup. Could not create backup directory at {0}. Error: {1}.", directoryBackup, ex.ToString()));

					return null;
				}
			}

			var filesSaved = new List<string>();

			// copy all backup files to the backup directory.
			foreach (var file in files)
			{
				var fileBackup = Path.Combine(directoryBackup, Path.GetFileName(file));

				try
				{
					File.Copy(file, fileBackup, true);

					filesSaved.Add(file);
				}
				catch (Exception ex)
				{
					Program.Logger.Log("Could not copy zipped backup to backup directory. Error: ›{0}‹", ex.ToString());
				}
			}

			return filesSaved;
		}

		internal override void DeleteDirectory(string directory)
		{
			var directoryPath = Path.Combine(_config.Target, directory);

			if (!Directory.Exists(directoryPath))
			{
				return;
			}

			Directory.Delete(directoryPath, true);
		}
		#endregion
	}
}