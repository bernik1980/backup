using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using ValueObjects;

namespace DataSources
{
	/// <summary>
	/// The mysql provider backups My-SQL databases
	/// </summary>
	internal class ProviderMySql : ProviderDatabase
	{
		#region Initialization
		internal ProviderMySql(Configurations.DataSource config)
			: base(config)
		{
		}
		#endregion

		#region ProviderDatabase
		// creating the backup will be handled via a dump binay
		protected override string DumpGetBinaryName()
		{
			return "mysqldump.exe";
		}
		#endregion

		#region ProviderBase
		protected override List<string> GetSources()
		{
			var databases = new List<string>();

			var connectionString = string.Format("Data Source={0};User Id={1};Password={2}", _host, _user, _password);
			if (!string.IsNullOrEmpty(_port))
			{
				connectionString += ";Port=" + _port;
			}

			// use simply script to get all databases
			using (var conn = new MySqlConnection(connectionString))
			{
				try
				{
					conn.Open();
				}
				catch (Exception ex)
				{
					Program.Logger.Log("Could not connect to database {0}. Error: ›{1}‹", _config.Name, ex.ToString());
					return null;
				}

				using (var cmd = conn.CreateCommand())
				{
					cmd.CommandType = CommandType.Text;
					cmd.CommandText = "SHOW DATABASES";
					cmd.CommandTimeout = _timeout;
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							databases.Add(reader[0] as string);
						}
					}
				}
			}

			return databases;
		}

		internal override IEnumerable<BackupFile> Load(string directory)
		{
			var databases = this.GetSourcesFiltered();

			if (databases.Count == 0)
			{
				return null;
			}

			var files = new List<BackupFile>();

			// format args for dump binary
			var args = new List<KeyValuePair<string, string>>();
			args.Add(new KeyValuePair<string, string>("host", _host));
			if (!string.IsNullOrEmpty(_port))
			{
				args.Add(new KeyValuePair<string, string>("port", _port));
			}
			if (!string.IsNullOrEmpty(_user))
			{
				args.Add(new KeyValuePair<string, string>("user", "\"" + _user + "\""));
			}
			args.Add(new KeyValuePair<string, string>("password", _password));

			var argsString = string.Empty;
			foreach (var arg in args)
			{
				argsString += " --" + arg.Key;

				if (!string.IsNullOrEmpty(arg.Value))
				{
					argsString += "=" + arg.Value;
				}
			}

			foreach (var database in databases)
			{
				var file = new BackupFile(directory, database + ".sql");

				var argsForDatabase = argsString;
				argsForDatabase += " --databases \"" + database + "\"";

				string error = null;
				this.DumpExecute(argsForDatabase, file.Path, true, out error);

				if (error == null && File.Exists(file.Path))
				{
					file.CreatedOn = DateTime.UtcNow;

					files.Add(file);
				}
			}

			return files;
		}
		#endregion
	}
}