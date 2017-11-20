using System.Configuration;

namespace Configurations
{
	/// <summary>
	/// Represents an logger from the config file.
	/// </summary>
	public class Logger : ConfigurationSection
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
		/// The identifier of the provider.
		/// </summary>
		[ConfigurationProperty("priorities")]
		public string Priorities
		{
			get
			{
				return this["priorities"] as string;
			}
			set
			{
				this["priorities"] = value;
			}
		}

		/// <summary>
		/// The settings of the provider.
		/// </summary>
		[ConfigurationProperty("settings")]
		public string Settings
		{
			get
			{
				return this["settings"] as string;
			}
			set
			{
				this["settings"] = value;
			}
		}
	}
}