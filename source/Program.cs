using Ionic.Zip;
using Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using ValueObjects;

/// <summary>
/// Entry class of the application.
/// </summary>
class Program
{
	/// <summary>
	/// Logger for the whole application.
	/// </summary>
	public static LoggerBase Logger { get; private set; }

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
		InitializeLogger();

		new Program().Run();
	}

	public Program()
	{
		InitializeTempDirectory();
	}

	/// <summary>
	/// Creates the logger for the application.
	/// </summary>
	private static void InitializeLogger()
	{
		// console logging is always available
		var loggerConsole = new LoggerConsole();

		var loggers = new List<LoggerBase>();
		loggers.Add(loggerConsole);

		// try to create the file logger
		try
		{
			loggers.Add(new LoggerFile());
		}
		catch (Exception ex)
		{
			loggerConsole.Log("Could not initilalize file logging. Error: {0}", ex.ToString());
		}

		Program.Logger = new LoggerMultiple(loggers);
	}

	/// <summary>
	/// Creates the temp directory for the backups.
	/// It will be created at the system temp path (Path.GetTempPath()).
	/// Full write permission will be set, to have it fully available by all data sources.
	/// </summary>
	private void InitializeTempDirectory()
	{
		var directoryTemp = Path.GetTempPath();
		directoryTemp = Path.Combine(directoryTemp, Guid.NewGuid().ToString());

		try
		{
			Directory.CreateDirectory(directoryTemp);
		}
		catch (Exception ex)
		{
			var text = string.Format("Can not create backup.Could not create temp directory at { 0}. Error: { 1}.", directoryTemp, ex.ToString());

			Program.Logger.Log(text);

			throw new Exception(text);
		}

		// set full access
		var directoryInfo = new DirectoryInfo(directoryTemp);
		var directorySecurity = directoryInfo.GetAccessControl();
		directorySecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
		directoryInfo.SetAccessControl(directorySecurity);

		_directoryTemp = directoryTemp;
	}
	#endregion

	#region Functionality
	/// <summary>
	/// Livecycle of the application
	/// </summary>
	private void Run()
	{
		Program.Logger.Log("Starting backup run.");

		// get sources and targets from config
		var providersSource = this.GetSources();
		var providersTarget = this.GetTargets();

		if (providersSource.Count() == 0)
		{
			Program.Logger.Log("No dataSources found. Quitting.");
			return;
		}

		if (providersTarget.Count() == 0)
		{
			Program.Logger.Log("No dataTargets found. Quitting.");
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
			Logger.Log("Could not delete created temp directory. Directory: {0}, Error: {1}", _directoryTemp, ex.ToString());
		}

		Logger.Log("Completed backup run.");
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
				Program.Logger.Log("Ignoring DataSource without provider.");
				continue;
			}

			DataSources.ProviderBase source = null;

			try
			{
				switch (dataSource.Provider.ToLower().Trim())
				{
					case "mssql":
						source = new DataSources.ProviderMsSql(dataSource);
						break;
					case "postgresql":
						source = new DataSources.ProviderPgSql(dataSource);
						break;
					case "oracle":
						source = new DataSources.ProviderOracle(dataSource);
						break;
					case "mysql":
						source = new DataSources.ProviderMySql(dataSource);
						break;
					case "sqlite":
						source = new DataSources.ProviderSqlite(dataSource);
						break;
					case "directory":
					case "file":
						source = new DataSources.ProviderFile(dataSource);
						break;
				}
			}
			catch (Exception ex)
			{
				Program.Logger.Log("Could not create DataSource ›{0}‹. Error: {1}", dataSource.Provider, ex);
				continue;
			}

			if (source == null)
			{
				Program.Logger.Log("Unknown DataSource ›{0}‹", dataSource.Provider);
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
				Program.Logger.Log("Ignoring DataTarget without provider.");
				continue;
			}

			DataTargets.ProviderBase target = null;

			switch (dataTarget.Provider.ToLower().Trim())
			{
				case "directory":
					target = new DataTargets.ProviderDirectory(dataTarget);
					break;
				case "dropbox":
					target = new DataTargets.ProviderDropbox(dataTarget);
					break;
			}

			if (target == null)
			{
				Program.Logger.Log("Unknown DataTarget ›{0}‹", dataTarget.Provider);
				continue;
			}

			if (dataTarget.Strategy == null)
			{
				continue;
			}

			if (string.IsNullOrEmpty(dataTarget.Strategy.Provider))
			{
				Program.Logger.Log("Ignoring strategy without provider.");
				continue;
			}

			// get strategy second
			DataStrategies.ProviderBase strategy = null;

			try
			{
				switch (dataTarget.Strategy.Provider.ToLower().Trim())
				{
					case "days":
						strategy = new DataStrategies.ProviderDays(dataTarget.Strategy, target);
						break;
					case "generations":
						strategy = new DataStrategies.ProviderGenerations(dataTarget.Strategy, target);
						break;
				}
			}
			catch (Exception ex)
			{
				Program.Logger.Log("Could not initialize strategry ›{0}‹. Error: {1}", dataTarget.Strategy.Provider, ex);
				continue;
			}

			if (strategy == null)
			{
				Program.Logger.Log("Unknown Strategry ›{0}‹", dataTarget.Strategy.Provider);
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

		foreach (var provider in providers)
		{
			var filesProvider = provider.Load(_directoryTemp);

			Logger.Log("Created {0} backups for ›{1}‹.", filesProvider == null ? "0" : filesProvider.Count().ToString(), provider.Config.Name);

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
			Program.Logger.Log("Zipping {0} backups of ›{1}‹", provider.Value.Count(), provider.Key.Config.Name);

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
						Program.Logger.Log("Could not zip source. Zip: {0}, Error: {1}", Path.GetFileName(fileZip), ex);
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
		foreach (var provider in providers)
		{
			Program.Logger.Log("Saving {0} backups with ›{1}‹", files.Count, provider.Target.Config.Name);

			provider.Save(files);
		}
	}
	#endregion
}