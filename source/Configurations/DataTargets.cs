using System.Configuration;

namespace Configurations
{
	public class DataTargets : ConfigurationElementCollection
	{
		protected override ConfigurationElement CreateNewElement()
		{
			return new DataTarget();
		}

		/// <summary>
		/// Represents the child-chollection of the dataTargets section.
		/// </summary>
		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((DataTarget)element).Name;
		}
	}
}