using Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using ValueObjects;

namespace DataSources
{
	/// <summary>
	/// The db2 provider backups IBM-DB2 databases-
	/// </summary>
	internal class ProviderDB2 : ProviderDatabase
	{
		#region Initialization
		internal ProviderDB2(Configurations.DataSource config, LoggerBase logger) : base(config, logger)
		{
		}
		#endregion

		#region Functionality
		protected override string DumpGetBinaryName()
		{
			return "db2.exe";
		}

		/// <summary>
		/// To get the db2 command line up and running we need to initialize it.
		/// This results in an environment var being set, which we return for further use.
		/// </summary>
		/// <returns></returns>
		private Dictionary<string, string> GetEnvironmentVars()
		{
			string output = string.Empty;

			// create the info for startup
			var startInfo = new ProcessStartInfo();
			startInfo.FileName = Path.Combine(_bin, "db2cmd.exe");
			startInfo.RedirectStandardInput = true;
			startInfo.RedirectStandardOutput = true;
			startInfo.RedirectStandardError = true;
			startInfo.Arguments = "-i -w db2clpsetcp";
			startInfo.UseShellExecute = false;

			// start the process
			var process = Process.Start(startInfo);

			process.OutputDataReceived += (s, e) =>
			{
				if (e.Data != null)
				{
					output += e.Data;
				}
			};
			process.BeginOutputReadLine();

			// let the process complete
			process.WaitForExit(2000); // need to timeout, since db2cmd will not complete

			if (output == null)
			{
				return null;
			}

			// get key and value for the var via regex
			// there should only be one var beeing set
			var match = Regex.Match(output, "([^ ]+)=([^ ]+)");
			if (match.Groups.Count < 3)
			{
				return null;
			}

			var environmentVars = new Dictionary<string, string>();
			environmentVars.Add(match.Groups[1].Value, match.Groups[2].Value);

			return environmentVars;
		}
		#endregion

		#region ProviderBase
		protected override List<string> GetSources()
		{
			// there is no relyable way to get all databases of a server
			// LIST DATABASE DIRECTORY and LIST ACTIVE DATABASES will only display language specific information
			// we could try to handle them via regex, but this seems prone to be broken

			return null;
		}

		internal override IEnumerable<BackupFile> Load(string directory)
		{
			if (_included == null || _included.Count == 0)
			{
				_logger.Log(_config.Name, LoggerPriorities.Error, "No includes are configured. This provider will not work without includes.");
				return null;
			}

			var environmentVars = this.GetEnvironmentVars();
			if (environmentVars == null)
			{
				_logger.Log(_config.Name, LoggerPriorities.Error, "Could not initialize backup environment.");
				return null;
			}

			var files = new List<BackupFile>();

			foreach (var database in _included)
			{
				var file = new BackupFile(directory, database + ".backup");

				// db2.exe needs a directory
				Directory.CreateDirectory(file.Path);

				var argsForDatabase = string.Format("BACKUP DATABASE {0} to {1} WITHOUT PROMPTING", database, file.Path);

				var didSucceed = this.DumpExecute(argsForDatabase, file.Path, false, environmentVars);

				if (didSucceed && Directory.Exists(file.Path))
				{
					file.CreatedOn = DateTime.UtcNow;

					files.Add(file);
				}
			}

			_logger.Log(_config.Name, LoggerPriorities.Info, "Created {0} backup{1:'s';'s';''}.", files.Count, files.Count - 1);

			return files;
		}
		#endregion
	}
}