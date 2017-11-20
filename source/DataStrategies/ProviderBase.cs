using Logging;
using System;
using System.Collections.Generic;
using Configurations;
using DataTargets;

namespace DataStrategies
{
	/// <summary>
	/// Base class for all strategies.
	/// </summary>
	internal abstract class ProviderBase : IDisposable
	{
		/// <summary>
		/// The related configuration.
		/// </summary>
		protected Configurations.DataStrategy _config;

		/// <summary>
		/// The logger to use.
		/// </summary>
		protected LoggerBase _logger;

		protected DataTargets.ProviderBase _target;
		/// <summary>
		/// The related target.
		/// </summary>
		public DataTargets.ProviderBase Target { get { return _target; } }

		/// <summary>
		/// The ensure, that each backup is handled via the same timestamp, we du not use DateTime.UtcNow every time, but init the timestamp when the provider is created.
		/// </summary>
		protected DateTime _timestamp;
		private DataStrategy config;
		private DataTargets.ProviderBase target;

		/// <summary>
		/// Creates a new privider.
		/// </summary>
		/// <param name="config">The configuration for this provider.</param>
		/// <param name="target">The DataTarget this strategy will handle-</param>
		public ProviderBase(Configurations.DataStrategy config, DataTargets.ProviderBase target, LoggerBase logger)
		{
			_config = config;
			_target = target;
			_logger = logger;

			_timestamp = DateTime.UtcNow;
		}

		public ProviderBase(DataStrategy config, DataTargets.ProviderBase target)
		{
			this.config = config;
			this.target = target;
		}

		/// <summary>
		/// Saves the list of files via the DataSource and applies the strategy.
		/// </summary>
		/// <param name="files"></param>
		public abstract void Save(IEnumerable<string> files);

		public virtual void Dispose()
		{
			_target.Dispose();
		}
	}
}