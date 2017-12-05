using System;
using System.IO;
using System.Reflection;

namespace Logging
{
	/// <summary>
	/// Logs as csv to a file.
	/// </summary>
	public class LoggerFile : LoggerBase
	{
		private string _directoryLogs;

		/// <summary>
		/// Object for locking when accessing log-files externally.
		/// </summary>
		private readonly object LockObject = new object();
		/// <summary>
		/// The field-separator for the created csv-file.
		/// </summary>
		private const string FieldSeparator = ";";
		/// <summary>
		/// If the separator exists in a message to log, it will be replaced with this.
		/// </summary>
		private const string _separatorReplacement = "_";
		/// <summary>
		/// The header to write at the beginning of newly created log files
		/// </summary>
		private readonly string _header = string.Format("Timestamp{0}Text", FieldSeparator);

		public LoggerFile(Configurations.Logger config) : base(config)
		{
			var directory = _config.Settings;

			// if no directory is set, we try to write to the application data folder
			if (string.IsNullOrEmpty(directory))
			{
				directory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
				directory = Path.Combine(directory, Assembly.GetExecutingAssembly().GetName().Name);
				directory = Path.Combine(directory, "Logs");
			}
			else if (!Path.IsPathRooted(directory))
			{
				directory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), directory);
			}

			_directoryLogs = directory;

			// this will thow if parent directory does not exist or permission is missing
			// this is ok to let the outside caller know about this
			if (!Directory.Exists(_directoryLogs))
			{
				Directory.CreateDirectory(_directoryLogs);
			}

			// write empty message to test access
			this.Log(new LoggerMessage { Text = "Testing file access." }, true);
		}

		/// <summary>
		/// Gets the full path to the log file for the specified date.
		/// </summary>
		/// <param name="date">The date to get the log file for. This has to be utc time.</param>
		/// <returns></returns>
		private string GetLogFileForDate(DateTime date)
		{
			return Path.Combine(_directoryLogs, date.ToString("yyyyMMdd'.csv'"));
		}

		protected override void LogInternal(LoggerMessage message)
		{
			this.Log(message, false);
		}

		private void Log(LoggerMessage message, bool probe)
		{
			if (_directoryLogs == null)
			{
				return;
			}

			var messageArgs = new string[]
			{
				message.CreatedOn.ToString("yyyy-MM-dd HH:mm:ss"),
				message.Tag != null ? message.Tag : string.Empty,
				message.Priority.ToString(),
				message.Text != null ? message.Text : string.Empty
			};

			var text = string.Join(FieldSeparator, messageArgs);

			lock (LockObject)
			{
				var filePath = this.GetLogFileForDate(DateTime.UtcNow);
				var writeHeader = !File.Exists(filePath);

				FileStream fileStream = null;

				try
				{
					fileStream = File.Open(filePath, FileMode.OpenOrCreate);
					fileStream.Position = fileStream.Length;
				}
				catch (Exception ex)
				{
					if (probe)
					{
						throw (ex);
					}
				}

				try
				{
					using (StreamWriter writer = new StreamWriter(fileStream))
					{
						if (writeHeader)
						{
							writer.WriteLine(_header);
						}

						writer.WriteLine(text);
					}
				}
				catch (Exception ex)
				{
					if (probe)
					{
						throw (ex);
					}
				}
				finally
				{
					if (fileStream != null)
					{
						fileStream.Close();
					}
				}
			}
		}
	}
}