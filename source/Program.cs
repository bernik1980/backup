using Ionic.Zip;
using Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading.Tasks;
using ValueObjects;

/// <summary>
/// Entry class of the application.
/// </summary>
class Program
{
	/// <summary>
	/// Logger for the whole application.
	/// </summary>
	private LoggerBase _logger;

	private const string _loggerTag = "program";

	/// <summary>
	/// Created backup will be saved here, before handled by the target provider.
	/// </summary>
	private string _directoryTemp;

	#region Initialization
	/// <summary>
	/// Entry point of the application.
	/// </summary>
	/// <param name="args"></param>
	static void Main(string[] args)
	{
		new Program().Run();
	}
	#endregion

	#region Functionality
	/// <summary>
	/// Livecycle of the application
	/// </summary>
	private void Run()
	{
		AppDomain.CurrentDomain.UnhandledException += (s, e) =>
		{
			if (_logger != null)
			{
				_logger.Log(_loggerTag, LoggerPriorities.Error, "An unhandled exception did occur: {0}", e.ExceptionObject);
			}
			else
			{
				Console.WriteLine("An unhandled exception did occur.");
			}
		};

		// initialize logging
		InitializeLogger();

		_logger.Log(_loggerTag, LoggerPriorities.Info, "Starting backup run.");

		// try to initialize temp directory
		InitializeTempDirectory();

		// get sources and targets from config
		var providersSource = this.GetSources();
		var providersTarget = this.GetTargets();

		if (providersSource.Count() == 0)
		{
			_logger.Log(_loggerTag, LoggerPriorities.Error, "No dataSources found. Quitting.");
			return;
		}

		if (providersTarget.Count() == 0)
		{
			_logger.Log(_loggerTag, LoggerPriorities.Error, "No dataTargets found. Quitting.");
			return;
		}

		// let all sources load the backups
		var files = this.SourcesLoad(providersSource);
		// zip the backups to the temp directory
		var zips = this.SourcesZip(files);
		// let all targets save the zips
		this.TargetsSave(providersTarget, zips);

		// clean up
		foreach (var provider in providersTarget)
		{
			provider.Dispose();
		}

		// delete created temp directory
		try
		{
			Directory.Delete(_directoryTemp, true);
		}
		catch (Exception ex)
		{
			_logger.Log(_loggerTag, LoggerPriorities.Error, "Could not delete created temp directory. Directory: {0}, Error: {1}", _directoryTemp, ex.ToString());
		}

		_logger.Log(_loggerTag, LoggerPriorities.Info, "Completed backup run.");

		if (Environment.UserInteractive)
		{
			Console.ReadKey();
		}
	}

	/// <summary>
	/// Creates the logger for the application.
	/// </summary>
	private void InitializeLogger()
	{
		var loggers = new List<LoggerBase>();
		// we want to show logger exception after all loggers have been initialized, so "remember" these
		var loggersFailed = new Dictionary<string, Exception>();

		// create loggers based on configuration
		foreach (Configurations.Logger configLogger in Configurations.LoggerSection.GetLoggers())
		{
			if (string.IsNullOrEmpty(configLogger.Provider))
			{
				_logger.Log(_loggerTag, LoggerPriorities.Error, "Ignoring Logger without provider.");
				continue;
			}

			LoggerBase logger = null;

			// try to greate loggers based on provider name
			try
			{
				switch (configLogger.Provider.ToLower())
				{
					case "console":
						logger = new LoggerConsole(configLogger);
						break;
					case "file":
						logger = new LoggerFile(configLogger);
						break;
				}
			}
			catch (Exception ex)
			{
				loggersFailed.Add(configLogger.Provider, ex);
			}

			if (logger == null)
			{
				_logger.Log(_loggerTag, LoggerPriorities.Error, "Unknown logger {0}", configLogger.Provider);
				continue;
			}

			loggers.Add(logger);
		}

		_logger = new LoggerMultiple(loggers);

		foreach (var logger in loggersFailed)
		{
			_logger.Log(_loggerTag, LoggerPriorities.Error, "Could not create logger {0}. Error: {1}", logger.Key, logger.Value);
		}
	}

