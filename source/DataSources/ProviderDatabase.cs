﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace DataSources
{
	/// <summary>
	/// Base class for all database related providers.
	/// </summary>
	internal abstract class ProviderDatabase : ProviderBase
	{
		/// <summary>
		/// Connection information for the database.
		/// </summary>
		private Dictionary<string, string> _connection;

		protected string _bin { get { return this.GetValueFromConnection("bin"); } }
		protected string _host { get { return this.GetValueFromConnection("host"); } }
		protected string _port { get { return this.GetValueFromConnection("port"); } }
		protected string _user { get { return this.GetValueFromConnection("user"); } }
		protected string _password { get { return this.GetValueFromConnection("password"); } }

		/// <summary>
		/// If a binary directory and a dump file name is set, this will be initalized with the full path.
		/// </summary>
		protected string _filePathDump;

		#region Initialization
		internal ProviderDatabase(Configurations.DataSource config) : base(config)
		{
			_connection = new Dictionary<string, string>();

			// parse configuration
			foreach (var nameValue in _config.Source.Split(';'))
			{
				var nameValueArgs = nameValue.Split('=');
				var name = nameValueArgs[0].Trim().ToLower().Replace(" ", "");
				var value = nameValueArgs.Length > 1 ? nameValueArgs[1].Trim() : null;

				_connection[name.ToLower()] = value;
			}

			// check availability of dump-binary
			_filePathDump = this.DumpGetBinaryName();

			// if bin is set, we check if dump exists. if not, we assume its set to the PATH
			if (!string.IsNullOrEmpty(_bin))
			{
				_filePathDump = Path.Combine(_bin, _filePathDump);

				if (!File.Exists(_filePathDump))
				{
					throw new Exception(string.Format("Could not access binary for creating backups at path ›{0}‹", _filePathDump));
				}
			}
		}
		#endregion

		#region ProviderDatabase
		/// <summary>
		/// If the database is backuped via a dump binary, return the name of the dump-file.
		/// </summary>
		/// <returns></returns>
		protected virtual string DumpGetBinaryName()
		{
			return null;
		}

		/// <summary>
		/// Executes the dump binary.
		/// </summary>
		/// <param name="args">The args to be passed to the binary.</param>
		/// <param name="filePathBackup">The path to the backup file to create.</param>
		/// <param name="outputToFile">If true, the output of the dump-process will be saved to the filePathBackup. If false, the dump-binary will handle this itself.</param>
		/// <param name="error">If set, an error did occur.</param>
		protected void DumpExecute(string args, string filePathBackup, bool outputToFile, out string error)
		{
			error = null;

			// create the info for startup
			var startInfo = new ProcessStartInfo();
			startInfo.FileName = _filePathDump;
			startInfo.RedirectStandardInput = true;
			startInfo.RedirectStandardOutput = outputToFile;
			startInfo.RedirectStandardError = true;
			startInfo.Arguments = args;
			startInfo.UseShellExecute = false;

			// start the process
			var process = Process.Start(startInfo);

			// write output to file if needed
			if (outputToFile)
			{
				process.OutputDataReceived += (s, e) =>
				{
					if (e.Data != null)
					{
						using (var writer = File.AppendText(filePathBackup))
						{
							writer.WriteLine(e.Data);
						}
					}
				};
				process.BeginOutputReadLine();
			}

			// let the process complete
			process.WaitForExit();

			// if an error did occur, set the error output.
			if (process.ExitCode != 0)
			{
				error = process.StandardError.ReadToEnd();

				if (string.IsNullOrEmpty(error))
				{
					error = "unknown";
				}

				Program.Logger.Log("Could not backup database {0}. Error: ›{1}‹", _config.Name, error);
			}
		}

		/// <summary>
		/// Gets the value from the _connection. This is a litte wrappter to avoid key not available errors.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		protected string GetValueFromConnection(string key)
		{
			return _connection.ContainsKey(key) ? _connection[key] : null;
		}

		/// <summary>
		/// Each database class should return all databases available without any filter applied.
		/// The filtering will be handled in GetDatabasesFiltered.
		/// </summary>
		/// <returns></returns>
		protected abstract List<string> GetDatabases();

		/// <summary>
		/// Gets all databases calling GetDatabases() and apply _included and _excluded.
		/// </summary>
		/// <returns></returns>
		protected List<string> GetDatabasesFiltered()
		{
			var databases = this.GetDatabases();

			if (_included != null)
			{
				databases.RemoveAll(database => !_included.Contains(database));
			}

			// ignore excluded databases
			if (_excluded != null)
			{
				databases.RemoveAll(database => _excluded.Contains(database));
			}

			return databases;
		}
		#endregion
	}
}