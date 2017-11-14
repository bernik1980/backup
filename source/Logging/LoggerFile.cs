using System;
using System.IO;

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

		public LoggerFile(string directory)
		{
			_directoryLogs = directory;

			if (!Directory.Exists(_directoryLogs))
			{
				Directory.CreateDirectory(_directoryLogs);
			}

			this.Log("Initialized logging", true);
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

		/// <summary>
		/// Adds a new log entry.
		/// </summary>
		/// <param name="caller">The instance making the log call.</param>
		/// <param name="user">The user currently logged in.</param>
		/// <param name="project">The project currently active.</param>
		/// <param name="messageParams">Values to log for ONE message.</param>
		public override void Log(string text)
		{
			this.Log(text, false);
		}

		/// <summary>
		/// Adds a message to the queue.
		/// </summary>
		/// <param name="caller"></param>
		/// <param name="text"></param>
		/// <param name="user"></param>
		/// <param name="project"></param>
		private void Log(string text, bool throwError)
		{
			if (_directoryLogs == null)
			{
				return;
			}

			var messageArgs = new string[]
			{
				DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
				text != null ? text : string.Empty
			};

			text = string.Join(FieldSeparator, messageArgs);

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
					if (throwError)
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
					if (throwError)
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