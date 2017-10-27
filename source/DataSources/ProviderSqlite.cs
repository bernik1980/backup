﻿using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using ValueObjects;

namespace DataSources
{
	/// <summary>
	/// The sqlite provider backups SQLite databases
	/// </summary>
	internal class ProviderSqlite : ProviderBase
	{
		// remember source directory and filter for later use
		// since the source of this provider allows multiple formats
		private string _directorySource;
		private string _directorySourceFilter;

		#region Initialization
		internal ProviderSqlite(Configurations.DataSource config) : base(config)
		{
			// assume the last path component is the filter
			_directorySourceFilter = Path.GetFileName(_config.Source);
			// if not, clear it
			if (!_directorySourceFilter.Contains("*"))
			{
				_directorySourceFilter = null;
			}

			// get the directory of the source
			// its either the source itself, or if the source is a file or a filter, the parent of the source-path
			_directorySource = _config.Source;
			if (!Directory.Exists(_directorySource))
			{
				_directorySource = Path.GetDirectoryName(_directorySource);
			}

			if (!Directory.Exists(_directorySource))
			{
				throw new Exception("Source directory does not exist.");
			}
		}
		#endregion

		#region Functionality
		private SQLiteConnection GetConnection(string path)
		{
			if (!File.Exists(path))
			{
				Program.Logger.Log("Could not open SQLite database at path: ›{0}‹", path);
				return null;
			}

			// init database with some optimizations
			var connectionString = string.Format("Data Source={0};Version=3;Journal Mode=Off;Synchronous=Off", path);

			var conn = new SQLiteConnection(connectionString);

			try
			{
				conn.Open();
			}
			catch (Exception ex)
			{
				conn.Dispose();
				conn = null;

				Program.Logger.Log("Could not connect to database ›{0}‹. Error: ›{1}‹", _config.Name, ex.ToString());
			}

			return conn;
		}
		#endregion

		#region ProviderBase
		protected override List<string> GetSources()
		{
			var databases = new List<string>();

			// if the source is a file, there can only be one database
			if (File.Exists(_config.Source))
			{
				databases.Add(Path.GetFileName(_config.Source));
			}
			else
			{
				// if the source is a directory, load all files of that directory applying a possible filter
				var files = _directorySourceFilter != null ? Directory.GetFiles(_directorySource, _directorySourceFilter) : Directory.GetFiles(_directorySource);
				foreach (var file in files)
				{
					databases.Add(Path.GetFileName(file));
				}
			}

			return databases;
		}

		internal override IEnumerable<BackupFile> Load(string directory)
		{
			var files = new List<BackupFile>();

			var databases = this.GetSourcesFiltered();

			foreach (var database in databases)
			{
				// open a connection to the source database
				var connSource = this.GetConnection(Path.Combine(_directorySource, database));
				if (connSource == null)
				{
					return null;
				}

				var backupFile = new BackupFile(directory, database);

				// create the database of the backup database
				try
				{
					SQLiteConnection.CreateFile(backupFile.Path);
				}
				catch (Exception ex)
				{
					Program.Logger.Log("Could not create target database for backup. ›{0}‹. Error: ›{1}‹", _config.Name, ex.ToString());
					return null;
				}

				// open a connection to the backup database
				var connBackup = this.GetConnection(backupFile.Path);

				if (connBackup == null)
				{
					connSource.Dispose();
					return null;
				}

				// use integrated backup functionality
				connSource.BackupDatabase(connBackup, "main", "main", -1, null, 0);

				connBackup.Dispose();
				connSource.Dispose();

				if (File.Exists(backupFile.Path))
				{
					files.Add(backupFile);
				}
			}

			return files;
		}
		#endregion
	}
}