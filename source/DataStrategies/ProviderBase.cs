using System;
using System.Collections.Generic;

namespace DataStrategies
{
	/// <summary>
	/// Base class for all strategies.
	/// </summary>
	internal abstract class ProviderBase : IDisposable
	{
		protected Configurations.DataStrategy _config;
		protected DataTargets.ProviderBase _target;

		/// <summary>
		/// The ensure, that each backup is handled via the same timestamp, we du not use DateTime.UtcNow every time, but init the timestamp when the provider is created.
		/// </summary>
		protected DateTime _timestamp;

		/// <summary>
		/// Creates a new privider.
		/// </summary>
		/// <param name="config">The configuration for this provider.</param>
		/// <param name="target">The DataTarget this strategy will handle-</param>
		public ProviderBase(Configurations.DataStrategy config, DataTargets.ProviderBase target)
		{
			_config = config;
			_target = target;

			_timestamp = DateTime.UtcNow;
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