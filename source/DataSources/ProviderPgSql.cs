using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using ValueObjects;

namespace DataSources
{
	/// <summary>
	/// The pgsql provider backups Postgre-SQL databases
	/// </summary>
	internal class ProviderPgSql : ProviderDatabase
	{
		#region Initialization
		internal ProviderPgSql(Configurations.DataSource config)
			: base(config)
		{
		}
		#endregion

		#region ProviderDatabase
		// creating the backup will be handled via a dump binay
		protected override string DumpGetBinaryName()
		{
			return "pg_dump.exe";
		}
		#endregion

		#region ProviderBase
		protected override List<string> GetSources()
		{
			var databases = new List<string>();

			// use simply script to get all databases
			using (var conn = new NpgsqlConnection(string.Format("Host={0};Username={1};Password={2}", _host, _user, _password)))
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
					cmd.CommandText = "SELECT datname FROM pg_database WHERE datistemplate = FALSE";
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
				args.Add(new KeyValuePair<string, string>("username", "\"" + _user + "\""));
			}
			args.Add(new KeyValuePair<string, string>("no-password", null));
			args.Add(new KeyValuePair<string, string>("format", "tar"));
			args.Add(new KeyValuePair<string, string>("blobs", null));

			var argsString = string.Empty;
			foreach (var arg in args)
			{
				argsString += " --" + arg.Key;

				if (!string.IsNullOrEmpty(arg.Value))
				{
					argsString += " " + arg.Value;
				}
			}

			foreach (var database in databases)
			{
				var file = new BackupFile(directory, database + ".backup");

				var argsForDatabase = argsString;
				argsForDatabase += " --file \"" + file.Path + "\"";
				argsForDatabase += " \"" + database + "\"";

				string error = null;
				this.DumpExecute(argsForDatabase, file.Path, false, out error);

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