	/// <summary>
	/// Creates the temp directory for the backups.
	/// It will be created at the system temp path (Path.GetTempPath()).
	/// Full write permission will be set, to have it fully available by all data sources.
	/// </summary>
	private void InitializeTempDirectory()
	{
		_directoryTemp = Path.GetTempPath();
		_directoryTemp = Path.Combine(_directoryTemp, Guid.NewGuid().ToString());

		try
		{
			Directory.CreateDirectory(_directoryTemp);

			// set full access
			var directoryInfo = new DirectoryInfo(_directoryTemp);
			var directorySecurity = directoryInfo.GetAccessControl();
			directorySecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
			directoryInfo.SetAccessControl(directorySecurity);
		}
		catch (Exception ex)
		{
			_logger.Log(_loggerTag, LoggerPriorities.Error, "Could not create temp directory at {0}. Error: {1}. Quitting", _directoryTemp, ex.ToString());
		}
	}

	/// <summary>
	/// Gets all sources from the configuration.
	/// </summary>
	/// <returns></returns>
	private List<DataSources.ProviderBase> GetSources()
	{
		var sources = new List<DataSources.ProviderBase>();

		foreach (Configurations.DataSource dataSource in Configurations.DataSourcesSection.GetDataSources())
		{
			if (string.IsNullOrEmpty(dataSource.Provider))
			{
				_logger.Log(_loggerTag, LoggerPriorities.Error, "Ignoring DataSource without provider.");
				continue;
			}

			DataSources.ProviderBase source = null;

			switch (dataSource.Provider.ToLower().Trim())
			{
				case "mssql":
					source = new DataSources.ProviderMsSql(dataSource, _logger);
					break;
				case "postgresql":
					source = new DataSources.ProviderPgSql(dataSource, _logger);
					break;
				case "oracle":
					source = new DataSources.ProviderOracle(dataSource, _logger);
					break;
				case "mysql":
					source = new DataSources.ProviderMySql(dataSource, _logger);
					break;
				case "sqlite":
					source = new DataSources.ProviderSqlite(dataSource, _logger);
					break;
				case "directory":
				case "file":
					source = new DataSources.ProviderFile(dataSource, _logger);
					break;
			}

			if (source == null)
			{
				_logger.Log(_loggerTag, LoggerPriorities.Error, "Unknown DataSource {0}", dataSource.Provider);
				continue;
			}

			sources.Add(source);
		}

		return sources;
	}

	/// <summary>
	/// Gets all targets with theire strategy from the configuration.
	/// </summary>
	/// <returns></returns>
	private List<DataStrategies.ProviderBase> GetTargets()
	{
		var targets = new List<DataStrategies.ProviderBase>();

		foreach (Configurations.DataTarget dataTarget in Configurations.DataTargetsSection.GetDataTargets())
		{
			// get target provider first
			if (string.IsNullOrEmpty(dataTarget.Provider))
			{
				_logger.Log(_loggerTag, LoggerPriorities.Error, "Ignoring DataTarget without provider.");
				continue;
			}

			DataTargets.ProviderBase target = null;

			switch (dataTarget.Provider.ToLower().Trim())
			{
				case "directory":
					target = new DataTargets.ProviderDirectory(dataTarget, _logger);
					break;
				case "dropbox":
					target = new DataTargets.ProviderDropbox(dataTarget, _logger);
					break;
			}

			if (target == null)
			{
				_logger.Log(_loggerTag, LoggerPriorities.Error, "Unknown DataTarget {0}", dataTarget.Provider);
				continue;
			}

			if (dataTarget.Strategy == null)
			{
				continue;
			}

			if (string.IsNullOrEmpty(dataTarget.Strategy.Provider))
			{
				_logger.Log(_loggerTag, LoggerPriorities.Error, "Ignoring strategy without provider.");
				continue;
			}

			// get strategy second
			DataStrategies.ProviderBase strategy = null;

			switch (dataTarget.Strategy.Provider.ToLower().Trim())
			{
				case "days":
					strategy = new DataStrategies.ProviderDays(dataTarget.Strategy, target, _logger);
					break;
				case "generations":
					strategy = new DataStrategies.ProviderGenerations(dataTarget.Strategy, target, _logger);
					break;
			}

			if (strategy == null)
			{
				_logger.Log(_loggerTag, LoggerPriorities.Error, "Unknown strategry {0}", dataTarget.Strategy.Provider);
				continue;
			}

			targets.Add(strategy);
		}

		return targets;
	}

