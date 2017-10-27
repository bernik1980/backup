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
		/// Creates a new provider with the related configuration.
		/// </summary>
		/// <param name="config"></param>
		internal ProviderBase(Configurations.DataTarget config)
		{
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