namespace Logging
{
	/// <summary>
	/// Base class for all loggers.
	/// </summary>
	public abstract class LoggerBase
	{
		/// <summary>
		/// Logs the specified text.
		/// The text will be used as format for the given args.
		/// </summary>
		/// <param name="format"></param>
		/// <param name="args"></param>
		public void Log(string format, params object[] args)
		{
			this.Log(args != null ? string.Format(format, args) : format);
		}

		/// <summary>
		/// Logs the specified text.
		/// </summary>
		/// <param name="text"></param>
		public abstract void Log(string text);
	}
}