using System.Configuration;

namespace Configurations
{
	/// <summary>
	/// Represents an dataTarget from the config file.
	/// </summary>
	public class DataTarget : ConfigurationSection
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
		/// The name for the target.
		/// Must be unique amongst all targets.
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
		/// Target arguments of the provider.
		/// </summary>
		[ConfigurationProperty("target", IsRequired = true)]
		public string Target
		{
			get
			{
				return this["target"] as string;
			}
			set
			{
				this["target"] = value;
			}
		}

		/// <summary>
		/// The strategy to apply
		/// </summary>
		[ConfigurationProperty("strategy")]
		public DataStrategy Strategy
		{
			get
			{
				return (DataStrategy)this["strategy"];
			}
			set
			{
				this["strategy"] = value;
			}
		}
	}
}