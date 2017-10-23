using System.Configuration;

namespace Configurations
{
	/// <summary>
	/// Represents the dataTargets-section in the app.config file.
	/// </summary>
	public class DataTargetsSection : ConfigurationSection
	{
		[ConfigurationProperty("", IsRequired = true, IsDefaultCollection = true)]
		public DataTargets DataTargets
		{
			get { return (DataTargets)this[""]; }
			set { this[""] = value; }
		}

		/// <summary>
		/// Just a litte wratter to access the DataTargets property easily.
		/// </summary>
		/// <returns></returns>
		public static DataTargets GetDataTargets()
		{
			return (ConfigurationManager.GetSection("dataTargets") as DataTargetsSection).DataTargets;
		}
	}
}