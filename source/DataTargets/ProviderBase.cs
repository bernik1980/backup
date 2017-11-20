using Logging;
using System;
using System.Collections.Generic;

namespace DataTargets
{
	/// <summary>
	/// A DataTarget provider handles saving of created backups.
	/// </summary>
	internal abstract class ProviderBase : IDisposable
	{
		protected Configurations.DataTarget _config;
		/// <summary>
		/// The related configuration.
		/// </summary>
		public Configurations.DataTarget Config { get { return _config; } }

		/// <summary>
		/// The logger to use.
		/// </summary>
		protected LoggerBase _logger;

		/// <summary>
		/// Creates a new provider with the related configuration.
		/// </summary>
		/// <param name="config"></param>
		internal ProviderBase(Configurations.DataTarget config, LoggerBase logger)
		{
			_logger = logger;

			_logger.Log(config.Name, LoggerPriorities.Verbose, "Initializing");

			_config = config;
		}

		/// <summary>
		/// Saves a list of files to the specified directory.
		/// </summary>
		/// <param name="directory"></param>
		/// <param name="files"></param>
		/// <returns></returns>
		internal abstract List<string> Save(string directory, IEnumerable<string> files);

		/// <summary>
		/// Deletes a directory with the given name.
		/// </summary>
		/// <param name="directory"></param>
		internal abstract void DeleteDirectory(string directory);

		public virtual void Dispose()
		{
			// override point
		}
	}
}