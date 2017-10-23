using System.Configuration;

namespace Configurations
{
	/// <summary>
	/// Represents the dataSources-section in the app.config file.
	/// </summary>
	public class DataSourcesSection : ConfigurationSection
	{
		[ConfigurationProperty("", IsRequired = true, IsDefaultCollection = true)]
		public DataSources DataSources
		{
			get { return (DataSources)this[""]; }
			set { this[""] = value; }
		}

		/// <summary>
		/// Just a litte wratter to access the DataSources property easily.
		/// </summary>
		/// <returns></returns>
		public static DataSources GetDataSources()
		{
			return (ConfigurationManager.GetSection("dataSources") as DataSourcesSection).DataSources;
		}
	}
}