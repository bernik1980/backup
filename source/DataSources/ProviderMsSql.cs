﻿using Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using ValueObjects;

namespace DataSources
{
	/// <summary>
	/// The mssql provider backups MS-SQL databases
	/// </summary>
	internal class ProviderMsSql : ProviderDatabase
	{
		#region Initialization
		internal ProviderMsSql(Configurations.DataSource config, LoggerBase logger)
			: base(config, logger)
		{
		}
		#endregion

		#region Functionality
		/// <summary>
		/// Small helper to open the connection to the database, since we need it multiple times.
		/// </summary>
		/// <returns></returns>
		private SqlConnection OpenConnection()
		{
			var parameters = new Dictionary<string, string>();

			parameters.Add("Data Source", _host);
			if (!string.IsNullOrEmpty(_port))
			{
				parameters["Data Source"] += "," + _port;
			}
			if (_user != null)
			{
				parameters.Add("User Id", _user);
				parameters.Add("Password", _password);
			}
			else
			{
				// if no user is specified, we assume integrated security
				parameters.Add("Integrated Security", "SSPI");
			}

			var connectionString = string.Join(";", parameters.Select(p => p.Key + "=" + p.Value));

			var conn = new SqlConnection(connectionString);

			try
			{
				conn.Open();
			}
			catch (Exception ex)
			{
				conn.Dispose();
				conn = null;

				_logger.Log(_config.Name, LoggerPriorities.Error, "Could not connect to server. Error: {0}.", ex.ToString());
			}

			return conn;
		}
		#endregion

		#region ProviderBase
		protected override List<string> GetSources()
		{
			var databases = new List<string>();

			// only get databases in state 'online'
			var conn = this.OpenConnection();
			if (conn != null)
			{
				using (var cmd = conn.CreateCommand())
				{
					cmd.CommandType = CommandType.Text;
					cmd.CommandText = "SELECT Name FROM master..sysdatabases WHERE databasepropertyex([Name],'Status') = 'online'";
					cmd.CommandTimeout = _timeout;
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							databases.Add(reader[0] as string);
						}
					}
				}

				conn.Dispose();
			}

			return databases;
		}

		internal override IEnumerable<BackupFile> Load(string directory)
		{
			var databases = this.GetSourcesFiltered();

			if (databases == null || databases.Count == 0)
			{
				_logger.Log(_config.Name, LoggerPriorities.Info, "No databases found.");
				return null;
			}

			var files = new List<BackupFile>();

			// use a query to backup the database
			var conn = this.OpenConnection();
			if (conn != null)
			{
				foreach (var database in databases)
				{
					var file = new BackupFile(directory, database + ".bak");

					using (var cmd = conn.CreateCommand())
					{
						cmd.CommandType = CommandType.Text;
						cmd.CommandText = "BACKUP DATABASE @database TO DISK = @filePath WITH FORMAT";
						cmd.CommandTimeout = _timeout;
						cmd.Parameters.AddWithValue("@database", database);
						cmd.Parameters.AddWithValue("@filePath", file.Path);
						try
						{
							cmd.ExecuteNonQuery();
						}
						catch (Exception ex)
						{
							_logger.Log(_config.Name, LoggerPriorities.Error, "Could not create backup for database {0}. Error: {1}.", database, ex.ToString());
						}
					}

					if (File.Exists(file.Path))
					{
						file.CreatedOn = DateTime.UtcNow;

						files.Add(file);
					}
				}

				conn.Dispose();
			}

			_logger.Log(_config.Name, LoggerPriorities.Info, "Created {0} backup{1:'s';'s';''}.", files.Count, files.Count - 1);

			return files;
		}
		#endregion
	}
}