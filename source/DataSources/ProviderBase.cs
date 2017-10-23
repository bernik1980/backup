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
	}
}