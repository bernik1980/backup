using Dropbox.Api;
using Dropbox.Api.Files;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace DataTargets
{
	/// <summary>
	/// The dropbox provider saves the backup files to a dropbox account.
	/// The target property of the configuration needs to contain the OAuth2AccessToken and optionally a directory path.
	/// </summary>
	internal class ProviderDropbox : ProviderBase
	{
		/// <summary>
		/// An optionally path where the files will be saved.
		/// This can be used, to differ different sources saving to the same dropbox.
		/// </summary>
		private string _path;

		// keep a reference to dropbox handlers because of mulitple usages
		private WebRequestHandler _requestHandler;
		private HttpClient _httpClient;
		private DropboxClient _client;

		#region Initialization
		internal ProviderDropbox(Configurations.DataTarget config)
			: base(config)
		{
			// parse config
			// we assume the target is the token
			var token = _config.Target;
			
			// check for optional path
			var args = token.Split(';');
			foreach (var arg in args)
			{
				var keyValue = arg.Split('=');

				if (keyValue.Length == 2)
				{
					switch (keyValue[0].Trim().ToLower())
					{
						case "token":
							token = keyValue[1].Trim();
							break;
						case "path":
							_path = keyValue[1].Trim();
							break;
					}
				}
			}

			if (string.IsNullOrEmpty(token))
			{
				throw new Exception("Token is missing.");
			}

			// format path
			if (_path == null)
			{
				_path = string.Empty;
			}
			if (!_path.StartsWith("/"))
			{
				_path = "/" + _path;
			}
			if (!_path.EndsWith("/"))
			{
				_path += "/";
			}

			// init client
			DropboxCertHelper.InitializeCertPinning();

			_requestHandler = new WebRequestHandler { ReadWriteTimeout = 10 * 1000 };

			// Specify socket level timeout which decides maximum waiting time when no bytes are
			// received by the socket.
			_httpClient = new HttpClient(_requestHandler)
			{
				// Specify request level timeout which decides maximum time that can be spent on
				// download/upload files.
				Timeout = TimeSpan.FromMinutes(20)
			};

			var clientConfig = new DropboxClientConfig(_config.Name)
			{
				HttpClient = _httpClient
			};

			_client = new DropboxClient(token, clientConfig);
		}

		public override void Dispose()
		{
			_requestHandler.Dispose();
			_httpClient.Dispose();
			_client.Dispose();

			base.Dispose();
		}
		#endregion

		#region Functionality
		internal override List<string> Save(string directory, IEnumerable<string> files)
		{
			var filesSaved = new List<string>();

			foreach (var file in files)
			{
				try
				{
					Task.Run(() => SaveAsync(directory, file)).Wait();

					filesSaved.Add(file);
				}
				catch (Exception ex)
				{
					Program.Logger.Log("Could not upload file to dropbox. File: {0}, Error: {1}", Path.GetFileName(file), ex.ToString());
				}
			}

			return filesSaved;
		}

		private async Task SaveAsync(string directory, string file)
		{
			var path = _path;
			path += directory;
			path += "/";
			path += Path.GetFileName(file);

			string sessionId = null;

			// upload in chunks with sessions
			var buffer = new byte[128 * 1024];

			using (var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
			{
				while (true)
				{
					var byteRead = fileStream.Read(buffer, 0, buffer.Length);

					using (var memoryStream = new MemoryStream(buffer, 0, byteRead))
					{
						if (sessionId == null)
						{
							sessionId = (await _client.Files.UploadSessionStartAsync(body: memoryStream)).SessionId;
						}
						else
						{
							var cursor = new UploadSessionCursor(sessionId, (ulong)(fileStream.Position - byteRead));

							if (byteRead == buffer.Length)
							{
								await _client.Files.UploadSessionAppendV2Async(cursor, body: memoryStream);
							}
							else
							{
								await _client.Files.UploadSessionFinishAsync(cursor, new CommitInfo(path, WriteMode.Overwrite.Instance), memoryStream);

								break;
							}
						}
					}
				}
			}
		}

		internal override void DeleteDirectory(string directory)
		{
			Task.Run(() => DeleteDirectoryAsync(directory)).Wait();
		}

		private async Task DeleteDirectoryAsync(string directory)
		{
			var path = _path;
			path += directory;

			try
			{
				await _client.Files.DeleteV2Async(path);
			}
			catch // error if path did not exists
			{
			}
		}
		#endregion
	}
}