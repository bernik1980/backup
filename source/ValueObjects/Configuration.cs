using System;
using System.Collections.Generic;

namespace ValueObjects
{
	public class Configuration
	{
		private Dictionary<string, string> _connection;

		public Configuration(string nameValueString)
		{
			_connection = new Dictionary<string, string>();

			if (!string.IsNullOrEmpty(nameValueString))
			{
				foreach (var nameValue in nameValueString.Split(';'))
				{
					var nameValueArgs = nameValue.Split('=');
					var name = nameValueArgs[0].Trim().ToLower().Replace(" ", "");

					if (string.IsNullOrEmpty(name))
					{
						continue;
					}

					var value = nameValueArgs.Length > 1 ? nameValueArgs[1].Trim() : null;

					_connection[name.ToLower()] = value;
				}
			}
		}

		public T GetValue<T>(string name, T defaultValue = default(T))
		{
			if (!_connection.ContainsKey(name))
			{
				return defaultValue;
			}

			var type = typeof(T);

			if (type == typeof(int))
			{
				var value = default(int);
				if (int.TryParse(_connection[name], out value))
				{
					return (T)Convert.ChangeType(value, type);
				}
			}
			else if (type == typeof(string))
			{
				return (T)Convert.ChangeType(_connection[name], type);
			}

			return defaultValue;
		}
	}
}