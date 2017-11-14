using System;
using System.Collections.Generic;
using System.IO;
using ValueObjects;

namespace DataSources
{
	/// <summary>
	/// The oracle provider backups Oracle-SQL databases
	/// </summary>
	internal class ProviderOracle : ProviderDatabase
	{
		#region Initialization
		internal ProviderOracle(Configurations.DataSource config) : base(config)
		{
		}
		#endregion

		#region ProviderDatabase
		protected override string DumpGetBinaryName()
		{
			return "rman.exe";
		}
		#endregion

		#region ProviderBase
		protected override List<string> GetSources()
		{
			// not needed for oracle
			return null;
		}

		internal override IEnumerable<BackupFile> Load(string directory)
		{
			var files = new List<BackupFile>();

			// an oracle server will only have one database
			// the backup of this database will contain 3 files
			// to get all 3 files into one zip, we create a seperate directory
			var file = new BackupFile(Path.Combine(directory, Guid.NewGuid().ToString()));
			file.Name = _host;

			Directory.CreateDirectory(file.Path);

			// save script file for rman
			var filePathCmd = Path.Combine(directory, file.Identifier + "_cmd");

			File.WriteAllLines(filePathCmd, this.GetCmdContent(file.Path, file.Name));

			// format args for rman
			var args = new List<KeyValuePair<string, string>>();
			args.Add(new KeyValuePair<string, string>("target", string.Format("{0}/{1}@{2}", _user, _password != null && _password.Length > 0 ? _password : "_", _host)));
			args.Add(new KeyValuePair<string, string>("cmdfile", filePathCmd));

			var argsString = string.Empty;
			foreach (var arg in args)
			{
				argsString += " " + arg.Key + " " + arg.Value;
			}

			// execute rman
			string error = null;
			this.DumpExecute(argsString, file.Path, false, out error);

			if (error == null && Directory.GetFiles(file.Path).Length == 3)
			{
				file.CreatedOn = DateTime.UtcNow;

				files.Add(file);
			}

			return files;
		}
		#endregion

		private List<string> GetCmdContent(string directory, string database)
		{
			var lines = new List<string>();
			lines.Add("SQL 'ALTER SYSTEM ARCHIVE LOG CURRENT';");
			lines.Add("RUN");
			lines.Add("{");
			lines.Add(string.Format("SET COMMAND ID TO '{0}OnlineBackupFull';", database));
			//lines.Add("ALLOCATE CHANNEL c1 DEVICE TYPE disk;");
			//lines.Add("ALLOCATE CHANNEL c2 DEVICE TYPE disk;");
			//lines.Add("ALLOCATE CHANNEL c3 DEVICE TYPE disk;");
			//lines.Add("ALLOCATE CHANNEL c4 DEVICE TYPE disk;");
			lines.Add(string.Format("BACKUP FULL DATABASE TAG '{0}_FULL' FORMAT '{1}\\%d_%I.full';", database, directory));
			lines.Add("SQL 'ALTER SYSTEM ARCHIVE LOG CURRENT';");
			lines.Add(string.Format("BACKUP TAG '{0}_ARCHIVE' FORMAT '{1}\\%d_%I.archive' ARCHIVELOG ALL DELETE ALL INPUT;", database, directory));
			lines.Add(string.Format("BACKUP TAG '{0}_CONTROL' CURRENT CONTROLFILE FORMAT '{1}\\%d_%I.control';", database, directory));
			//lines.Add("release channel c1;");
			//lines.Add("release channel c2;");
			//lines.Add("release channel c3;");
			//lines.Add("release channel c4;");
			lines.Add("}");

			return lines;
		}
	}
}