using System.Configuration;

namespace Configurations
{
	/// <summary>
	/// Represents the logger-section in the app.config file.
	/// </summary>
	public class LoggerSection : ConfigurationSection
	{
		[ConfigurationProperty("", IsRequired = true, IsDefaultCollection = true)]
		public Loggers Loggers
		{
			get { return (Loggers)this[""]; }
			set { this[""] = value; }
		}

		/// <summary>
		/// Just a litte wratter to access the Loggers property easily.
		/// </summary>
		/// <returns></returns>
		public static Loggers GetLoggers()
		{
			return (ConfigurationManager.GetSection("loggers") as LoggerSection).Loggers;
		}
	}
}