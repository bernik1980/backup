using System.Configuration;

namespace Configurations
{
	/// <summary>
	/// Represents the child-chollection of the dataSources section.
	/// </summary>
	public class DataSources : ConfigurationElementCollection
	{
		protected override ConfigurationElement CreateNewElement()
		{
			return new DataSource();
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((DataSource)element).Name;
		}
	}
}