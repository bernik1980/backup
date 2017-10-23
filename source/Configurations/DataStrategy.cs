using System.Configuration;

namespace Configurations
{
	/// <summary>
	/// Represents a strategy in the config file.
	/// </summary>
	public class DataStrategy : ConfigurationElement
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
		/// The number of revisions to keep.
		/// This is the string representation, use the Revisions-Property to access the int value.
		/// </summary>
		[ConfigurationProperty("revisions")]
		public string RevisionsString
		{
			get
			{
				return this["revisions"] as string;
			}
			set
			{
				this["revisions"] = value;
			}
		}

		/// <summary>
		/// The number of revisions to keep.
		/// </summary>
		public int Revisions
		{
			get
			{
				var revisions = 0;

				int.TryParse(this.RevisionsString, out revisions);

				return revisions;
			}
		}
	}
}