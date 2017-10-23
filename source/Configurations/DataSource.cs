using System.Configuration;

namespace Configurations
{
	/// <summary>
	/// Represents an dataSource from the config file.
	/// </summary>
	public class DataSource : ConfigurationSection
	{
		/// <summary>
		/// The identifier of the provider.
		/// </summary>
		[ConfigurationProperty("provider", IsRequired = true)]
		public string Provider
		{
			get
			{
				return this["provider"] as string;
			}
			set
			{
				this["provider"] = value;
			}
		}

		/// <summary>
		/// The name for the source.
		/// Must be unique amongst all sources.
		/// </summary>
		[ConfigurationProperty("name", IsRequired = true)]
		public string Name
		{
			get
			{
				return this["name"] as string;
			}
			set
			{
				this["name"] = value;
			}
		}

		/// <summary>
		/// Source arguments of the provider.
		/// </summary>
		[ConfigurationProperty("source", IsRequired = true)]
		public string Source
		{
			get
			{
				return this["source"] as string;
			}
			set
			{
				this["source"] = value;
			}
		}

		/// <summary>
		/// Excludes for the source.
		/// Data depends on the provider.
		/// </summary>
		[ConfigurationProperty("exclude", IsRequired = false)]
		public string Exclude
		{
			get
			{
				return this["exclude"] as string;
			}
			set
			{
				this["exclude"] = value;
			}
		}

		/// <summary>
		/// Includes for the source.
		/// If set, only this will be backed up.
		/// Data depends on the provider.
		/// </summary>
		[ConfigurationProperty("include", IsRequired = false)]
		public string Include
		{
			get
			{
				return this["include"] as string;
			}
			set
			{
				this["include"] = value;
			}
		}
	}
}