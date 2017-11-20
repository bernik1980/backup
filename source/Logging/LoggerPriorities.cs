using System;

namespace Logging
{
	[Flags]
	public enum LoggerPriorities
	{
		None = 0,
		Verbose = 1,
		Info = 2,
		Error = 4,
		All = Verbose | Info | Error
	}
}