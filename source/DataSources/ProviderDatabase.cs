using Logging;
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
		protected int _timeout {  get { return this.GetValueFromConnectionInt("timeout", 120); } }
		protected string _user { get { return this.GetValueFromConnection("user"); } }
		protected string _password { get { return this.GetValueFromConnection("password"); } }

		/// <summary>
		/// If a binary directory and a dump file name is set, this will be initalized with the full path.
		/// </summary>
		protected string _filePathDump;

		#region Initialization
		internal ProviderDatabase(Configurations.DataSource config, LoggerBase logger) : base(config, logger)
		{
			_connection = new Dictionary<string, string>();

			// parse configuration
			if (_config != null)
			{
				foreach (var nameValue in _config.Source.Split(';'))
				{
					var nameValueArgs = nameValue.Split('=');
					var name = nameValueArgs[0].Trim().ToLower().Replace(" ", "");

					if (string.IsNullOrEmpty(name))
					{
						continue;
					}

					var value = nameValueArgs.Length > 1 ? nameValueArgs[1].Trim() : null;

					_connection[name.ToLower()] = value;
				}
			}

			// check availability of dump-binary
			_filePathDump = this.DumpGetBinaryName();

			// if bin is set, we check if dump exists. if not, we assume its set to the PATH
			if (!string.IsNullOrEmpty(_filePathDump) && !string.IsNullOrEmpty(_bin))
			{
				_filePathDump = Path.Combine(_bin, _filePathDump);
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
		protected bool DumpExecute(string args, string filePathBackup, bool outputToFile)
		{
			if (_filePathDump == null)
			{
				return false;
			}

			if (Path.IsPathRooted(_filePathDump) && !File.Exists(_filePathDump))
			{
				_logger.Log(_config.Name, LoggerPriorities.Error, "Could not access binary for creating backups at path {0}.", _filePathDump);
				return false;
			}

			// create the info for startup
			var startInfo = new ProcessStartInfo();
			startInfo.FileName = _filePathDump;
			startInfo.RedirectStandardInput = true;
			startInfo.RedirectStandardOutput = true;
			startInfo.RedirectStandardError = true;
			startInfo.Arguments = args;
			startInfo.UseShellExecute = false;

			// start the process
			var process = Process.Start(startInfo);

			process.OutputDataReceived += (s, e) =>
			{
				if (e.Data != null)
				{
					// write to file or log
					if (outputToFile)
					{
						using (var writer = File.AppendText(filePathBackup))
						{
							writer.WriteLine(e.Data);
						}
					}
					else
					{
						_logger.Log(_config.Name, LoggerPriorities.Verbose, e.Data);
					}
				}
			};
			process.BeginOutputReadLine();

			// let the process complete
			process.WaitForExit();

			// if an error did occur, set the error output.
			if (process.ExitCode != 0)
			{
				var error = process.StandardError.ReadToEnd();

				if (string.IsNullOrEmpty(error))
				{
					error = "unknown";
				}

				_logger.Log(_config.Name, LoggerPriorities.Error, "Could not backup database. Error: {0}.", error);

				return false;
			}

			return true;
		}

		/// <summary>
		/// Gets the value from the _connection. This is a litte wrappter to avoid key not available errors.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		protected string GetValueFromConnection(string key, string defaultValue = null)
		{
			return _connection != null && _connection.ContainsKey(key) ? _connection[key] : defaultValue;
		}

		protected int GetValueFromConnectionInt(string key, int defaultValue = 0)
		{
			var value = default(int);

			if (!int.TryParse(this.GetValueFromConnection(key), out value))
			{
				value = defaultValue;
			}

			return value;
		}
		#endregion
	}
}