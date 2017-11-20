using Configurations;
using Logging;
using System.Collections.Generic;
using ValueObjects;

namespace DataSources
{
	/// <summary>
	/// Base class for all source providers.
	/// </summary>
	internal abstract class ProviderBase
	{
		protected DataSource _config;
		/// <summary>
		/// The related configuration.
		/// </summary>
		public DataSource Config { get { return _config; } }

		/// <summary>
		/// The logger to use.
		/// </summary>
		protected LoggerBase _logger;

		/// <summary>
		/// Do not backup these.
		/// </summary>
		protected List<string> _excluded;
		/// <summary>
		/// Only backup these.
		/// </summary>
		protected List<string> _included;
		private DataSource config;

		internal ProviderBase(DataSource config, LoggerBase logger)
		{
			_logger = logger;

			_logger.Log(config.Name, LoggerPriorities.Verbose, "Initializing");

			if (string.IsNullOrEmpty(config.Source))
			{
				_logger.Log(config.Name, LoggerPriorities.Error, "No source specified.");
				return;
			}

			_config = config;

			// get excluded databases
			if (!string.IsNullOrEmpty(_config.Exclude))
			{
				_excluded = new List<string>();

				foreach (var database in _config.Exclude.Split(','))
				{
					_excluded.Add(database.Trim());
				}
			}

			// get included databases
			if (!string.IsNullOrEmpty(_config.Include))
			{
				_included = new List<string>();

				foreach (var database in _config.Include.Split(','))
				{
					_included.Add(database.Trim());
				}
			}
		}

		public ProviderBase(DataSource config)
		{
			this.config = config;
		}

		/// <summary>
		/// Loads all files from the source.
		/// </summary>
		/// <param name="directory"></param>
		/// <returns></returns>
		internal abstract IEnumerable<BackupFile> Load(string directory);

		/// <summary>
		/// Each provider class should return all possible source files available without any filter applied.
		/// The filtering will be handled in GetSourcesFiltered.
		/// </summary>
		/// <returns></returns>
		protected abstract List<string> GetSources();

		/// <summary>
		/// Gets all possible source files calling GetDatabases() and apply _included and _excluded.
		/// </summary>
		/// <returns></returns>
		protected List<string> GetSourcesFiltered()
		{
			var sources = this.GetSources();

			if (sources != null)
			{
				if (_included != null)
				{
					sources.RemoveAll(source => !_included.Contains(source));
				}

				// ignore excluded databases
				if (_excluded != null)
				{
					sources.RemoveAll(source => _excluded.Contains(source));
				}
			}

			return sources;
		}
	}
}