	/// <summary>
	/// Create backup from sources.
	/// </summary>
	/// <param name="providers"></param>
	/// <returns></returns>
	private Dictionary<DataSources.ProviderBase, IEnumerable<BackupFile>> SourcesLoad(IEnumerable<DataSources.ProviderBase> providers)
	{
		var files = new Dictionary<DataSources.ProviderBase, IEnumerable<BackupFile>>();

		// loading is handled prallel in different tasks for each provider
		var tasks = new Dictionary<DataSources.ProviderBase, Task<IEnumerable<BackupFile>>>();
		foreach (var provider in providers)
		{
			var task = Task<IEnumerable<BackupFile>>.Run(() => { return provider.Load(_directoryTemp); });
			tasks.Add(provider, task);
		}

		// join all tasks
		foreach (var task in tasks)
		{
			task.Value.Wait();

			var provider = task.Key;
			var filesProvider = task.Value.Result;

			if (filesProvider != null && filesProvider.Count() > 0)
			{
				files.Add(provider, filesProvider);
			}
		}

		return files;
	}

	private List<string> SourcesZip(Dictionary<DataSources.ProviderBase, IEnumerable<BackupFile>> sources)
	{
		var zips = new List<string>();

		foreach (var provider in sources)
		{
			_logger.Log(provider.Key.Config.Name, LoggerPriorities.Info, "Zipping {0} backup{1:'s';'s';''}.", provider.Value.Count(), provider.Value.Count() - 1);

			// prefix backup with provider name, to ensure unique file names
			var namePrefix = provider.Key.Config.Name;
			// be sure to only contain valid file name chars
			namePrefix = Path.GetInvalidFileNameChars().Aggregate(namePrefix, (current, c) => current.Replace(c.ToString(), string.Empty));

			foreach (var file in provider.Value)
			{
				var fileZip = Path.Combine(_directoryTemp, string.Format("{0}_{1}_{2:yyyyMMddHHmmss}.zip", namePrefix, file.Name, file.CreatedOn != null ? file.CreatedOn.Value : DateTime.UtcNow));

				using (var zipFile = new ZipFile(fileZip))
				{
					zipFile.CompressionMethod = CompressionMethod.Deflate;
					zipFile.CompressionLevel = Ionic.Zlib.CompressionLevel.BestCompression;

					if (File.Exists(file.Path)) // detect files
					{
						var entry = zipFile.AddFile(file.Path, string.Empty);
						entry.FileName = file.Name;
					}
					else if (Directory.Exists(file.Path)) // detect directories
					{
						zipFile.AddDirectory(file.Path, string.Empty);
					}

					try
					{
						zipFile.Save();

						zips.Add(fileZip);
					}
					catch (Exception ex)
					{
						_logger.Log(_loggerTag, LoggerPriorities.Error, "Could not zip source. Zip: {0}, Error: {1}", Path.GetFileName(fileZip), ex);
					}
				}
			}
		}

		return zips;
	}

	/// <summary>
	/// Saves the files for each provider.
	/// </summary>
	/// <param name="providers"></param>
	/// <param name="files"></param>
	private void TargetsSave(IEnumerable<DataStrategies.ProviderBase> providers, List<string> files)
	{
		// loading is handled prallel in different tasks for each provider
		var tasks = new Dictionary<DataStrategies.ProviderBase, Task>();
		foreach (var provider in providers)
		{
			var task = Task.Run(() => { provider.Save(files); });
			tasks.Add(provider, task);
		}

		foreach (var task in tasks)
		{
			task.Value.Wait();
		}
	}
	#endregion
}