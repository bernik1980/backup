using System.Configuration;

namespace Configurations
{
	public class Loggers : ConfigurationElementCollection
	{
		protected override ConfigurationElement CreateNewElement()
		{
			return new Logger();
		}

		/// <summary>
		/// Represents the child-chollection of the loggers section.
		/// </summary>
		protected override object GetElementKey(ConfigurationElement element)
		{
			return element;
		}
	}
}