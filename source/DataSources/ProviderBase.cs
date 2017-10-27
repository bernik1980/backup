using System;
using System.Collections.Generic;
using ValueObjects;

namespace DataSources
{
	/// <summary>
	/// Base class for all source providers.
	/// </summary>
	internal abstract class ProviderBase
	{
		protected Configurations.DataSource _config;
		/// <summary>
		/// The related configuration.
		/// </summary>
		public Configurations.DataSource Config { get { return _config; } }

		/// <summary>
		/// Do not backup these.
		/// </summary>
		protected List<string> _excluded;
		/// <summary>
		/// Only backup these.
		/// </summary>
		protected List<string> _included;

		internal ProviderBase(Configurations.DataSource config)
		{
			if (string.IsNullOrEmpty(config.Source))
			{
				throw new Exception("No source specified");
			}

			_config = config;

			// get excluded databases
			if (config.Exclude != null && !string.IsNullOrEmpty(config.Exclude))
			{
				_excluded = new List<string>();

				foreach (var database in config.Exclude.Split(','))
				{
					_excluded.Add(database.Trim());
				}
			}

			// get included databases
			if (config.Include != null && !string.IsNullOrEmpty(config.Include))
			{
				_included = new List<string>();

				foreach (var database in config.Include.Split(','))
				{
					_included.Add(database.Trim());
				}
			}
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

			if (_included != null)
			{
				sources.RemoveAll(source => !_included.Contains(source));
			}

			// ignore excluded databases
			if (_excluded != null)
			{
				sources.RemoveAll(source => _excluded.Contains(source));
			}

			return sources;
		}
	}
}