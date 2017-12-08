using Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using ValueObjects;

namespace DataTargets
{
	/// <summary>
	/// The ftp provider saves the backup files to a ftp server.
	/// </summary>
	internal class ProviderFtp : ProviderBase
	{
		protected Configuration _connection;

		private string _host;
		private string _user { get { return _connection.GetValue<string>("user"); } }
		private string _password { get { return _connection.GetValue<string>("password"); } }

		#region Initialization
		internal ProviderFtp(Configurations.DataTarget config, LoggerBase logger)
			: base(config, logger)
		{
			_connection = new Configuration(config.Target);

			// add path to host and ensure a trailing /
			_host = _connection.GetValue<string>("host");
			if (!_host.EndsWith("/"))
			{
				_host += "/";
			}

			var path = _connection.GetValue<string>("path");
			if (path != null)
			{
				_host += path;

				if (!_host.EndsWith("/"))
				{
					_host += "/";
				}
			}
		}
		#endregion

		#region Functionality
		private WebRequest CreateRequest(string method, string path = null)
		{
			// try to create directory first
			var request = WebRequest.Create(_host + (path ?? string.Empty));
			request.Method = method;
			if (!string.IsNullOrEmpty(_user))
			{
				request.Credentials = new NetworkCredential(_user, _password);
			}

			return request;
		}
		#endregion

		#region ProviderBase
		internal override List<string> Save(string directory, IEnumerable<string> files)
		{
			_logger.Log(_config.Name, LoggerPriorities.Info, "Saving {0} backup{1:'s';'s';''}.", files.Count(), files.Count() - 1);

			// try to create directory first
			var request = this.CreateRequest(WebRequestMethods.Ftp.MakeDirectory, directory);
			// an error will occur when the directory does already exist
			// ignore every other error, they will throw later too
			try
			{
				request.GetResponse().Close();
			}
			catch
			{
			}

			var filesSaved = new List<string>();

			foreach (var file in files)
			{
				_logger.Log(_config.Name, LoggerPriorities.Verbose, "Uploading file: {0}", Path.GetFileName(file));

				try
				{
					using (var client = new WebClient())
					{
						if (!string.IsNullOrEmpty(_user))
						{
							client.Credentials = new NetworkCredential(_user, _password);
						}

						client.UploadFile(_host + directory + "/" + Path.GetFileName(file), WebRequestMethods.Ftp.UploadFile, file);
					}

					filesSaved.Add(file);
				}
				catch (Exception ex)
				{
					_logger.Log(_config.Name, LoggerPriorities.Error, "Could not upload file via ftp. File: {0}, Error: {1}", Path.GetFileName(file), ex);
				}
			}

			_logger.Log(_config.Name, LoggerPriorities.Info, "Saved {0} backup{1:'s';'s';''}.", filesSaved.Count, filesSaved.Count - 1);

			return filesSaved;
		}

		internal override void DeleteDirectory(string directory)
		{
			// since we cannot delete a non-empty directory,
			// we need to do this recursivly
			var files = this.GetFiles(directory);

			if (files == null || files.Count == 0)
			{
				return;
			}

			// delete every file
			// we assume, that the directory only contains files, since we do never create directories
			foreach (var file in files)
			{
				this.DeleteFile(directory + "/" + file);
			}

			var request = this.CreateRequest(WebRequestMethods.Ftp.RemoveDirectory, directory);
			// an error will occur when the directory does already exist
			// ignore every other error, they will throw later too
			try
			{
				request.GetResponse().Close();
			}
			catch (Exception ex)
			{
				_logger.Log(_config.Name, LoggerPriorities.Info, "Could not delete directory {0}. Error: {1}", directory, ex);
			}
		}

		private List<string> GetFiles(string directory)
		{
			List<string> files = new List<string>();

			var request = this.CreateRequest(WebRequestMethods.Ftp.ListDirectory, directory);

			// an error will occur if the directory does not exist
			try
			{
				var response = (FtpWebResponse)request.GetResponse();
				var responseStream = response.GetResponseStream();
				using (var reader = new StreamReader(responseStream))
				{
					while (!reader.EndOfStream)
					{
						files.Add(reader.ReadLine());
					}
				}
				response.Close();
			}
			catch
			{
			}

			return files;
		}

		private void DeleteFile(string path)
		{
			var request = this.CreateRequest(WebRequestMethods.Ftp.DeleteFile, path);
			try
			{
				request.GetResponse().Close();
			}
			catch
			{
			}
		}
		#endregion
	}
}